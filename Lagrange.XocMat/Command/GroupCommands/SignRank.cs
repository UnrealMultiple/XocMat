using System.Text;
using Lagrange.XocMat.Command.CommandArgs;
using Lagrange.XocMat.Extensions;
using Lagrange.XocMat.Internal;
using Lagrange.XocMat.Utility.Images;
using Microsoft.Extensions.Logging;

namespace Lagrange.XocMat.Command.GroupCommands;

public class SignRank : Command
{
    public override string[] Alias => ["签到排行"];

    public override string HelpText => "查看签到排行榜";

    public override string[] Permissions => [OneBotPermissions.Sign];

    public override async Task InvokeAsync(GroupCommandArgs args, ILogger log)
    {
        try
        {
            IEnumerable<DB.Manager.Sign> signs = DB.Manager.Sign.GetSigns().OrderByDescending(x => x.Date).Take(10);
            var builder = new TableBuilder();
            builder.AddHeader("排名", "账号", "时长");
            int i = 1;
            foreach (DB.Manager.Sign? sign in signs)
            {
               builder.AddRow(i.ToString(), sign.UserId.ToString(), sign.Date.ToString());
                i++;
            }
            var table = new TableGenerate
            {
                AvatarPath = args.MemberUin,
                Title = "签到排行榜",
                TableRows = builder.Build()
            };
            await args.MessageBuilder.Image(table.Generate()).Reply();
        }
        catch (Exception e)
        {
            await args.Event.Reply(e.Message);
        }
    }
}
