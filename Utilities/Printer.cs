using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Printing;
using Gtk;
using PdfiumViewer;

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
				printerProfile.SetPrintQueue (printerProfileConfig.PrinterName);
				this.PrinterProfiles.Add (printerProfile.Name, printerProfile);
			}

			// Create Default If Missing:
			if (!hasDefault) {
				PrinterProfile printerProfile = new PrinterProfile("Default");
				printerProfile.SetPrintQueue(DefaultPrinterName);
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
				printerProfileConfig.PrinterName = printerProfile.GetPrintQueue ().FullName;
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
		public void PrintFile(Byte[] data, string printerProfileName = "Default")
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
		}


		// ========== Print PDF ==========
		public void PrintPDF(string filePath, string printerProfileName = "Default")
		{
			PrinterProfile printerProfile = this.GetPrinterProfile(printerProfileName);
			Program.LogAlert("Printer", "Printing PDF label with printer: " + printerProfile.GetPrintQueue().Name + "...");
			Application.Invoke(delegate
			{
				try {
					PdfDocument pdf = PdfDocument.Load(filePath);
					PrintDocument printDoc = pdf.CreatePrintDocument();
					printDoc.PrinterSettings.PrinterName = printerProfile.GetPrintQueue().Name;
					printDoc.Print();
					pdf.Dispose();
				}
				catch (Exception e) {
					Program.LogError ("Printer", "An error occured when attempting to print.");
					Program.LogException(e);
				}
			});
		}


		// ========== Print PNG ==========
		public void PrintPNG(string filePath, string printerProfileName = "Default")
		{
			PrinterProfile printerProfile = this.GetPrinterProfile(printerProfileName);
			Program.LogAlert("Printer", "Printing PNG label with printer: " + printerProfile.GetPrintQueue().Name + "...");
			Application.Invoke(delegate
			{
				try
				{
					PrintDocument printDoc = new PrintDocument();
					printDoc.PrinterSettings.PrinterName = printerProfile.GetPrintQueue().Name;
					printDoc.PrintPage += new PrintPageEventHandler(delegate(object o, PrintPageEventArgs e) {
						System.Drawing.Image img = System.Drawing.Image.FromFile(filePath);
						Point p = new Point(printerProfile.ImageOffsetX, printerProfile.ImageOffsetY);
						e.Graphics.DrawImage(img, p);
					});
					printDoc.Print();
				}
				catch (Exception e)
				{
					Program.LogError("Printer", "An error occured when attempting to print.");
					Program.LogException(e);
				}
			});
		}
	}
}

