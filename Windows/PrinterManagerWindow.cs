using System;
using System.Collections.Generic;
using System.Printing;
using Gtk;

namespace UberDespatch
{
	public partial class PrinterManagerWindow : Gtk.Window
	{
		public PrinterProfile SelectedProfile;
		public int NewProfileCount = 0;

		public PrinterManagerWindow() : base(Gtk.WindowType.Toplevel)
		{
			this.Build ();
			this.OnOpen ();
		}


		// ========== Open ==========
		protected void OnOpen()
		{
			try {
				this.LoadProfiles();
				this.SelectProfile("Default");
			}
			catch (Exception e) {
				Program.LogError ("Printer", "A problem occured when trying to load printers into the Printer Manager.");
				Program.LogException (e);
			}
		}


		// ========== Select Profile ==========
		protected void SelectProfile(string profileName)
		{
			this.SelectedProfile = Program.printer.GetPrinterProfile (profileName);
			this.PrinterProfileNameEntry.Text = this.SelectedProfile.Name;
			this.PrinterProfileNameEntry.IsEditable = this.SelectedProfile.Name != "Default";
			this.LoadPrinters ();
		}


		// ========== Load Profiles ==========
		protected void LoadProfiles()
		{
			if (this.SelectedProfile == null)
				this.SelectedProfile = Program.printer.GetPrinterProfile("Default");
			this.PrinterProfileSelectionCombo.Model = new ListStore(typeof(string), typeof(string));
			this.PrinterProfileSelectionCombo.AppendText("Default");
			foreach (string profileName in Program.printer.PrinterProfiles.Keys) {
				if (profileName != "Default")
					this.PrinterProfileSelectionCombo.AppendText(profileName);
			}
			TreeIter iter;
			this.PrinterProfileSelectionCombo.Model.IterNthChild(out iter, 0);
			this.PrinterProfileSelectionCombo.SetActiveIter(iter);
		}


		// ========== Load Printers ==========
		protected void LoadPrinters()
		{
			try
			{
				this.PrinterProfilePrinterCombo.Model = new ListStore(typeof(string), typeof(string));
				this.PrinterProfilePrinterCombo.AppendText(Printer.DefaultPrinterName);
				int currentPrinterIndex = 0;
				int index = 0;
				foreach (PrintQueue printQueue in LocalPrintServer.GetDefaultPrintQueue().HostingPrintServer.GetPrintQueues())
				{
					index++;
					this.PrinterProfilePrinterCombo.AppendText(printQueue.FullName);
					if (this.SelectedProfile.GetPrintQueue().FullName == printQueue.FullName)
						currentPrinterIndex = index;
				}
				TreeIter iter;
				this.PrinterProfilePrinterCombo.Model.IterNthChild(out iter, currentPrinterIndex);
				this.PrinterProfilePrinterCombo.SetActiveIter(iter);
			}
			catch (Exception e)
			{
				Program.LogError ("Printer", "A problem occured when trying to load a list of printers.");
				Program.LogException (e);
			}
		}


		// ========== Profile Changed ==========
		protected void OnProfileComboChanged(object sender, EventArgs e)
		{
			this.SelectProfile (this.PrinterProfileSelectionCombo.ActiveText);
		}


		// ========== Add Profile ==========
		protected void OnPrinterProfileAddReleased(object sender, EventArgs e)
		{
			try {
				PrinterProfile newProfile = new PrinterProfile ("New Profile " + ++this.NewProfileCount);
				Program.printer.AddPrinterProfile (newProfile);
				this.PrinterProfileSelectionCombo.AppendText (newProfile.Name);
				this.SelectProfile (newProfile.Name);
				TreeIter iter;
				this.PrinterProfileSelectionCombo.Model.IterNthChild(out iter, this.PrinterProfileSelectionCombo.Model.IterNChildren () - 1);
				this.PrinterProfileSelectionCombo.SetActiveIter(iter);
			}
			catch (Exception ex) {
				Program.LogException(ex);
			}
		}


		// ========== Remove Profile ==========
		protected void OnPrinterProfileRemoveButtonReleased(object sender, EventArgs e)
		{
			try {
				string removeProfileName = this.SelectedProfile.Name;
				if (removeProfileName == "Default") {
					Program.LogWarning ("Printer", "Cannot remove the Default printer profile.");
					return;
				}
				Program.printer.RemovePrinterProfile(removeProfileName);
				this.SelectedProfile = null;
				this.LoadProfiles ();
			}
			catch (Exception ex) {
				Program.LogException(ex);
			}
		}


		// ========== Save ==========
		protected void OnPrinterProfileSaveReleased(object sender, EventArgs e)
		{
			try
			{
				// Update Name:
				if (this.SelectedProfile.Name != "Default" && this.PrinterProfileNameEntry.Text != this.SelectedProfile.Name) {
					Program.printer.RemovePrinterProfile (this.SelectedProfile.Name);
					this.SelectedProfile.Name = this.PrinterProfileNameEntry.Text;
					Program.printer.AddPrinterProfile(this.SelectedProfile);
					this.LoadProfiles();
				}

				// Update Printer:
				this.SelectedProfile.SetPrintQueue (this.PrinterProfilePrinterCombo.ActiveText);
				Program.printer.SaveConfig ();
			}
			catch (Exception ex)
			{
				Program.LogException(ex);
			}
		}


		// ========== Confirm ==========
		protected void OnConfirmButtonReleased(object sender, EventArgs e)
		{
			this.Destroy();
		}


		// ========== Cancel ==========
		protected void OnCancelButtonReleased(object sender, EventArgs e)
		{
			this.Destroy();
		}
	}
}

