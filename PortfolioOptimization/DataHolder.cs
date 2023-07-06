namespace PortfolioOptimization;

public class DataHolder
{
    public List<Row> InitialData { get; } = new();
    public List<string> StrategyList { get; } = new();

    public DataHolder(string inputFileName, DateTime startDate, DateTime endDate)
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
            var lineParts = readText[i].Split(',');

            //first part in the line is date
            if (!DateTime.TryParse(lineParts[0], out var date))
            {
                //wrong format
                throw new MyException("Wrong date format " + lineParts[0] + " at line " + i);
            }

            if (date.Date < startDate || date.Date >= endDate) continue;



            var listOfAccumulatedPnl = new decimal[lineParts.Length - 1];
            for (var j = 1; j < lineParts.Length; j++)
            {
                //parse
                if (!decimal.TryParse(lineParts[j], out var pnl))
                {
                    throw new MyException("Wrong PnL format " + lineParts[j] + " at line " + i);
                }

                listOfAccumulatedPnl[j - 1] = pnl;
            }

            //add line to data holder
            Row row = new Row(date, listOfAccumulatedPnl);
            InitialData.Add(row);
        }

    }

    public class Row
    {
        public DateTime Date { get; }
        public decimal[] DailyAccumulatedPnlByStrategy { get; }

        public Row(DateTime date, decimal[] dailyAccumulatedPnlByStrategy)
        {
            Date = date;
            DailyAccumulatedPnlByStrategy = dailyAccumulatedPnlByStrategy;
        }

    }
}