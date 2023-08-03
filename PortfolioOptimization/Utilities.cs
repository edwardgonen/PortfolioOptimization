namespace PortfolioOptimization;

public static class Logger
{
    public static void Log(string message)
    {
        Console.Out.WriteLine(message);
    }
}

public static class Utilities
{
    public static bool GetNextDayOfWeek(DateTime currentDate, DateTime lastAvailableDate, DayOfWeek soughtDay,
        out DateTime foundDate)
    {
        bool result = false;
        foundDate = currentDate;

        for (var i = 0; i < 8; i++)
        {
            if (foundDate > lastAvailableDate)
            {
                foundDate = lastAvailableDate; //didn't find the day, so use the last available date anyway
                return result; //we are past the end of array
            }

            if (foundDate.DayOfWeek == soughtDay)
            {
                return true;
            }
            foundDate = foundDate.AddDays(1);
        }

        foundDate = currentDate; //should not happen
        throw new MyException("Next " + soughtDay + " was not found");
    }
    
    public static bool GetPreviousDayOfWeek(DateTime currentDate, DateTime firstAvailableDate, DayOfWeek soughtDay,
        out DateTime foundDate)
    {
        bool result = false;
        foundDate = currentDate;

        for (var i = 0; i < 8; i++)
        {
            if (foundDate < firstAvailableDate)
            {
                foundDate = firstAvailableDate; //didn't find the day, so use the last available date anyway
                return result; //we are past the end of array
            }

            if (foundDate.DayOfWeek == soughtDay)
            {
                return true;
            }
            foundDate = foundDate.AddDays(-1);
        }

        foundDate = currentDate; //should not happen
        throw new MyException("Previous " + soughtDay + " was not found");
    }

    public static double CalculateStandardDeviation(double[] returns)
    {
        double average = CalculateAverage(returns);
        double sumSquaredDiff = 0;
        foreach (var ret in returns)
        {
            double diff = ret - average;
            sumSquaredDiff += diff * diff;
        }
        double variance = sumSquaredDiff / returns.Length;
        return Math.Sqrt(variance);
    }

    public static double CalculateAverage(double[] returns)
    {
        double sum = 0;
        foreach (var ret in returns)
        {
            sum += ret;
        }
        return sum / returns.Length;
    }


    public static double[] CalculateEma(double[] data, int period)
    {
        double smoothingConstant = 2.0 / (period + 1);
        double[] emaValues = new double[data.Length];
        emaValues[0] = data[0];

        for (int i = 1; i < data.Length; i++)
        {
            emaValues[i] = (data[i] - emaValues[i - 1]) * smoothingConstant + emaValues[i - 1];
        }

        return emaValues;
    }
    

    public static double[] CalculateDailyPnls(int[] array, DataHolder initialDataHolder)
    {
        double[] totalDailyPnLs = new double[initialDataHolder.InitialData.Count];
        
        for (var j = 0; j < initialDataHolder.InitialData.Count; j++)
        {
            totalDailyPnLs[j] = 0;
            for (var i = 0; i < array.Length; i++)
            {
                totalDailyPnLs[j] += initialDataHolder.InitialData[j].DailyAccumulatedPnlByStrategy[i] * array[i];
            }
        }

        return totalDailyPnLs;
    }
}

