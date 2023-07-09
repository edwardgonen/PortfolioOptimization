using System.Globalization;

namespace PortfolioOptimization;

public class ContractsAllocation
{
    private readonly Dictionary<string, List<AllocationPerDate>> _contractAllocationsDictionary = new Dictionary<string, List<AllocationPerDate>>();
    private readonly string _contractAllocationFileName;

    public ContractsAllocation(string dataPath)
    {
        _contractAllocationFileName = dataPath + Path.DirectorySeparatorChar + "ContractsAllocation.csv";
    }
    
    
    public void AddAllocation(DateTime allocationStartDate, string strategyName, decimal numberOfContracts)
    {
        lock (_contractAllocationsDictionary)
        {
            if (!_contractAllocationsDictionary.ContainsKey(strategyName))
            {
                _contractAllocationsDictionary.Add(strategyName, new List<AllocationPerDate>());
            }

            _contractAllocationsDictionary[strategyName]
                .Add(new AllocationPerDate(allocationStartDate, numberOfContracts));
        }
    }

    public decimal GetAllocation(DateTime processedDate, string strategyName)
    {
        decimal result = -1;

        lock (_contractAllocationsDictionary)
        {
            if (!_contractAllocationsDictionary.ContainsKey(strategyName)) throw new MyException("Strategy " + strategyName + " was not found in the allocation table.");
            var allocationsForStrategy = _contractAllocationsDictionary[strategyName];
            
            for (var i = 0; i < allocationsForStrategy.Count - 1; i++)
            {
                if (processedDate < allocationsForStrategy.First().AllocationStartDate) return 0; //don't count
                if (processedDate >= allocationsForStrategy.Last().AllocationStartDate)
                    return allocationsForStrategy.Last().NumberOfContracts;
                
                if (processedDate >= allocationsForStrategy[i].AllocationStartDate &&
                    processedDate < allocationsForStrategy[i + 1].AllocationStartDate)
                {
                    return allocationsForStrategy[i].NumberOfContracts;
                }
            }
        }

        return result;
    }

    public void SortByDate()
    {
        lock (_contractAllocationsDictionary)
        {
            foreach (var pair in _contractAllocationsDictionary)
            {
                pair.Value.Sort((p, q) => p.AllocationStartDate.CompareTo(q.AllocationStartDate));
            }
        }
    }

    public void SaveToFile()
    {
        lock (_contractAllocationsDictionary)
        {
            using StreamWriter outputFile = new StreamWriter(_contractAllocationFileName);

            string[] parts = new string [_contractAllocationsDictionary.First().Value.Count + 1];
            parts[0] = "Project name and time segments";
            for (var i = 0; i < _contractAllocationsDictionary.First().Value.Count; i++)
            {
                parts[i + 1] = _contractAllocationsDictionary.First().Value[i].AllocationStartDate.ToShortDateString();
            }

            outputFile.WriteLine(string.Join(",", parts));
            
            foreach (var pair in _contractAllocationsDictionary)
            {
                parts = new string [_contractAllocationsDictionary.First().Value.Count + 1];
                parts[0] = pair.Key;
                for (var i = 0; i < pair.Value.Count; i++)
                {
                    parts[i + 1] = pair.Value[i].NumberOfContracts.ToString(CultureInfo.CurrentCulture);
                }
                outputFile.WriteLine(string.Join(",", parts));
            }

        }
    }

    class AllocationPerDate
    {
        public DateTime AllocationStartDate { get; }
        public decimal NumberOfContracts { get; }

        public AllocationPerDate(DateTime allocationStartDate, decimal numberOfContracts)
        {
            AllocationStartDate = allocationStartDate;
            NumberOfContracts = numberOfContracts;
        }
    }
}