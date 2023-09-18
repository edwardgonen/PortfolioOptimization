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

    public double CalculateSelectedFitnessOutSample(DataHolder currentDataOutSample, ContractsAllocation contractsAllocation)
    {
        double evaluationValue;
        //calculate linearity on out sample
        double[] allocationArray = new double[currentDataOutSample.StrategyList.Count];
        for (int i = 0; i < currentDataOutSample.StrategyList.Count; i++)
        {
            allocationArray[i] = contractsAllocation.GetLastAllocation(currentDataOutSample.StrategyList[i]);
        }

        switch (_fitnessAlgorithm)
        {
            case OptimizerContracts.FitnessAlgorithm.Linearity:
                evaluationValue = Linearity.CalculateLinearityForOnePermutation(allocationArray, currentDataOutSample);
                break;            
            case OptimizerContracts.FitnessAlgorithm.RSquared:
                evaluationValue = LinearInterpolation.CalculateRSquaredForOnePermutation(allocationArray, currentDataOutSample);
                break;
            case OptimizerContracts.FitnessAlgorithm.ProfitByDrawdown:
                evaluationValue = Profit.CalculateProfitForOnePermutation(allocationArray, currentDataOutSample);
                evaluationValue /= DrawDown.CalculateMaxDrawdownForOnePermutation(allocationArray, currentDataOutSample);
                break;
            case OptimizerContracts.FitnessAlgorithm.SharpeOnEma:
                evaluationValue = SharpeOnEma.CalculateSharpeOnEmaForOnePermutation(allocationArray, currentDataOutSample);
                break;
            case OptimizerContracts.FitnessAlgorithm.Sortino:
                evaluationValue = Sortino.CalculateSortinoForOnePermutation(allocationArray, currentDataOutSample);
                break;
            case OptimizerContracts.FitnessAlgorithm.MaxProfit:
                evaluationValue = MaxProfit.CalculateAccumulatedProfit(allocationArray, currentDataOutSample);
                break;
            case OptimizerContracts.FitnessAlgorithm.Correlation:
                evaluationValue = Sharpe.CalculateSharpeForOnePermutation(allocationArray, currentDataOutSample);
                break;
            case OptimizerContracts.FitnessAlgorithm.Sharpe:
            default:
                evaluationValue = Sharpe.CalculateSharpeForOnePermutation(allocationArray, currentDataOutSample);
                break;
                
        }
        return evaluationValue;
    }
    public void OptimizeAndUpdate(DataHolder currentDataInSample, ContractsAllocation contractsAllocation, int contractsRangeStart, int contractsRangeEnd)
    {
        //optimize

        IOptimizationAlgorithm gsa;

        switch (_algorithmType)
        {
            case GeneticAlgorithmType.GeneticSharp:
                gsa = new OptimizationSharpAlgorithm(currentDataInSample.StrategyList.Count, currentDataInSample, contractsRangeStart, contractsRangeEnd, _fitnessAlgorithm);
                break;
            case GeneticAlgorithmType.Random:
                gsa = new RandomAlgorithm(currentDataInSample.StrategyList.Count, contractsRangeStart, contractsRangeEnd);
                break;
            case GeneticAlgorithmType.GradientDescent:
                gsa = new MyGradientDescentOptimizationAlgorithm(currentDataInSample.StrategyList.Count, currentDataInSample, contractsRangeStart, contractsRangeEnd, _fitnessAlgorithm);
                break;
            case GeneticAlgorithmType.ByStrategy:
                gsa = new OptimizationByEachStrategyAlgorithm(currentDataInSample.StrategyList.Count, currentDataInSample, contractsRangeStart, contractsRangeEnd, _fitnessAlgorithm);
                break;
            default:
                throw new MyException("Algorithm not selected");
        }

        gsa.Start();
        //store results
        var bestChromosome = gsa.BestChromosome();
        var bestFitness = gsa.BestFitness();
        Logger.Log("Final Solution for " + currentDataInSample.InitialData.First().Date.ToShortDateString() + " - " + currentDataInSample.InitialData.Last().Date.ToShortDateString());
        Logger.Log(string.Join(", ", bestChromosome));
        Logger.Log("Best fitness value is " + bestFitness);
        var j = 0;
        foreach (var strategyName in currentDataInSample.StrategyList)
        {
            contractsAllocation.AddAllocation(currentDataInSample.InitialData.Last().Date.AddDays(1), strategyName, bestChromosome[j++]);
        }
    }

    public enum GeneticAlgorithmType
    {
        GeneticSharp,
        Random,
        GradientDescent,
        ByStrategy,
        DynamicProgramming
    }

    public enum FitnessAlgorithm
    {
        Sharpe,
        Linearity,
        ProfitByDrawdown,
        SharpeOnEma,
        Sortino,
        MaxProfit,
        ConstNumberOfContracts,
        Correlation,
        RSquared
    }
}
