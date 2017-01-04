using System;
using Gtk;

namespace UberDespatch
{
	public partial class NoCarriersDialog : Gtk.Dialog
	{
		public NoCarriersDialog ()
		{
			this.Build ();
		}

		protected void OnConfirmButtonReleased (object sender, EventArgs e)
		{
			this.Destroy ();
		}
	}
}

