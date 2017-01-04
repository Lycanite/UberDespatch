using System;
namespace UberDespatch
{
	public partial class AlertDialog : Gtk.Dialog
	{
		public AlertDialog(string message)
		{
			this.Build();
			this.MessageLabel.Text = message;
		}

		protected void OnConfirmButtonReleased(object sender, EventArgs e)
		{
			this.Destroy();
		}
	}
}
