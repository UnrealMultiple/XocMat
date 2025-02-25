using Lagrange.XocMat.Command.CommandArgs;
using Lagrange.XocMat.Configuration;
using Lagrange.XocMat.Extensions;
using Lagrange.XocMat.Internal;
using Microsoft.Extensions.Logging;


namespace Lagrange.XocMat.Command.GroupCommands;

public class TerrariaConfig : Command
{
    public override string[] Alias => ["config"];

    public override string HelpText => "设置服务器配置";

    public override string[] Permissions => [OneBotPermissions.SetConfig];

    public override async Task InvokeAsync(GroupCommandArgs args, ILogger log)
    {
        if (args.Parameters.Count < 2)
        {
            await args.Event.Reply($"语法错误,正确语法；{args.CommamdPrefix}{args.Name} [选项] [值]");
            return;
        }
        if (UserLocation.Instance.TryGetServer(args.MemberUin, args.GroupUin, out Terraria.TerrariaServer? server) && server != null)
        {
            bool status = CommandUtils.ParseBool(args.Parameters[1]);
            switch (args.Parameters[0].ToLower())
            {
                case "prize":
                    server.EnabledPrize = status;
                    await args.Event.Reply($"[{server.Name}]奖池状态设置为`{status}`");
                    break;
                case "shop":
                    server.EnabledShop = status;
                    await args.Event.Reply($"[{server.Name}]商店状态设置为`{status}`");
                    break;
                default:
                    await args.Event.Reply($"[{args.Parameters[1]}]不可被设置!");
                    break;
            }
            XocMatSetting.Save();
        }
        else
        {
            await args.Event.Reply($"未切换服务器或，服务器不存在!");
        }
    }
}
