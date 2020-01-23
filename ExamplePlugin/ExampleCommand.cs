using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino;
using Rhino.Commands;
using Rhino.DocObjects;
using Rhino.Geometry;

namespace ExamplePlugin
{
    public class ExampleCommand : Command
    {
        public override string EnglishName => "ExampleCommand";

        List<Guid> _baked = new List<Guid>();

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            var get = new Rhino.Input.Custom.GetNumber();
            get.SetCommandPrompt("Select length");

            double length = 30;

            while (true)
            {
                var result = get.Get();

                if (get.CommandResult() != Result.Success)
                    return get.CommandResult();

                if (result == Rhino.Input.GetResult.Number)
                {
                    length = get.Number();
                    RhinoApp.WriteLine($"Number is: {length}");
                    break;
                }
            }

            var objects = doc.Objects.FindByObjectType(ObjectType.Curve);

            doc.Objects.Delete(_baked, true);
            _baked.Clear();

            foreach (var obj in objects)
            {
                var curve = obj.Geometry as Curve;
                var extrusions = Brep.CreateFromTaperedExtrude(curve, length, Vector3d.ZAxis, curve.PointAtStart, Math.PI * 0.25, ExtrudeCornerType.None, doc.ModelAbsoluteTolerance, doc.ModelAngleToleranceRadians);

                foreach (var extrusion in extrusions)
                {
                    var guid = doc.Objects.AddBrep(extrusion);
                    _baked.Add(guid);
                }
            }

            doc.Views.Redraw();
            //  RhinoApp.WriteLine("Goodbye world.");
            return Result.Success;
        }
    }
}
