// See https://aka.ms/new-console-template for more information

using PortfolioOptimization;

OptimizerContracts.GeneticAlgorithmType algorithmType =
    OptimizerContracts.GeneticAlgorithmType.GeneticSharp;

OptimizerContracts.FitnessAlgorithm fitnessAlgorithm = OptimizerContracts.FitnessAlgorithm.Sharpe;

DailyPlList allPnls = new DailyPlList();

Parameters parameters = Parameters.Load("Parameters.xml") ?? new Parameters();

switch (args.Length)
{
    case 0:
        break;       
    case 1:
        parameters.DailyPlFileName = args[0];
        break;
    case 2:
        parameters.DailyPlFileName = args[0];
        if (!int.TryParse(args[1], out parameters.InSampleDays))
        {
            Logger.Log("Wrong in sample length " + args[1]);
            return -3;
        }
        break;
    case 3:
        parameters.DailyPlFileName = args[0];
        if (!int.TryParse(args[1], out parameters.InSampleDays))
        {
            Logger.Log("Wrong in sample length " + args[1]);
            return -4;
        }
        if (!int.TryParse(args[2], out parameters.OutSampleDays))
        {
            Logger.Log("Wrong out sample length " + args[2]);
            return -4;
        }
        break;
    case 4:
        parameters.DailyPlFileName = args[0];
        if (!int.TryParse(args[1], out parameters.InSampleDays))
        {
            Logger.Log("Wrong in sample length " + args[1]);
            return -4;
        }
        if (!int.TryParse(args[2], out parameters.OutSampleDays))
        {
            Logger.Log("Wrong out sample length " + args[2]);
            return -4;
        }
        if (!int.TryParse(args[3], out parameters.ContractsRangeStart))
        {
            Logger.Log("Wrong minimal contracts " + args[3]);
            return -4;
        }        
        break;
    case 5:
        parameters.DailyPlFileName = args[0];
        if (!int.TryParse(args[1], out parameters.InSampleDays))
        {
            Logger.Log("Wrong in sample length " + args[1]);
            return -4;
        }
        if (!int.TryParse(args[2], out parameters.OutSampleDays))
        {
            Logger.Log("Wrong out sample length " + args[2]);
            return -4;
        }
        if (!int.TryParse(args[3], out parameters.ContractsRangeStart))
        {
            Logger.Log("Wrong minimal contracts " + args[3]);
            return -4;
        }      
        if (!int.TryParse(args[4], out parameters.ContractsRangeEnd))
        {
            Logger.Log("Wrong max contracts " + args[4]);
            return -4;
        }  
        break;
    case 6:
        parameters.DailyPlFileName = args[0];
        if (!int.TryParse(args[1], out parameters.InSampleDays))
        {
            Logger.Log("Wrong in sample length " + args[1]);
            return -4;
        }
        if (!int.TryParse(args[2], out parameters.OutSampleDays))
        {
            Logger.Log("Wrong out sample length " + args[2]);
            return -4;
        }
        if (!int.TryParse(args[3], out parameters.ContractsRangeStart))
        {
            Logger.Log("Wrong minimal contracts " + args[3]);
            return -4;
        }      
        if (!int.TryParse(args[4], out parameters.ContractsRangeEnd))
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
        else if (args[5].ToUpper().StartsWith("O"))
            algorithmType = OptimizerContracts.GeneticAlgorithmType.ByStrategy;
        else
            algorithmType = OptimizerContracts.GeneticAlgorithmType.GeneticSharp;
        break;
    case 7:
        parameters.DailyPlFileName = args[0];
        if (!int.TryParse(args[1], out parameters.InSampleDays))
        {
            Logger.Log("Wrong in sample length " + args[1]);
            return -4;
        }
        if (!int.TryParse(args[2], out parameters.OutSampleDays))
        {
            Logger.Log("Wrong out sample length " + args[2]);
            return -4;
        }
        if (!int.TryParse(args[3], out parameters.ContractsRangeStart))
        {
            Logger.Log("Wrong minimal contracts " + args[3]);
            return -4;
        }      
        if (!int.TryParse(args[4], out parameters.ContractsRangeEnd))
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
        else if (args[5].ToUpper().StartsWith("O"))
            algorithmType = OptimizerContracts.GeneticAlgorithmType.ByStrategy;
        else
            algorithmType = OptimizerContracts.GeneticAlgorithmType.GeneticSharp;


        if (args[6].ToUpper().StartsWith("PD"))
            fitnessAlgorithm = OptimizerContracts.FitnessAlgorithm.ProfitByDrawdown;
        else if (args[6].ToUpper().StartsWith("LI"))
            fitnessAlgorithm = OptimizerContracts.FitnessAlgorithm.Linearity;
        else if (args[6].ToUpper().StartsWith("SE"))
            fitnessAlgorithm = OptimizerContracts.FitnessAlgorithm.SharpeOnEma;
        else if (args[6].ToUpper().StartsWith("SO"))
            fitnessAlgorithm = OptimizerContracts.FitnessAlgorithm.Sortino;
        else if (args[6].ToUpper().StartsWith("MP"))
            fitnessAlgorithm = OptimizerContracts.FitnessAlgorithm.MaxProfit;
        else if (args[6].ToUpper().StartsWith("CS"))
            fitnessAlgorithm = OptimizerContracts.FitnessAlgorithm.ConstNumberOfContracts;
        else
            fitnessAlgorithm = OptimizerContracts.FitnessAlgorithm.Sharpe;
        break;
    default:
        Logger.Log("Wrong number of parameters. Usage <input data file> <in sample length> <out sample length> <T/G/R> <PD/SH/LI/SE/SO>");
        return -2;
}

