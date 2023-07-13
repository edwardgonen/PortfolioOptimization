// See https://aka.ms/new-console-template for more information

using PortfolioOptimization;       

int contractsRangeStart = 1;
int contractsRangeEnd = 30;
string inputFileName = "../../../../data/Strategy_DailyPL.csv";
int inSampleDays = 300;
int outSampleDays = 30;
bool bParallel = true;
OptimizerContracts.GeneticAlgorithmType algorithmType =
    OptimizerContracts.GeneticAlgorithmType.GeneticSharp;

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
            algorithmType = OptimizerContracts.GeneticAlgorithmType.GeneticSharp;
        else if (args[5].ToUpper().StartsWith("R")) 
            algorithmType = OptimizerContracts.GeneticAlgorithmType.Random;
        else if (args[5].ToUpper().StartsWith("G"))
            algorithmType = OptimizerContracts.GeneticAlgorithmType.GradientDescent;
        else
            algorithmType = OptimizerContracts.GeneticAlgorithmType.GeneticSharp;
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
var optimizer = new OptimizerContracts(algorithmType);
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

return 0;

