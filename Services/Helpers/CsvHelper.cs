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
                csvData = this.GetNewDataTableAxleLoad();
                
                ////Default FileId column
                //csvData.Columns.Add("FileId");

                //foreach (string column in colFields)
                //{
                //    DataColumn dataColumn = new(column)
                //    {
                //        AllowDBNull = true
                //    };
                //    csvData.Columns.Add(dataColumn);
                //}
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
    private DataTable GetNewDataTableAxleLoad()
    {
        DataTable dt = new();
        //Default FileId column
        dt.Columns.Add("FileId", typeof(int));

        dt.Columns.Add("TransactionNumber", typeof(string));
        dt.Columns.Add("LaneNumber", typeof(int));
        dt.Columns.Add("DateTime", typeof(DateTime));
        dt.Columns.Add("PlateNumber", typeof(string));
        dt.Columns.Add("VehicleId", typeof(string));
        dt.Columns.Add("NumberOfAxle", typeof(int));
        dt.Columns.Add("VehicleSpeed", typeof(decimal));
        dt.Columns.Add("Axle1st", typeof(decimal));
        dt.Columns.Add("Axle2nd", typeof(decimal));
        dt.Columns.Add("Axle3rd", typeof(decimal));
        dt.Columns.Add("Axle4th", typeof(decimal));
        dt.Columns.Add("Axle5th", typeof(decimal));
        dt.Columns.Add("Axle6th", typeof(decimal));
        dt.Columns.Add("Axle7th", typeof(decimal));
        dt.Columns.Add("AxleRemaining", typeof(decimal));
        dt.Columns.Add("GrossVehicleWeight", typeof(decimal));
        dt.Columns.Add("IsUnloaded", typeof(bool));
        dt.Columns.Add("IsOverloaded", typeof(bool));
        dt.Columns.Add("OverSizedModified", typeof(bool));
        dt.Columns.Add("Wheelbase", typeof(int));
        dt.Columns.Add("Axle1stTime", typeof(DateTime));
        dt.Columns.Add("Axle2ndTime", typeof(DateTime));
        dt.Columns.Add("Axle3rdTime", typeof(DateTime));
        dt.Columns.Add("Axle4thTime", typeof(DateTime));
        dt.Columns.Add("Axle5thTime", typeof(DateTime));
        dt.Columns.Add("Axle6thTime", typeof(DateTime));
        dt.Columns.Add("Axle7thTime", typeof(DateTime));
        
        return dt;
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