public abstract class Profit
{
    public static double CalculateProfitForOnePermutation(int[] array, DataHolder initialDataHolder)
    {
        double[] totalDailyPnLs = new double[initialDataHolder.InitialData.Count];
        
        double totalProfit = 0;
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

        double accumulatedProfit = 0;
        foreach (var row in initialData.InitialData)
        {
            double dailyProfit = 0;
            for (var i = 0; i < row.DailyAccumulatedPnlByStrategy.Length; i++)
            {
                double allocationForStrategyToday = contractsAllocation.GetAllocation(row.Date, strategiesNames[i]);
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
        public double ProfitToday;
        public double ProfitToDate;
    }
}

public abstract class Linearity
{
    public static double CalculateLinearityForOnePermutation(int[] array, DataHolder initialDataHolder)
    {
        double[] totalDailyPnLs = Utilities.CalculateDailyPnls(array, initialDataHolder);
        double tmp = Utilities.CalculateStandardDeviation(totalDailyPnLs);
        
        if (tmp == 0) return double.PositiveInfinity;
        return 1 / tmp;
    }
}
public abstract class Sharpe
{
    
    private static readonly double RiskFreeRate = 15.8745078664;

    public static double CalculateSharpeForOnePermutation(int[] array, DataHolder initialDataHolder)
    {
        double[] totalDailyPnLs = Utilities.CalculateDailyPnls(array, initialDataHolder);
        var sharpe = CalculateSharpeRatio(totalDailyPnLs);

        //let's multiply by array all strategies PnLs
        return sharpe;
    }

    private static double CalculateSharpeRatio(double[] returns)
    {
        double averageReturn = Utilities.CalculateAverage(returns);
        double standardDeviation = Utilities.CalculateStandardDeviation(returns);

        if (standardDeviation == 0) standardDeviation = 1;
        return averageReturn / standardDeviation * RiskFreeRate;
        
    }

}

public abstract class Sortino
{
    public static double CalculateSortinoForOnePermutation(int[] array, DataHolder initialDataHolder)
    {
        double[] totalDailyPnLs = Utilities.CalculateDailyPnls(array, initialDataHolder);
        var sortino = CalculateSortinoRatio(totalDailyPnLs);
        
        return sortino;
    }
    private static double CalculateSortinoRatio(double[] returns)
    {
        if (returns == null || returns.Length == 0)
        {
            throw new ArgumentException("The 'returns' array must not be null or empty.");
        }

        double meanReturn = 0;
        double sumSquaredNegativeReturns = 0;
        int negativeReturnsCount = 0;

        // Calculate the mean return and count negative returns
        foreach (double ret in returns)
        {
            meanReturn += ret;
            if (ret < 0)
            {
                sumSquaredNegativeReturns += ret * ret;
                negativeReturnsCount++;
            }
        }

        meanReturn /= returns.Length;

        if (negativeReturnsCount == 0)
        {
            // Return a large value for the Sortino ratio when there are no negative returns
            // to indicate a higher risk-adjusted performance.
            return double.MaxValue;
        }

        // Calculate the downside risk (standard deviation of negative returns)
        double downsideRisk = Math.Sqrt(sumSquaredNegativeReturns / negativeReturnsCount);

        // Calculate the Sortino ratio
        double sortinoRatio = (meanReturn - 0) / downsideRisk; // Assuming the risk-free rate is 0, change this value if needed.

        return sortinoRatio;
    }
}

public abstract class SharpeOnEma
{
    private static readonly double RiskFreeRate = 15.8745078664;
    private static readonly int EmaPeriod = 50;

    public static double CalculateSharpeOnEmaForOnePermutation(int[] array, DataHolder initialDataHolder)
    {
        double[] totalDailyPnLs = Utilities.CalculateDailyPnls(array, initialDataHolder);
        totalDailyPnLs = Utilities.CalculateEma(totalDailyPnLs, Math.Min(EmaPeriod, totalDailyPnLs.Length));
        var sharpe = CalculateSharpeOnEmaRatio(totalDailyPnLs);

        //let's multiply by array all strategies PnLs
        return sharpe;
    }

    private static double CalculateSharpeOnEmaRatio(double[] returns)
    {
        double averageReturn = Utilities.CalculateAverage(returns);
        double standardDeviation = Utilities.CalculateStandardDeviation(returns);

        if (standardDeviation == 0) standardDeviation = 1;
        return averageReturn / standardDeviation * RiskFreeRate;
        
    }
}
public abstract class DrawDown
{
    public static double CalculateMaxDrawdownForOnePermutation(int[] array, DataHolder initialDataHolder)
    {
        double[] totalDailyPnLs = Utilities.CalculateDailyPnls(array, initialDataHolder);
        
        double[] dailyDrawdown = new double[initialDataHolder.InitialData.Count];
        double maxDrawdown = double.MaxValue;
        for (var i = 0; i < totalDailyPnLs.Length; i++)
        {
            if (i == 0) dailyDrawdown[i] = 0;
            else dailyDrawdown[i] = Math.Min(0, totalDailyPnLs[i] + dailyDrawdown[i - 1]);
            if (dailyDrawdown[i] < maxDrawdown) maxDrawdown = dailyDrawdown[i];
        }

        if (maxDrawdown == 0) maxDrawdown = 1;
        return Math.Abs(maxDrawdown);
    }
}