// See https://aka.ms/new-console-template for more information

using PortfolioOptimization;       

int contractsRangeStart = 1;
int contractsRangeEnd = 30;
string inputFileName = "../../../../data/Strategy_DailyPL.csv";
int inSampleDays = 300;
int outSampleDays = 30;
bool bParallel = true;
OptimizerContractsToSharpe.GeneticAlgorithmType algorithmType =
    OptimizerContractsToSharpe.GeneticAlgorithmType.GeneticMine;

switch (args.Length)
{
    case 0:
        break;       
    case 1:
        inputFileName = args[0];
        break;
    case 2:
        inputFileName = args[0];
        if (!int.TryParse(args[1], out inSampleDays))
        {
            Logger.Log("Wrong in sample length " + args[1]);
            return -3;
        }
        break;
    case 3:
        inputFileName = args[0];
        if (!int.TryParse(args[1], out inSampleDays))
        {
            Logger.Log("Wrong in sample length " + args[1]);
            return -4;
        }
        if (!int.TryParse(args[2], out outSampleDays))
        {
            Logger.Log("Wrong out sample length " + args[2]);
            return -4;
        }
        break;
    case 4:
        inputFileName = args[0];
        if (!int.TryParse(args[1], out inSampleDays))
        {
            Logger.Log("Wrong in sample length " + args[1]);
            return -4;
        }
        if (!int.TryParse(args[2], out outSampleDays))
        {
            Logger.Log("Wrong out sample length " + args[2]);
            return -4;
        }
        if (!int.TryParse(args[3], out contractsRangeStart))
        {
            Logger.Log("Wrong minimal contracts " + args[3]);
            return -4;
        }        
        break;
    case 5:
        inputFileName = args[0];
        if (!int.TryParse(args[1], out inSampleDays))
        {
            Logger.Log("Wrong in sample length " + args[1]);
            return -4;
        }
        if (!int.TryParse(args[2], out outSampleDays))
        {
            Logger.Log("Wrong out sample length " + args[2]);
            return -4;
        }
        if (!int.TryParse(args[3], out contractsRangeStart))
        {
            Logger.Log("Wrong minimal contracts " + args[3]);
            return -4;
        }      
        if (!int.TryParse(args[4], out contractsRangeEnd))
        {
            Logger.Log("Wrong max contracts " + args[4]);
            return -4;
        }  
        break;
    case 6:
        inputFileName = args[0];
        if (!int.TryParse(args[1], out inSampleDays))
        {
            Logger.Log("Wrong in sample length " + args[1]);
            return -4;
        }
        if (!int.TryParse(args[2], out outSampleDays))
        {
            Logger.Log("Wrong out sample length " + args[2]);
            return -4;
        }
        if (!int.TryParse(args[3], out contractsRangeStart))
        {
            Logger.Log("Wrong minimal contracts " + args[3]);
            return -4;
        }      
        if (!int.TryParse(args[4], out contractsRangeEnd))
        {
            Logger.Log("Wrong max contracts " + args[4]);
            return -4;
        }

        if (args[5].ToUpper().StartsWith("T"))
            algorithmType = OptimizerContractsToSharpe.GeneticAlgorithmType.GeneticSharp;
        else if (args[5].ToUpper().StartsWith("R"))
            algorithmType = OptimizerContractsToSharpe.GeneticAlgorithmType.Random;
        else
            algorithmType = OptimizerContractsToSharpe.GeneticAlgorithmType.GeneticMine;
        break;
    default:
        Logger.Log("Wrong number of parameters. Usage <input data file> <in sample length> <out sample length>");
        return -2;
}

string? dataFolder = Path.GetDirectoryName(inputFileName);
if (dataFolder == null)
{
    Logger.Log("Wrong data folder in name " + inputFileName);
    return -6;
}

Logger.Log("Using " + inputFileName + " InSample length " + inSampleDays + " OutSample length " + outSampleDays);

Logger.Log("Reading input data");
var dataHolder = new DataHolder(inputFileName);
var optimizer = new OptimizerContractsToSharpe(algorithmType);
var strategyList = dataHolder.StrategyList;

Logger.Log("Loaded " + strategyList.Count + " strategies and " + dataHolder.InitialData.Count + " data points");

//do the moving window

