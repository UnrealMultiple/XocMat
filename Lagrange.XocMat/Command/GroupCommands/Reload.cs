using Lagrange.XocMat.Command.CommandArgs;
using Lagrange.XocMat.Event;
using Lagrange.XocMat.EventArgs;
using Lagrange.XocMat.Extensions;
using System.Diagnostics;

namespace Lagrange.XocMat.Command.InternalCommands
{
    public class Reload : Command
    {
        public override string[] Name => ["reload"];

        public override string HelpText => "重读配置文件";

        public override string Permission => "reload";

        public override async Task InvokeAsync(GroupCommandArgs args)
        {
            var sw = new Stopwatch();
            sw.Start();
            var reloadArgs = new ReloadEventArgs(args.EventArgs.Chain.GroupUin!.Value);
            await OperatHandler.Reload(reloadArgs);
            sw.Stop();
            reloadArgs.Message.Text($"所有配置文件已成功重新加载，耗时 {sw.Elapsed.TotalSeconds:F5} 秒。");
            await args.EventArgs.Reply(reloadArgs.Message);
        }
    }
}
