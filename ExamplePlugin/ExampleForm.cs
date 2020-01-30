using System;
using System.Collections.Generic;
using System.ComponentModel;
using Eto.Drawing;
using Eto.Forms;
using Rhino;
using Rhino.Commands;
using Rhino.DocObjects;
using Rhino.Geometry;
using Command = Rhino.Commands.Command;

namespace ExamplePlugin
{
    public class ExampleForm : Command
    {
        public override string EnglishName => "ExampleForm";

        Form _form;

        public ExampleForm()
        {
            _form = new MyForm { Visible = false };
            _form.Show();
        }

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            RhinoApp.WriteLine("Showing form...");
            _form.Visible = true;
            return Result.Success;
        }
    }


    class MyForm : Form
    {
        TextBox _lengthField;
        Label _countText;

        List<Guid> _baked = new List<Guid>();

        public MyForm()
        {
            Title = "My Form";
            ClientSize = new Size(400, 300);
            Owner = Rhino.UI.RhinoEtoApp.MainWindow;

            Content = new StackLayout
            {
                Orientation = Orientation.Vertical,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                Padding = new Padding(40),
                Spacing = 10,
                Items =
                {
                    (_lengthField = new TextBox() { PlaceholderText = "Length of extrusion.", Width = 150 }),
                    new Button(Extrude) { Text = "Extrude", Width = 100 },
                    (_countText = new Label() { Text = "Type a number and press the button." })
                }
            };
        }

        private void Extrude(object sender, EventArgs e)
        {
            if (double.TryParse(_lengthField.Text, out var length))
                _countText.TextColor = Colors.Black;
            else
            {
                _countText.TextColor = Colors.Red;
                _countText.Text = $"'{_lengthField.Text}' is not a valid number.";
                return;
            }

            var doc = RhinoDoc.ActiveDoc;
            var objects = doc.Objects.FindByObjectType(ObjectType.Curve);

            doc.Objects.Delete(_baked, true);
            _baked.Clear();

            foreach (var obj in objects)
            {
                var curve = obj.Geometry as Curve;
                var extrusion = Surface.CreateExtrusion(curve, Vector3d.ZAxis * length);
                var guid = doc.Objects.AddSurface(extrusion);
                _baked.Add(guid);
            }

            doc.Views.Redraw();

            var s = _baked.Count == 1 ? "" : "s";
            _countText.Text = $"{_baked.Count} curve{s} extruded.";
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            RhinoApp.WriteLine("Hiding form...");
            e.Cancel = true;
            Visible = false;
        }
    }
}
