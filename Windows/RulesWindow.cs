using System;

namespace UberDespatch
{
	public partial class RulesWindow : Gtk.Window
	{
		public RulesWindow () :
			base (Gtk.WindowType.Toplevel)
		{
			this.Build ();
			this.OnOpen();
		}


		// ========== Open ==========
		protected void OnOpen()
		{
			this.LocalRulesFileChooser.SetCurrentFolder(Program.configGlobal.localRulesPath);
			this.LocalRulesFileChooser.SetFilename(Program.configGlobal.localRulesFilename);
			this.RemoteRulesEntry.Text = Program.configGlobal.remoteRulesPath;

			this.LocalZonesFileChooser.SetCurrentFolder(Program.configGlobal.localZonesPath);
			this.LocalZonesFileChooser.SetFilename(Program.configGlobal.localZonesFilename);
			this.RemoteZonesEntry.Text = Program.configGlobal.remoteZonesPath;

			this.LocalAlertsFileChooser.SetCurrentFolder(Program.configGlobal.localAlertsPath);
			this.LocalAlertsFileChooser.SetFilename(Program.configGlobal.localAlertsFilename);
			this.RemoteAlertsEntry.Text = Program.configGlobal.remoteAlertsPath;

			this.LocalLabelsFileChooser.SetCurrentFolder(Program.configGlobal.localLabelsPath);
			this.LocalLabelsFileChooser.SetFilename(Program.configGlobal.localLabelsFilename);
			this.RemoteLabelsEntry.Text = Program.configGlobal.remoteLabelsPath;
			this.LocalLabelsTemplateFileChooser.SetCurrentFolder(Program.configGlobal.localLabelsTemplatePath);
		}


		// ========== Confirm ==========
		protected void OnConfirmButtonReleased(object sender, EventArgs e)
		{
			this.Save();
			this.Destroy();
		}


		// ========== Save ==========
		protected void Save()
		{
			Program.configGlobal.localRulesPath = this.LocalRulesFileChooser.CurrentFolder;
			Program.configGlobal.localRulesFilename = this.LocalRulesFileChooser.Filename;
			Program.configGlobal.remoteRulesPath = this.RemoteRulesEntry.Text;

			Program.configGlobal.localZonesPath = this.LocalZonesFileChooser.CurrentFolder;
			Program.configGlobal.localZonesFilename = this.LocalZonesFileChooser.Filename;
			Program.configGlobal.remoteZonesPath = this.RemoteZonesEntry.Text;

			Program.configGlobal.localAlertsPath = this.LocalAlertsFileChooser.CurrentFolder;
			Program.configGlobal.localAlertsFilename = this.LocalAlertsFileChooser.Filename;
			Program.configGlobal.remoteAlertsPath = this.RemoteAlertsEntry.Text;

			Program.configGlobal.localLabelsPath = this.LocalLabelsFileChooser.CurrentFolder;
			Program.configGlobal.localLabelsFilename = this.LocalLabelsFileChooser.Filename;
			Program.configGlobal.remoteLabelsPath = this.RemoteLabelsEntry.Text;
			Program.configGlobal.localLabelsTemplatePath = this.LocalLabelsTemplateFileChooser.CurrentFolder;

			Program.configGlobal.SaveFile();
			Rule.LoadRules();
		}


		// ========== Cancel ==========
		protected void OnCancelButtonReleased(object sender, EventArgs e)
		{
			this.Destroy();
		}
	}
}

