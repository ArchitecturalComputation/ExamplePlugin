using System;
using System.Collections.Generic;
using Rhino;
using Rhino.Input;
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
            double length = 10;
            var result = RhinoGet.GetNumber("Select length", false, ref length);

            if (result != Result.Success)
                return result;

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
