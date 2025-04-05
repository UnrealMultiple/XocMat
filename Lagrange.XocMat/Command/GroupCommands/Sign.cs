using Lagrange.XocMat.Command.CommandArgs;
using Lagrange.XocMat.Configuration;
using Lagrange.XocMat.Extensions;
using Lagrange.XocMat.Internal;
using Lagrange.XocMat.Utility.Images;
using Microsoft.Extensions.Logging;


namespace Lagrange.XocMat.Command.GroupCommands
{
    public class Sign : Command
    {
        public override string[] Alias => ["签到"];

        public override string HelpText => "每天都可以签到";

        public override string[] Permissions => [OneBotPermissions.Sign];

        public override async Task InvokeAsync(GroupCommandArgs args, ILogger log)
        {
            try
            {
                var rand = new Random();
                long num = rand.NextInt64(XocMatSetting.Instance.SignMinCurrency, XocMatSetting.Instance.SignMaxCurrency);
                DB.Manager.Sign result = DB.Manager.Sign.SingIn(args.MemberUin);
                DB.Manager.Currency currency = DB.Manager.Currency.Add(args.MemberUin, num);
                var builder = new ProfileItemBuilder()
                    .SetTitle("签到提示")
                    .SetMemberUin(args.MemberUin)
                    .AddItem($"QQ账号", args.MemberUin.ToString())
                    .AddItem($"QQ昵称", args.MemberCard)
                    .AddItem("签到时长", result.Date.ToString())
                    .AddItem($"本次获得{XocMatSetting.Instance.Currency}", num.ToString())
                    .AddItem($"{XocMatSetting.Instance.Currency}总数", currency.Num.ToString());
                await args.MessageBuilder
                    .Image(builder.Build())
                    .Reply();
            }
            catch (Exception e)
            {
                await args.Event.Reply(e.Message);
            }
        }
    }
}
