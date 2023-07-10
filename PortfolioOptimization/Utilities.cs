namespace PortfolioOptimization;

public static class Logger
{
    public static void Log(string message)
    {
        Console.Out.WriteLine(message);
    }
}


public abstract class Profit
{
    public static decimal CalculateProfitForOnePermutation(int[] array, DataHolder initialDataHolder)
    {
        decimal[] totalDailyPnLs = new decimal[initialDataHolder.InitialData.Count];
        
        decimal totalProfit = 0;
        for (var j = 0; j < initialDataHolder.InitialData.Count; j++)
        {
            totalDailyPnLs[j] = 0;
            for (var i = 0; i < array.Length; i++)
            {
                totalDailyPnLs[j] += initialDataHolder.InitialData[j].DailyAccumulatedPnlByStrategy[i] * array[i];
                totalProfit += totalDailyPnLs[j];
            }
        }
        
        return totalProfit;
    }

    public static List<AccumulatedProfit> CalculateAccumulatedProfit(DataHolder initialData,
        ContractsAllocation contractsAllocation, List<string> strategiesNames)
    {
        var result = new List<AccumulatedProfit>();

        decimal accumulatedProfit = 0;
        foreach (var row in initialData.InitialData)
        {
            decimal dailyProfit = 0;
            for (var i = 0; i < row.DailyAccumulatedPnlByStrategy.Length; i++)
            {
                decimal allocationForStrategyToday = contractsAllocation.GetAllocation(row.Date, strategiesNames[i]);
                dailyProfit += row.DailyAccumulatedPnlByStrategy[i] * allocationForStrategyToday;
            }

            accumulatedProfit += dailyProfit;
            result.Add(new AccumulatedProfit(){Date = row.Date, ProfitToday = dailyProfit, ProfitToDate = accumulatedProfit});
        }

        return result;
    }

    public class AccumulatedProfit
    {
        public DateTime Date;
        public decimal ProfitToday;
        public decimal ProfitToDate;
    }
}

public abstract class Linearity
{
    public static decimal CalculateLinearityForOnePermutation(int[] array, DataHolder initialDataHolder)
    {
        decimal[] totalDailyPnLs = new decimal[initialDataHolder.InitialData.Count];
        
        for (var j = 0; j < initialDataHolder.InitialData.Count; j++)
        {
            totalDailyPnLs[j] = 0;
            for (var i = 0; i < array.Length; i++)
            {
                totalDailyPnLs[j] += initialDataHolder.InitialData[j].DailyAccumulatedPnlByStrategy[i] * array[i];
            }
        }

        decimal tmp =Linearity.CalculateStandardDeviation(totalDailyPnLs);
        if (tmp == 0) tmp = 0.0000000000000000001m;
        var linearity = 1/tmp;

        //let's multiply by array all strategies PnLs
        return linearity;
    }
    private static decimal CalculateAverage(decimal[] returns)
    {
        decimal sum = 0m;
        foreach (var ret in returns)
        {
            sum += ret;
        }
        return sum / returns.Length;
    }

    private static decimal CalculateStandardDeviation(decimal[] returns)
    {
        decimal average = CalculateAverage(returns);
        decimal sumSquaredDiff = 0m;
        foreach (var ret in returns)
        {
            decimal diff = ret - average;
            sumSquaredDiff += diff * diff;
        }
        decimal variance = sumSquaredDiff / returns.Length;
        return (decimal)Math.Sqrt((double)variance);
    }
}
public abstract class Sharpe
{
    
    private static readonly decimal RiskFreeRate = new(15.8745078664);

    public static decimal CalculateSharpeForOnePermutation(int[] array, DataHolder initialDataHolder)
    {
        decimal[] totalDailyPnLs = new decimal[initialDataHolder.InitialData.Count];
        
        for (var j = 0; j < initialDataHolder.InitialData.Count; j++)
        {
            totalDailyPnLs[j] = 0;
            for (var i = 0; i < array.Length; i++)
            {
                totalDailyPnLs[j] += initialDataHolder.InitialData[j].DailyAccumulatedPnlByStrategy[i] * array[i];
            }
        }

        var sharpe = Sharpe.CalculateSharpeRatio(totalDailyPnLs);

        //let's multiply by array all strategies PnLs
        return sharpe;
    }

    private static decimal CalculateSharpeRatio(decimal[] returns)
    {
        decimal averageReturn = CalculateAverage(returns);
        decimal standardDeviation = CalculateStandardDeviation(returns);

        if (standardDeviation == 0) return 0;
        return averageReturn / standardDeviation * RiskFreeRate;
        
    }

    private static decimal CalculateAverage(decimal[] returns)
    {
        decimal sum = 0m;
        foreach (var ret in returns)
        {
            sum += ret;
        }
        return sum / returns.Length;
    }

    private static decimal CalculateStandardDeviation(decimal[] returns)
    {
        decimal average = CalculateAverage(returns);
        decimal sumSquaredDiff = 0m;
        foreach (var ret in returns)
        {
            decimal diff = ret - average;
            sumSquaredDiff += diff * diff;
        }
        decimal variance = sumSquaredDiff / returns.Length;
        return (decimal)Math.Sqrt((double)variance);
    }
}
