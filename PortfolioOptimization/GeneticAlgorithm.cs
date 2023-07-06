namespace PortfolioOptimization;

using System;
using System.Linq;

public class GeneticAlgorithm<T>
{
    private readonly int populationSize;
    private readonly T[] range;
    private readonly Func<T[], double> fitnessFunction;
    private readonly int maxGenerations;
    private readonly Random random;

    public GeneticAlgorithm(int populationSize, T[] range, Func<T[], double> fitnessFunction, int maxGenerations)
    {
        this.populationSize = populationSize;
        this.range = range;
        this.fitnessFunction = fitnessFunction;
        this.maxGenerations = maxGenerations;
        random = new Random();
    }

    public T[]? OptimizeArray()
    {
        T[][] population = GenerateInitialPopulation();

        for (int generation = 0; generation < maxGenerations; generation++)
        {
            T[][] nextGeneration = new T[populationSize][];

            for (int i = 0; i < populationSize; i++)
            {
                T[] parent1 = SelectParent(population);
                T[] parent2 = SelectParent(population);

                T[] child = Crossover(parent1, parent2);
                Mutate(child);

                nextGeneration[i] = child;
            }

            population = nextGeneration;

            T[]? fittestIndividual = GetFittestIndividual(population);
            if (fitnessFunction(fittestIndividual) == 1.0)
            {
                return fittestIndividual;
            }
        }

        return GetFittestIndividual(population);
    }

    private T[][] GenerateInitialPopulation()
    {
        T[][] population = new T[populationSize][];

        for (int i = 0; i < populationSize; i++)
        {
            T[] individual = new T[range.Length];

            for (int j = 0; j < range.Length; j++)
            {
                individual[j] = GenerateRandomGene();
            }

            population[i] = individual;
        }

        return population;
    }

    private T GenerateRandomGene()
    {
        int index = random.Next(range.Length);
        return range[index];
    }

    private T[] SelectParent(T[][] population)
    {
        int index1 = random.Next(populationSize);
        int index2 = random.Next(populationSize);

        return fitnessFunction(population[index1]) > fitnessFunction(population[index2])
            ? population[index1]
            : population[index2];
    }

    private T[] Crossover(T[] parent1, T[] parent2)
    {
        T[] child = new T[parent1.Length];

        int crossoverPoint = random.Next(parent1.Length);

        for (int i = 0; i < parent1.Length; i++)
        {
            child[i] = i < crossoverPoint ? parent1[i] : parent2[i];
        }

        return child;
    }

    private void Mutate(T[] individual)
    {
        int index = random.Next(individual.Length);
        individual[index] = GenerateRandomGene();
    }

    private T[]? GetFittestIndividual(T[][] population)
    {
        return population.MaxBy(fitnessFunction);
    }
}
/*
public class Program
{
    public static void Main(string[] args)
    {
        int populationSize = 50;
        int[] range = Enumerable.Range(1, 30).ToArray();
        Func<int[], double> fitnessFunction = FitnessFunction;
        int maxGenerations = 1000;

        GeneticAlgorithm<int> ga = new GeneticAlgorithm<int>(populationSize, range, fitnessFunction, maxGenerations);
        int[] optimizedArray= ga.OptimizeArray();

        Console.WriteLine("Optimized Array:");
        Console.WriteLine(string.Join(", ", optimizedArray));
    }

    private static double FitnessFunction(int[] individual)
    {
        // Custom fitness function implementation
        // Example: maximize the sum of the array elements

        double sum = individual.Sum();
        return sum / (individual.Length * 30); // Normalize fitness between 0 and 1
    }
}
*/