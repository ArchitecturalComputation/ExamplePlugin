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
using System.Threading;

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

            while (true)
            {
                go.Get();

                if (go.CommandResult() != Result.Success)
                    return go.CommandResult();

                if (go.Option().EnglishName == "Run")
                {
                    RhinoApp.WriteLine("Running optimization...");
                    break;
                }
            }

            var optimize = new OptimizerB(50, 10);

            var conduit = new ConduitB();
            conduit.Enabled = true;
            conduit.Path = optimize.Path;
            conduit.Info = optimize.Info;

            Task redrawTask = null;

            optimize.UpdateCandidate = () =>
            {
                conduit.Path = optimize.Path;
                conduit.Info = optimize.Info;

                if (redrawTask?.IsCompleted != false)
                    redrawTask = Task.Run(doc.Views.Redraw);
            };

            optimize.Finished = () =>
             {
                 RhinoApp.WriteLine(optimize.Info);
                 doc.Objects.AddPolyline(optimize.Path);
                 doc.Views.Redraw();
                 conduit.Enabled = false;
             };

            Task.Run(optimize.RandomSearch);

            return Result.Success;
        }
    }

    class ConduitB : DisplayConduit
    {
        public Polyline Path { get; set; }
        public string Info { get; set; }

        protected override void CalculateBoundingBox(CalculateBoundingBoxEventArgs e)
        {
            var bbox = new BoundingBox(0, 0, 0, 10, 10, 10);
            e.IncludeBoundingBox(bbox);
        }

        protected override void DrawForeground(DrawEventArgs e)
        {
            var position = new Point2d(100, 100);
            e.Display.Draw2dText(Info, Color.White, position, false, 24);
        }

        protected override void PostDrawObjects(DrawEventArgs e)
        {
            e.Display.DrawPolyline(Path, Color.White);
        }
    }
}
