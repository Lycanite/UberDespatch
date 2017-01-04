using System;

namespace UberDespatch
{
	public partial class CarrierDPDWindow : Gtk.Window
	{
		public Carrier carrier;

		public CarrierDPDWindow (Carrier carrier) :
		base (Gtk.WindowType.Toplevel)
		{
			this.Build ();
			this.carrier = carrier;
			if (carrier.GetIcon () != null) {
				this.CarrierImage.Pixbuf = carrier.GetIcon ();
				this.Icon = carrier.GetIcon ();
			}
			this.OnOpen ();
		}


		// ========== Open ==========
		protected void OnOpen ()
		{
			this.InputFileChooser.SetCurrentFolder (this.carrier.GetConfigValue ("inputPath"));
			this.InputFilenameEntry.Text = this.carrier.GetConfigValue ("inputFilename");
			this.OutputFileChooser.SetCurrentFolder (this.carrier.GetConfigValue ("outputPath"));
			this.OutputFilenameEntry.Text = this.carrier.GetConfigValue ("outputFilename");
			this.TemplateTextView.Buffer.Text = this.carrier.GetConfigValue ("template");
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
			this.carrier.SetConfigValue ("inputPath", this.InputFileChooser.CurrentFolder);
			this.carrier.SetConfigValue ("inputFilename", this.InputFilenameEntry.Text);
			this.carrier.SetConfigValue ("outputPath", this.OutputFileChooser.CurrentFolder);
			this.carrier.SetConfigValue ("outputFilename", this.OutputFilenameEntry.Text);
			this.carrier.SetConfigValue ("template", this.TemplateTextView.Buffer.Text);
			this.carrier.SaveConfig ();
		}


		// ========== Cancel ==========
		protected void OnCancelButtonReleased (object sender, EventArgs e)
		{
			this.Destroy ();
		}
	}
}

