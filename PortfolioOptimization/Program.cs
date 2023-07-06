// See https://aka.ms/new-console-template for more information

using PortfolioOptimization;

int rangeStart = 1;
int rangeEnd = 30;
int step = 14;
DateTime startDate = new DateTime(2019, 1, 1);
DateTime endDate = new DateTime(2019, 12, 31);

Logger.Log("Reading input data");
var dataHolder = new DataHolder("../../../../data/Strategy_DailyPL1.csv", startDate, endDate);
var optimizer = new OptimizerContractsToSharpe(dataHolder);
var strategyList = dataHolder.StrategyList;

Logger.Log("Loaded " + strategyList.Count + " strategies");


Logger.Log("Calculating permutations started at " + DateTime.Now);
var permutations = new Permutations(optimizer, dataHolder);
permutations.GeneratePermutations(new int[strategyList.Count], rangeStart, rangeEnd, step);
Logger.Log("Calculating permutations completed at " + DateTime.Now);

Logger.Log("Best total Sharpe is " + permutations.BestSharpe);
Logger.Log("Best permutation is ");
foreach (var element in permutations.BestPermutation)
{
    Console.Out.Write(element + ",");
}

var permutationsTable = permutations.Result;

/*Logger.Log("Saving permutations in a file started at " + DateTime.Now);
// Create a file to write to.
using (StreamWriter sw = File.CreateText("../../../../data/Permutations.csv"))
{
    foreach (var line in permutations.Result)
    {
        string lineToWrite = String.Empty;
        foreach (var num in line)
        {
            lineToWrite += num + ",";
        }
        sw.WriteLine(lineToWrite);
    }
}	*/

Logger.Log("Saving permutations in a file completed at " + DateTime.Now);


