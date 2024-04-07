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

                    //if (fieldData[i] == "")
                    //{
                    //    fieldData[i] = null;
                    //}
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

        dt.Columns.Add(NewDataColumn("TransactionNumber", typeof(string)));
        dt.Columns.Add(NewDataColumn("LaneNumber", typeof(int)));
        dt.Columns.Add(NewDataColumn("DateTime", typeof(DateTime)));
        dt.Columns.Add(NewDataColumn("PlateNumber", typeof(string)));
        dt.Columns.Add(NewDataColumn("VehicleId", typeof(string)));
        dt.Columns.Add(NewDataColumn("NumberOfAxle", typeof(int)));
        dt.Columns.Add(NewDataColumn("VehicleSpeed", typeof(decimal)));
        dt.Columns.Add(NewDataColumn("Axle1st", typeof(decimal)));
        dt.Columns.Add(NewDataColumn("Axle2nd", typeof(decimal)));
        dt.Columns.Add(NewDataColumn("Axle3rd", typeof(decimal)));
        dt.Columns.Add(NewDataColumn("Axle4th", typeof(decimal)));
        dt.Columns.Add(NewDataColumn("Axle5th", typeof(decimal)));
        dt.Columns.Add(NewDataColumn("Axle6th", typeof(decimal)));
        dt.Columns.Add(NewDataColumn("Axle7th", typeof(decimal)));
        dt.Columns.Add(NewDataColumn("AxleRemaining", typeof(decimal)));
        dt.Columns.Add(NewDataColumn("GrossVehicleWeight", typeof(decimal)));
        dt.Columns.Add(NewDataColumn("IsUnloaded", typeof(bool)));
        dt.Columns.Add(NewDataColumn("IsOverloaded", typeof(bool)));
        dt.Columns.Add(NewDataColumn("OverSizedModified", typeof(bool)));
        dt.Columns.Add(NewDataColumn("Wheelbase", typeof(int)));
        dt.Columns.Add(NewDataColumn("Axle1stTime", typeof(DateTime)));
        dt.Columns.Add(NewDataColumn("Axle2ndTime", typeof(DateTime)));
        dt.Columns.Add(NewDataColumn("Axle3rdTime", typeof(DateTime)));
        dt.Columns.Add(NewDataColumn("Axle4thTime", typeof(DateTime)));
        dt.Columns.Add(NewDataColumn("Axle5thTime", typeof(DateTime)));
        dt.Columns.Add(NewDataColumn("Axle6thTime", typeof(DateTime)));
        dt.Columns.Add(NewDataColumn("Axle7thTime", typeof(DateTime)));
        
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
        return new()
        {
            ColumnName = columnName,
            DataType = type,
            DefaultValue = null
        };
    }
}
