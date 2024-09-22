using BOL;
using Microsoft.VisualBasic.FileIO;
using System.Data;
using System.Text;
using System.Text.RegularExpressions;

namespace Services.Helpers;

public interface ICsvHelper
{
    (bool isSuccess, DataTable csvData, string summary) GetDataTableFromByte(byte[] bytes, UploadedFile file);
}

public class CsvHelper : ICsvHelper
{
    public (bool isSuccess, DataTable csvData, string summary) GetDataTableFromByte(byte[] bytes, UploadedFile file)
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

            if (file.FileType == (int)UploadedFileType.FineData)
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
                string csvLineString = csvReader.ReadLine() ?? string.Empty;
                //Remove space and commas
                csvLineString = csvLineString.Replace(" ", "").Replace(",", "");
                if (string.IsNullOrEmpty(csvLineString))
                {
                    summary += row + "-Empty Row,";
                }
                else
                {
                    string[] csvFieldData = csvLineString.Split(','); //csvReader.ReadFields() ?? [];
                    bool requiredFieldsValid = true;
                    if (csvFieldData is not null)
                    {
                        if (string.IsNullOrEmpty(csvFieldData[0]) || string.IsNullOrEmpty(csvFieldData[2]))
                        {
                            requiredFieldsValid = false;
                            summary += row + "-Missing Required field,";
                        }
                    }

                    //Check required
                    //List<int> requiredIndexes = [1];
                    //foreach (int i in requiredIndexes)
                    //{
                    //    if (string.IsNullOrEmpty(fieldData[i]))
                    //    {
                    //        summary += row + "-Required (" + row + "," + (i - 1) + ")";
                    //        requiredFieldsValid = false;
                    //        break;
                    //    }
                    //}

                    //Adjust boolean fields if required fields are valid
                    if (requiredFieldsValid)
                    {
                        string[] fieldData = [file.Id.ToString()];
                        fieldData = fieldData.Concat(csvFieldData).ToArray();

                        List<int> boolenIndexes = [5];
                        if (file.FileType == (int)UploadedFileType.LoadData)
                        {
                            boolenIndexes = [19, 20, 21, 25];
                        }

                        foreach (int i in boolenIndexes)
                        {
                            fieldData[i] = fieldData[i] == "1" ? "true" : "false";
                        }

                        try
                        {
                            csvData.Rows.Add(fieldData);
                        }
                        catch (Exception ex)
                        {
                            if (ex.Message.Contains("unique"))
                            {
                                summary += row + "-Duplicate";
                            }
                            else
                            {
                                summary += row + "-Parse error,";
                            }
                            //summary += row + "-" + ex.Message + ",";
                        }
                    }
                }
                
