using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using System.Xml;
using u24628299_Ass3.Models;
using u24628299_Ass3.Repository;

namespace u24628299_Ass3.Controllers
{
    public class ReportController : Controller
    {
        private BikeStoresEntities db = new BikeStoresEntities();
        private readonly ReportRepository repo = new ReportRepository();

        // Stock Items Report: Products in stock but never sold
        public ActionResult StockItemsReport()
        {
            var model = new ReportViewModel();
            model.StockItems = repo.GetUnsoldStockItems();
            ViewBag.StockItemsJson = JsonConvert.SerializeObject(model.StockItems);
            return View(model);
        }

        public ActionResult Report(int? customerId, string searchTerm)
        {
            var model = new ReportViewModel();

            // Get all unsold stock items
            model.StockItems = repo.GetUnsoldStockItems();

            // Filter customers by search term if provided
            var customersQuery = db.customers.AsQueryable();
            if (!string.IsNullOrEmpty(searchTerm))
            {
                customersQuery = customersQuery.Where(c =>
                    c.first_name.Contains(searchTerm) ||
                    c.last_name.Contains(searchTerm));
            }

            model.Customers = customersQuery
                .Select(c => new SelectedListItem
                {
                    Value = c.customer_id,
                    Text = c.first_name + " " + c.last_name
                }).ToList();

            ViewBag.StockItemsJson = JsonConvert.SerializeObject(model.StockItems);

            ViewBag.SearchTerm = searchTerm; // For repopulating the search box

            // Load saved reports for archive
            model.SavedReports = GetSavedReports();

            return View(model);
        }

        public ContentResult DataPoints(int count = 10, string type = "json")
        {
            // Generate random data points for demonstration
            var random = new Random();
            var dataPoints = new List<DataPoint>();
            for (int i = 1; i <= count; i++)
            {
                // X: i, Y: random value between 10 and 100
                dataPoints.Add(new DataPoint(i, random.Next(10, 101)));
            }
            _dataPoints = dataPoints;

            switch (type.ToLower())
            {
                case "json":
                    return Content(JsonConvert.SerializeObject(_dataPoints, _jsonSetting), "application/json");

                case "xml":
                    XmlDocument doc = new XmlDocument();
                    XmlDeclaration docNode = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
                    doc.AppendChild(docNode);

                    XmlElement data = (XmlElement)doc.AppendChild(doc.CreateElement("data"));

                    foreach (DataPoint DataPoint in _dataPoints)
                    {
                        XmlElement dataPointNode = doc.CreateElement("point");
                        XmlNode xNode = doc.CreateElement("x");
                        xNode.AppendChild(doc.CreateTextNode(DataPoint.X.ToString()));
                        dataPointNode.AppendChild(xNode);
                        XmlNode yNode = doc.CreateElement("y");
                        yNode.AppendChild(doc.CreateTextNode(DataPoint.Y.ToString()));
                        dataPointNode.AppendChild(yNode);
                        data.AppendChild(dataPointNode);
                    }

                    var xmlString = doc.OuterXml;
                    return Content(xmlString, "text/xml");

                case "csv":
                    string csv = "";
                    foreach (DataPoint DataPoint in _dataPoints)
                        csv += DataPoint.X.ToString() + "," + DataPoint.Y.ToString() + "\n";
                    return Content(csv, "text/csv");

                default:
                    return Content(JsonConvert.SerializeObject(_dataPoints, _jsonSetting), "application/json");
            }
        }

        private List<SavedReport> GetSavedReports()
        {
            var dir = Server.MapPath("~/App_Data/Reports/");
            if (!Directory.Exists(dir))
                return new List<SavedReport>();

            return Directory.GetFiles(dir)
                .Where(f => !f.EndsWith(".desc.txt"))
                .Select(path => new FileInfo(path))
                .Select(fileInfo => new SavedReport
                {
                    FileName = fileInfo.Name,
                    FileType = fileInfo.Extension.TrimStart('.').ToUpperInvariant(),
                    SavedOn = fileInfo.LastWriteTime,
                    DateCreated = fileInfo.CreationTime,
                    Description = System.IO.File.Exists(Path.Combine(dir, fileInfo.Name + ".desc.txt"))
                        ? System.IO.File.ReadAllText(Path.Combine(dir, fileInfo.Name + ".desc.txt"))
                        : ""
                })
                .ToList();
        }

        JsonSerializerSettings _jsonSetting = new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore };
        private static List<DataPoint> _dataPoints = new List<DataPoint>();

