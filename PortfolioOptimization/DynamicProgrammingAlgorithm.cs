namespace PortfolioOptimization;

public class DynamicProgrammingAlgorithm : IOptimizationAlgorithm
{
    private double _bestFitnessValue;
    private int[] _finalChromosome;
    
    private readonly int _minValue;
    private readonly int _maxValue;
    private readonly DataHolder _initialDataHolder;
    private readonly int[][] _allCombinationsArray;
    private readonly OptimizerContracts.FitnessAlgorithm _fitnessAlgorithm;
    private readonly int _numOfStrategies;
    private readonly int _numOfValuesFromMinToMax;
    
    public DynamicProgrammingAlgorithm(int numberOfStrategies, DataHolder dataHolder, int minValue, int maxValue,
        OptimizerContracts.FitnessAlgorithm fitnessAlgorithm)
    {

        _minValue = minValue;
        _maxValue = maxValue;
        _numOfValuesFromMinToMax = _maxValue - _minValue + 1;
        _initialDataHolder = dataHolder;
        _finalChromosome = new int[numberOfStrategies];
        _fitnessAlgorithm = fitnessAlgorithm;
        _numOfStrategies = numberOfStrategies;
        
        
        _allCombinationsArray = new int[_numOfStrategies][];
        for (int i = 0; i < _numOfStrategies; i++)
        {
            _allCombinationsArray[i] = new int [_numOfValuesFromMinToMax];
        }
        
        for (int i = 0; i < _numOfStrategies; i++)
        {
            for (int j = 0; j < _allCombinationsArray[i].Length; j++)
            {
                _allCombinationsArray[i][j] = _minValue + j;
            }
        }
        
        
    }

    public void Start()
    {
        // Initialize the DP array for dynamic programming.
        double[][] dp = new double[_numOfStrategies + 1][];
        for (int i = 0; i <= _numOfStrategies; i++)
        {
            dp[i] = new double[_numOfValuesFromMinToMax + 1];
        }

        // Fill the DP array based on your fitness function.
        for (int i = 1; i <= _numOfStrategies; i++)
        {
            for (int j = 1; j <= _numOfValuesFromMinToMax; j++)
            {
                // Calculate the fitness score based on your proprietary function.
                int[] setOfValues = new int[_numOfStrategies];
                for (int k = 0; k < _numOfStrategies; k++)
                {
                    setOfValues[k] = _allCombinationsArray[i - 1][k];
                }

                double currentFitness = CalculateFitness(setOfValues); // Placeholder function.

                // Choose the maximum fitness score between including and excluding the element.
                dp[i][j] = Convert.ToDouble(Math.Max(dp[i - 1][j], dp[i][j - 1] + currentFitness));
            }
        }

        // Find the maximum fitness score.
        double maxFitness = dp[_numOfStrategies][_numOfValuesFromMinToMax];
        Console.WriteLine($"Maximum fitness score: {maxFitness}");
        _bestFitnessValue = maxFitness;
        
        // Backtrack to find the selected elements for the best combination.
        List<int>[] selectedElements = BacktrackSelectedElements(dp, _allCombinationsArray);

        // Extract the best combination of values into a single list of 28 integers.
        List<int> bestCombination = EnsureAllArraysContributed(selectedElements, _allCombinationsArray);
        
        Console.WriteLine(string.Join(", ", bestCombination));
        _finalChromosome = bestCombination.ToArray();
    }

    public int[] BestChromosome()
    {
        return _finalChromosome;
    }

    public double BestFitness()
    {
        return _bestFitnessValue;
    }
    
    

    private double CalculateFitness(int[] chromosome)
    {
        double evaluationValue = Sharpe.CalculateSharpeForOnePermutation(chromosome, _initialDataHolder);
        //double evaluationValue = Linearity.CalculateLinearityForOnePermutation(_targetArray, _initialDataHolder);
        /*
        double evaluationValue = Profit.CalculateProfitForOnePermutation(chromosome, _initialDataHolder);
        evaluationValue = evaluationValue /
                          DrawDown.CalculateMaxDrawdownForOnePermutation(chromosome, _initialDataHolder);
        //double evaluationValue = Linearity.CalculateLinearityForOnePermutation(_targetArray, _initialDataHolder) * Profit.CalculateProfitForOnePermutation(_targetArray, _initialDataHolder);
        */
        return evaluationValue;
    }
    

    /*static int CalculateFitness(int[] array, int endIndex)
    {
        // Implement your proprietary fitness function here.
        // It should calculate and return the fitness score based on the selected elements.
        // You should replace this placeholder with your actual fitness function.
        int fitness = 0;
        for (int i = 0; i < endIndex; i++)
        {
            fitness += array[i]; // Placeholder calculation.
        }
        return fitness;
    }*/

    static List<int>[] BacktrackSelectedElements(double[][] dp, int[][] arrays)
    {
        int i = dp.Length - 1;
        int j = dp[0].Length - 1;
        List<int>[] selectedElements = new List<int>[arrays.Length];

        // Initialize selectedElements lists.
        for (int k = 0; k < selectedElements.Length; k++)
        {
            selectedElements[k] = new List<int>();
        }

        while (i > 0 && j > 0)
        {
            if (dp[i][j] == dp[i - 1][j])
            {
                // Element at arrays[i-1][j-1] was not selected.
                i--;
            }
            else
            {
                // Element at arrays[i-1][j-1] was selected.
                selectedElements[i - 1].Add(j - 1);
                j--;
            }
        }

        return selectedElements;
    }

    static List<int> EnsureAllArraysContributed(List<int>[] selectedElements, int[][] arrays)
    {
        List<int> bestCombination = new List<int>();

        for (int i = 0; i < arrays.Length; i++)
        {
            if (selectedElements[i].Count > 0)
            {
                int selectedIndex = selectedElements[i][0];
                bestCombination.Add(arrays[i][selectedIndex]);
            }
        }

        while (bestCombination.Count < arrays.Length)
        {
            // Find the array that has not contributed yet.
            int missingArrayIndex = bestCombination.Count;

            // Add an element from that array with the value 99.
            bestCombination.Add(99);
        }

        return bestCombination;
    }
    
}