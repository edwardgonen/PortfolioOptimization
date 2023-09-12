using GeneticSharp;

namespace PortfolioOptimization;

public class OptimizationSharpAlgorithm : IOptimizationAlgorithm
{
    private readonly GeneticAlgorithm _ga;
    private readonly OptimizerContracts.FitnessAlgorithm _fitnessAlgorithm;
    private readonly DataHolder _initialDataHolder;

    public OptimizationSharpAlgorithm(int numberOfStrategies, DataHolder dataHolder, int minValue, int maxValue, OptimizerContracts.FitnessAlgorithm fitnessAlgorithm)
    {

        _fitnessAlgorithm = fitnessAlgorithm;
        _initialDataHolder = dataHolder;

        int populationSize = 1000;
        var population = new Population(populationSize, populationSize, new YourCustomClass(numberOfStrategies, minValue, maxValue));
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
        var result = new int[_ga.BestChromosome.Length];
        for (int i = 0; i < _ga.BestChromosome.Length; i++)
        {
            result[i] = Convert.ToInt32(((YourCustomClass)_ga.BestChromosome).GetGene(i).Value);
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
            targetArray[i] = Convert.ToInt32(((YourCustomClass) chromosome).GetGene(i).Value);
        }
        

        double evaluationValue;
        switch (_fitnessAlgorithm)
        {
            case OptimizerContracts.FitnessAlgorithm.Linearity:
                evaluationValue = LinearInterpolation.CalculateRSquaredForOnePermutation(targetArray, _initialDataHolder);
                break;
            case OptimizerContracts.FitnessAlgorithm.ProfitByDrawdown:
                evaluationValue = Profit.CalculateProfitForOnePermutation(targetArray, _initialDataHolder);
                evaluationValue /= DrawDown.CalculateMaxDrawdownForOnePermutation(targetArray, _initialDataHolder);
                break;
            case OptimizerContracts.FitnessAlgorithm.SharpeOnEma:
                evaluationValue = SharpeOnEma.CalculateSharpeOnEmaForOnePermutation(targetArray, _initialDataHolder);
                break;
            case OptimizerContracts.FitnessAlgorithm.Sortino:
                evaluationValue = Sortino.CalculateSortinoForOnePermutation(targetArray, _initialDataHolder);
                break;
            case OptimizerContracts.FitnessAlgorithm.Sharpe:
            default:
                evaluationValue = Sharpe.CalculateSharpeForOnePermutation(targetArray, _initialDataHolder);
                break;
        }
        return evaluationValue;
    }
}
public class YourCustomClass : ChromosomeBase
{
    private readonly int _numberOfStrategies;
    private readonly int _minValue;
    private readonly int _maxValue;

    public YourCustomClass(int numberOfStrategies, int minValue, int maxValue) : base(numberOfStrategies)
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
        return new YourCustomClass(_numberOfStrategies, _minValue, _maxValue);
    }
}