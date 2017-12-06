using System;
namespace UberDespatch
{
	public partial class AlertDialog : Gtk.Dialog
	{
		public AlertDialog(string message)
		{
			this.Modal = true;
			this.Parent = Program.mainWindow;
			this.Build();
			this.MessageLabel.Text = message;
		}

		protected void OnConfirmButtonReleased(object sender, EventArgs e)
		{
			this.Destroy();
		}
	}
}
