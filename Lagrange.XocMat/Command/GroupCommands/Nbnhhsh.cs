using System.Text.Json.Nodes;
using Lagrange.XocMat.Command.CommandArgs;
using Lagrange.XocMat.Extensions;
using Lagrange.XocMat.Internal;
using Microsoft.Extensions.Logging;

namespace Lagrange.XocMat.Command.GroupCommands;

public class Nbnhhsh : Command
{
    public override string[] Alias => ["缩写"];
    public override string HelpText => "查询缩写";
    public override string[] Permissions => [OneBotPermissions.Nbnhhsh];

    public override async Task InvokeAsync(GroupCommandArgs args, ILogger log)
    {
        if (args.Parameters.Count == 1)
        {
            string url = $"https://oiapi.net/API/Nbnhhsh?text={args.Parameters[0]}";
            HttpClient client = new();
            string result = await client.GetStringAsync(url);
            JsonNode? data = JsonNode.Parse(result);
            JsonArray? trans = data?["data"]?[0]?["trans"]?.AsArray();
            if (trans != null && trans.Any())
            {
                await args.Event.Reply($"缩写:`{args.Parameters[0]}`可能为:\n{string.Join(",", trans)}");
            }
            else
            {
                await args.Event.Reply("也许该缩写没有被收录!");
            }
        }
        else
        {
            await args.Event.Reply($"语法错误，正确语法:{args.CommamdPrefix}缩写 [文本]");
        }
    }
}
