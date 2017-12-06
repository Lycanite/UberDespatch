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


		public void ConvertToPDF(string html, string outputPath, string copies = "1", string zoom = "1", string orientation = "Portrait", string options = "") {
			PdfConvert.ConvertHtmlToPdf(
				new PdfDocument
				{
					Url = "-",
					Html = html,
					HeaderLeft = "",
					HeaderRight = "",
					FooterCenter = "",
					ExtraParams = new Dictionary<string, string> () {
						{"disable-javascript", ""},
						{"disable-smart-shrinking", ""},
						{"images", ""},
						{"viewport-size", "800x600"},
						{"dpi", "72"},
						{"encoding", "'utf-8'"},
						{"copies", copies},
						{"zoom", zoom},
						{"orientation", orientation},
						{"quiet", " " + options}
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
