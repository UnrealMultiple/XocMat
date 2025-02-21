using Lagrange.XocMat.Command.CommandArgs;
using Lagrange.XocMat.Configuration;
using Lagrange.XocMat.DB.Manager;
using Lagrange.XocMat.Enumerates;
using Lagrange.XocMat.Extensions;
using Lagrange.XocMat.Internal;

namespace Lagrange.XocMat.Command.GroupCommands;

public class ResetTShock : Command
{
    public override string[] Alias => ["泰拉服务器重置"];
    public override string HelpText => "重置服务器";
    public override string[] Permissions => [OneBotPermissions.StartTShock];

    public override async Task InvokeAsync(GroupCommandArgs args)
    {
        if (UserLocation.Instance.TryGetServer(args.MemberUin, args.GroupUin, out Terraria.TerrariaServer? server) && server != null)
        {
            TerrariaUser.RemoveByServer(server.Name);
            await server.Reset(args.CommamdLine, async type =>
            {
                switch (type)
                {
                    case RestServerType.WaitFile:
                        {
                            await args.Event.Reply("正在等待上传地图，60秒后失效!");
                            break;
                        }
                    case RestServerType.TimeOut:
                        {
                            await args.Event.Reply("地图上传超时，自动创建地图。");
                            break;
                        }
                    case RestServerType.Success:
                        {
                            await args.Event.Reply("正在重置服务器!!");
                            break;
                        }
                    case RestServerType.LoadFile:
                        {
                            await args.Event.Reply("已接受到地图，正在上传服务器!!");
                            break;
                        }
                    case RestServerType.UnLoadFile:
                        {
                            await args.Event.Reply("上传的地图非国际正版，或地图不合法，请尽快重写上传!");
                            break;
                        }
                }
            });
        }
        else
        {
            await args.Event.Reply("未切换服务器或服务器无效!", true);
        }
    }
}
