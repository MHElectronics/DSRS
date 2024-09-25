using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Syncfusion.Pdf.Graphics;
using Syncfusion.Pdf.Grid;
using Syncfusion.Pdf;
using System.Text;
using Syncfusion.Drawing;
using Syncfusion.HtmlConverter;

namespace Services.Helpers;
public static class ExportHelper
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

    //Export weather data to PDF document.
    public static MemoryStream CreatePdf<T>(List<T> data)
    {
        if (data == null)
        {
            throw new ArgumentNullException("Forecast cannot be null");
        }
        //Create a new PDF document.
        using (PdfDocument pdfDocument = new PdfDocument())
        {
            int paragraphAfterSpacing = 8;
            int cellMargin = 8;
            //Add page to the PDF document.
            PdfPage page = pdfDocument.Pages.Add();
            page.Rotation = PdfPageRotateAngle.RotateAngle90;
            //Create a new font.
            PdfStandardFont font = new PdfStandardFont(PdfFontFamily.TimesRoman, 16);

            //Create a text element to draw a text in PDF page.
            PdfTextElement title = new PdfTextElement("Weather Forecast", font, PdfBrushes.Black);
            PdfLayoutResult result = title.Draw(page, new PointF(0, 0));
            PdfStandardFont contentFont = new PdfStandardFont(PdfFontFamily.TimesRoman, 12);
            PdfTextElement content = new PdfTextElement("This component demonstrates fetching data from a service and Exporting the data to PDF document using Syncfusion .NET PDF library.", contentFont, PdfBrushes.Black);
            PdfLayoutFormat format = new PdfLayoutFormat();
            format.Layout = PdfLayoutType.Paginate;
            //Draw a text to the PDF document.
            result = content.Draw(page, new RectangleF(0, result.Bounds.Bottom + paragraphAfterSpacing, page.GetClientSize().Width, page.GetClientSize().Height), format);

            //Create a PdfGrid.
            PdfGrid pdfGrid = new PdfGrid();
            pdfGrid.Style.CellPadding.Left = cellMargin;
            pdfGrid.Style.CellPadding.Right = cellMargin;
            //Applying built-in style to the PDF grid.
            pdfGrid.ApplyBuiltinStyle(PdfGridBuiltinStyle.GridTable4Accent1);

            //Assign data source.
            pdfGrid.DataSource = data;
            pdfGrid.Style.Font = contentFont;
            //Draw PDF grid into the PDF page.
            pdfGrid.Draw(page, new PointF(0, result.Bounds.Bottom + paragraphAfterSpacing));

            using (MemoryStream stream = new MemoryStream())
            {
                //Saving the PDF document into the stream.
                pdfDocument.Save(stream);
                //Closing the PDF document.
                pdfDocument.Close(true);
                return stream;
            }
        }
    }

    public static MemoryStream CreatePdfFromHtml(string html)
    {
        //Initialize HTML to PDF converter.
        HtmlToPdfConverter htmlConverter = new HtmlToPdfConverter();
        string baseUrl = @"D:/Projects/AxleLoadSystemSolution/AxleLoadSystem/AxleLoadSystem/wwwroot/";
        string htmlPage = WrapHtmlInPage(html);
        //Convert URL to PDF document.
        PdfDocument document = htmlConverter.Convert(htmlPage, baseUrl);
        //Create memory stream.
        MemoryStream stream = new MemoryStream();
        //Save the document to memory stream.
        document.Save(stream);
        return stream;
    }
    private static string WrapHtmlInPage(string html)
    {
        string start = @"<!DOCTYPE html>
<html lang=""en"">

<head>
    <meta charset=""utf-8"" name=""view-transition"" content=""same-origin"" />

    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"" />
    <base href=""/"" />

    <!-- Google Font: Source Sans Pro -->
    <link rel=""stylesheet"" href=""https://fonts.googleapis.com/css?family=Source+Sans+Pro:300,400,400i,700&display=fallback"">
    <!-- Font Awesome -->
    <link rel=""stylesheet"" href=""plugins/fontawesome-free/css/all.min.css"">
    <!-- Tempusdominus Bootstrap 4 -->
    <link rel=""stylesheet"" href=""plugins/tempusdominus-bootstrap-4/css/tempusdominus-bootstrap-4.min.css"">
    <!-- iCheck -->
    <link rel=""stylesheet"" href=""plugins/icheck-bootstrap/icheck-bootstrap.min.css"">
    <!-- Theme style -->
    <link rel=""stylesheet"" href=""dist/css/adminlte.min.css"">
    <!-- overlayScrollbars -->
    <link rel=""stylesheet"" href=""plugins/overlayScrollbars/css/OverlayScrollbars.min.css"">
    <!-- Daterange picker -->
    <link rel=""stylesheet"" href=""plugins/daterangepicker/daterangepicker.css"">
    <!-- summernote -->
    <link rel=""stylesheet"" href=""plugins/summernote/summernote-bs4.min.css"">
    <link href=""_content/Syncfusion.Blazor.Themes/bootstrap5.css"" rel=""stylesheet"" />

    <link rel=""stylesheet"" href=""AxleLoadSystem.styles.css"" />
    <!-- Customization-->
    <link rel=""stylesheet"" href=""css/style.css"" />
</head>

<body class=""sidebar-mini layout-fixed"">";


        string end = "</body></html>";

        return start + html + end;
    }
}
