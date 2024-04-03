using Microsoft.VisualBasic.FileIO;
using System.Data;
using System.Text;

namespace Services.Helpers;

public interface ICsvHelper
{
    DataTable GetDataTableFromByte(byte[] bytes);
}

public class CsvHelper : ICsvHelper
{
    public DataTable GetDataTableFromByte(byte[] bytes)
    {
        using TextFieldParser csvReader = new(new MemoryStream(bytes), Encoding.Default);

        csvReader.SetDelimiters([","]);
        csvReader.HasFieldsEnclosedInQuotes = true;

        DataTable csvData = new();
        try
        {
            string[] colFields = csvReader.ReadFields() ?? [];
            foreach (string column in colFields)
            {
                DataColumn dataColumn = new(column)
                {
                    AllowDBNull = true
                };
                csvData.Columns.Add(dataColumn);
            }

            while (!csvReader.EndOfData)
            {
                string[] fieldData = csvReader.ReadFields() ?? [];
                //Making empty value as null
                for (int i = 0; i < fieldData.Length; i++)
                {
                    if (fieldData[i] == "")
                    {
                        fieldData[i] = null;
                    }
                }
                csvData.Rows.Add(fieldData);
            }
        }
        catch (Exception)
        {
            throw;
        }

        return csvData;
    }
}
