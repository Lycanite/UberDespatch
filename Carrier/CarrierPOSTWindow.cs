using System;
using Gtk;

namespace UberDespatch
{
	public partial class CarrierPOSTWindow : Gtk.Window
	{
		public CarrierGroup CarrierGroup;
		public Carrier SelectedCarrier;

		// ========== Constructor ==========
		public CarrierPOSTWindow (CarrierGroup carrierGroup) :
		base (Gtk.WindowType.Toplevel)
		{
			this.Build ();
			try {
				this.CarrierGroup = carrierGroup;
				this.LoadCarriers ();
			}
			catch (Exception e) {
				Program.LogError (this.CarrierGroup.Name, "A problem occured when trying to load carriers.");
				Program.LogException (e);
			}
		}


		// ========== New Carrier ==========
		protected void NewCarrier ()
		{
			try {
				int newNameCount = 2;
				string newName = "New POST Carrier";
				while (this.CarrierGroup.GetCarrier (newName) != null) {
					newName = "New POST Carrier " + newNameCount;
					newNameCount++;
				}
				Carrier newCarrier = this.CarrierGroup.CreateCarrier (newName, "", "Default", "");
				this.LoadCarriers (newCarrier.Name);
			}
			catch (Exception e) {
				Program.LogError (this.CarrierGroup.Name, "A problem occured when trying to create a new carrier.");
				Program.LogException (e);
			}
		}


		// ========== Load Carriers ==========
		protected void LoadCarriers (string selectedCarrierName = null)
		{
			this.CarrierSelectionCombo.Model = new ListStore(typeof(string), typeof(string));
			int carrierIndex = 0;
			int index = 0;
			foreach (string carrierName in this.CarrierGroup.Carriers.Keys) {
				this.CarrierSelectionCombo.AppendText(carrierName);
				if (this.SelectedCarrier == null || (selectedCarrierName != null && carrierName == selectedCarrierName))
					this.SelectCarrier (this.CarrierGroup.GetCarrier (carrierName));
				if (carrierName == this.SelectedCarrier.Name)
					carrierIndex = index;
				index++;
			}
			TreeIter iter;
			this.CarrierSelectionCombo.Model.IterNthChild(out iter, carrierIndex);
			this.CarrierSelectionCombo.SetActiveIter(iter);
		}


		// ========== Select Carrier ==========
		protected void SelectCarrier (Carrier carrier)
		{
			if (carrier == null)
				throw new Exception ("Tried to view a null carrier.");
			
			this.SelectedCarrier = carrier;

			this.CarrierNameEntry.Text = this.SelectedCarrier.GetConfigValue ("name");

			if (SelectedCarrier.GetIcon () != null) {
				this.CarrierImage.Pixbuf = this.SelectedCarrier.GetIcon ();
				this.Icon = this.SelectedCarrier.GetIcon ();
			}

			this.URLEntry.Text = this.SelectedCarrier.GetConfigValue ("url");

			this.PrinterProfileSelectionCombo.AppendText("Default");
			int printerIndex = 0;
			int index = 0;
			foreach (string profileName in Program.printer.PrinterProfiles.Keys) {
				index++;
				if (profileName != "Default")
					this.PrinterProfileSelectionCombo.AppendText(profileName);
				if (profileName == this.SelectedCarrier.GetConfigValue ("printerProfile"))
					printerIndex = index;
			}
			TreeIter iter;
			this.PrinterProfileSelectionCombo.Model.IterNthChild(out iter, printerIndex); // TODO Not accurate.
			this.PrinterProfileSelectionCombo.SetActiveIter(iter);

			this.AdditionalPOSTTextView.Buffer.Text = this.SelectedCarrier.GetConfigValue ("additionalPOST");
		}


		// ========== Save ==========
		protected void Save ()
		{
			if (this.SelectedCarrier == null) {
				this.NewCarrier ();
			}
			this.SelectedCarrier.SetConfigValue ("name", this.CarrierNameEntry.Text);
			this.SelectedCarrier.SetConfigValue ("url", this.URLEntry.Text);
			this.SelectedCarrier.SetConfigValue("printerProfile", this.PrinterProfileSelectionCombo.ActiveText);
			this.SelectedCarrier.SetConfigValue ("additionalPOST", this.AdditionalPOSTTextView.Buffer.Text);
			this.SelectedCarrier.SaveConfig ();
			this.LoadCarriers ();
		}


		// ========== Confirm ==========
		protected void OnConfirmButtonReleased (object sender, EventArgs e)
		{
			this.Save ();
			this.Destroy ();
		}


		// ========== Cancel ==========
		protected void OnCancelButtonReleased (object sender, EventArgs e)
		{
			this.Destroy ();
		}


		// ========== Select Carrier ==========
		protected void OnCarrierComboChanged (object sender, EventArgs e)
		{
			Carrier carrier = this.CarrierGroup.GetCarrier (this.CarrierSelectionCombo.ActiveText);
			this.SelectCarrier (carrier);
		}


		// ========== Add Carrier ==========
		protected void OnAddCarrierButtonReleased (object sender, EventArgs e)
		{
			this.NewCarrier ();
		}


		// ========== Remove Carrier ==========
		protected void OnRemoveCarrierButtonReleased (object sender, EventArgs e)
		{
			if (this.SelectedCarrier == null)
				return;
			try {
				this.CarrierGroup.RemoveCarrier (this.SelectedCarrier);
				this.SelectedCarrier = null;
				this.LoadCarriers ();
			}
			catch (Exception exception) {
				Program.LogError (this.CarrierGroup.Name, "A problem occured when trying to remove a carrier.");
				Program.LogException (exception);
			}
		}


		// ========== Save Carrier ==========
		protected void OnSaveCarrierButtonReleased (object sender, EventArgs e)
		{
			this.Save ();
		}
	}
}
