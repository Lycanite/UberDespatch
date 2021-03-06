﻿using System;

namespace UberDespatch
{
	public class PrinterProfile
	{
		public string Name;
		protected string PrinterName;
		//public PrintQueue PrintQueue;
		public int PageWidth = 0;
		public int PageHeight = 0;
		public double ImageScale = 2;


		// ========== Config ==========
		public class ConfigPrinterProfile
		{
			public string ProfileName = "Default";
			public string PrinterName = Printer.DefaultPrinterName;
			public int PageWidth = 0;
			public int PageHeight = 0;
			public double ImageScale = 2;
		}


		// ========== Constructor ==========
		public PrinterProfile(string name)
		{
			this.Name = name;
		}


		// ========== Get Printer Name ==========
		/** Returns the name of the printer used by this profile. **/
		public string GetPrinterName()
		{
			return this.PrinterName;
		}


		// ========== Set Printer Name ==========
		/** Set the name of the printer to be used by this profile. **/
		public void SetPrinterName(string name)
		{
			this.PrinterName = name;
		}


		/*/ ========== Get Print Queue ==========
		public PrintQueue GetPrintQueue()
		{
			return this.PrintQueue != null ? this.PrintQueue : LocalPrintServer.GetDefaultPrintQueue();
		}


		// ========== Set Print Queue ==========
		public void SetPrintQueue(string printQueueName)
		{
			try {
				this.PrintQueue = LocalPrintServer.GetDefaultPrintQueue();
			}
			catch (Exception e) {
				Program.LogError("Printer", "Unable to get the Default Printer, please ensure that this is set on your system.");
				Program.LogException(e);
				return;
			}

			if (printQueueName == Printer.DefaultPrinterName) {
				Program.LogSuccess("Printer", this.Name + " Profile printer has been set to: " + printQueueName + " which is: " + this.PrintQueue.Name);
				return;
			}

			try {
				PrintServer printServer = new LocalPrintServer();
				string printerName = printQueueName;
				if (printQueueName.StartsWith(@"\\", StringComparison.InvariantCulture)) {
					printServer = new PrintServer(@"\\" + printQueueName.Substring(2).Split('\\')[0]);
					printerName = printQueueName.Substring(2).Split('\\')[1];
				}
				this.PrintQueue = printServer.GetPrintQueue(printerName);
				Program.LogSuccess("Printer", this.Name + " Profile printer has been set to: " + this.PrintQueue.Name + (printQueueName != this.PrintQueue.Name ? " (" + printQueueName + ")" : ""));
			}
			catch (Exception e) {
				Program.LogWarning("Printer", this.Name + " Profile printer was not recognised: " + printQueueName + " the system default printer will be used instead which is: " + this.PrintQueue.Name);
				Program.LogException(e);
			}
		}*/
	}
}

