using System.Diagnostics;
using Lagrange.XocMat.Command.CommandArgs;
using Lagrange.XocMat.Event;
using Lagrange.XocMat.EventArgs;
using Lagrange.XocMat.Extensions;
using Lagrange.XocMat.Internal;

namespace Lagrange.XocMat.Command.GroupCommands
{
    public class Reload : Command
    {
        public override string[] Alias => ["reload"];

        public override string HelpText => "重读配置文件";

        public override string[] Permissions => [OneBotPermissions.Reload];

        public override async Task InvokeAsync(GroupCommandArgs args)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            ReloadEventArgs reloadArgs = new ReloadEventArgs(args.GroupUin);
            await OperatHandler.Reload(reloadArgs);
            sw.Stop();
            reloadArgs.Message.Text($"所有配置文件已成功重新加载，耗时 {sw.Elapsed.TotalSeconds:F5} 秒。");
            await args.Event.Reply(reloadArgs.Message);
        }
    }
}
