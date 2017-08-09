using System;
using Gtk;

namespace UberDespatch
{
	public partial class CarriersWindow : Gtk.Window
	{
		// ========== Constructor ==========
		public CarriersWindow () :
			base (Gtk.WindowType.Toplevel)
		{
			this.Build ();

			// Load Carriers To Dropdown:
			foreach (Carrier carrier in Carrier.Carriers.Values) {
				this.CarrierCombobox.AppendText (carrier.Name);
			}
			foreach (CarrierGroup carrierGroup in Carrier.CarrierGroups.Values) {
				this.CarrierCombobox.AppendText (carrierGroup.Name);
			}
			TreeIter iter;
			this.CarrierCombobox.Model.IterNthChild (out iter, 0);
			this.CarrierCombobox.SetActiveIter (iter);
		}


		// ========== Configure ==========
		protected void OnConfigureButtonReleased (object sender, EventArgs e)
		{
			String carrierName = this.CarrierCombobox.ActiveText;
			Carrier carrier = Carrier.GetCarrier (carrierName);
			if (carrier != null)
				carrier.OpenSettingsWindow ();
			else {
				CarrierGroup carrierGroup = Carrier.GetCarrierGroup (carrierName);
				carrierGroup.OpenSettingsWindow ();
			}
			this.Destroy ();
		}


		// ========== Cancel ==========
		protected void OnCancelButtonReleased (object sender, EventArgs e)
		{
			this.Destroy ();
		}
	}
}

