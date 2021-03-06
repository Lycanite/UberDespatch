
// This file has been generated by the GUI designer. Do not modify.
namespace UberDespatch
{
	public partial class AlertDialog
	{
		private global::Gtk.Label MessageLabel;

		private global::Gtk.Button ConfirmButton;

		protected virtual void Build()
		{
			global::Stetic.Gui.Initialize(this);
			// Widget UberDespatch.AlertDialog
			this.Name = "UberDespatch.AlertDialog";
			this.Title = global::Mono.Unix.Catalog.GetString("Important");
			this.Icon = global::Stetic.IconLoader.LoadIcon(this, "gtk-dialog-info", global::Gtk.IconSize.Menu);
			this.TypeHint = ((global::Gdk.WindowTypeHint)(1));
			this.WindowPosition = ((global::Gtk.WindowPosition)(4));
			this.BorderWidth = ((uint)(8));
			this.DefaultWidth = 400;
			this.DefaultHeight = 100;
			this.Gravity = ((global::Gdk.Gravity)(5));
			// Internal child UberDespatch.AlertDialog.VBox
			global::Gtk.VBox w1 = this.VBox;
			w1.Name = "dialog1_VBox";
			w1.BorderWidth = ((uint)(2));
			// Container child dialog1_VBox.Gtk.Box+BoxChild
			this.MessageLabel = new global::Gtk.Label();
			this.MessageLabel.Name = "MessageLabel";
			this.MessageLabel.LabelProp = global::Mono.Unix.Catalog.GetString("This is an alert message!");
			this.MessageLabel.Wrap = true;
			this.MessageLabel.Justify = ((global::Gtk.Justification)(2));
			this.MessageLabel.Selectable = true;
			w1.Add(this.MessageLabel);
			global::Gtk.Box.BoxChild w2 = ((global::Gtk.Box.BoxChild)(w1[this.MessageLabel]));
			w2.Position = 0;
			w2.Expand = false;
			w2.Fill = false;
			// Internal child UberDespatch.AlertDialog.ActionArea
			global::Gtk.HButtonBox w3 = this.ActionArea;
			w3.Name = "__gtksharp_102_Stetic_TopLevelDialog_ActionArea";
			// Container child __gtksharp_102_Stetic_TopLevelDialog_ActionArea.Gtk.ButtonBox+ButtonBoxChild
			this.ConfirmButton = new global::Gtk.Button();
			this.ConfirmButton.CanFocus = true;
			this.ConfirmButton.Name = "ConfirmButton";
			this.ConfirmButton.UseUnderline = true;
			this.ConfirmButton.Label = global::Mono.Unix.Catalog.GetString("Ok");
			this.AddActionWidget(this.ConfirmButton, 0);
			global::Gtk.ButtonBox.ButtonBoxChild w4 = ((global::Gtk.ButtonBox.ButtonBoxChild)(w3[this.ConfirmButton]));
			w4.Expand = false;
			w4.Fill = false;
			if ((this.Child != null))
			{
				this.Child.ShowAll();
			}
			this.Show();
			this.ConfirmButton.Released += new global::System.EventHandler(this.OnConfirmButtonReleased);
		}
	}
}
