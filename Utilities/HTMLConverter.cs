using System;
using System.IO;
using Codaxy.WkHtmlToPdf;
using System.Collections.Generic;

namespace UberDespatch
{
	public class HTMLConverter
	{
		public HTMLConverter()
		{
		}


		public static string WKHTMLToPDFPath()
		{
			if (System.Environment.OSVersion.ToString().ToLower().Contains("windows")) {
				string wkHTMLToPDFPath = Program.ExecutableFolder + Path.DirectorySeparatorChar;
				if (System.Environment.Is64BitOperatingSystem) {
					return wkHTMLToPDFPath + "wkhtmltopdf.exe";
				}
				else {
					return wkHTMLToPDFPath + "wkhtmltopdf32.exe";
				}
			}
			else {
				return "wkhtmltopdf";
			}
		}


		public void ConvertToPDF(string html, string outputPath) {
			PdfConvert.ConvertHtmlToPdf(
				new PdfDocument
				{
					Url = "-",
					Html = html,
					HeaderLeft = "",
					HeaderRight = "",
					FooterCenter = "",
					ExtraParams = new Dictionary<string, string> () {
						{"quiet", ""},
						{"disable-javascript", ""},
						{"disable-smart-shrinking", ""},
						{"images", ""},
						{"zoom", "1.33"},
						{"encoding", "'utf-8'"}
					}
				}, new PdfConvertEnvironment
				{
					TempFolderPath = Path.GetTempPath(),
					WkHtmlToPdfPath = WKHTMLToPDFPath (),
					Timeout = 60000
				}, new PdfOutput
				{
					OutputFilePath = outputPath
				}
			);
		}
	}
}
