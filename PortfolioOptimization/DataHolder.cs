namespace PortfolioOptimization;

public class DataHolder
{
    public List<Row> InitialData { get; private set; } = new();
    public List<string> StrategyList { get; private init; } = new();
    
    public DataHolder(string inputFileName)
    {
        //0. read the input file
        var readText = File.ReadAllLines(inputFileName);
        if (readText.Length < 2) throw new MyException("Input file is less than 2 lines");

        //1. first line is a list of strategies
        string[] parts = readText[0].Split(',');
        if (parts.Length < 2) throw new MyException("List of strategies too short");
        //trim all strategy names
        for (var i = 1; i < parts.Length; i++)
        {
            parts[i] = parts[i].Trim();
        }

        for (var i = 1; i < parts.Length; i++) StrategyList.Add(parts[i]);


        for (var i = 1; i < readText.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(readText[i])) continue;
            
            var lineParts = readText[i].Split(',');

            //first part in the line is date
            if (!DateTime.TryParse(lineParts[0], out var date))
            {
                //wrong format
                throw new MyException("Wrong date format " + lineParts[0] + " at line " + i);
            }
            

            var listOfPnl = new double[lineParts.Length - 1];
            for (var j = 1; j < lineParts.Length; j++)
            {
                //parse
                if (!double.TryParse(lineParts[j], out var pnl))
                {
                    throw new MyException("Wrong PnL format " + lineParts[j] + " at line " + i);
                }

                listOfPnl[j - 1] = pnl;
            }

            //add line to data holder
            Row row = new Row(date, listOfPnl);
            InitialData.Add(row);
        }

    }

    public DataHolder()
    {
    }

    public void AddRow(DateTime date, double[] dailyAccumulatedPnlByStrategy)
    {
        
    }

    public DataHolder GetRangeOfData(DateTime startDate, DateTime endDate)
    {
        DataHolder result = new DataHolder
        {
            StrategyList = StrategyList
        };
        if (InitialData.Count <= 0) return result; //just empty
        var rangeOfData = InitialData.FindAll(x => x.Date >= startDate && x.Date <= endDate);
        result.InitialData = rangeOfData;
        return result;
    }

    public class Row
    {
        public DateTime Date { get; }
        public double[] DailyPnlByStrategy { get; }

        public Row(DateTime date, double[] dailyPnlByStrategy)
        {
            Date = date;
            DailyPnlByStrategy = dailyPnlByStrategy;
        }

    }
}