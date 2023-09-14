using GeneticSharp;

namespace PortfolioOptimization;

public class OptimizationSharpAlgorithm : IOptimizationAlgorithm
{
    private readonly GeneticAlgorithm _ga;
    private readonly OptimizerContracts.FitnessAlgorithm _fitnessAlgorithm;
    private readonly DataHolder _dataHolder;
    private readonly DataHolder _originalDataHolder;
    private readonly int _maxValue;
    private readonly int _minValue;
    private List<HashSet<string>> _correlatedStrategies;

    public OptimizationSharpAlgorithm(int numberOfStrategies, DataHolder dataHolder, int minValue, int maxValue, OptimizerContracts.FitnessAlgorithm fitnessAlgorithm)
    {

        _fitnessAlgorithm = fitnessAlgorithm;
        _dataHolder = dataHolder;
        _originalDataHolder = dataHolder;
        _maxValue = maxValue;
        _minValue = minValue;
        
        int populationSize = 1000;

        if (_fitnessAlgorithm == OptimizerContracts.FitnessAlgorithm.Correlation)
        {
            double correlationThreshold = 0.7;
            List<AccumulatedProfitByStrategy> accumulatedPnlByStrategies = new();
            //if we are working with correlation do the following:
            //1. create for each strategy accumulated Pnl
            HashSet<string> allStrategies = new HashSet<string>();
            foreach (var strategyName in _dataHolder.StrategyList)
            {
                allStrategies.Add(strategyName);
                accumulatedPnlByStrategies.Add(new AccumulatedProfitByStrategy(){StrategyName = strategyName});
            }
            for (var rowNumber = 0; rowNumber < _dataHolder.InitialData.Count; rowNumber++)
            {
                for (int i = 0; i < _dataHolder.StrategyList.Count; i++)
                {
                    if (rowNumber == 0)
                    {
                        accumulatedPnlByStrategies[i].AccumulatedProfitDaily.Add(
                            _dataHolder.InitialData[rowNumber].DailyPnlByStrategy[i]);
                    }
                    else
                    {
                        accumulatedPnlByStrategies[i].AccumulatedProfitDaily.Add(
                            accumulatedPnlByStrategies[i].AccumulatedProfitDaily[rowNumber - 1] +
                            _dataHolder.InitialData[rowNumber].DailyPnlByStrategy[i]);
                    }

                }
            }
            //2. given correlation threshold distribute all strategies by correlated sets
            _correlatedStrategies = new List<HashSet<string>>();
            foreach (var strategyName in allStrategies)
            {                    
                //add new set of correlated strategies
                _correlatedStrategies.Add(new HashSet<string>());
                //and add ourself to it
                _correlatedStrategies.Last().Add(strategyName);
                //remove from all
                allStrategies.RemoveWhere(s => s == strategyName);
                var xProfits = 
                    accumulatedPnlByStrategies.Find(s => s.StrategyName == strategyName)?.AccumulatedProfitDaily;
                
                foreach (var correlatedStrategyName in allStrategies)
                {
                    var yProfits = 
                        accumulatedPnlByStrategies.Find(s => s.StrategyName == correlatedStrategyName)?.AccumulatedProfitDaily;

                    double correlation = 0;
                    //check for correlation
                    if (xProfits != null && yProfits != null)
                    {
                        correlation = Correlation.CalculateCorrelation(xProfits, yProfits);
                    }
                    else
                    {
                        throw new MyException("One of profit lists is null");
                    }
                    
                    //if correlation is higher than threshold 
                    if (correlation >= correlationThreshold)
                    {
                        //add the strategy to our set and remove from allStrategies
                        _correlatedStrategies.Last().Add(correlatedStrategyName);
                        allStrategies.RemoveWhere(s => s == correlatedStrategyName);
                    }
                }
            }
            DataHolder newDataHolder = new DataHolder();
            //3. select from each set one strategy (just for beginning)
            foreach (var set in _correlatedStrategies)
            {
                //string strategyName = set.First();
                string strategyName = set.FirstOrDefault(x => x.Contains("VWAPSMA"));
                if (strategyName == default)
                {
                    //TODO Check
                    strategyName = set.First();
                }
                newDataHolder.StrategyList.Add(strategyName);
            }
            //4. Go over the create a new initialDataHolder and copy from dataHolder to it only one
            //   strategy from each correlated set
            foreach (var row in dataHolder.InitialData)
            {
                int i = 0;                    
                double[] selectedPnls = new double[newDataHolder.StrategyList.Count];
                foreach (var strategy in newDataHolder.StrategyList)
                {
                    int indexOfStrategyInInitialData = dataHolder.StrategyList.IndexOf(strategy);
                    selectedPnls[i++] = row.DailyPnlByStrategy[indexOfStrategyInInitialData];

                }                    
                newDataHolder.InitialData.Add(new DataHolder.Row(row.Date, selectedPnls));
            }
            
            //5. Assign _initialDataHolder = newDataHolder;
            _dataHolder = newDataHolder;
        }

        

        var population = new Population(populationSize, populationSize, 
            new MyChromosome(_dataHolder.StrategyList.Count, minValue, maxValue));
        var selection = new EliteSelection();
        var crossover = new UniformCrossover();
        var mutation = new UniformMutation(true);
        var fitness = new FuncFitness(EvaluateFitness);

        _ga = new GeneticAlgorithm(population, fitness, selection, crossover, mutation)
        {
            Termination = new GenerationNumberTermination(100)
        };
    }

