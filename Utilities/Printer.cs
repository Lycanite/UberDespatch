using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using Gtk;
using PdfiumViewer;
using System.Diagnostics;

namespace UberDespatch
{
	public class Printer
	{
		public static string DefaultPrinterName = "System Default";
		public Dictionary<string, PrinterProfile> PrinterProfiles = new Dictionary<string, PrinterProfile> ();
		public ConfigPrinter Config;


		// ========== Config ==========
		public class ConfigPrinter : ConfigBase<ConfigPrinter>
		{
			public PrinterProfile.ConfigPrinterProfile[] PrinterProfileEntries = new PrinterProfile.ConfigPrinterProfile[] { new PrinterProfile.ConfigPrinterProfile () };

			public static ConfigPrinter LoadFile()
			{
				return Load("config" + Path.DirectorySeparatorChar + "printer.json");
			}

			public void SaveFile()
			{
				Save("config" + Path.DirectorySeparatorChar + "printer.json");
			}
		}


		// ========== Constructor ==========
		public Printer()
		{
			this.LoadConfig ();
		}


		// ========== Load Config ==========
		public void LoadConfig()
		{
			this.Config = ConfigPrinter.LoadFile();

			// Create Default If Empty:
			if (this.Config.PrinterProfileEntries.Length == 0) {
				PrinterProfile.ConfigPrinterProfile printerProfileConfig = new PrinterProfile.ConfigPrinterProfile();
				this.Config.PrinterProfileEntries = new PrinterProfile.ConfigPrinterProfile[] { printerProfileConfig };
			}

			// Load Profiles:
			bool hasDefault = false;
			foreach (PrinterProfile.ConfigPrinterProfile printerProfileConfig in this.Config.PrinterProfileEntries) {
				if (printerProfileConfig.ProfileName == "Default")
					hasDefault = true;
				PrinterProfile printerProfile = new PrinterProfile(printerProfileConfig.ProfileName);
				//printerProfile.SetPrintQueue (printerProfileConfig.PrinterName);
				printerProfile.SetPrinterName(printerProfileConfig.PrinterName);
				printerProfile.ImageScale = printerProfileConfig.ImageScale;
				printerProfile.PageWidth = printerProfileConfig.PageWidth;
				printerProfile.PageHeight = printerProfileConfig.PageHeight;
				this.PrinterProfiles.Add (printerProfile.Name, printerProfile);
			}

			// Create Default If Missing:
			if (!hasDefault) {
				PrinterProfile printerProfile = new PrinterProfile("Default");
				//printerProfile.SetPrintQueue(DefaultPrinterName);
				printerProfile.SetPrinterName(DefaultPrinterName);
				this.PrinterProfiles.Add(printerProfile.Name, printerProfile);
			}
		}


		// ========== Save Config ==========
		public void SaveConfig()
		{
			List<PrinterProfile.ConfigPrinterProfile> printerProfileConfigs = new List<PrinterProfile.ConfigPrinterProfile> ();
			foreach (PrinterProfile printerProfile in this.PrinterProfiles.Values) {
				PrinterProfile.ConfigPrinterProfile printerProfileConfig = new PrinterProfile.ConfigPrinterProfile();
				printerProfileConfig.ProfileName = printerProfile.Name;
				printerProfileConfig.ImageScale = printerProfile.ImageScale;
				printerProfileConfig.PageWidth = printerProfile.PageWidth;
				printerProfileConfig.PageHeight = printerProfile.PageHeight;
				printerProfileConfig.PrinterName = printerProfile.GetPrinterName();
				printerProfileConfigs.Add (printerProfileConfig);
			}
			this.Config.PrinterProfileEntries = printerProfileConfigs.ToArray ();
			this.Config.SaveFile ();
		}


		// ========== Add Printer Profile ==========
		public void AddPrinterProfile(PrinterProfile profile)
		{
			this.PrinterProfiles.Add(profile.Name, profile);
			this.SaveConfig();
		}


		// ========== Remove Printer Profile ==========
		public void RemovePrinterProfile(string profileName)
		{
			this.PrinterProfiles.Remove (profileName);
			this.SaveConfig();
		}


		// ========== Get Printer Profile ==========
		public PrinterProfile GetPrinterProfile(string printerProfileName = "Default")
		{
			if (!PrinterProfiles.ContainsKey("Default"))
				PrinterProfiles.Add ("Default", new PrinterProfile ("Default"));
			if (printerProfileName == null || !PrinterProfiles.ContainsKey(printerProfileName))
				return PrinterProfiles["Default"];
			return PrinterProfiles[printerProfileName];
		}


