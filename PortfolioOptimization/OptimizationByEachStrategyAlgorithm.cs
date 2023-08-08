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
                _result = GetAllocationByMaxProfit(numberOfStrategies, dataHolder, minValue, maxValue);
            } break;
            case OptimizerContracts.FitnessAlgorithm.ProfitByDrawdown:
            {
                _result = GetAllocationByProfitByDrawDown(numberOfStrategies, dataHolder, minValue, maxValue);
            } break;
            case OptimizerContracts.FitnessAlgorithm.Linearity:
            {
                _result = GetAllocationByLinearity(numberOfStrategies, dataHolder, minValue, maxValue);
            } break;
            case OptimizerContracts.FitnessAlgorithm.Sortino:
            {
                _result = GetAllocationBySortino(numberOfStrategies, dataHolder, minValue, maxValue);
            } break;
            case OptimizerContracts.FitnessAlgorithm.ConstNumberOfContracts:
            {
                _result = GetAllocationConstantNumber(numberOfStrategies, 1, maxValue);
            } break;
            case OptimizerContracts.FitnessAlgorithm.Sharpe:
            default:
            {
                //By Sharpe
                _result = GetAllocationBySharpe(numberOfStrategies, dataHolder, minValue, maxValue);
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

        double maxRSquared = double.MinValue;
        var rSquaredPerStrategy = new double[numberOfStrategies];
        for (int strategyNumber = 0; strategyNumber < numberOfStrategies; strategyNumber++)
        {
            for (int dateRowNumber = 0; dateRowNumber < dataHolder.InitialData.Count; dateRowNumber++)
            {
                pnlPerStrategy[strategyNumber, dateRowNumber] = dataHolder.InitialData[dateRowNumber].DailyAccumulatedPnlByStrategy[strategyNumber];
            }
            //calculate rSquared

            var yDoubles = Utilities.CustomArray<double>.GetRow(pnlPerStrategy, strategyNumber);            
            var xDoubles = new double[yDoubles.Length];
            for (int i = 0; i < yDoubles.Length; i++) xDoubles[i] = i;
            LinearInterpolation.CalculateLinearInterpolation(xDoubles, yDoubles, out double k, out double b);
            rSquaredPerStrategy[strategyNumber] = LinearInterpolation.CalculateRSquared(xDoubles, Utilities.CustomArray<double>.GetRow(pnlPerStrategy, strategyNumber), k, b);
            if (rSquaredPerStrategy[strategyNumber] > maxRSquared) maxRSquared = rSquaredPerStrategy[strategyNumber];
        }
        
        for (int strategyNumber = 0; strategyNumber < numberOfStrategies; strategyNumber++)
        {
            if (dataHolder.InitialData.Last().DailyAccumulatedPnlByStrategy[strategyNumber] <=
                dataHolder.InitialData.First().DailyAccumulatedPnlByStrategy[strategyNumber] || maxRSquared == 0)
            {
                result[strategyNumber] = minNumOfContracts;
            }
            else
            {
                result[strategyNumber] = Math.Max((int)(Math.Round((rSquaredPerStrategy[strategyNumber] / maxRSquared) * maxNumOfContracts)), minNumOfContracts);
            }
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

    private int[] GetAllocationConstantNumber(int numberOfStrategies, int minNumOfContracts,
        int maxNumOfContracts)
    {
        var result = new int[numberOfStrategies];
        for (int strategyNumber = 0; strategyNumber < numberOfStrategies; strategyNumber++)
        {
            result[strategyNumber] = maxNumOfContracts;
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
