using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lagrange.XocMat.Command.ServerCommand;

public class Lottery : Command
{
    public override string[] Name => ["抽"];

    public override string HelpText => "抽取";
}