//if command line is used - we run in simulation mode
if (args.Length > 0) parameters.BRealTime = false;

if (parameters.BRealTime)
{
    allPnls = DailyPlList.LoadFromTradeCompletionLog(parameters);

    //save to file
    allPnls.SaveToFile(parameters.DailyPlFileName, parameters.StrategyMaxInactivityDays);
}


string? dataFolder = Path.GetDirectoryName(parameters.DailyPlFileName);
if (dataFolder == null)
{
    Logger.Log("Wrong data folder in name " + parameters.DailyPlFileName);
    return -6;
}

Logger.Log("Using " + parameters.DailyPlFileName + " InSample length " + parameters.InSampleDays + " OutSample length " + parameters.OutSampleDays);

Logger.Log("Reading input data");
var dataHolder = new DataHolder(parameters.DailyPlFileName);
var optimizer = new OptimizerContracts(algorithmType, fitnessAlgorithm);
var strategyList = dataHolder.StrategyList;

Logger.Log("Loaded " + strategyList.Count + " strategies and " + dataHolder.InitialData.Count + " data points");

//do the moving window

//1. get the first point as start + in sample days
DateTime firstAvailableDate = dataHolder.InitialData[0].Date;
DateTime lastAvailableDate = dataHolder.InitialData.Last().Date;
DateTime endDateOfInSample = firstAvailableDate.AddDays(parameters.InSampleDays);
Logger.Log("First available date is " + firstAvailableDate + ". Last available date is " + lastAvailableDate);
if (endDateOfInSample >= lastAvailableDate)
{
    //no place for out sample - exit
    Logger.Log("!!! End of In Sample " + endDateOfInSample + " is beyond the last available date " + lastAvailableDate + ". Exiting.");
    return -1;
}


var contractsAllocation = new ContractsAllocation();

List<Task> optimizationTasks = new List<Task>();

//now loop. I'll go from end (latter) to begin (former)
//1. find the start and end of last days of in sample
//1.1 the end is the closest Friday before last available
//do we have one?
if (!Utilities.GetPreviousDayOfWeek(lastAvailableDate, firstAvailableDate, DayOfWeek.Friday, out endDateOfInSample))
{
    throw new MyException("Could not find last Friday before " + lastAvailableDate);
}
//here the end date of in sample is the last Friday
//1.2 calculate first day of insample
//it is prev monday before (end - length of insample)
if (!Utilities.GetNextDayOfWeek(endDateOfInSample.AddDays(-parameters.InSampleDays), lastAvailableDate, DayOfWeek.Sunday, out var startDateOfInSample))
{
    throw new MyException("Could not find next Sunday after " + endDateOfInSample.AddDays(-parameters.InSampleDays));
}

do
{
    Logger.Log("Working on in sample from " + startDateOfInSample.ToShortDateString() + " to " + endDateOfInSample.ToShortDateString());
    DataHolder currentData = dataHolder.GetRangeOfData(startDateOfInSample, endDateOfInSample);

    var tokenSource = new CancellationTokenSource();
    var token = tokenSource.Token;

    if (!parameters.BParallel)
    {
        optimizer.OptimizeAndUpdate(currentData, strategyList, contractsAllocation, parameters.ContractsRangeStart,
            parameters.ContractsRangeEnd);
    }
    else
    {
        var allocation = contractsAllocation;
        optimizationTasks.Add(Task.Factory.StartNew(() =>
            {
                optimizer.OptimizeAndUpdate(currentData, strategyList, allocation, parameters.ContractsRangeStart,
                    parameters.ContractsRangeEnd);
            }
            , token));
    }
    
    //move back to the previous chunk by outSampleLength
    endDateOfInSample = endDateOfInSample.AddDays(-parameters.OutSampleDays);
    startDateOfInSample = endDateOfInSample.AddDays(-parameters.InSampleDays);
    if (startDateOfInSample < firstAvailableDate) startDateOfInSample = firstAvailableDate;
    
    if ((endDateOfInSample - startDateOfInSample).Days < parameters.InSampleDays)
    {
        break;
    }
    //find the closest friday
    if (!Utilities.GetPreviousDayOfWeek(endDateOfInSample, firstAvailableDate, DayOfWeek.Friday, out endDateOfInSample))
    {
        break;
    }
    if (!Utilities.GetNextDayOfWeek(startDateOfInSample, lastAvailableDate, DayOfWeek.Sunday, out startDateOfInSample))
    {
        break;
    }
    

} while (true);


await Task.WhenAll(optimizationTasks);
//sort allocations by date
contractsAllocation.SortByDate();

//save to allocation file
contractsAllocation = contractsAllocation.SaveToFile(parameters.BRealTime, parameters.ContractsAllocationFileName, parameters.MultiplicationFactor, allPnls);
//make the total result
var accumulatedProfit = Profit.CalculateAccumulatedProfit(dataHolder, contractsAllocation, strategyList);

await using StreamWriter outputFile = new StreamWriter(dataFolder + Path.DirectorySeparatorChar + "AccumulatedProfit.csv");
foreach (var row in accumulatedProfit)
{
    outputFile.WriteLine(row.Date.ToShortDateString() + "," + row.ProfitToday + "," + row.ProfitToDate);
}

return 0;

