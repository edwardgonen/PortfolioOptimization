namespace PortfolioOptimization;

public class OptimizerContractsToSharpe
{
    private readonly DataHolder _initialDataHolder;
    public OptimizerContractsToSharpe(DataHolder dataHolder)
    {
        _initialDataHolder = dataHolder;
    }

    public void OptimizeAndUpdate(DataHolder currentData, List<string> strategyList, ContractsAllocation contractsAllocation, int contractsRangeStart, int contractsRangeEnd)
    {
        //optimize
        GeneticSharpAlgorithm gsa = new GeneticSharpAlgorithm(strategyList.Count, currentData, contractsRangeStart, contractsRangeEnd);
        gsa.Start();
        //store results
        var bestChromosome = gsa.BestChromosome();
        var bestFitness = gsa.BestFitness();
        Logger.Log("Final Solution for " + currentData.InitialData.First().Date.ToShortDateString() + " - " + currentData.InitialData.Last().Date.ToShortDateString());
        Logger.Log(string.Join(", ", bestChromosome));
        Logger.Log("Best fitness value is " + bestFitness);
        var j = 0;
        foreach (var strategyName in strategyList)
        {
            contractsAllocation.AddAllocation(currentData.InitialData.Last().Date.AddDays(1), strategyName, bestChromosome[j++]);
        }
    }

}
