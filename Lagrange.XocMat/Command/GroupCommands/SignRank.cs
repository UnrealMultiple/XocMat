﻿using Lagrange.XocMat.Command.CommandArgs;
using Lagrange.XocMat.Extensions;
using System.Text;

namespace Lagrange.XocMat.Command.InternalCommands
{
    public class SignRank : Command 
    {
        public override string[] Name => ["签到排行"];

        public override string HelpText => "查看签到排行榜";

        public override string Permission => base.Permission;

        public override async Task InvokeAsync(GroupCommandArgs args)
        {
            try
            {
                var signs = DB.Manager.Sign.GetSigns().Where(x => x.GroupID == args.EventArgs.Chain.GroupUin!.Value).OrderByDescending(x => x.Date).Take(10);
                var sb = new StringBuilder("签到排行\n\n");
                int i = 1;
                foreach (var sign in signs)
                {
                    sb.AppendLine($"签到排名: {i}");
                    sb.AppendLine($"账号: {sign.UserId}");
                    sb.AppendLine($"时长: {sign.Date}");
                    sb.AppendLine();
                    i++;
                }

                await args.EventArgs.Reply(sb.ToString().Trim());
            }
            catch (Exception e)
            {
                await args.EventArgs.Reply(e.Message);
            }
        }
    }
}
