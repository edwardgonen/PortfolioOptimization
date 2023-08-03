namespace PortfolioOptimization;

public class OptimizationByEachStrategyAlgorithm : IOptimizationAlgorithm
{
    private int[] result;
    private double bestFitness;
    public OptimizationByEachStrategyAlgorithm(int numberOfStrategies, DataHolder dataHolder, int minValue, int maxValue, OptimizerContracts.FitnessAlgorithm fitnessAlgorithm)
    {
        result = new int[numberOfStrategies];
        var sharpePerStrategy = new double[numberOfStrategies];
        var pnlPerStrategy = new double[numberOfStrategies, dataHolder.InitialData.Count];

        for (int strategyNumber = 0; strategyNumber < numberOfStrategies; strategyNumber++)
        {
            double accumulatedPnl = 0;
            for (int dateRowNumber = 0; dateRowNumber < dataHolder.InitialData.Count; dateRowNumber++)
            {
                accumulatedPnl += dataHolder.InitialData[dateRowNumber].DailyAccumulatedPnlByStrategy[strategyNumber];
                pnlPerStrategy[strategyNumber, dateRowNumber] = accumulatedPnl;
            }
            //calculate sharpe
            sharpePerStrategy[strategyNumber] = Sharpe.CalculateSharpeRatio(Utilities.CustomArray<double>.GetRow(pnlPerStrategy, strategyNumber));
        }
        
        //calculate contracts allocations
        //1. add up all sharpe
        double totalSharpe = sharpePerStrategy.Sum();
        double maxContracts = maxValue;

        for (int strategyNumber = 0; strategyNumber < numberOfStrategies; strategyNumber++)
        {
            result[strategyNumber] = Math.Max((int)(Math.Round((sharpePerStrategy[strategyNumber] / totalSharpe) * maxContracts)), 1);
        }

        bestFitness = totalSharpe;

    }

    public void Start()
    {

    }

    public int[] BestChromosome()
    {
        return result;
    }

    public double BestFitness()
    {
        return bestFitness;
    }
}
