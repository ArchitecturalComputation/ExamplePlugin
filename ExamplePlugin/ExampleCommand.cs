using System;
using System.Collections.Generic;
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

            double length;

            while (true)
            {
                var result = get.Get();

                if (get.CommandResult() != Result.Success)
                    return get.CommandResult();

                if (result == Rhino.Input.GetResult.Number)
                {
                    length = get.Number();                    
                    RhinoApp.WriteLine($"Length is: {length}");
                    break;
                }
            }

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
            return Result.Success;
        }
    }
}
