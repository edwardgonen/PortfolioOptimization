namespace PortfolioOptimization;

public class OptimizationByEachStrategyAlgorithm : IOptimizationAlgorithm
{
    private readonly int[] _result;
    private readonly double _bestFitness;
    public OptimizationByEachStrategyAlgorithm(int numberOfStrategies, DataHolder dataHolder, int minValue, int maxValue, OptimizerContracts.FitnessAlgorithm fitnessAlgorithm)
    {
        _result = new int[numberOfStrategies];
        var sharpePerStrategy = new double[numberOfStrategies];
        var pnlPerStrategy = new double[numberOfStrategies, dataHolder.InitialData.Count];
        //calculate contracts allocations
        double totalSharpe = 0;
        double maxSharpe = double.MinValue;
        for (int strategyNumber = 0; strategyNumber < numberOfStrategies; strategyNumber++)
        {
            for (int dateRowNumber = 0; dateRowNumber < dataHolder.InitialData.Count; dateRowNumber++)
            {
                pnlPerStrategy[strategyNumber, dateRowNumber] = dataHolder.InitialData[dateRowNumber].DailyAccumulatedPnlByStrategy[strategyNumber];
            }
            //calculate sharpe
            sharpePerStrategy[strategyNumber] = Sharpe.CalculateSharpeRatio(Utilities.CustomArray<double>.GetRow(pnlPerStrategy, strategyNumber));
            if (sharpePerStrategy[strategyNumber] > 0) totalSharpe += sharpePerStrategy[strategyNumber];
            if (sharpePerStrategy[strategyNumber] > maxSharpe) maxSharpe = sharpePerStrategy[strategyNumber];
        }
        
        
        double maxContracts = maxValue;

        for (int strategyNumber = 0; strategyNumber < numberOfStrategies; strategyNumber++)
        {
            if (sharpePerStrategy[strategyNumber] <= 0) _result[strategyNumber] = 1;
            else 
                _result[strategyNumber] = Math.Max((int)(Math.Round((sharpePerStrategy[strategyNumber] / maxSharpe) * maxContracts)), 1);
        }

        _bestFitness = totalSharpe;

    }

    public void Start()
    {

    }

    public int[] BestChromosome()
    {
        return _result;
    }

    public double BestFitness()
    {
        return _bestFitness;
    }
}
