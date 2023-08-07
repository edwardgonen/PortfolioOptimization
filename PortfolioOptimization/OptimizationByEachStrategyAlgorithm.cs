namespace PortfolioOptimization;

public class OptimizationByEachStrategyAlgorithm : IOptimizationAlgorithm
{
    private readonly int[] _result;
    private readonly double _bestFitness = 1;
    public OptimizationByEachStrategyAlgorithm(int numberOfStrategies, DataHolder dataHolder, int minValue, int maxValue, OptimizerContracts.FitnessAlgorithm fitnessAlgorithm)
    {
        switch (fitnessAlgorithm)
        {
            case OptimizerContracts.FitnessAlgorithm.MaxProfit:
            {
                //By Max Profit
                _result = GetAllocationByMaxProfit(numberOfStrategies, dataHolder, 1, maxValue);
            } break;
            case OptimizerContracts.FitnessAlgorithm.ProfitByDrawdown:
            {
                _result = GetAllocationByProfitByDrawDown(numberOfStrategies, dataHolder, 1, maxValue);
            } break;
            case OptimizerContracts.FitnessAlgorithm.Linearity:
            {
                _result = GetAllocationByLinearity(numberOfStrategies, dataHolder, 1, maxValue);
            } break;
            case OptimizerContracts.FitnessAlgorithm.Sortino:
            {
                _result = GetAllocationBySortino(numberOfStrategies, dataHolder, 1, maxValue);
            } break;
            case OptimizerContracts.FitnessAlgorithm.Sharpe:
            default:
            {
                //By Sharpe
                _result = GetAllocationBySharpe(numberOfStrategies, dataHolder, 1, maxValue);
            } break;
        }
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

    private int[] GetAllocationByLinearity(int numberOfStrategies, DataHolder dataHolder, int minNumOfContracts, int maxNumOfContracts)
    {
        var result = new int[numberOfStrategies];
        var pnlPerStrategy = new double[numberOfStrategies, dataHolder.InitialData.Count];
        //calculate contracts allocations
        double minDeviation = double.MaxValue;
        double maxDeviation = double.MinValue;
        var deviationPerStrategy = new double[numberOfStrategies];
        for (int strategyNumber = 0; strategyNumber < numberOfStrategies; strategyNumber++)
        {
            for (int dateRowNumber = 0; dateRowNumber < dataHolder.InitialData.Count; dateRowNumber++)
            {
                pnlPerStrategy[strategyNumber, dateRowNumber] = dataHolder.InitialData[dateRowNumber].DailyAccumulatedPnlByStrategy[strategyNumber];
            }
            //calculate sharpe
            deviationPerStrategy[strategyNumber] = Utilities.CalculateStandardDeviation(Utilities.CustomArray<double>.GetRow(pnlPerStrategy, strategyNumber));
            if (deviationPerStrategy[strategyNumber] < minDeviation) minDeviation = deviationPerStrategy[strategyNumber];
            if (deviationPerStrategy[strategyNumber] > maxDeviation) maxDeviation = deviationPerStrategy[strategyNumber];
        }

        double deviationRange = Math.Abs(maxDeviation - minDeviation);
        for (int strategyNumber = 0; strategyNumber < numberOfStrategies; strategyNumber++)
        {
            if (deviationPerStrategy[strategyNumber] <= 0) result[strategyNumber] = maxNumOfContracts;
            else 
                result[strategyNumber] = Math.Max((int)(Math.Round((maxNumOfContracts - deviationPerStrategy[strategyNumber] / deviationRange * maxNumOfContracts) )), minNumOfContracts);
        }

        return result;
    }
    private int[] GetAllocationBySharpe(int numberOfStrategies, DataHolder dataHolder, int minNumOfContracts, int maxNumOfContracts)
    {
        var result = new int[numberOfStrategies];
        var pnlPerStrategy = new double[numberOfStrategies, dataHolder.InitialData.Count];
        //calculate contracts allocations
        double maxSharpe = double.MinValue;        
        var sharpePerStrategy = new double[numberOfStrategies];
        for (int strategyNumber = 0; strategyNumber < numberOfStrategies; strategyNumber++)
        {
            for (int dateRowNumber = 0; dateRowNumber < dataHolder.InitialData.Count; dateRowNumber++)
            {
                pnlPerStrategy[strategyNumber, dateRowNumber] = dataHolder.InitialData[dateRowNumber].DailyAccumulatedPnlByStrategy[strategyNumber];
            }
            //calculate sharpe
            sharpePerStrategy[strategyNumber] = Sharpe.CalculateSharpeRatio(Utilities.CustomArray<double>.GetRow(pnlPerStrategy, strategyNumber));
            if (sharpePerStrategy[strategyNumber] > maxSharpe) maxSharpe = sharpePerStrategy[strategyNumber];
        }
        

        for (int strategyNumber = 0; strategyNumber < numberOfStrategies; strategyNumber++)
        {
            if (sharpePerStrategy[strategyNumber] <= 0) result[strategyNumber] = minNumOfContracts;
            else
            {
                double ratio = sharpePerStrategy[strategyNumber] / maxSharpe;
                result[strategyNumber] =
                    Math.Max((int) (Math.Round(ratio * maxNumOfContracts)),
                        minNumOfContracts);
            }
        }

        return result;
    }

    private int[] GetAllocationByMaxProfit(int numberOfStrategies, DataHolder dataHolder, int minNumOfContracts, int maxNumOfContracts)
    {
        
        var result = new int[numberOfStrategies];
        var pnlPerStrategy = new double[numberOfStrategies, dataHolder.InitialData.Count];
        //calculate contracts allocations
        double maxProfit = double.MinValue;        
        var profitPerStrategy = new double[numberOfStrategies];
        for (int strategyNumber = 0; strategyNumber < numberOfStrategies; strategyNumber++)
        {
            for (int dateRowNumber = 0; dateRowNumber < dataHolder.InitialData.Count; dateRowNumber++)
            {
                pnlPerStrategy[strategyNumber, dateRowNumber] = dataHolder.InitialData[dateRowNumber].DailyAccumulatedPnlByStrategy[strategyNumber];
                profitPerStrategy[strategyNumber] += pnlPerStrategy[strategyNumber, dateRowNumber];
            }

            if (profitPerStrategy[strategyNumber] > maxProfit) maxProfit = profitPerStrategy[strategyNumber];
        }

        for (int strategyNumber = 0; strategyNumber < numberOfStrategies; strategyNumber++)
        {
            if (profitPerStrategy[strategyNumber] <= 0) result[strategyNumber] = minNumOfContracts;
            else 
                result[strategyNumber] = Math.Max((int)(Math.Round((profitPerStrategy[strategyNumber] / maxProfit) * maxNumOfContracts)), minNumOfContracts);
        }

        return result;
    }
    
    private int[] GetAllocationBySortino(int numberOfStrategies, DataHolder dataHolder, int minNumOfContracts, int maxNumOfContracts)
    {
        var result = new int[numberOfStrategies];
        var pnlPerStrategy = new double[numberOfStrategies, dataHolder.InitialData.Count];
        //calculate contracts allocations
        double maxSortino = double.MinValue;        
        var sortinoPerStrategy = new double[numberOfStrategies];
        for (int strategyNumber = 0; strategyNumber < numberOfStrategies; strategyNumber++)
        {
            for (int dateRowNumber = 0; dateRowNumber < dataHolder.InitialData.Count; dateRowNumber++)
            {
                pnlPerStrategy[strategyNumber, dateRowNumber] = dataHolder.InitialData[dateRowNumber].DailyAccumulatedPnlByStrategy[strategyNumber];
            }
            //calculate sharpe
            sortinoPerStrategy[strategyNumber] = Sortino.CalculateSortinoRatio(Utilities.CustomArray<double>.GetRow(pnlPerStrategy, strategyNumber));
            if (sortinoPerStrategy[strategyNumber] > maxSortino) maxSortino = sortinoPerStrategy[strategyNumber];
        }
        

        for (int strategyNumber = 0; strategyNumber < numberOfStrategies; strategyNumber++)
        {
            if (sortinoPerStrategy[strategyNumber] <= 0) result[strategyNumber] = minNumOfContracts;
            else 
                result[strategyNumber] = Math.Max((int)(Math.Round((sortinoPerStrategy[strategyNumber] / maxSortino) * maxNumOfContracts)), minNumOfContracts);
        }

        return result;
    }

    private int[] GetAllocationByProfitByDrawDown(int numberOfStrategies, DataHolder dataHolder, int minNumOfContracts, int maxNumOfContracts)
    {
        throw new NotImplementedException("Profit by Drawdown algorithm is not yet implemented per strategy");
    }
}