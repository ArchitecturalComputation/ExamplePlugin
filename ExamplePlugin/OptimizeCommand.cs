using System;
using System.Collections.Generic;
using Rhino;
using Rhino.Commands;
using Rhino.DocObjects;
using Rhino.Geometry;
using Rhino.Input.Custom;
using Rhino.Display;
using System.Drawing;
using System.Threading.Tasks;
using System.Linq;

namespace ExamplePlugin
{
    public class OptimizeCommand : Command
    {
        public override string EnglishName => "Optimize";

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            var cities = new OptionInteger(50, 3, 1000);
            var time = new OptionInteger(10, 5, 120);
            var algorithmIndex = 0;

            var get = new GetOption();
            get.SetCommandPrompt("Optimize TSP");
            get.AddOptionInteger("Cities", ref cities);
            get.AddOptionInteger("Time", ref time);
            get.AddOptionList("Algorithm", new[] { "RandomSearch", "HillClimbing", "SimulatedAnnealing" }, 0);
            get.AddOption("Run");

            while (true)
            {
                get.Get();

                if (get.CommandResult() != Result.Success)
                    return get.CommandResult();

                if (get.Option().EnglishName == "Algorithm")
                {
                    algorithmIndex = get.Option().CurrentListOptionIndex;
                }

                if (get.Option().EnglishName == "Run")
                {
                    RhinoApp.WriteLine("Optimization started...");
                    break;
                }
            }

            var optimizer = new Optimizer(cities.CurrentValue, time.CurrentValue);
            var conduit = new OptimizeConduit(optimizer.Path) { Enabled = true };

            Task updateTask = null;

            optimizer.UpdatedCandidate += (o, e) =>
            {
                conduit.Path = optimizer.Path;
                conduit.Info = optimizer.Info;

                if (updateTask?.IsCompleted != false)
                    updateTask = Task.Run(() => doc.Views.Redraw());
            };

            optimizer.Finished += (o, e) =>
            {
                conduit.Enabled = false;
                doc.Objects.AddPolyline(optimizer.Path);
                doc.Views.Redraw();
                RhinoApp.WriteLine($"Optimization finished ({optimizer.Info}).");
            };

            var algorithms = new Action[]
            {
                optimizer.RandomSearch,
                optimizer.HillClimbing,
                optimizer.SimulatedAnnealing
            };

            Task.Run(algorithms[algorithmIndex]);

            return Result.Success;
        }
    }


    class OptimizeConduit : DisplayConduit
    {
        public Polyline Path { get; set; }
        public string Info { get; set; } = "";

        List<Polyline> _circles;

        public OptimizeConduit(Polyline cities)
        {
            var circle = Polyline.CreateInscribedPolygon(new Circle(Point3d.Origin, 0.06), 12);
            _circles = cities.Select(c => new Polyline(circle.Skip(1).Select(p => p += c))).ToList();
            Path = cities;
        }

        protected override void CalculateBoundingBox(CalculateBoundingBoxEventArgs e)
        {
            var bbox = new BoundingBox(0, 0, 0, 10, 10, 10);
            e.BoundingBox.Union(bbox);
        }

        protected override void PostDrawObjects(DrawEventArgs e)
        {
            e.Display.DrawPolyline(Path, Color.White);

            foreach (var circle in _circles)
                e.Display.DrawPolygon(circle, Color.White, true);
        }

        protected override void DrawForeground(DrawEventArgs e)
        {
            var pt = new Point2d(100, 100);
            e.Display.Draw2dText(Info, Color.Black, pt, false, 24);
        }
    }
}
