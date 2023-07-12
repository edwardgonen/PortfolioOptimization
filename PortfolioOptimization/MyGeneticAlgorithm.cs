namespace PortfolioOptimization;

using System;
using System.Collections.Generic;
using System.Linq;

class MyGeneticAlgorithm
{
    private double _bestFitnessValue;
    private int[] _finalChromosome;
    
    private readonly Random _random;
    private readonly int _populationSize;
    private readonly int _chromosomeLength;
    private readonly int _maxGenerations;
    private readonly int _minValue;
    private readonly int _maxValue;
    private readonly DataHolder _initialDataHolder;

    //public MyGeneticAlgorithm(int populationSize, int chromosomeLength, int maxGenerations, int minValue, int maxValue, DataHolder initialDataHolder)
    public MyGeneticAlgorithm(int numberOfStrategies, DataHolder dataHolder, int minValue, int maxValue)
    {
        _random = new Random();
        this._populationSize = 100;
        this._chromosomeLength = numberOfStrategies;
        this._maxGenerations = 100;
        this._minValue = minValue;
        this._maxValue = maxValue;
        _initialDataHolder = dataHolder;
        _finalChromosome = new int[_chromosomeLength];
    }

    public void Start()
    {
        double bestFitness = 0;
        List<int[]> population = GenerateInitialPopulation();

        for (int generation = 1; generation <= _maxGenerations; generation++)
        {
            List<double> fitnessValues = EvaluatePopulationFitness(population);
            List<int[]> parents = SelectParents(population, fitnessValues);
            List<int[]> offspring = CreateOffspring(parents);
            population = offspring;
            bestFitness = fitnessValues.Max();

            //Logger.Log($"Generation {generation}: Best Fitness = {bestFitness}");
        }

        _bestFitnessValue = bestFitness;
        _finalChromosome = population[0];
    }

    public int[] BestChromosome()
    {
        return _finalChromosome;
    }

    public double BestFitness()
    {
        return _bestFitnessValue;
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

    private List<double> EvaluatePopulationFitness(List<int[]> population)
    {
        List<double> fitnessValues = new List<double>();

        foreach (int[] chromosome in population)
        {
            double fitness = CalculateFitness(chromosome);
            fitnessValues.Add(fitness);
        }

        return fitnessValues;
    }

    private double CalculateFitness(int[] chromosome)
    {
        //double evaluationValue = Sharpe.CalculateSharpeForOnePermutation(_targetArray, _initialDataHolder);
        //double evaluationValue = Linearity.CalculateLinearityForOnePermutation(_targetArray, _initialDataHolder);
        double evaluationValue = Profit.CalculateProfitForOnePermutation(chromosome, _initialDataHolder);
        evaluationValue = evaluationValue /
                          DrawDown.CalculateMaxDrawdownForOnePermutation(chromosome, _initialDataHolder);
        //double evaluationValue = Linearity.CalculateLinearityForOnePermutation(_targetArray, _initialDataHolder) * Profit.CalculateProfitForOnePermutation(_targetArray, _initialDataHolder);

        return evaluationValue;
    }

    private List<int[]> SelectParents(List<int[]> population, List<double> fitnessValues)
    {
        // Your parent selection logic goes here
        // You'll need to implement a selection method such as roulette wheel selection, tournament selection, etc.
        // The selection method should choose chromosomes from the population based on their fitness values

        // For illustration purposes, let's assume a simple roulette wheel selection
        double totalFitness = fitnessValues.Sum();
        List<int[]> parents = new List<int[]>();

        for (int i = 0; i < _populationSize; i++)
        {
            double randomFitness = _random.NextDouble() * totalFitness;
            double cumulativeFitness = 0;

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