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

    public static double CalculateExponentialFunctionValue(double input)
    {
        if (input < 0 || input > 1)
        {
            throw new ArgumentException("The input must be in the range of 0 to 1");
        }

        double baseValue = Math.E;// Euler's number (e)
        double exponent = -5 * (1 - input); // Adjust the exponent as needed
        double exponentialValue = Math.Pow(baseValue, exponent);

        return exponentialValue;
    }

    public static double CalculateSigmoid(double x, double slope = 10, double xOffset = 0)
    {
        return 1.0 / (1.0 + Math.Exp(-slope*(x - xOffset)));
    }
    
    public static class CustomArray<T>
    {
        public static T[] GetColumn(T[,] matrix, int columnNumber)
        {
            return Enumerable.Range(0, matrix.GetLength(0))
                .Select(x => matrix[x, columnNumber])
                .ToArray();
        }

        public static T[] GetRow(T[,] matrix, int rowNumber)
        {
            return Enumerable.Range(0, matrix.GetLength(1))
                .Select(x => matrix[rowNumber, x])
                .ToArray();
        }
    }
    
    
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
                totalDailyPnLs[j] += initialDataHolder.InitialData[j].DailyPnlByStrategy[i] * array[i];
            }
        }

        return totalDailyPnLs;
    }
}

public abstract class Correlation
{
    public static double CalculateCorrelation(List<double> x, List<double> y)
    {
        //copy to arrays 
        var xArray = x.ToArray();
        var yArray = y.ToArray();
        return CalculateCorrelation(xArray, yArray);
    }
    static double CalculateCorrelation(double[] x, double[] y)
    {
        // Check if both arrays have the same length.
        if (x.Length != y.Length)
        {
            throw new ArgumentException("Arrays must have the same length.");
        }

        int n = x.Length;

        // Calculate the means of both arrays.
        double meanX = x.Average();
        double meanY = y.Average();

        // Calculate the numerator and denominators of the Pearson correlation formula.
        double numerator = 0;
        double denominatorX = 0;
        double denominatorY = 0;

        for (int i = 0; i < n; i++)
        {
            double deltaX = x[i] - meanX;
            double deltaY = y[i] - meanY;
            numerator += deltaX * deltaY;
            denominatorX += deltaX * deltaX;
            denominatorY += deltaY * deltaY;
        }

        // Calculate the correlation coefficient.
        double correlation = numerator / (Math.Sqrt(denominatorX) * Math.Sqrt(denominatorY));

        return correlation;
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
                totalDailyPnLs[j] += initialDataHolder.InitialData[j].DailyPnlByStrategy[i] * array[i];
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
            for (var i = 0; i < row.DailyPnlByStrategy.Length; i++)
            {
                double allocationForStrategyToday = contractsAllocation.GetAllocation(row.Date, strategiesNames[i]);
                dailyProfit += row.DailyPnlByStrategy[i] * allocationForStrategyToday;
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

public abstract class MaxProfit
{
    public static double CalculateAccumulatedProfit(int[] array, DataHolder initialDataHolder)
    {
        double[] totalDailyPnLs = Utilities.CalculateDailyPnls(array, initialDataHolder);
        return totalDailyPnLs.Sum();
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

    public static double CalculateSharpeRatio(double[] returns)
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
    public static double CalculateSortinoRatio(double[] returns)
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
    public abstract class LinearInterpolation
    {
        public static double CalculateRSquaredForOnePermutation(int[] array, DataHolder initialDataHolder)
        {
            double[] totalDailyPnLs = Utilities.CalculateDailyPnls(array, initialDataHolder);
            double[] accumulatedDailyPnls = new double[totalDailyPnLs.Length];
            for (int i = 0; i < totalDailyPnLs.Length; i++)
            {
                accumulatedDailyPnls[i] += totalDailyPnLs[i];
            }
            
            var yDoubles = accumulatedDailyPnls;            
            var xDoubles = new double[yDoubles.Length];
            for (int i = 0; i < yDoubles.Length; i++) xDoubles[i] = i;
            LinearInterpolation.CalculateLinearInterpolation(xDoubles, yDoubles, out double k, out double b);
            var rSquaredPerStrategy = LinearInterpolation.CalculateRSquared(xDoubles, yDoubles, k, b);
            return rSquaredPerStrategy;
        }
        public static double CalculateRSquared(double[] x, double[] y, double slope, double intercept)
        {
            if (x.Length != y.Length)
            {
                throw new ArgumentException("Input arrays must have the same length.");
            }

            double yMean = y.Average();
            double sumSquaredTotal = 0;
            double sumSquaredResidual = 0;

            for (int i = 0; i < x.Length; i++)
            {
                double predictedY = slope * x[i] + intercept;
                sumSquaredTotal += Math.Pow(y[i] - yMean, 2);
                sumSquaredResidual += Math.Pow(y[i] - predictedY, 2);
            }

            if (Math.Abs(sumSquaredTotal) < double.Epsilon)
            {
                //throw new InvalidOperationException("Cannot calculate R-squared due to division by zero.");
                return 0;
            }

            return 1 - (sumSquaredResidual / sumSquaredTotal);
        }
        public static void CalculateLinearInterpolation(double[] x, double[] y, out double k, out double b)
        {
            if (x == null || y == null || x.Length != y.Length)
            {
                throw new ArgumentException("Input arrays must have the same length.", nameof(x));
            }

            double sumX = 0;
            double sumY = 0;
            double sumXy = 0;
            double sumX2 = 0;

            for (int i = 0; i < x.Length; i++)
            {
                sumX += x[i];
                sumY += y[i];
                sumXy += x[i] * y[i];
                sumX2 += x[i] * x[i];
            }

            double n = x.Length;
            double denominator = n * sumX2 - sumX * sumX;

            if (Math.Abs(denominator) < double.Epsilon)
            {
                throw new InvalidOperationException("Cannot perform linear interpolation due to division by zero.");
            }

            k = (n * sumXy - sumX * sumY) / denominator;
            b = (sumY - k * sumX) / n;
        }
    }