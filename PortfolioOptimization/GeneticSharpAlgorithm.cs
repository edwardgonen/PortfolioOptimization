using GeneticSharp;

namespace PortfolioOptimization;

public class GeneticSharpAlgorithm
{
    private readonly GeneticAlgorithm _ga;
    public GeneticSharpAlgorithm(int numberOfStrategies, DataHolder dataHolder, int minValue, int maxValue)
    {
        var targetArray = new int[numberOfStrategies];
        var fitness = new ArrayFitness(dataHolder, targetArray);
        //var chromosome = new IntegerChromosome(minValue, maxValue);
        
        //for floating point chromosome
        double[] minimums = new double[numberOfStrategies];
        double[] maximums = new double[numberOfStrategies];
        int[] digits = new int[numberOfStrategies];
        int[] fractions = new int[numberOfStrategies];

        for (var i = 0; i < numberOfStrategies; i++)
        {
            minimums[i] = minValue;
            maximums[i] = maxValue;
            digits[i] = 5;
            fractions[i] = 0;
        }
        
        var chromosome = new FloatingPointChromosome(
            minimums,
            maximums,
            digits,
            fractions);
        
        //var chromosome = new FloatingPointChromosome(mins, maxValue, 1); 
        
        var population = new Population(50, 100, chromosome);
        var selection = new EliteSelection();
        var crossover = new UniformCrossover();
        var mutation = new UniformMutation(true);

        _ga = new GeneticAlgorithm(population, fitness, selection, crossover, mutation)
        {
            Termination = new GenerationNumberTermination(100)
        };
    }

    public void Start()
    {
        _ga.Start();
    }

    public int[] BestChromosome()
    {
        var bestChromosome = _ga.BestChromosome as FloatingPointChromosome;
        if (bestChromosome == null) return new int[1];
        var doubleValues = bestChromosome.ToFloatingPoints();
        var result = new int[doubleValues.Length];
        
        for (var i = 0; i < doubleValues.Length; i++)
        {
            result[i] = (int)doubleValues[i];
        }
        return result;
    }

    public double BestFitness()
    {
        if (_ga.BestChromosome.Fitness != null) return _ga.BestChromosome.Fitness.Value;
        return -1;
    }
}

public class ArrayFitness : IFitness
{
    private readonly DataHolder _initialDataHolder;
    private readonly int[] _targetArray; 

    public ArrayFitness(DataHolder initialDataHolder, int[] targetArray)
    {
        _initialDataHolder = initialDataHolder;
        _targetArray = targetArray;
    }
    public double Evaluate(IChromosome chromosome)
    {
        var currentChromosome = chromosome as FloatingPointChromosome;
        if (currentChromosome == null) return 0;
        var doubleValues = currentChromosome.ToFloatingPoints();
        for (var i = 0; i < doubleValues.Length; i++)
        {
            _targetArray[i] = (int)doubleValues[i];
        }
        //double evaluationValue = Sharpe.CalculateSharpeForOnePermutation(_targetArray, _initialDataHolder);
        //double evaluationValue = Linearity.CalculateLinearityForOnePermutation(_targetArray, _initialDataHolder);
        double evaluationValue = Profit.CalculateProfitForOnePermutation(_targetArray, _initialDataHolder);
        evaluationValue = evaluationValue /
                          DrawDown.CalculateMaxDrawdownForOnePermutation(_targetArray, _initialDataHolder);
        //double evaluationValue = Linearity.CalculateLinearityForOnePermutation(_targetArray, _initialDataHolder) * Profit.CalculateProfitForOnePermutation(_targetArray, _initialDataHolder);
        
        return evaluationValue;
    }
}