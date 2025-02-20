using Lagrange.Core;
using Lagrange.XocMat.Configuration;
using Lagrange.XocMat.DB.Manager;
using Lagrange.XocMat.Terraria;

namespace Lagrange.XocMat.Command.CommandArgs;

public class ServerCommandArgs(BotContext bot, string serverName, string userName, string cmdName, string commamdPrefix, List<string> parameters, Dictionary<string, string> commamdLine)
    : BaseCommandArgs(bot, cmdName, commamdPrefix, parameters, commamdLine)
{
    public string ServerName { get; } = serverName;

    public string UserName { get; } = userName;

    public TerrariaServer? Server => XocMatSetting.Instance.GetServer(ServerName);

    public TerrariaUser? User => TerrariaUser.GetUsersByName(UserName, ServerName);

    public Account Account => Account.GetAccountNullDefault(User == null ? 0 : User.Id);

}
