namespace PortfolioOptimization;

public static class Logger
{
    public static void Log(string message)
    {
        Console.Out.WriteLine(message);
    }
}

public class Permutations
{
    public List<int[]> Result { get; } = new();
    private readonly OptimizerContractsToSharpe _optimizerContractsToSharpe;
    public decimal BestSharpe { get; private set; } = Decimal.MinValue;
    private readonly DataHolder _dataHolder;
    public int[] BestPermutation { get; }

    public Permutations(OptimizerContractsToSharpe optimizerContractsToSharpe, DataHolder dataHolder)
    {
        _optimizerContractsToSharpe = optimizerContractsToSharpe;
        _dataHolder = dataHolder;
        BestPermutation = new int[_dataHolder.StrategyList.Count];
    }
    public void GeneratePermutations(int[] array, int rangeStart, int rangeEnd, int step)
    {

        int[] range = new int[(rangeEnd - rangeStart) / step + 1];
        for (int i = 0; i < range.Length; i++)
        {
            range[i] = rangeStart + i * step;
        }

        Parallel.For(0, range.Length, i =>
        { 
            array[0] = range[i];
            GeneratePermutationsHelper(array, 1, range);
        });
        
        // foreach (var t in range)
        // {
        //     array[0] = t;
        //     GeneratePermutationsHelper(array, 1, range);
        // }
    }

    private void GeneratePermutationsHelper(int[] array, int index, int[] range)
    {
        if (index >= array.Length)
        {
            //SaveArray(array);
            var tmpSharpe = _optimizerContractsToSharpe.CalculateSharpeForOnePermutation(array);
            if ( tmpSharpe > BestSharpe)
            {
                BestSharpe = tmpSharpe;
                //copy array
                for (var i = 0; i < array.Length; i++) BestPermutation[i] = array[i];
                Logger.Log("Best Sharpe " + BestSharpe);
            }
            return;
        }

        foreach (var t in range)
        {
            array[index] = t;
            GeneratePermutationsHelper(array, index + 1, range);
        }
    }
}

public static class Sharpe
{

    // static void Main(string[] args)
    // {
    //     decimal[] returns = { 0.05m, 0.08m, 0.1m, 0.06m, 0.07m }; // Modify the returns array as per your requirement
    //     decimal riskFreeRate = 0.02m; // Modify the risk-free rate as per your requirement
    //
    //     decimal sharpeRatio = CalculateSharpeRatio(returns, riskFreeRate);
    //     Console.WriteLine("Sharpe Ratio: " + sharpeRatio);
    // }

    private static readonly decimal RiskFreeRate = new(15.8745078664);
    
    public static decimal CalculateSharpeRatio(decimal[] returns)
    {
        decimal averageReturn = CalculateAverage(returns);
        decimal standardDeviation = CalculateStandardDeviation(returns);
        decimal excessReturn = averageReturn - RiskFreeRate;

        return excessReturn / standardDeviation;
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
