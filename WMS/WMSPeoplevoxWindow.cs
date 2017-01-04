using System;
using System.Threading;
using Gtk;

namespace UberDespatch
{
	public partial class WMSPeoplevoxWindow : Gtk.Window
	{
		public WMSPeoplevoxWindow() : base(Gtk.WindowType.Toplevel)
		{
			this.Build();
			this.OnOpen();
		}


		// ========== Open ==========
		protected void OnOpen()
		{
			this.URLEntry.Text = Program.wms.GetConfigValue("url");
			this.ClientIDEntry.Text = Program.wms.GetConfigValue("clientID");
			this.UsernameEntry.Text = Program.wms.GetConfigValue("username");
			this.PasswordEntry.Text = Program.wms.GetConfigValue("password");
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
			Program.wms.SetConfigValue("url", this.URLEntry.Text);
			Program.wms.SetConfigValue("clientID", this.ClientIDEntry.Text);
			Program.wms.SetConfigValue("username", this.UsernameEntry.Text);
			Program.wms.SetConfigValue("password", this.PasswordEntry.Text);
			Program.wms.SaveConfig();

			Thread wmsThread = new Thread(new ThreadStart(delegate
			{
				Program.wms.Connect();
			}));
			wmsThread.Start();
		}


		// ========== Cancel ==========
		protected void OnCancelButtonReleased(object sender, EventArgs e)
		{
			this.Destroy();
		}


		// ========== Test API ==========
		protected void OnTestButtonReleased(object sender, EventArgs e)
		{
			this.Save();
			if (Program.wms.Connect())
			{
				this.TestImage.Pixbuf = Stetic.IconLoader.LoadIcon(this, Stock.Yes, IconSize.Dialog);
			}
			else {
				this.TestImage.Pixbuf = Stetic.IconLoader.LoadIcon(this, Stock.No, IconSize.Dialog);
			}
		}
	}
}
