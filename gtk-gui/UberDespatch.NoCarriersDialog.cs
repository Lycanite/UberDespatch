
// This file has been generated by the GUI designer. Do not modify.
namespace UberDespatch
{
	public partial class NoCarriersDialog
	{
		private global::Gtk.HBox MessageLayout;

		private global::Gtk.Image MessageIcon;

		private global::Gtk.Label MessageLabel;

		private global::Gtk.Button ConfirmButton;

		protected virtual void Build()
		{
			global::Stetic.Gui.Initialize(this);
			// Widget UberDespatch.NoCarriersDialog
			this.Name = "UberDespatch.NoCarriersDialog";
			this.Title = global::Mono.Unix.Catalog.GetString("No Carriers");
			this.Icon = new global::Gdk.Pixbuf(global::System.IO.Path.Combine(global::System.AppDomain.CurrentDomain.BaseDirectory, "./Icons/png/no-small.png"));
			this.WindowPosition = ((global::Gtk.WindowPosition)(4));
			this.BorderWidth = ((uint)(8));
			// Internal child UberDespatch.NoCarriersDialog.VBox
			global::Gtk.VBox w1 = this.VBox;
			w1.Name = "MainLayout";
			w1.BorderWidth = ((uint)(2));
			// Container child MainLayout.Gtk.Box+BoxChild
			this.MessageLayout = new global::Gtk.HBox();
			this.MessageLayout.Name = "MessageLayout";
			this.MessageLayout.Spacing = 6;
			// Container child MessageLayout.Gtk.Box+BoxChild
			this.MessageIcon = new global::Gtk.Image();
			this.MessageIcon.Name = "MessageIcon";
			this.MessageIcon.Pixbuf = new global::Gdk.Pixbuf(global::System.IO.Path.Combine(global::System.AppDomain.CurrentDomain.BaseDirectory, "./Icons/png/no.png"));
			this.MessageLayout.Add(this.MessageIcon);
			global::Gtk.Box.BoxChild w2 = ((global::Gtk.Box.BoxChild)(this.MessageLayout[this.MessageIcon]));
			w2.Position = 0;
			w2.Expand = false;
			w2.Fill = false;
			// Container child MessageLayout.Gtk.Box+BoxChild
			this.MessageLabel = new global::Gtk.Label();
			this.MessageLabel.Name = "MessageLabel";
			this.MessageLabel.LabelProp = global::Mono.Unix.Catalog.GetString("There are currently no carriers loaded.\r\nPlease check that the Plugins Folder is " +
					"set correctly in Settings > Options then reload the plugins via File > Reload Pl" +
					"ugins.");
			this.MessageLabel.Wrap = true;
			this.MessageLabel.Selectable = true;
			this.MessageLayout.Add(this.MessageLabel);
			global::Gtk.Box.BoxChild w3 = ((global::Gtk.Box.BoxChild)(this.MessageLayout[this.MessageLabel]));
			w3.Position = 1;
			w3.Expand = false;
			w3.Fill = false;
			w1.Add(this.MessageLayout);
			global::Gtk.Box.BoxChild w4 = ((global::Gtk.Box.BoxChild)(w1[this.MessageLayout]));
			w4.Position = 0;
			w4.Expand = false;
			w4.Fill = false;
			// Internal child UberDespatch.NoCarriersDialog.ActionArea
			global::Gtk.HButtonBox w5 = this.ActionArea;
			w5.Name = "MainActionArea";
			w5.Spacing = 10;
			w5.BorderWidth = ((uint)(5));
			w5.LayoutStyle = ((global::Gtk.ButtonBoxStyle)(4));
			// Container child MainActionArea.Gtk.ButtonBox+ButtonBoxChild
			this.ConfirmButton = new global::Gtk.Button();
			this.ConfirmButton.CanDefault = true;
			this.ConfirmButton.CanFocus = true;
			this.ConfirmButton.Name = "ConfirmButton";
			this.ConfirmButton.UseStock = true;
			this.ConfirmButton.UseUnderline = true;
			this.ConfirmButton.Label = "gtk-apply";
			this.AddActionWidget(this.ConfirmButton, -10);
			global::Gtk.ButtonBox.ButtonBoxChild w6 = ((global::Gtk.ButtonBox.ButtonBoxChild)(w5[this.ConfirmButton]));
			w6.Expand = false;
			w6.Fill = false;
			if ((this.Child != null))
			{
				this.Child.ShowAll();
			}
			this.DefaultWidth = 479;
			this.DefaultHeight = 129;
			this.Show();
			this.ConfirmButton.Released += new global::System.EventHandler(this.OnConfirmButtonReleased);
		}
	}
}
