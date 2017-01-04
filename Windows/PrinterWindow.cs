using System;
using System.Drawing.Printing;
using Gtk;

namespace UberDespatch
{
	public partial class PrinterWindow : Gtk.Window
	{
		public PrinterWindow() : base(Gtk.WindowType.Toplevel)
		{
			this.Build();
			this.OnOpen();
		}


		// ========== Open ==========
		protected void OnOpen()
		{
			// Load printers to dropdown:
			this.PrinterSelectionCombo.AppendText(Printer.DefaultPrinterName);
			int currentPrinterIndex = 0;
			int index = 0;
			//foreach (PrintQueue printQueue in LocalPrintServer.GetDefaultPrintQueue ().HostingPrintServer.GetPrintQueues ())
			foreach (string printerName in PrinterSettings.InstalledPrinters)
			{
				index++;
				this.PrinterSelectionCombo.AppendText(printerName);
				if (Program.printer.GetPrinterProfile("Default").GetPrintQueue().FullName == printerName)
					currentPrinterIndex = index;
			}
			TreeIter iter;
			this.PrinterSelectionCombo.Model.IterNthChild(out iter, currentPrinterIndex);
			this.PrinterSelectionCombo.SetActiveIter(iter);
		}


		// ========== Confirm ==========
		protected void OnConfirmButtonReleased(object sender, EventArgs e)
		{
			this.Save ();
			this.Destroy ();
		}


		// ========== Save ==========
		protected void Save()
		{
			PrinterProfile printerProfile = Program.printer.GetPrinterProfile("Default");
			printerProfile.SetPrintQueue (this.PrinterSelectionCombo.ActiveText);
			Program.printer.SaveConfig ();
		}


		// ========== Cancel ==========
		protected void OnCancelButtonReleased(object sender, EventArgs e)
		{
			this.Destroy ();
		}
	}
}

