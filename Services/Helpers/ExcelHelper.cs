
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Text;

namespace Services.Helpers;
public static class ExcelHelper
{
    public static byte[] CreateSpreadsheetWorkbook<T>(List<(string Header, string FieldName, Type FieldType)> fields, List<T> data)
    {
        MemoryStream memoryStream = new MemoryStream();

        // Create a spreadsheet document by supplying the filepath.
        // By default, AutoSave = true, Editable = true, and Type = xlsx.
        SpreadsheetDocument spreadsheetDocument = SpreadsheetDocument.Create(memoryStream, SpreadsheetDocumentType.Workbook);

        // Add a WorkbookPart to the document.
        WorkbookPart workbookPart = spreadsheetDocument.AddWorkbookPart();
        workbookPart.Workbook = new Workbook();

        // Add a WorksheetPart to the WorkbookPart.
        WorksheetPart worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
        SheetData sheetData = new SheetData();
        worksheetPart.Worksheet = new Worksheet(sheetData);

        //Header row
        Row headerRow = new Row();
        Cell[] headerCells = new Cell[fields.Count];

        int i = 0;
        foreach((string Header, string FieldName, Type FieldType) item in fields)
        {
            headerCells[i] = new Cell();
            headerCells[i].DataType = GetCellValuesFromType(item.FieldType);
            headerCells[i].CellValue = new CellValue(item.Header);
            headerRow.Append(headerCells[i]);
            i++;
        }
        //Add header row in sheet
        sheetData.Append(headerRow);

        //Add the data rows
        foreach(T item in data)
        {
            int columnIndex = 0;
            Row dataRow = new Row();
            Cell[] dataCells = new Cell[fields.Count];

            foreach ((string Header, string FieldName, Type FieldType) column in fields)
            {
                dataCells[columnIndex] = new Cell();
                dataCells[columnIndex].DataType = GetCellValuesFromType(column.FieldType);
                //var value = (item as dynamic)[column.FieldName];
                var value2 = item.GetType().GetProperty(column.FieldName).GetValue(item, null);
                dataCells[columnIndex].CellValue = new CellValue(value2.ToString());
                dataRow.Append(dataCells[columnIndex]);
                columnIndex++;
            }
            
            sheetData.Append(dataRow);
        }

        // Add Sheets to the Workbook.
        Sheets sheets = workbookPart.Workbook.AppendChild(new Sheets());

        // Append a new worksheet and associate it with the workbook.
        Sheet sheet = new Sheet() { Id = workbookPart.GetIdOfPart(worksheetPart), SheetId = 1, Name = "Data" };
        sheets.Append(sheet);

        workbookPart.Workbook.Save();
        spreadsheetDocument.WorkbookPart.Workbook.Save();
        spreadsheetDocument.Save();

        // Dispose the document.
        spreadsheetDocument.Dispose();

        return memoryStream.ToArray();
    }

    private static CellValues GetCellValuesFromType(Type type)
    {
        if (type == typeof(int) || type == typeof(decimal) || type == typeof(double))
        {
            return CellValues.Number;
        }
        else if (type == typeof(bool))
        {
            return CellValues.Boolean;
        }
        else if (type == typeof(DateTime) || type == typeof(DateOnly))
        {
            return CellValues.Date;
        }

        return CellValues.String;
    }

    public static string GenerateCSVString<T>(List<(string Header, string FieldName, Type FieldType)> fields, List<T> data)
    {
        StringBuilder sb = new StringBuilder();
        
        string header = "";
        foreach ((string Header, string FieldName, Type FieldType) item in fields)
        {
            header += item.Header + ",";
        }
        //Remove , from end
        header = header.TrimEnd(',');

        //Add header
        sb.AppendLine(header);

        //Add data rows
        foreach (T item in data)
        {
            string row = "";

            foreach ((string Header, string FieldName, Type FieldType) column in fields)
            {
                row += item.GetType().GetProperty(column.FieldName).GetValue(item, null) + ",";
            }
            //Remove , from end
            row = row.TrimEnd(',');

            sb.AppendLine(row);
        }

        return sb.ToString();
    }
    public static MemoryStream GenerateCSVStream<T>(List<(string Header, string FieldName, Type FieldType)> fields, List<T> data)
    {
        string csvString = GenerateCSVString(fields, data);

        MemoryStream stream = new MemoryStream();
        StreamWriter writer = new StreamWriter(stream);
        writer.Write(csvString);
        writer.Flush();
        stream.Position = 0;

        return stream;
    }
}
