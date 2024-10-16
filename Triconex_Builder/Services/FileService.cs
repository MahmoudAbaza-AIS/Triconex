using ClosedXML.Excel;
using DocumentFormat.OpenXml.Presentation;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Packaging;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Triconex.Models;

namespace Triconex.Services
{
    public static class FileService
    {
        public static List<string> GetFile(string filePath)   
        {
            StringBuilder fileText = new StringBuilder();
            try
            {
                using (StreamReader reader = new StreamReader(filePath))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                        fileText.AppendLine(line);
                }
            }
            catch (IOException e)
            {
                return new List<string> { "The file could not be read:\n" + e.Message };
            }


            var DataCleansing = Regex.Replace(fileText.ToString(), @"\t", " ");
            var FileData = DataCleansing.Split(['\r', '\n',])
                            .Where(s => !string.IsNullOrEmpty(s))
                            .ToList();

            return FileData;
        }

        public static void CreateFile(ProgramBuilder programBuilder)  
        {
            var FunctionResult = FunctionService.GetFinalParser(programBuilder.Functions);
            var FunctionBlockResult = FunctionBlockService.GetFinalParser(programBuilder.FunctionBlocks);
            var StandaloneNetworkResult = NetworkService.GetFinalParser(programBuilder.StandaloneNetwork, "");

            Guid ID = Guid.NewGuid();
            var filePath = $@"C:\Projects\GitHub\Triconex\saved_file\Triconex_Parser_{ID}.xlsx";
            var workbook = new XLWorkbook();
 
            if(programBuilder.FunctionBlocks.Count() > 0)
            {
                var worksheet = workbook.Worksheets.Add("Function Blocks");
                worksheet.Cell(1, 1).Value = "Logic";
                worksheet.Cell(1, 2).Value = "Function Block";
                worksheet.Cell(1, 3).Value = "Network Number";

                int cellNum = 2;    
                foreach (var item in FunctionBlockResult) 
                {
                    worksheet.Cell(cellNum, 1).Value = item.Logic;
                    worksheet.Cell(cellNum, 2).Value = item.FunctionBlock;
                    worksheet.Cell(cellNum, 3).Value = item.NetworkNumber;
                    cellNum++;
                }
            }


            if (programBuilder.Functions.Count() > 0)   
            {
                var worksheet = workbook.Worksheets.Add("Functions");
                worksheet.Cell(1, 1).Value = "Logic";
                worksheet.Cell(1, 2).Value = "Network Number";

                int cellNum = 2;
                foreach (var item in FunctionResult)
                {
                    worksheet.Cell(cellNum, 1).Value = item.Logic;
                    worksheet.Cell(cellNum, 2).Value = item.NetworkNumber;
                    cellNum++;
                }
            }


            if (programBuilder.StandaloneNetwork.Count() > 0)
            {
                var worksheet = workbook.Worksheets.Add("Standalone Network");
                worksheet.Cell(1, 1).Value = "Logic";

                int cellNum = 2;
                foreach (var item in StandaloneNetworkResult)
                {
                    worksheet.Cell(cellNum, 1).Value = item.Logic;
                    worksheet.Cell(cellNum, 2).Value = item.NetworkNumber;
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

                h1.Style.Font.FontSize = 30;
                h1.Style.Font.SetBold(true);
                h1.Style.Fill.BackgroundColor = XLColor.LightGreen;

                h2.Style.Font.FontSize = 30;
                h2.Style.Font.SetBold(true);
                h2.Style.Fill.BackgroundColor = XLColor.LightGreen;

                h3.Style.Font.FontSize = 30;
                h3.Style.Font.SetBold(true);
                h3.Style.Fill.BackgroundColor = XLColor.LightGreen;


                item.Columns().AdjustToContents();
            }

            workbook.SaveAs(filePath); // Save the workbook
            Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true });
        }


























        public static List<string> GetFileMoch(string fileContent)    
        {
            var DataCleansing = Regex.Replace(fileContent.ToString(), @"\t", " ");  
            var FileData = DataCleansing.Split(['\r', '\n',])
                            .Where(s => !string.IsNullOrEmpty(s))
                            .ToList();
            return FileData;
        }
    }
}
