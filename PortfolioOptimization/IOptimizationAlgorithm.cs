namespace PortfolioOptimization;

public interface IOptimizationAlgorithm
{
    public void Start();
    public double[] BestChromosome();
    public double BestFitness();

}