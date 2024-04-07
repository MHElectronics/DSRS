using BOL;
using Microsoft.VisualBasic.FileIO;
using System.Data;
using System.Text;

namespace Services.Helpers;

public interface ICsvHelper
{
    DataTable GetDataTableFromByte(byte[] bytes, UploadedFile file);
}

public class CsvHelper : ICsvHelper
{
    public DataTable GetDataTableFromByte(byte[] bytes, UploadedFile file)
    {
        using TextFieldParser csvReader = new(new MemoryStream(bytes), Encoding.Default);

        csvReader.SetDelimiters([","]);
        csvReader.HasFieldsEnclosedInQuotes = true;

        DataTable csvData = new();
        try
        {
            string[] colFields = csvReader.ReadFields() ?? [];
            
            if(file.FileType == (int)UploadedFileType.FineData)
            {
                csvData = this.GetNewDataTableFinePayment();
            }
            else
            {
                //Default FileId column
                csvData.Columns.Add("FileId");

                foreach (string column in colFields)
                {
                    DataColumn dataColumn = new(column)
                    {
                        AllowDBNull = true
                    };
                    csvData.Columns.Add(dataColumn);
                }
            }
            
            while (!csvReader.EndOfData)
            {
                string[] fieldData = [file.Id.ToString()];
                string[] csvFieldData = csvReader.ReadFields() ?? [];
                fieldData = fieldData.Union(csvFieldData).ToArray();
                //Making empty value as null
                for (int i = 0; i < fieldData.Length; i++)
                {
                    if(csvData.Columns[i].DataType == typeof(bool))
                    {
                        fieldData[i] = fieldData[i] == "1" ? "true" : "false";
                    }

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
    private DataTable GetNewDataTableFinePayment()
    {
        DataTable dt = new();
        //Default FileId column
        dt.Columns.Add("FileId", typeof(int));

        dt.Columns.Add("TransactionNumber", typeof(string));
        dt.Columns.Add("DateTime", typeof(DateTime));
        dt.Columns.Add("IsPaid", typeof(bool));
        dt.Columns.Add("FineAmount", typeof(decimal));
        dt.Columns.Add("PaymentMethod", typeof(string));
        dt.Columns.Add("ReceiptNumber", typeof(string));
        dt.Columns.Add("BillNumber", typeof(string));
        dt.Columns.Add("WarehouseCharge", typeof(decimal));
        dt.Columns.Add("DriversLicenseNumber", typeof(string));

        return dt;
    }
}