		// ========== Print File ==========
		/*public void PrintFile(Byte[] data, string printerProfileName = "Default")
		{
			PrinterProfile printerProfile = this.GetPrinterProfile(printerProfileName);
			Program.LogAlert("Printer", "Printing label with printer: " + printerProfile.GetPrintQueue().Name + "...");
			Application.Invoke(delegate
			{
				using (PrintSystemJobInfo job = printerProfile.GetPrintQueue().AddJob())
				using (Stream stream = job.JobStream)
				{
					stream.Write(data, 0, data.Length);
				}
			});
		}*/


		// ========== Print PDF ==========
		public void PrintPDF(string filePath, string printerProfileName = "Default")
		{
			PrinterProfile printerProfile = this.GetPrinterProfile(printerProfileName);
			Program.LogAlert("Printer", "Printing PDF label with printer: " + printerProfile.GetPrinterName() + "...");
			Application.Invoke(delegate {
				try {
					if (System.Environment.OSVersion.ToString().ToLower().Contains("windows")) {
						PdfDocument pdf = PdfDocument.Load(filePath);
						PrintDocument printDoc = pdf.CreatePrintDocument();
						if (printerProfile.PageWidth > 0 && printerProfile.PageHeight > 0) {
							Program.Log("Printer", "Using custom paper size: " + printerProfile.PageWidth + "x" + printerProfile.PageHeight + " (100th of an inch).");
							printDoc.DefaultPageSettings.PaperSize = new System.Drawing.Printing.PaperSize("Custom Size", printerProfile.PageWidth, printerProfile.PageHeight);
						}
						printDoc.PrinterSettings.PrinterName = printerProfile.GetPrinterName();
						printDoc.Print();
						pdf.Dispose();
					}
					else {
						Process process = new Process();
						process.StartInfo.FileName = "lp";
						if(printerProfile.GetPrinterName() == "System Default")
							process.StartInfo.Arguments = filePath;
						else
							process.StartInfo.Arguments = "-d \"" + printerProfile.GetPrinterName() + "\" \"" + filePath + "\"";
						process.StartInfo.UseShellExecute = false;
						process.StartInfo.RedirectStandardOutput = true;
						process.StartInfo.RedirectStandardError = true;
						process.StartInfo.RedirectStandardInput = true;
						Program.LogAlert("Printer", "Starting Linux Print: " + process.StartInfo.FileName + " " + process.StartInfo.Arguments);
						process.Start();
						process.WaitForExit();
					}
				}
				catch (Exception e) {
					Program.LogError ("Printer", "An error occured when attempting to print PDF.");
					Program.LogException(e);
				}
			});
		}


		// ========== Print PNG ==========
		public void PrintPNG(string filePath, string printerProfileName = "Default")
		{
			PrinterProfile printerProfile = this.GetPrinterProfile(printerProfileName);
			printerProfileName = printerProfile.Name;
			string printerName = printerProfile.GetPrinterName();
			Program.LogAlert("Printer", "Printing PNG label with printer: " + printerName + "...");
			Application.Invoke (delegate
				{
					try
					{
						PrintDocument printDoc = new PrintDocument();
						printDoc.DefaultPageSettings.PrinterSettings.PrinterName = printerName;
						printDoc.DefaultPageSettings.Landscape = false;
						printDoc.PrintPage += new PrintPageEventHandler(delegate(object o, PrintPageEventArgs e) {
							System.Drawing.Image img = System.Drawing.Image.FromFile(filePath);
							Point p = new Point(2, 2);
							e.Graphics.DrawImage(img, p);
						});
						/*printDoc.PrintPage += (sender, args) => {
							double scale = printerProfile.ImageScale;
							System.Drawing.Image img = System.Drawing.Image.FromFile(filePath);
							//Point m = new Point(printerProfile.ImageOffsetX, printerProfile.ImageOffsetY);
							Rectangle m = args.MarginBounds;
							if ((double)img.Width / (double)img.Height > (double)m.Width / (double)m.Height) {
								m.Height = (int)(((double)img.Height / (double)img.Width * (double)m.Width) * scale);
							}
							else {
								m.Width = (int)(((double)img.Width / (double)img.Height * (double)m.Height) * scale);
							}
							args.Graphics.DrawImage(img, m);
						};*/
						printDoc.Print();
					}
					catch (Exception e)
					{
						Program.LogError("Printer", "An error occured when attempting to print PNG. Profile: " + printerProfileName + " Printer Name: " + printerName + " File Path: " + filePath);
						Program.LogException(e);
					}
				}
			);
		}
	}
}

