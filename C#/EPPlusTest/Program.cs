using System;
using OfficeOpenXml;
using System.IO;

namespace EPPlusTest
{
	class Program
	{
		static void Main(string[] args)
		{
			

			var newFile = new FileInfo("example.xlsx");
			using (var package = new ExcelPackage(newFile)) {
				var worksheet = package.Workbook.Worksheets.Add("Example");

				var boldRichText = worksheet.Cells[1, 1].RichText.Add("Hello");
				boldRichText.Bold = true;

				var normalRichText = worksheet.Cells[1, 1].RichText.Add(" World");
				normalRichText.Bold = false;

				package.Save();
			}
		}
	}
}
