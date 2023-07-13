namespace PortfolioOptimization;

public class RandomAlgorithm : IOptimizationAlgorithm
{
    private readonly Random _random;
    private int _minValue;
    private int _maxValue;
    private int _numberOfStrategies;

    public RandomAlgorithm(int numberOfStrategies, DataHolder dataHolder, int minValue, int maxValue)
    {
        _random = new Random();
        _numberOfStrategies = numberOfStrategies;
        _minValue = minValue;
        _maxValue = maxValue;
    }
    public void Start()
    {
    }

    public int[] BestChromosome()
    {
        int[] bestChromosome = new int [_numberOfStrategies];
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