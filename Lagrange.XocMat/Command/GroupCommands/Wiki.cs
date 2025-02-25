using System.Web;
using Lagrange.XocMat.Command.CommandArgs;
using Lagrange.XocMat.Extensions;
using Lagrange.XocMat.Internal;
using Microsoft.Extensions.Logging;

namespace Lagrange.XocMat.Command.GroupCommands;

public class Wiki : Command
{
    public override string[] Alias => ["wiki"];
    public override string HelpText => "â–“ÃšÂªÂ»Wiki";
    public override string[] Permissions => [OneBotPermissions.TerrariaWiki];

    public override async Task InvokeAsync(GroupCommandArgs args, ILogger log)
    {
        string url = "https://terraria.wiki.gg/zh/index.php?search=";
        string msg = args.Parameters.Count > 0 ? url += HttpUtility.UrlEncode(args.Parameters[0]) : url.Split("?")[0];
        await args.Event.Reply(msg);
    }
}
