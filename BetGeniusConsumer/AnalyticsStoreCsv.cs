using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using BetGeniusConsumer.Interfaces;

namespace BetGeniusConsumer;

public class AnalyticsStoreCsv : IAnalyticsStore
{
    public void Store(string fileName, IEnumerable<object> obj, bool isAppend = false)
    {
        if (isAppend) {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = false
            };
            using (var stream = File.Open(fileName, FileMode.Append))
            using (var writer = new StreamWriter(stream))
            using (var csv = new CsvWriter(writer, config))
            {
                csv.WriteRecords(obj);
            }
        }
        else
        {
            using (var writer = new StreamWriter(fileName))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(obj);
            }
        }
    }
}