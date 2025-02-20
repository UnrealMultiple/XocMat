using Lagrange.XocMat.Attributes;
using Lagrange.XocMat.Command.CommandArgs;
using Lagrange.XocMat.Configuration;
using Lagrange.XocMat.Extensions;
using Lagrange.XocMat.Permission;


namespace Lagrange.XocMat.Command.InternalCommands;

public class TerrariaConfig : Command
{
    public override string[] Name => ["config"];

    public override string HelpText => "设置服务器配置";

    public override string Permission => OneBotPermissions.SetConfig;

    public override async Task InvokeAsync(GroupCommandArgs args)
    {
        if (args.Parameters.Count < 2)
        {
            await args.EventArgs.Reply($"语法错误,正确语法；{args.CommamdPrefix}{args.Name} [选项] [值]");
            return;
        }
        if (UserLocation.Instance.TryGetServer(args.EventArgs.Chain.GroupMemberInfo!.Uin, args.EventArgs.Chain.GroupUin!.Value, out var server) && server != null)
        {
            var status = CommandUtils.ParseBool(args.Parameters[1]);
            switch (args.Parameters[0].ToLower())
            {
                case "prize":
                    server.EnabledPrize = status;
                    await args.EventArgs.Reply($"[{server.Name}]奖池状态设置为`{status}`");
                    break;
                case "shop":
                    server.EnabledShop = status;
                    await args.EventArgs.Reply($"[{server.Name}]商店状态设置为`{status}`");
                    break;
                default:
                    await args.EventArgs.Reply($"[{args.Parameters[1]}]不可被设置!");
                    break;
            }
            XocMatSetting.Save();
        }
        else
        {
            await args.EventArgs.Reply($"未切换服务器或，服务器不存在!");
        }
    }
}
