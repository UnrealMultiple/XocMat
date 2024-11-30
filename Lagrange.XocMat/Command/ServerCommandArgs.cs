using Lagrange.Core;
using Lagrange.XocMat.DB.Manager;
using Lagrange.XocMat.Terraria;

namespace Lagrange.XocMat.Commands;

public class ServerCommandArgs(BotContext bot, string serverName, string userName, string cmdName, string commamdPrefix, List<string> parameters, Dictionary<string, string> commamdLine)
    : BaseCommandArgs(bot, cmdName, commamdPrefix, parameters, commamdLine)
{
    public string ServerName { get; } = serverName;

    public string UserName { get; } = userName;

    public TerrariaServer? Server => XocMatAPI.Setting.GetServer(ServerName);

    public TerrariaUserManager.User? User => XocMatAPI.TerrariaUserManager.GetUsersByName(UserName, ServerName);

    public AccountManager.Account Account => XocMatAPI.AccountManager.GetAccountNullDefault(User == null ? 0 : User.Id);

}
