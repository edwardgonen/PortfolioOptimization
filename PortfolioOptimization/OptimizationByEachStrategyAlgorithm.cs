namespace PortfolioOptimization;

public class OptimizationByEachStrategyAlgorithm : IOptimizationAlgorithm
{
    public OptimizationByEachStrategyAlgorithm(int numberOfStrategies, DataHolder dataHolder, int minValue, int maxValue, OptimizerContracts.FitnessAlgorithm fitnessAlgorithm)
    {
        var targetArray = new int[numberOfStrategies];
        var sharpePerStrategy = new double[numberOfStrategies];
        var pnlPerStrategy = new double[numberOfStrategies, dataHolder.InitialData.Count];

        for (int strategyNumber = 0; strategyNumber < numberOfStrategies; strategyNumber++)
        {
            for (int dateRowNumber = 0; dateRowNumber < dataHolder.InitialData.Count; dateRowNumber++)
            {
                pnlPerStrategy[strategyNumber, dateRowNumber] += dataHolder.InitialData[dateRowNumber].DailyAccumulatedPnlByStrategy[strategyNumber];
            }
            //calculate sharpe
            sharpePerStrategy[strategyNumber] = Sharpe.CalculateSharpeRatio(Utilities.CustomArray<double>.GetColumn(pnlPerStrategy, strategyNumber));
        }
        
    }

    public void Start()
    {

    }

    public int[] BestChromosome()
    {
        return new int [5];
    }

    public double BestFitness()
    {
        return -1;
    }
}
