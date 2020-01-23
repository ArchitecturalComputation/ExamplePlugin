using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino;
using Rhino.Commands;

namespace ExamplePlugin
{
    public class ExampleCommand : Command
    {
        public override string EnglishName => "ExampleCommand";

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            RhinoApp.WriteLine("Hello world");
            return Result.Success;
        }
    }
}
