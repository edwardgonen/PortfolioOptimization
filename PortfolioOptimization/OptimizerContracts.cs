namespace PortfolioOptimization;

public class OptimizerContracts
{
    private readonly GeneticAlgorithmType _algorithmType;
    private readonly FitnessAlgorithm _fitnessAlgorithm;
    public OptimizerContracts(GeneticAlgorithmType algorithmType, FitnessAlgorithm fitnessAlgorithm)
    {
        _fitnessAlgorithm = fitnessAlgorithm;
        _algorithmType = algorithmType;
    }

    public void OptimizeAndUpdate(DataHolder currentData, List<string> strategyList, ContractsAllocation contractsAllocation, int contractsRangeStart, int contractsRangeEnd)
    {
        //optimize

        IOptimizationAlgorithm gsa;

        switch (_algorithmType)
        {
            case GeneticAlgorithmType.GeneticSharp:
                gsa = new OptimizationSharpAlgorithm(strategyList.Count, currentData, contractsRangeStart, contractsRangeEnd, _fitnessAlgorithm);
                break;
            case GeneticAlgorithmType.Random:
                gsa = new RandomAlgorithm(strategyList.Count, contractsRangeStart, contractsRangeEnd);
                break;
            case GeneticAlgorithmType.GradientDescent:
                gsa = new MyGradientDescentOptimizationAlgorithm(strategyList.Count, currentData, contractsRangeStart, contractsRangeEnd);
                break;
            default:
                throw new MyException("Algorithm not selected");
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
        Random,
        GradientDescent
    }

    public enum FitnessAlgorithm
    {
        Sharpe,
        Linearity,
        ProfitByDrawdown
    }
}
