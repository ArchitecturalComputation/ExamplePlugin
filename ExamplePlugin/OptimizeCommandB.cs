using Rhino;
using Rhino.Commands;
using Rhino.Display;
using Rhino.Geometry;
using Rhino.Input.Custom;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExamplePlugin
{
    public class OptimizeCommandB : Command
    {
        public override string EnglishName => "OptimizeB";

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            var go = new GetOption();
            go.SetCommandPrompt("Optimize B");
            go.AddOption("Run");

            var conduit = new ConduitB();
            conduit.Enabled = true;
            doc.Views.Redraw();

            while (true)
            {
                go.Get();

                if (go.CommandResult() != Result.Success)
                    return go.CommandResult();

                if(go.Option().EnglishName == "Run")
                {
                    RhinoApp.WriteLine("Running optimization...");
                    break;
                }
            }

            conduit.Enabled = false;

            return Result.Success;
        }
    }

    class ConduitB : DisplayConduit
    {
        protected override void CalculateBoundingBox(CalculateBoundingBoxEventArgs e)
        {
            var bbox = new BoundingBox(0, 0, 0, 10, 10, 10);
            e.IncludeBoundingBox(bbox);
        }

        protected override void PostDrawObjects(DrawEventArgs e)
        {
            var c = new Circle(new Point3d(5, 5, 0), 4);
            e.Display.DrawCircle(c, Color.White);
        }
    }
}
