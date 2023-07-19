using System.Globalization;

namespace PortfolioOptimization;

public class ContractsAllocation
{
    private readonly Dictionary<string, List<AllocationPerDate>> _contractAllocationsDictionary = new Dictionary<string, List<AllocationPerDate>>();
    private string _contractAllocationFileName;

    public ContractsAllocation(string? dataPath)
    {
        _contractAllocationFileName = dataPath + Path.DirectorySeparatorChar + "ContractsAllocation.csv";
    }
    
    
    public void AddAllocation(DateTime allocationStartDate, string strategyName, double numberOfContracts)
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

    public double GetAllocation(DateTime processedDate, string strategyName)
    {
        double result = -1;

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

    public static ContractsAllocation LoadFromFile(string allocationFileName)
    {
        ContractsAllocation contractsAllocation = new ContractsAllocation(Path.GetDirectoryName(allocationFileName));

        
        //0. read the input file
        var readText = File.ReadAllLines(allocationFileName);
        if (readText.Length < 2) throw new MyException("Input file is less than 2 lines");

        //1. first line is a list of dates
        
        string[] parts = readText[0].Split(',');
        if (parts.Length <= 1) //wrong format
        {
            throw new MyException("Failed to load contract allocation file. Wrong first line " + readText[0]);
        }

        List<DateTime> dates = new List<DateTime>();
        for (var i = 1; i < parts.Length; i++)
        {
            if (!DateTime.TryParse(parts[i], out var date))
            {
                throw new MyException("Failed to load contract allocation file. Wrong date " + parts[i]);
            }
            //add to list 
            dates.Add(date);
        }

        for (var i = 1; i < readText.Length; i++)
        {
            parts = readText[i].Split(','); //<strategy name, allocation1, allocation2 ...>
            for (var j = 1; j < parts.Length; j++)
            {
                if (!double.TryParse(parts[j], out var allocation))
                {
                    throw new MyException("Failed to convert allocation size in line " + i + " position " + j);
                }
                contractsAllocation.AddAllocation(dates[j - 1], parts[0], allocation);
            }
        }
        
        return contractsAllocation;
    }


    public void SaveToFile(bool bRealTime, string contractsAllocationFileName)
    {
        if (!bRealTime)
        {
            lock (_contractAllocationsDictionary)
            {
                using StreamWriter outputFile = new StreamWriter(_contractAllocationFileName);

                string[] parts = new string [_contractAllocationsDictionary.First().Value.Count + 1];
                parts[0] = "Project name and time segments";
                for (var i = 0; i < _contractAllocationsDictionary.First().Value.Count; i++)
                {
                    parts[i + 1] = _contractAllocationsDictionary.First().Value[i].AllocationStartDate
                        .ToShortDateString();
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
        else //real time
        {
            //1. read existing contracts allocation
            ContractsAllocation contractsAllocation = ContractsAllocation.LoadFromFile(contractsAllocationFileName);
        }
    }

    class AllocationPerDate
    {
        public DateTime AllocationStartDate { get; }
        public double NumberOfContracts { get; }

        public AllocationPerDate(DateTime allocationStartDate, double numberOfContracts)
        {
            AllocationStartDate = allocationStartDate;
            NumberOfContracts = numberOfContracts;
        }
    }
}