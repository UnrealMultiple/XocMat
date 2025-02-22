using System.Text;
using Lagrange.XocMat.Command.CommandArgs;
using Lagrange.XocMat.Extensions;
using Lagrange.XocMat.Internal;

namespace Lagrange.XocMat.Command.GroupCommands;

public class SignRank : Command
{
    public override string[] Alias => ["签到排行"];

    public override string HelpText => "查看签到排行榜";

    public override string[] Permissions => [OneBotPermissions.Sign];

    public override async Task InvokeAsync(GroupCommandArgs args)
    {
        try
        {
            IEnumerable<DB.Manager.Sign> signs = DB.Manager.Sign.GetSigns().OrderByDescending(x => x.Date).Take(10);
            StringBuilder sb = new StringBuilder("签到排行\n\n");
            int i = 1;
            foreach (DB.Manager.Sign? sign in signs)
            {
                sb.AppendLine($"签到排名: {i}");
                sb.AppendLine($"账号: {sign.UserId}");
                sb.AppendLine($"时长: {sign.Date}");
                sb.AppendLine();
                i++;
            }

            await args.Event.Reply(sb.ToString().Trim());
        }
        catch (Exception e)
        {
            await args.Event.Reply(e.Message);
        }
    }
}
