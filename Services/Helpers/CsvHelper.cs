using BOL;
using Microsoft.VisualBasic.FileIO;
using System.Data;
using System.Text;

namespace Services.Helpers;

public interface ICsvHelper
{
    (bool isSuccess, DataTable csvData, string summary) GetDataTableFromByte(byte[] bytes, UploadedFile file);
}

public class CsvHelper : ICsvHelper
{
    public (bool isSuccess, DataTable csvData,string summary) GetDataTableFromByte(byte[] bytes, UploadedFile file)
    {
        using TextFieldParser csvReader = new(new MemoryStream(bytes), Encoding.Default);

        csvReader.SetDelimiters([","]);
        csvReader.HasFieldsEnclosedInQuotes = true;

        DataTable csvData = new();
        string summary = String.Empty;
        bool isSuccess = true;
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
            }

            int row = 1;
            while (!csvReader.EndOfData)
            {
                string[] fieldData = [file.Id.ToString()];
                string[] csvFieldData = csvReader.ReadFields() ?? [];
                fieldData = fieldData.Concat(csvFieldData).ToArray();
                
                //Check booleans
                List<int> boolenIndexes = [3];
                if(file.FileType == (int)UploadedFileType.LoadData)
                {
                    boolenIndexes = [19,20,21];
                }

                foreach(int i in boolenIndexes)
                {
                    fieldData[i] = fieldData[i] == "1" ? "true" : "false";
                }

                try
                {
                    csvData.Rows.Add(fieldData);
                }
                catch (Exception ex)
                {
                    summary += "Error in line " + row;
                }

                row++;
            }
        }
        catch (Exception ex)
        {
            isSuccess = false;
            summary += ex.Message;
        }
        
        return (isSuccess, csvData, summary);
    }
    private DataTable GetNewDataTableAxleLoad()
    {
        DataTable dt = new();
        //Default FileId column
        dt.Columns.Add("FileId", typeof(int));

        dt.Columns.Add(NewDataColumn("TransactionNumber", typeof(string)));
        dt.Columns.Add(NewDataColumn("LaneNumber", typeof(int)));
        dt.Columns.Add(NewDataColumn("DateTime", typeof(DateTime)));
        dt.Columns.Add(NewDataColumn("PlateZone", typeof(string)));
        dt.Columns.Add(NewDataColumn("PlateSeries", typeof(string)));
        dt.Columns.Add(NewDataColumn("PlateNumber", typeof(string)));
        dt.Columns.Add(NewDataColumn("VehicleId", typeof(string)));
        dt.Columns.Add(NewDataColumn("NumberOfAxle", typeof(int)));
        dt.Columns.Add(NewDataColumn("VehicleSpeed", typeof(decimal)));
        dt.Columns.Add(NewDataColumn("Axle1", typeof(decimal)));
        dt.Columns.Add(NewDataColumn("Axle2", typeof(decimal)));
        dt.Columns.Add(NewDataColumn("Axle3", typeof(decimal)));
        dt.Columns.Add(NewDataColumn("Axle4", typeof(decimal)));
        dt.Columns.Add(NewDataColumn("Axle5", typeof(decimal)));
        dt.Columns.Add(NewDataColumn("Axle6", typeof(decimal)));
        dt.Columns.Add(NewDataColumn("Axle7", typeof(decimal)));
        dt.Columns.Add(NewDataColumn("AxleRemaining", typeof(decimal)));
        dt.Columns.Add(NewDataColumn("GrossVehicleWeight", typeof(decimal)));
        dt.Columns.Add(NewDataColumn("IsUnloaded", typeof(bool)));
        dt.Columns.Add(NewDataColumn("IsOverloaded", typeof(bool)));
        dt.Columns.Add(NewDataColumn("OverSizedModified", typeof(bool)));
        dt.Columns.Add(NewDataColumn("Wheelbase", typeof(int)));
        dt.Columns.Add(NewDataColumn("ReceiptNumber", typeof(string)));
        dt.Columns.Add(NewDataColumn("BillNumber", typeof(string)));
        dt.Columns.Add(NewDataColumn("Axle1Time", typeof(DateTime)));
        dt.Columns.Add(NewDataColumn("Axle2Time", typeof(DateTime)));
        dt.Columns.Add(NewDataColumn("Axle3Time", typeof(DateTime)));
        dt.Columns.Add(NewDataColumn("Axle4Time", typeof(DateTime)));
        dt.Columns.Add(NewDataColumn("Axle5Time", typeof(DateTime)));
        dt.Columns.Add(NewDataColumn("Axle6Time", typeof(DateTime)));
        dt.Columns.Add(NewDataColumn("Axle7Time", typeof(DateTime)));
        
        return dt;
    }
    private DataTable GetNewDataTableFinePayment()
    {
        DataTable dt = new();
        //Default FileId column
        dt.Columns.Add("FileId", typeof(int));

        dt.Columns.Add(NewDataColumn("TransactionNumber", typeof(string)));
        dt.Columns.Add(NewDataColumn("DateTime", typeof(DateTime)));
        dt.Columns.Add(NewDataColumn("IsPaid", typeof(bool)));
        dt.Columns.Add(NewDataColumn("FineAmount", typeof(decimal)));
        dt.Columns.Add(NewDataColumn("PaymentMethod", typeof(string)));
        dt.Columns.Add(NewDataColumn("ReceiptNumber", typeof(string)));
        dt.Columns.Add(NewDataColumn("BillNumber", typeof(string)));
        dt.Columns.Add(NewDataColumn("WarehouseCharge", typeof(decimal)));
        dt.Columns.Add(NewDataColumn("DriversLicenseNumber", typeof(string)));

        return dt;
    }
    private DataColumn NewDataColumn(string columnName, Type type)
    {
        DataColumn dc = new()
        {
            ColumnName = columnName,
            DataType = type,
            DefaultValue = null
        };
        
        return dc;
    }
}
