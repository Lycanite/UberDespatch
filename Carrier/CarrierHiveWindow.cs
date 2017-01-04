using System;
using Gtk;

namespace UberDespatch
{
	public partial class CarrierHiveWindow : Gtk.Window
	{
		public Carrier carrier;

		public CarrierHiveWindow (Carrier carrier) :
		base (Gtk.WindowType.Toplevel)
		{
			this.Build ();
			this.carrier = carrier;
			this.CarrierLabel.Text = carrier.name + " (Hive Carrier)";
			this.CarrierDescriptionLabel.Text = carrier.description;
			if (carrier.GetIcon () != null) {
				this.CarrierImage.Pixbuf = carrier.GetIcon ();
				this.Icon = carrier.GetIcon ();
			}
			this.OnOpen ();
		}


		// ========== Open ==========
		protected void OnOpen ()
		{
			this.InputHiveURLEntry.Text = this.carrier.GetConfigValue ("hiveURL");

			this.PrinterProfileSelectionCombo.AppendText("Default");
			int profileIndex = 0;
			int index = 0;
			foreach (string profileName in Program.printer.PrinterProfiles.Keys) {
				index++;
				if (profileName != "Default")
					this.PrinterProfileSelectionCombo.AppendText(profileName);
				if (profileName == this.carrier.GetConfigValue ("printerProfile"))
					profileIndex = index;
			}
			TreeIter iter;
			this.PrinterProfileSelectionCombo.Model.IterNthChild(out iter, profileIndex); // TODO Not accurate.
			this.PrinterProfileSelectionCombo.SetActiveIter(iter);

			this.HiveDetailsTextView.Buffer.Text = this.carrier.GetConfigValue ("hiveDetails");
		}


		// ========== Confirm ==========
		protected void OnConfirmButtonReleased (object sender, EventArgs e)
		{
			this.Save ();
			this.Destroy ();
		}


		// ========== Save ==========
		protected void Save ()
		{
			this.carrier.SetConfigValue ("hiveURL", this.InputHiveURLEntry.Text);
			this.carrier.SetConfigValue("printerProfile", this.PrinterProfileSelectionCombo.ActiveText);
			this.carrier.SetConfigValue ("hiveDetails", this.HiveDetailsTextView.Buffer.Text);
			this.carrier.SaveConfig ();
		}


		// ========== Cancel ==========
		protected void OnCancelButtonReleased (object sender, EventArgs e)
		{
			this.Destroy ();
		}
	}
}