    public void Start()
    {
        _ga.Start();
    }

    public int[] BestChromosome()
    {
        var result = new int[_originalDataHolder.StrategyList.Count];
        //produce a new chromosome where all the correlated strategies hae 0
        for (int i = 0; i < _originalDataHolder.StrategyList.Count; i++)
        {
            //find index of strategy in new dataholder
            int indexOfStrategy = _dataHolder.StrategyList.IndexOf(_originalDataHolder.StrategyList[i]);
            if (indexOfStrategy >= 0)
            {
                result[i] = Convert.ToInt32(((MyChromosome)_ga.BestChromosome).GetGene(indexOfStrategy).Value);
            }
            else
            {
                //TODO check
                result[i] = _maxValue / 6; //should be real value. maybe Gene divided by num of correlated strategies?
            }
        }

        return result;
    }

    public double BestFitness()
    {
        if (_ga.BestChromosome.Fitness != null) return _ga.BestChromosome.Fitness.Value;
        return -1;
    }

    private double EvaluateFitness(IChromosome chromosome)
    {


        int[] targetArray = new int[chromosome.Length];
        for (int i = 0; i < chromosome.Length; i++)
        {
            targetArray[i] = Convert.ToInt32(((MyChromosome) chromosome).GetGene(i).Value);
        }
        

        double evaluationValue;
        switch (_fitnessAlgorithm)
        {
            case OptimizerContracts.FitnessAlgorithm.Linearity:
                evaluationValue = LinearInterpolation.CalculateRSquaredForOnePermutation(targetArray, _dataHolder);
                break;
            case OptimizerContracts.FitnessAlgorithm.ProfitByDrawdown:
                evaluationValue = Profit.CalculateProfitForOnePermutation(targetArray, _dataHolder);
                evaluationValue /= DrawDown.CalculateMaxDrawdownForOnePermutation(targetArray, _dataHolder);
                break;
            case OptimizerContracts.FitnessAlgorithm.SharpeOnEma:
                evaluationValue = SharpeOnEma.CalculateSharpeOnEmaForOnePermutation(targetArray, _dataHolder);
                break;
            case OptimizerContracts.FitnessAlgorithm.Sortino:
                evaluationValue = Sortino.CalculateSortinoForOnePermutation(targetArray, _dataHolder);
                break;
            case OptimizerContracts.FitnessAlgorithm.MaxProfit:
                evaluationValue = MaxProfit.CalculateAccumulatedProfit(targetArray, _dataHolder);
                break;
            case OptimizerContracts.FitnessAlgorithm.Correlation:
                evaluationValue = Sharpe.CalculateSharpeForOnePermutation(targetArray, _dataHolder);
                break;
            case OptimizerContracts.FitnessAlgorithm.Sharpe:
            default:
                evaluationValue = Sharpe.CalculateSharpeForOnePermutation(targetArray, _dataHolder);
                break;
        }
        return evaluationValue;
    }
}
public class MyChromosome : ChromosomeBase
{
    private readonly int _numberOfStrategies;
    private readonly int _minValue;
    private readonly int _maxValue;

    public MyChromosome(int numberOfStrategies, int minValue, int maxValue) : base(numberOfStrategies)
    {
        _numberOfStrategies = numberOfStrategies;
        _minValue = minValue;
        _maxValue = maxValue;

        for (int i = 0; i < Length; i++)
        {
            ReplaceGene(i, GenerateGene(i));
        }
    }


    public sealed override Gene GenerateGene(int geneIndex)
    {
        // Generate a random value between min and max with step size.
        int value = RandomizationProvider.Current.GetInt(_minValue, _maxValue + 1);
        return new Gene(value);
    }

    public override IChromosome CreateNew()
    {
        return new MyChromosome(_numberOfStrategies, _minValue, _maxValue);
    }
}

public class AccumulatedProfitByStrategy
{
    public string StrategyName = string.Empty;
    public readonly List<double> AccumulatedProfitDaily = new();
}