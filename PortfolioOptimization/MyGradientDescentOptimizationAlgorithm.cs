namespace PortfolioOptimization;

public class MyGradientDescentOptimizationAlgorithm : IOptimizationAlgorithm
{
    private double _bestFitnessValue;
    private double[] _finalSolution;
    
    
    private static readonly Random RandomGenerator = new Random();
    private readonly int _solutionArrayLength;

    private const int MaxIterations = 5000;
    private const double LearningRate = 0.1;
    
    private readonly int _minValue;
    private readonly int _maxValue;
    private readonly DataHolder _initialDataHolder;
    private readonly OptimizerContracts.FitnessAlgorithm _fitnessAlgorithm;

    public MyGradientDescentOptimizationAlgorithm(int numberOfStrategies, DataHolder dataHolder, int minValue,
        int maxValue, OptimizerContracts.FitnessAlgorithm fitnessAlgorithm)
    {
        _solutionArrayLength = numberOfStrategies;
        _minValue = minValue;
        _maxValue = maxValue;
        _initialDataHolder = dataHolder;
        _finalSolution = new double[_solutionArrayLength];
        _fitnessAlgorithm = fitnessAlgorithm;
    }
    public void Start()
    {
        double[] array = InitializeArray();

        for (int iteration = 0; iteration < MaxIterations; iteration++)
        {
            int[] gradient = ComputeGradient(array, CalculateFitness);
            UpdateArray(array, gradient);
        }

        _finalSolution = array;
    }

    public double[] BestChromosome()
    {
        return _finalSolution;
    }

    public double BestFitness()
    {
        return 1/_bestFitnessValue;
    }
    
    private double[] InitializeArray()
    {
        double[] array = new double[_solutionArrayLength];

        for (int i = 0; i < _solutionArrayLength; i++)
        {
            array[i] = RandomGenerator.Next(_minValue, _maxValue + 1);
        }

        return array;
    }
    private int[] ComputeGradient(double[] array, Func<double[], double> fittingFunction)
    {
        int[] gradient = new int[_solutionArrayLength];

        for (int i = 0; i < _solutionArrayLength; i++)
        {
            double[] modifiedArray = (double[])array.Clone();
            modifiedArray[i] += 1;

            double currentValue = fittingFunction(array);
            double modifiedValue = fittingFunction(modifiedArray);
            gradient[i] = (int)((modifiedValue - currentValue) / 1);
        }

        return gradient;
    }

    private void UpdateArray(double[] array, int[] gradient)
    {
        for (int i = 0; i < _solutionArrayLength; i++)
        {
            array[i] = Math.Clamp(array[i] + (int)(LearningRate * gradient[i]), _minValue, _maxValue);
        }
    }
    private double CalculateFitness(double[] chromosome)
    {
        double evaluationValue;
        switch (_fitnessAlgorithm)
        {
            case OptimizerContracts.FitnessAlgorithm.Linearity:
                evaluationValue = Linearity.CalculateLinearityForOnePermutation(chromosome, _initialDataHolder);
                break;
            case OptimizerContracts.FitnessAlgorithm.RSquared:
                evaluationValue = LinearInterpolation.CalculateRSquaredForOnePermutation(chromosome, _initialDataHolder);
                break;
            case OptimizerContracts.FitnessAlgorithm.ProfitByDrawdown:
                evaluationValue = Profit.CalculateProfitForOnePermutation(chromosome, _initialDataHolder);
                evaluationValue /= DrawDown.CalculateMaxDrawdownForOnePermutation(chromosome, _initialDataHolder);
                break;
            case OptimizerContracts.FitnessAlgorithm.SharpeOnEma:
                evaluationValue = SharpeOnEma.CalculateSharpeOnEmaForOnePermutation(chromosome, _initialDataHolder);
                break;
            case OptimizerContracts.FitnessAlgorithm.MaxProfit:
                evaluationValue = MaxProfit.CalculateAccumulatedProfit(chromosome, _initialDataHolder);
                break;
            case OptimizerContracts.FitnessAlgorithm.Sortino:
                evaluationValue = Sortino.CalculateSortinoForOnePermutation(chromosome, _initialDataHolder);
                break;
            case OptimizerContracts.FitnessAlgorithm.Sharpe:
            default:
                evaluationValue = Sharpe.CalculateSharpeForOnePermutation(chromosome, _initialDataHolder);
                break;
        }

        _bestFitnessValue = 1/evaluationValue; 
        return _bestFitnessValue;
    }
}