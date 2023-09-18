namespace PortfolioOptimization;

public class RandomAlgorithm : IOptimizationAlgorithm
{
    private readonly Random _random;
    private readonly int _minValue;
    private readonly int _maxValue;
    private readonly int _numberOfStrategies;

    public RandomAlgorithm(int numberOfStrategies, int minValue, int maxValue)
    {
        _random = new Random();
        _numberOfStrategies = numberOfStrategies;
        _minValue = minValue;
        _maxValue = maxValue;
    }
    public void Start()
    {
    }

    public double[] BestChromosome()
    {
        double[] bestChromosome = new double [_numberOfStrategies];
        for (var i = 0; i < bestChromosome.Length; i++)
        {
            bestChromosome[i] = _random.Next(_minValue, _maxValue);
        }

        return bestChromosome;
    }

    public double BestFitness()
    {
        return 0;
    }
}