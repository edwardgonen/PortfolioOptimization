namespace PortfolioOptimization;

public interface GeneticAlgorithmInterface
{
    public void Start();
    public int[] BestChromosome();
    public double BestFitness();
}