using Lagrange.Core;
using Lagrange.XocMat.DB.Manager;
using Lagrange.XocMat.Extensions;
using Lagrange.XocMat.Terraria;
using Lagrange.XocMat.Terraria.Protocol.Action.Response;
using System.Drawing;

namespace Lagrange.XocMat.Command.CommandArgs;

public class ServerCommandArgs(BotContext bot, TerrariaServer server, TerrariaUser user, Account account, string cmdName, string commamdPrefix, List<string> parameters, Dictionary<string, string> commamdLine) : BaseCommandArgs(bot, cmdName, commamdPrefix, parameters, commamdLine)
{
    public string ServerName => Server.Name;

    public string UserName => User.Name;

    public TerrariaServer Server { get; } = server;

    public TerrariaUser User { get; } = user;

    public Account Account { get; } = account;

    public Task<BaseActionResponse> Reply(string msg, Color color) => Server.PrivateMsg(UserName, msg, color);

    public override string ToPreviewString() => $"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}] [ServerCommand({User.Id})({UserName})] [{CommandPrefix}{Name}] [Parameters]: {Parameters.JoinToString(",")}";

    public override string ToPerviewErrorString(Exception e) => $"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}] [ServerCommand({User.Id})({UserName})] [{CommandPrefix}{Name}] [ErrorText]: {e}";

    public override string ToSkippingString() => $"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}] [ServerCommand({User.Id})({UserName})] [试图越级使用命令]: {CommandPrefix}{Name}";
}
