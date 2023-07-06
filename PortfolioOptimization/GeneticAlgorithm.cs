namespace PortfolioOptimization;

using System;
using System.Collections.Generic;
using System.Linq;

class GeneticAlgorithm
{
    private readonly Random _random;
    private readonly int _populationSize;
    private readonly int _chromosomeLength;
    private readonly int _maxGenerations;
    private readonly int _minValue;
    private readonly int _maxValue;
    private readonly DataHolder _initialDataHolder;

    public GeneticAlgorithm(int populationSize, int chromosomeLength, int maxGenerations, int minValue, int maxValue, DataHolder initialDataHolder)
    {
        _random = new Random();
        this._populationSize = populationSize;
        this._chromosomeLength = chromosomeLength;
        this._maxGenerations = maxGenerations;
        this._minValue = minValue;
        this._maxValue = maxValue;
        _initialDataHolder = initialDataHolder;
    }

    public int[] Optimize()
    {
        List<int[]> population = GenerateInitialPopulation();

        for (int generation = 1; generation <= _maxGenerations; generation++)
        {
            List<decimal> fitnessValues = EvaluatePopulationFitness(population);
            List<int[]> parents = SelectParents(population, fitnessValues);
            List<int[]> offspring = CreateOffspring(parents);
            population = offspring;

            decimal bestFitness = fitnessValues.Max();
            //Logger.Log($"Generation {generation}: Best Fitness = {bestFitness}");
        }

        int[] finalChromosome = population[0];
        Logger.Log("Final Solution:");
        Logger.Log(string.Join(", ", finalChromosome));

        return finalChromosome;
    }

    private List<int[]> GenerateInitialPopulation()
    {
        List<int[]> population = new List<int[]>();

        for (int i = 0; i < _populationSize; i++)
        {
            int[] chromosome = new int[_chromosomeLength];
            for (int j = 0; j < _chromosomeLength; j++)
            {
                chromosome[j] = _random.Next(_minValue, _maxValue + 1);
            }
            population.Add(chromosome);
        }

        return population;
    }

    private List<decimal> EvaluatePopulationFitness(List<int[]> population)
    {
        List<decimal> fitnessValues = new List<decimal>();

        foreach (int[] chromosome in population)
        {
            decimal fitness = CalculateFitness(chromosome);
            fitnessValues.Add(fitness);
        }

        return fitnessValues;
    }

    private decimal CalculateFitness(int[] chromosome)
    {
        // Your fitness calculation logic goes here
        // You'll need to define a fitness function based on your specific problem
        // The fitness function should evaluate how well a chromosome solves the problem

        // For illustration purposes, let's assume a simple fitness function that sums all the integers
        decimal sharpe = Sharpe.CalculateSharpeForOnePermutation(chromosome, _initialDataHolder);
        return sharpe;
    }

    private List<int[]> SelectParents(List<int[]> population, List<decimal> fitnessValues)
    {
        // Your parent selection logic goes here
        // You'll need to implement a selection method such as roulette wheel selection, tournament selection, etc.
        // The selection method should choose chromosomes from the population based on their fitness values

        // For illustration purposes, let's assume a simple roulette wheel selection
        decimal totalFitness = fitnessValues.Sum();
        List<int[]> parents = new List<int[]>();

        for (int i = 0; i < _populationSize; i++)
        {
            decimal randomFitness = (decimal)_random.NextDouble() * totalFitness;
            decimal cumulativeFitness = 0;

            for (int j = 0; j < _populationSize; j++)
            {
                cumulativeFitness += fitnessValues[j];

                if (cumulativeFitness >= randomFitness)
                {
                    parents.Add(population[j]);
                    break;
                }
            }
        }

        return parents;
    }

    private List<int[]> CreateOffspring(List<int[]> parents)
    {
        List<int[]> offspring = new List<int[]>();

        while (offspring.Count < _populationSize)
        {
            int[] parent1 = parents[_random.Next(parents.Count)];
            int[] parent2 = parents[_random.Next(parents.Count)];
            int[] child = Crossover(parent1, parent2);
            Mutate(child);
            offspring.Add(child);
        }

        return offspring;
    }

    private int[] Crossover(int[] parent1, int[] parent2)
    {
        int[] child = new int[_chromosomeLength];

        int crossoverPoint = _random.Next(1, _chromosomeLength - 1);

        for (int i = 0; i < crossoverPoint; i++)
        {
            child[i] = parent1[i];
        }

        for (int i = crossoverPoint; i < _chromosomeLength; i++)
        {
            child[i] = parent2[i];
        }

        return child;
    }

    private void Mutate(int[] chromosome)
    {
        decimal mutationRate = (decimal) 0.01;

        for (int i = 0; i < _chromosomeLength; i++)
        {
            if ((decimal)_random.NextDouble() < mutationRate)
            {
                chromosome[i] = _random.Next(_minValue, _maxValue + 1);
            }
        }
    }
}

/*
class Program
{
    static void Main()
    {
        int populationSize = 100;
        int chromosomeLength = 47;
        int maxGenerations = 100;
        int minValue = 3;
        int maxValue = 33;

        GeneticAlgorithm geneticAlgorithm = new GeneticAlgorithm(populationSize, chromosomeLength, maxGenerations, minValue, maxValue);
        int[] solution = geneticAlgorithm.Optimize();

        // Use the solution as needed
    }
}
*/