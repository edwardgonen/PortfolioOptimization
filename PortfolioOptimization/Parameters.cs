using System.Xml.Serialization;

namespace PortfolioOptimization;

[Serializable]
public class Parameters
{
    public int ContractsRangeStart = 1;
    public int ContractsRangeEnd = 10;
    public string DailyPlFileName = string.Empty;
    public string TradeCompletionFileName = string.Empty;
    public int InSampleDays = 300;
    public int OutSampleDays = 11;
    public bool BParallel = true;
    public bool BRealTime = true;
    public int NumberOfLinesToReadFromTheEndOfTradesLog = 10000;
    
    public static Parameters? Load(string fileName)
    {

        XmlSerializer s = new XmlSerializer(typeof(Parameters));
        TextReader? reader = null;
        Parameters? parameters;

        var parametersFileName = Directory.GetCurrentDirectory();
        //1. check that the parameters location ended with Path separator
        if (!parametersFileName.EndsWith(Path.DirectorySeparatorChar))
        {
            parametersFileName += Path.DirectorySeparatorChar;
        }

        parametersFileName += fileName;
        
        try
        {
            reader = new StreamReader(parametersFileName);
            parameters = s.Deserialize(reader) as Parameters;

        }
        catch (Exception)
        {
            throw new MyException("Failed to read parameters from " + parametersFileName);
        }
        finally
        {
            reader?.Close();
        }

        return parameters;
    }
}