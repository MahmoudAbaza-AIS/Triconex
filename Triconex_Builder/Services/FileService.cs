using ClosedXML.Excel;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using TriconexMigration.Application.Models;
using TriconexMigration.Application.Utilities;

namespace TriconexMigration.Application.Services;

public class FileService(NetworkService networkService, FunctionService functionService, FunctionBlockService functionBlockService)
{
    private string originalFilePath = "";
    private readonly NetworkService networkService = networkService;
    private readonly FunctionService functionService = functionService;
    private readonly FunctionBlockService functionBlockService = functionBlockService;

    public List<string> GetFile(string filePath)
    {
        originalFilePath = filePath;
        FileUtilities.InitializeFC();
        StringBuilder fileText = new();
        try
        {
            using StreamReader reader = new(filePath);
            string line;
            while ((line = reader.ReadLine()) != null)
                fileText.AppendLine(line);
        }
        catch (IOException e)
        {
            return ["The file could not be read:\n" + e.Message];
        }

        var DataCleansing = Regex.Replace(fileText.ToString(), @"\t", " ");
        var FileData = DataCleansing.Split(['\r', '\n',])
                        .Where(s => !string.IsNullOrEmpty(s))
                        .ToList();

        return FileData;
    }

    public void CreateFile(ProgramBuilder programBuilder)
    {
        var FunctionResult = functionService.GetFinalParser(programBuilder.Functions);
        var FunctionBlockResult = functionBlockService.GetFinalParser(programBuilder.FunctionBlocks);
        var StandaloneNetworkResult = networkService.GetFinalParser(programBuilder.StandaloneNetwork ?? [], "");

        var date = $"{DateTimeOffset.Now.DayOfWeek}-{DateTimeOffset.Now:yyyy-MM-dd_HH-mm-ss-fff}";
        var filePath = FileUtilities.SaveFileInDocuments("TriconexMigration", $"ParsedOn_{date}.xlsx");
        var workbook = new XLWorkbook();

        if (programBuilder.FunctionBlocks?.Count > 0)
        {
            var worksheet = workbook.Worksheets.Add("Function Blocks");
            worksheet.Cell(1, 1).Value = "Logic";
            worksheet.Cell(1, 2).Value = "Network Number";
            worksheet.Cell(1, 3).Value = "Function Block";
            worksheet.Cell(1, 4).Value = "Inputs";
            worksheet.Cell(1, 5).Value = "Status";

            int cellNum = 2;
            foreach (var item in FunctionBlockResult)
            {
                worksheet.Cell(cellNum, 1).Value = item.Logic;
                worksheet.Cell(cellNum, 2).Value = item.NetworkNumber;
                worksheet.Cell(cellNum, 3).Value = item.FunctionBlock;
                worksheet.Cell(cellNum, 4).Value = FileUtilities.GetInputs(item.Logic);
                worksheet.Cell(cellNum, 5).Value = !string.IsNullOrWhiteSpace(item.FunctionBlock) ? "Passed" : "Failed, Requires Manual Work";
                cellNum++;
            }
        }


        if (programBuilder.Functions?.Count > 0)
        {
            var worksheet = workbook.Worksheets.Add("Functions");
            worksheet.Cell(1, 1).Value = "Logic";
            worksheet.Cell(1, 2).Value = "Network Number";
            worksheet.Cell(1, 3).Value = "Reference";
            worksheet.Cell(1, 4).Value = "Inputs";
            worksheet.Cell(1, 5).Value = "Status";

            int cellNum = 2;
            foreach (var item in FunctionResult)
            {
                var reference = FileUtilities.FindFCForMemoryAddress(originalFilePath, item.Logic.Contains("NOP") ? item.Logic.Split("NOP")[0] : item.Logic.Split("=")[0]);
                worksheet.Cell(cellNum, 1).Value = item.Logic;
                worksheet.Cell(cellNum, 2).Value = item.NetworkNumber;
                worksheet.Cell(cellNum, 3).Value = reference;
                worksheet.Cell(cellNum, 4).Value = FileUtilities.GetInputs(item.Logic);
                worksheet.Cell(cellNum, 5).Value = !reference.Equals("Unclear", StringComparison.CurrentCultureIgnoreCase) ? "Passed" : "Failed, Requires Manual Work";

                cellNum++;
            }
        }


        if ((programBuilder.StandaloneNetwork?.Count ?? 0) > 0)
        {
            var worksheet = workbook.Worksheets.Add("Standalone Network");
            worksheet.Cell(1, 1).Value = "Logic";
            worksheet.Cell(1, 3).Value = "Inputs";

            int cellNum = 2;
            foreach (var item in StandaloneNetworkResult)
            {
                worksheet.Cell(cellNum, 1).Value = item.Logic;
                worksheet.Cell(cellNum, 2).Value = item.NetworkNumber;
                worksheet.Cell(cellNum, 3).Value = FileUtilities.GetInputs(item.Logic);
                cellNum++;
            }
        }


        foreach (var item in workbook.Worksheets)
        {
            // All Items
            item.Style.Font.FontSize = 20;


            // Heading Items
            var h1 = item.Cell(1, 1);
            var h2 = item.Cell(1, 2);
            var h3 = item.Cell(1, 3);
            var h4 = item.Cell(1, 4);
            var h5 = item.Cell(1, 5);

            h1.Style.Font.FontSize = 30;
            h1.Style.Font.SetBold(true);
            h1.Style.Fill.BackgroundColor = XLColor.LightGreen;

            h2.Style.Font.FontSize = 30;
            h2.Style.Font.SetBold(true);
            h2.Style.Fill.BackgroundColor = XLColor.LightGreen;

            h3.Style.Font.FontSize = 30;
            h3.Style.Font.SetBold(true);
            h3.Style.Fill.BackgroundColor = XLColor.LightGreen;

            h4.Style.Font.FontSize = 30;
            h4.Style.Font.SetBold(true);
            h4.Style.Fill.BackgroundColor = XLColor.LightGreen;

            h5.Style.Font.FontSize = 30;
            h5.Style.Font.SetBold(true);
            h5.Style.Fill.BackgroundColor = XLColor.LightGreen;


            item.Columns().AdjustToContents();
        }

        if (workbook.Worksheets.Count == 0)
        {
            var sheet = workbook.Worksheets.Add("Standalone Network");
            sheet.Cell(1, 1).Value = "Logic";
        }

        workbook.SaveAs(filePath);
        Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true });
    }

    public static List<string> GetFileMock(string fileContent)
    {
        var DataCleansing = Regex.Replace(fileContent.ToString(), @"\t", " ");
        var FileData = DataCleansing.Split(['\r', '\n',])
                        .Where(s => !string.IsNullOrEmpty(s))
                        .ToList();
        return FileData;
    }
}
