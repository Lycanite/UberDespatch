using System;
using Gtk;
using System.Threading;

namespace UberDespatch
{
	public partial class SettingsWindow : Gtk.Window
	{
		public SettingsWindow () :
			base (Gtk.WindowType.Toplevel)
		{
			this.Build ();
			this.OnOpen ();
		}


		// ========== Open ==========
		protected void OnOpen ()
		{
			this.DownloadsFileChooser.SetCurrentFolder (Program.configGlobal.downloadsPath);
			this.ArchiveFileChooser.SetCurrentFolder (Program.configGlobal.archivePath);
			this.PluginsFileChooser.SetCurrentFolder (Program.configGlobal.pluginsPath);

			this.WebLoggingURLEntry.Text = Program.configGlobal.logURL;
			this.WebLoggingUsernameEntry.Text = Program.configGlobal.logUsername;
			this.WebLoggingPasswordEntry.Text = Program.configGlobal.logPassword;

			if (Program.Translator != null) {
				this.TranslationURLEntry.Text = Program.Translator.Config.URL;
				this.TranslationKeyEntry.Text = Program.Translator.Config.Key;
				if (Program.Translator.Config.Countries == null || Program.Translator.Config.Countries.Length == 0)
					this.TranslationCountriesEntry.Text = "";
				else
					this.TranslationCountriesEntry.Text = String.Join(",", Program.Translator.Config.Countries);
			}
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
			Program.configGlobal.downloadsPath = this.DownloadsFileChooser.CurrentFolder;
			Program.configGlobal.archivePath = this.ArchiveFileChooser.CurrentFolder;
			Program.configGlobal.pluginsPath = this.PluginsFileChooser.CurrentFolder;
			
			if (Program.configGlobal.logURL != this.WebLoggingURLEntry.Text
			   || Program.configGlobal.logUsername != this.WebLoggingUsernameEntry.Text
			   || Program.configGlobal.logPassword != this.WebLoggingPasswordEntry.Text)
				Program.ClearLogToken ();
			Program.configGlobal.logURL = this.WebLoggingURLEntry.Text;
			Program.configGlobal.logUsername = this.WebLoggingUsernameEntry.Text;
			Program.configGlobal.logPassword = this.WebLoggingPasswordEntry.Text;

			Program.configGlobal.SaveFile ();

			if (Program.Translator != null) {
				Program.Translator.Config.URL = this.TranslationURLEntry.Text;
				Program.Translator.Config.Key = this.TranslationKeyEntry.Text;
				Program.Translator.Config.Countries = this.TranslationCountriesEntry.Text.ToUpper().Replace(" ", "").Split(',');
				Program.Translator.Config.SaveFile ();
				Program.Translator.CheckAvailability ();
			}
		}


		// ========== Cancel ==========
		protected void OnCancelButtonReleased (object sender, EventArgs e)
		{
			this.Destroy ();
		}
	}
}