//1. get the first point as start + in sample days
DateTime firstAvailableDate = dataHolder.InitialData[0].Date;
DateTime lastAvailableDate = dataHolder.InitialData.Last().Date;
DateTime startDateOfInSample = firstAvailableDate;
DateTime endDateOfInSample = firstAvailableDate.AddDays(inSampleDays);
Logger.Log("First available date is " + firstAvailableDate + ". Last available date is " + lastAvailableDate);
if (endDateOfInSample >= lastAvailableDate)
{
    //no place for out sample - exit
    Logger.Log("!!! End of In Sample " + endDateOfInSample + " is beyond the last available date " + lastAvailableDate + ". Exiting.");
    return -1;
}


var contractsAllocation = new ContractsAllocation(dataFolder);

List<Task> optimizationTasks = new List<Task>();

for (DateTime date = firstAvailableDate; date <= lastAvailableDate; date = date.AddDays(outSampleDays))
{
    startDateOfInSample = startDateOfInSample.AddDays(outSampleDays);
    startDateOfInSample = (lastAvailableDate <= startDateOfInSample ? lastAvailableDate : startDateOfInSample);
    
    endDateOfInSample = startDateOfInSample.AddDays(inSampleDays);
    //always end out sample on Friday
    //find next friday
    var bFridayFound = false;
    for (var i = 0; i < 8; i++)
    {
        var nextDay = endDateOfInSample.AddDays(i);
        if (nextDay.DayOfWeek == DayOfWeek.Friday)
        {
            endDateOfInSample = nextDay;
            bFridayFound = true;
            break;
        }
    }

    if (!bFridayFound)
    {
        throw new MyException("Next Friday was not found");
    }
    endDateOfInSample = (lastAvailableDate <= endDateOfInSample ? lastAvailableDate : endDateOfInSample);

    if ((endDateOfInSample - startDateOfInSample).Days < inSampleDays)
    {
        Logger.Log("Available data for In Sample is less than needed " + inSampleDays + ". Exiting");
        break;
    }
    
    Logger.Log("Working on in sample from " + startDateOfInSample.ToShortDateString() + " to " + endDateOfInSample.ToShortDateString());
    DataHolder currentData = dataHolder.GetRangeOfData(startDateOfInSample, endDateOfInSample);
    
    
    var tokenSource = new CancellationTokenSource();
    var token = tokenSource.Token;

    if (!bParallel)
    {
        optimizer.OptimizeAndUpdate(currentData, strategyList, contractsAllocation, contractsRangeStart,
            contractsRangeEnd);
    }
    else
    {
        optimizationTasks.Add(Task.Factory.StartNew(() =>
            {
                optimizer.OptimizeAndUpdate(currentData, strategyList, contractsAllocation, contractsRangeStart,
                    contractsRangeEnd);
            }
            , token));
    }
}
await Task.WhenAll(optimizationTasks);
//sort allocations by date
contractsAllocation.SortByDate();
//make the total result
var accumulatedProfit = Profit.CalculateAccumulatedProfit(dataHolder, contractsAllocation, strategyList);

await using StreamWriter outputFile = new StreamWriter(dataFolder + Path.DirectorySeparatorChar + "AccumulatedProfit.csv");
foreach (var row in accumulatedProfit)
{
    outputFile.WriteLine(row.Date.ToShortDateString() + "," + row.ProfitToday + "," + row.ProfitToDate);
}

//save to allocation file
contractsAllocation.SaveToFile();

/*
Logger.Log("Optimize using my Genetic");
int populationSize = 100;
int chromosomeLength = strategyList.Count;
int maxGenerations = 100;

MyGeneticAlgorithm myGeneticAlgorithm = new MyGeneticAlgorithm(populationSize, chromosomeLength, maxGenerations, 
    rangeStart, 
    rangeEnd,
    dataHolder
    );
int[] solution = myGeneticAlgorithm.Optimize();
Logger.Log("Final Solution:");
Logger.Log(string.Join(", ", solution));
Logger.Log("Best sharpe is " + myGeneticAlgorithm.BestFitness);
*/

/*
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

Logger.Log("Saving permutations in a file started at " + DateTime.Now);
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
}	

Logger.Log("Saving permutations in a file completed at " + DateTime.Now);
*/
return 0;

