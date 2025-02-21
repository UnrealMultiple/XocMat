using Lagrange.Core;
using Lagrange.XocMat.Configuration;
using Lagrange.XocMat.DB.Manager;
using Lagrange.XocMat.Internal.Socket.Action.Response;
using Lagrange.XocMat.Terraria;
using System.Drawing;

namespace Lagrange.XocMat.Command.CommandArgs;

public class ServerCommandArgs(BotContext bot, TerrariaServer server, TerrariaUser user, Account account, string cmdName, string commamdPrefix, List<string> parameters, Dictionary<string, string> commamdLine) : BaseCommandArgs(bot, cmdName, commamdPrefix, parameters, commamdLine)
{
    public string ServerName => Server.Name;

    public string UserName => User.Name;

    public TerrariaServer Server { get; } = server;

    public TerrariaUser User { get; } = user;

    public Account Account { get; } = account;

    public async Task<BaseActionResponse> Reply(string msg, Color color)
    {
        return await Server.PrivateMsg(UserName, msg, color);
    }
}
