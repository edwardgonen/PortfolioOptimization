namespace PortfolioOptimization;

public class DailyPlList
{
    private readonly List<DailyRowAllStrategies> _allStrategiesPnl = new();
    private readonly List<string> _strategiesList = new();

    private readonly Dictionary<string, DateTime> _lastStrategyTradeDates = new();
    
    private void AddTradePnlForDate(DateTime date, string strategyName, double tradePnl)
    {
        //store last trade date
        _lastStrategyTradeDates[strategyName] = date;
        //add strategy to the list of strategies
        if (_strategiesList.Find(x => x == strategyName) == default)
        {
            //not found
            _strategiesList.Add(strategyName);
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
    
    public void SaveToFile(string parametersDailyPlFileName, int strategyInactivityTimeOutDays)
    {
        //delete inactive strategies
        foreach (var pair in _lastStrategyTradeDates.Where(pair => (DateTime.Now - pair.Value).Days > strategyInactivityTimeOutDays))
        {
            _strategiesList.RemoveAll(x => x == pair.Key);
        }
        
        
        using StreamWriter outputFile = new StreamWriter(parametersDailyPlFileName);
        //write strategies line
        outputFile.WriteLine("date," + string.Join(",", _strategiesList));

        foreach (var t in _allStrategiesPnl)
        {
            List<double> dailyPnls = new List<double>();
            foreach (var strategy in _strategiesList)
            {
                var pnl = t.GetStrategyPnl(strategy);
                dailyPnls.Add(pnl);
            }
            outputFile.WriteLine(t.Date.ToShortDateString() + "," + string.Join(",", dailyPnls));
        }
    }
    
    public static DailyPlList LoadFromTradeCompletionLog(string parametersTradeCompletionFileName, int numOfLastLinesToRead)
    {
        DailyPlList result = new DailyPlList();
        //read the trade completion log
        var readText = File.ReadAllLines(parametersTradeCompletionFileName);
        var allLines = readText.Skip(Math.Max(0, readText.Count() - numOfLastLinesToRead));

        foreach (var line in allLines)
        {
            //parse
            string[] parts = line.Split(",");
            if (parts.Length != 15)
            {
                throw new MyException("Line in TradeCompletionLog file is wrong - not 15 elements " + line);
            }

            string strategyName = parts[10];
            
            //[6] is close date, [9] - strategy name, [11] PL
            if (!DateTime.TryParse(parts[6], out var closedDate))
            {
                throw new MyException("Wrong closed date in TradeCompletionLog file " + parts[6]);
            }
            closedDate = closedDate.Date;
            result._lastStrategyTradeDates[strategyName] = closedDate;

            if (!double.TryParse(parts[11], out var tradePnl))
            {
                throw new MyException("Wrong PnL in TradeCompletionLog file " + parts[11]);
            }

            result.AddTradePnlForDate(closedDate, strategyName, tradePnl);
        }

        //sort alphabetically
        result._strategiesList.Sort();
        return result;
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

        public double GetStrategyPnl(string strategyName)
        {
            var strategyWithPnl = _allStrategiesPnl.Find(x => x.Name == strategyName);
            if (strategyWithPnl == default) return 0;
            return strategyWithPnl.Pnl;
        }
    }

    private class DailyRowOneStrategy
    {
        public string Name = string.Empty;
        public double Pnl;
    }


}