        [HttpPost]
        public ActionResult GenerateReport(string Filename, string FileType, string ChartData)
        {
            if (string.IsNullOrWhiteSpace(Filename) || string.IsNullOrWhiteSpace(FileType))
            {
                TempData["Error"] = "Filename and file type are required.";
                return RedirectToAction("Report");
            }

            // Ensure that the file directory exists
            var dir = Server.MapPath("~/App_Data/Reports/");
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            // Sanitise the actual filename
            var safeFilename = Path.GetFileNameWithoutExtension(Filename);
            var ext = FileType.ToLower() == "pdf" ? ".pdf" : ".xlsx";
            var filePath = Path.Combine(dir, safeFilename + ext);

            // Parse chart image from ChartData (as JSON with base64 image - deserialization)
            string stockChartBase64 = null;
            if (!string.IsNullOrEmpty(ChartData))
            {
                try
                {
                    var chartObj = Newtonsoft.Json.JsonConvert.DeserializeObject<ChartDataModel>(ChartData);
                    stockChartBase64 = chartObj?.stockChart;
                }
                catch { }
            }

            if (FileType.ToLower() == "pdf")
            {
                // Save as PDF (simple image PDF)
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    var writer = new iText.Kernel.Pdf.PdfWriter(stream);
                    var pdf = new iText.Kernel.Pdf.PdfDocument(writer);
                    var doc = new iText.Layout.Document(pdf);

                    var boldFont = iText.IO.Font.Constants.StandardFonts.HELVETICA_BOLD;
                    var font = iText.Kernel.Font.PdfFontFactory.CreateFont(boldFont);
                    doc.Add(new iText.Layout.Element.Paragraph("Stock Items Report").SetFont(font).SetFontSize(16));
                    if (!string.IsNullOrEmpty(stockChartBase64))
                    {
                        var imgBytes = ConvertBase64ToBytes(stockChartBase64);
                        if (imgBytes != null)
                        {
                            var imgData = iText.IO.Image.ImageDataFactory.Create(imgBytes);
                            var img = new iText.Layout.Element.Image(imgData).SetAutoScale(true);
                            doc.Add(img);
                        }
                    }
                    doc.Close();
                }
            }
            else // using XLSX
            {
                using (var workbook = new ClosedXML.Excel.XLWorkbook())
                {
                    var ws = workbook.Worksheets.Add("Report");
                    ws.Cell(1, 1).Value = "Stock Items Report";

                    // Add chart image if available
                    if (!string.IsNullOrEmpty(stockChartBase64))
                    {
                        var imgBytes = ConvertBase64ToBytes(stockChartBase64);
                        if (imgBytes != null)
                        {
                            using (var ms = new MemoryStream(imgBytes))
                            {
                                var picture = ws.AddPicture(ms)
                                    .MoveTo(ws.Cell(4, 1))
                                    .WithSize(600, 300);
                            }
                        }
                    }

                    workbook.SaveAs(filePath);
                }
            }

            TempData["Success"] = "Report saved successfully!";
            return RedirectToAction("Report");
        }

        // Method to download a saved report file
        public ActionResult DownloadReport(string reportName)
        {
            if (string.IsNullOrEmpty(reportName))
                return HttpNotFound();

            var dir = Server.MapPath("~/App_Data/Reports/");
            var filePath = Path.Combine(dir, reportName);

            if (!System.IO.File.Exists(filePath))
                return HttpNotFound();

            var fileBytes = System.IO.File.ReadAllBytes(filePath);
            var fileType = Path.GetExtension(reportName).ToLower();
            string contentType = "application/octet-stream";
            if (fileType == ".pdf") contentType = "application/pdf";
            if (fileType == ".xlsx") contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

            return File(fileBytes, contentType, reportName);
        }

        [HttpPost]
        public ActionResult DeleteReport(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return RedirectToAction("Report");

            var dir = Server.MapPath("~/App_Data/Reports/");
            var filePath = Path.Combine(dir, fileName);

            if (System.IO.File.Exists(filePath))
                System.IO.File.Delete(filePath);

            TempData["Success"] = "Report deleted successfully!";
            return RedirectToAction("Report");
        }

        // Helper for deserializing chart data JSON
        private class ChartDataModel
        {
            public string stockChart { get; set; }
        }

        // Helper for converting base64 string to byte array
        private byte[] ConvertBase64ToBytes(string base64)
        {
            if (string.IsNullOrEmpty(base64)) return null;
            var match = System.Text.RegularExpressions.Regex.Match(base64, @"data:image/(?<type>.+?),(?<data>.+)");
            var data = match.Success ? match.Groups["data"].Value : base64;
            try
            {
                return Convert.FromBase64String(data);
            }
            catch
            {
                return null;
            }
        }
    }
}