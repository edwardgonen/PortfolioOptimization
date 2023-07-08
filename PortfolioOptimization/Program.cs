﻿// See https://aka.ms/new-console-template for more information

using System.Runtime.InteropServices.JavaScript;
using PortfolioOptimization;

int contractsRangeStart = 0;
int contractsRangeEnd = 30;
string inputFileName = "../../../../data/Strategy_DailyPL.csv";
int inSampleDays = 100;
int outSampleDays = 30;

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
    default:
        Logger.Log("Wrong number of parameters. Usage <input data file> <in sample length> <out sample length>");
        return -2;
}

Logger.Log("Using " + inputFileName + " InSample length " + inSampleDays + " OutSample length " + outSampleDays);

Logger.Log("Reading input data");
var dataHolder = new DataHolder(inputFileName);
//var optimizer = new OptimizerContractsToSharpe(dataHolder);
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

for (DateTime date = firstAvailableDate; date <= lastAvailableDate; date = date.AddDays(outSampleDays))
{
    startDateOfInSample = startDateOfInSample.AddDays(inSampleDays);
    startDateOfInSample = (lastAvailableDate <= startDateOfInSample ? lastAvailableDate : startDateOfInSample);
    
    endDateOfInSample = startDateOfInSample.AddDays(inSampleDays);
    endDateOfInSample = (lastAvailableDate <= endDateOfInSample ? lastAvailableDate : endDateOfInSample);

    if ((endDateOfInSample - startDateOfInSample).Days < inSampleDays)
    {
        Logger.Log("Available data for In Sample is less than needed " + inSampleDays + ". Exiting");
        break;
    }
    
    Logger.Log("Working on in sample from " + startDateOfInSample.ToShortDateString() + " to " + endDateOfInSample.ToShortDateString());
    DataHolder currentData = dataHolder.GetRangeOfData(startDateOfInSample, endDateOfInSample);
    
    //optimize
    GeneticSharpAlgorithm gsa = new GeneticSharpAlgorithm(strategyList.Count, currentData, contractsRangeStart, contractsRangeEnd);
    gsa.Start();
    //store results
    var bestChromosome = gsa.BestChromosome();
    var bestFitness = gsa.BestFitness();
    
    Logger.Log("Final Solution for " + startDateOfInSample.ToShortDateString() + " - " + endDateOfInSample.ToShortDateString());
    Logger.Log(string.Join(", ", bestChromosome));
    Logger.Log("Best sharpe is " + bestFitness);
    
}

//save to allocation file
int i = 0;

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

