namespace PortfolioOptimization;

public class OptimizerContractsToSharpe
{
    private readonly GeneticAlgorithmType _algorithmType;
    public OptimizerContractsToSharpe(GeneticAlgorithmType algorithmType)
    {
        _algorithmType = algorithmType;
    }

    public void OptimizeAndUpdate(DataHolder currentData, List<string> strategyList, ContractsAllocation contractsAllocation, int contractsRangeStart, int contractsRangeEnd)
    {
        //optimize

        GeneticAlgorithmInterface gsa;

        switch (_algorithmType)
        {
            case GeneticAlgorithmType.GeneticSharp:
                gsa = new GeneticSharpAlgorithm(strategyList.Count, currentData, contractsRangeStart, contractsRangeEnd);
                break;
            case GeneticAlgorithmType.Random:
                gsa = new RandomAlgorithm(strategyList.Count, currentData, contractsRangeStart, contractsRangeEnd);
                break;
            case GeneticAlgorithmType.GeneticMine:
            default:
                gsa = new MyGeneticAlgorithm(strategyList.Count, currentData, contractsRangeStart, contractsRangeEnd);
                break;
        }

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

    public enum GeneticAlgorithmType
    {
        GeneticSharp,
        GeneticMine,
        Random
    }
}