                row++;
            }

            if (csvData.Rows.Count == 0)
            {
                isSuccess = false;
                summary = "No Data Found";
            }
            else if (!string.IsNullOrEmpty(summary))
            {
                summary = "Error in line: " + summary.TrimEnd(',');
            }
        }
        catch (Exception ex)
        {
            isSuccess = false;
            summary += "Exception: " + ex.Message;
        }

        return (isSuccess, csvData, summary);
    }
    private DataTable GetNewDataTableAxleLoad()
    {
        DataTable dt = new();
        //Default FileId column
        dt.Columns.Add("FileId", typeof(uint));

        dt.Columns.Add(NewDataColumn("TransactionNumber", typeof(string), 10));
        dt.Columns.Add(NewDataColumn("LaneNumber", typeof(uint)));
        dt.Columns.Add(NewDataColumn("DateTime", typeof(DateTime)));
        dt.Columns.Add(NewDataColumn("PlateZone", typeof(string), 50));
        dt.Columns.Add(NewDataColumn("PlateSeries", typeof(string), 10));
        dt.Columns.Add(NewDataColumn("PlateNumber", typeof(string), 10));
        dt.Columns.Add(NewDataColumn("VehicleId", typeof(string), 50));
        dt.Columns.Add(NewDataColumn("NumberOfAxle", typeof(uint)));
        dt.Columns.Add(NewDataColumn("VehicleSpeed", typeof(uint)));
        dt.Columns.Add(NewDataColumn("Axle1", typeof(uint)));
        dt.Columns.Add(NewDataColumn("Axle2", typeof(uint)));
        dt.Columns.Add(NewDataColumn("Axle3", typeof(uint)));
        dt.Columns.Add(NewDataColumn("Axle4", typeof(uint)));
        dt.Columns.Add(NewDataColumn("Axle5", typeof(uint)));
        dt.Columns.Add(NewDataColumn("Axle6", typeof(uint)));
        dt.Columns.Add(NewDataColumn("Axle7", typeof(uint)));
        dt.Columns.Add(NewDataColumn("AxleRemaining", typeof(uint)));
        dt.Columns.Add(NewDataColumn("GrossVehicleWeight", typeof(uint)));
        dt.Columns.Add(NewDataColumn("IsUnloaded", typeof(bool)));
        dt.Columns.Add(NewDataColumn("IsOverloaded", typeof(bool)));
        dt.Columns.Add(NewDataColumn("OverSizedModified", typeof(bool)));
        dt.Columns.Add(NewDataColumn("Wheelbase", typeof(uint)));
        dt.Columns.Add(NewDataColumn("ClassStatus", typeof(uint)));
        dt.Columns.Add(NewDataColumn("RecognizedBy", typeof(uint)));
        dt.Columns.Add(NewDataColumn("IsBRTAInclude", typeof(bool)));
        dt.Columns.Add(NewDataColumn("LadenWeight", typeof(uint)));
        dt.Columns.Add(NewDataColumn("UnladenWeight", typeof(uint)));
        dt.Columns.Add(NewDataColumn("ReceiptNumber", typeof(string), 10));
        dt.Columns.Add(NewDataColumn("BillNumber", typeof(string), 10));
        dt.Columns.Add(NewDataColumn("Axle1Time", typeof(DateTime)));
        dt.Columns.Add(NewDataColumn("Axle2Time", typeof(DateTime)));
        dt.Columns.Add(NewDataColumn("Axle3Time", typeof(DateTime)));
        dt.Columns.Add(NewDataColumn("Axle4Time", typeof(DateTime)));
        dt.Columns.Add(NewDataColumn("Axle5Time", typeof(DateTime)));
        dt.Columns.Add(NewDataColumn("Axle6Time", typeof(DateTime)));
        dt.Columns.Add(NewDataColumn("Axle7Time", typeof(DateTime)));

        // unique contrain
        UniqueConstraint uniqueTransaction = new UniqueConstraint(new DataColumn[] { dt.Columns["TransactionNumber"] });
        UniqueConstraint uniqueComposit = new UniqueConstraint(new DataColumn[] { dt.Columns["TransactionNumber"], dt.Columns["LaneNumber"], dt.Columns["DateTime"] });
        // add unique constraint to the list of constraints for your DataTable
        dt.Constraints.Add(uniqueTransaction);
        dt.Constraints.Add(uniqueComposit);

        return dt;
    }
    private DataTable GetNewDataTableFinePayment()
    {
        DataTable dt = new();
        //Default FileId column
        dt.Columns.Add("FileId", typeof(int));
        dt.Columns.Add(NewDataColumn("LaneNumber", typeof(uint)));
        dt.Columns.Add(NewDataColumn("TransactionNumber", typeof(string), 10));
        dt.Columns.Add(NewDataColumn("PaymentTransactionId", typeof(string), 10));
        dt.Columns.Add(NewDataColumn("DateTime", typeof(DateTime)));
        dt.Columns.Add(NewDataColumn("IsPaid", typeof(bool)));
        dt.Columns.Add(NewDataColumn("FineAmount", typeof(decimal)));
        dt.Columns.Add(NewDataColumn("PaymentMethod", typeof(string), 10));
        dt.Columns.Add(NewDataColumn("ReceiptNumber", typeof(string), 10));
        dt.Columns.Add(NewDataColumn("BillNumber", typeof(string), 10));
        dt.Columns.Add(NewDataColumn("WarehouseCharge", typeof(decimal)));
        dt.Columns.Add(NewDataColumn("DriversLicenseNumber", typeof(string), 15));
        dt.Columns.Add(NewDataColumn("TransportAgencyInformation", typeof(string), 50));

        // unique contrain
        UniqueConstraint uniqueTransaction = new UniqueConstraint(new DataColumn[] { dt.Columns["TransactionNumber"] });
        // add unique constraint to the list of constraints for your DataTable
        dt.Constraints.Add(uniqueTransaction);

        return dt;
    }
    private DataColumn NewDataColumn(string columnName, Type type, int maxStringLength = 0)
    {
        DataColumn dc = new()
        {
            ColumnName = columnName,
            DataType = type,
            DefaultValue = null
        };

        if (type == typeof(string))
        {
            dc.MaxLength = maxStringLength;
        }

        return dc;
    }

    public string CheckRegexForAxleLoad()
    {
        string strRegex = "^";

        //Transaction Number, alphanumaric, length 1 to 2
        strRegex += "\\b[a-zA-Z0-9]{2,10}\\b+";
        //Lane number
        strRegex += "[,][0-9]";
        //Date
        //strRegex += ",[0-9]+{2}";
        ////PlateZone
        //strRegex += "[,][a-zA-Z0-9]+{50}";
        ////PlaceSeries
        //strRegex += "[,][a-zA-Z0-9]+{10}";
        ////PlaceNumber
        //strRegex += "[,][a-zA-Z0-9]+{10}";
        ////VehicleId", typeof(string), 50));
        //strRegex += "[,][a-zA-Z0-9]+{50}";
        ////NumberOfAxle", typeof(int)));
        //strRegex += "[,][0-9]+{2}";
        ////VehicleSpeed", typeof(decimal)));
        //strRegex += "[,][0-9]+{3}";
        ////Axle 1 to 7, Remaining, gross wieght typeof(decimal)));
        //for(int i = 1; i <= 9; i++)
        //{
        //    strRegex += "[,][0-9]+{3}";
        //}
        ////IsUnloaded", typeof(bool)));
        //strRegex += "[,][0-1]+{1}";
        ////IsOverloaded", typeof(bool)));
        //strRegex += "[,][0-1]+{1}";
        ////OverSizedModified", typeof(bool)));
        //strRegex += "[,][0-1]+{1}";
        ////Wheelbase", typeof(int)));
        //strRegex += "[,][0-9]+{2}";
        ////ReceiptNumber", typeof(string), 10));
        //strRegex += "[,][a-zA-Z0-9]+{10}";
        ////BillNumber", typeof(string), 10));
        //strRegex += "[,][a-zA-Z0-9]+{10}";
        //Axle1Time", typeof(DateTime)));
        //Axle2Time", typeof(DateTime)));
        //Axle3Time", typeof(DateTime)));
        //Axle4Time", typeof(DateTime)));
        //Axle5Time", typeof(DateTime)));
        //Axle6Time", typeof(DateTime)));
        //Axle7Time", typeof(DateTime)));

        //End of line
        //strRegex += "$";
        return strRegex;
        //return new Regex(strRegex).IsMatch(line);
    }
}
