
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Helpers;
public class ExcelHelper
{
    public static MemoryStream CreateSpreadsheetWorkbook()
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
        String[] headerNames = ["Name", "Age"];
        Cell[] headerCells = new Cell[headerNames.Length];

        for(int i=0; i<headerNames.Length; i++)
        {
            headerCells[i] = new Cell();
            headerCells[i].DataType = CellValues.String;
            headerCells[i].CellValue = new CellValue(headerNames[i]);
            headerRow.Append(headerCells[i]);
        }
        sheetData.Append(headerRow);

        //Add the data rows
        Row dataRow = new Row();
        Cell[] dataCells = new Cell[2];
        
        dataCells[0] = new Cell();
        dataCells[0].DataType = CellValues.String;
        dataCells[0].CellValue = new CellValue("Test 1");
        dataRow.Append(dataCells[0]);

        dataCells[1] = new Cell();
        dataCells[1].DataType = CellValues.String;
        dataCells[1].CellValue = new CellValue("Test 2");
        dataRow.Append(dataCells[1]);

        sheetData.Append(dataRow);


        // Add Sheets to the Workbook.
        Sheets sheets = workbookPart.Workbook.AppendChild(new Sheets());

        // Append a new worksheet and associate it with the workbook.
        Sheet sheet = new Sheet() { Id = workbookPart.GetIdOfPart(worksheetPart), SheetId = 1, Name = "mySheet" };
        sheets.Append(sheet);

        workbookPart.Workbook.Save();
        spreadsheetDocument.Save();

        // Dispose the document.
        spreadsheetDocument.Dispose();

        return memoryStream;
    }
}
