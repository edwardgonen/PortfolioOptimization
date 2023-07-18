namespace PortfolioOptimization;

public class DailyPlList
{
    private readonly List<DailyRowAllStrategies> _allStrategiesPnl = new List<DailyRowAllStrategies>();
    public readonly List<string> StrategiesList = new List<string>();

    public void AddTradePnlForDate(DateTime date, string strategyName, double tradePnl)
    {
        //add strategy to the list of strategies
        if (StrategiesList.Find(x => x == strategyName) == default)
        {
            //not found
            StrategiesList.Add(strategyName);
        }
        //find date
        var dailyRow = _allStrategiesPnl.FirstOrDefault(x => x.Date == date);
        if (dailyRow == default) //not found 
        {
            //add
            var newDateLine = new DailyRowAllStrategies
            {
                Date = date
            };
            DailyRowOneStrategy oneStrategy = new DailyRowOneStrategy
            {
                Name = strategyName,
                Pnl = tradePnl
            };
            newDateLine.AddStrategyPnl(oneStrategy);
            _allStrategiesPnl.Add(newDateLine);
        }
        else //found
        {
            //update
            dailyRow.AddStrategyPnl(new DailyRowOneStrategy(){Name = strategyName, Pnl = tradePnl});
        }
    }
    public void SaveToFile(string parametersDailyPlFileName)
    {
        using StreamWriter outputFile = new StreamWriter(parametersDailyPlFileName);
        //write strategies line
        outputFile.WriteLine("date," + string.Join(",", StrategiesList));
    }
    private class DailyRowAllStrategies
    {
        public DateTime Date;
        private readonly List<DailyRowOneStrategy> _allStrategiesPnl = new List<DailyRowOneStrategy>();

        public void AddStrategyPnl(DailyRowOneStrategy dailyRowOneStrategy)
        {
            //do we have such strategy?
            var strategyItem = _allStrategiesPnl.FirstOrDefault(x => x.Name == dailyRowOneStrategy.Name);
            if (strategyItem == default) //not found
            {
                //add
                _allStrategiesPnl.Add(dailyRowOneStrategy);
            }
            else //just sum up
            {
                strategyItem.Pnl += dailyRowOneStrategy.Pnl;
            }
        }
    }

    private class DailyRowOneStrategy
    {
        public string Name = string.Empty;
        public double Pnl;
    }


}