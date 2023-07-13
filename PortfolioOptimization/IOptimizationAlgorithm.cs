namespace PortfolioOptimization;

public interface IOptimizationAlgorithm
{
    public void Start();
    public int[] BestChromosome();
    public double BestFitness();

    public double Sss()
    {
        return 5;
    }
}