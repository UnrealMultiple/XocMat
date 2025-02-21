using Lagrange.XocMat.Command.CommandArgs;
using Lagrange.XocMat.Configuration;
using Lagrange.XocMat.Extensions;
using Lagrange.XocMat.Internal;
using Lagrange.XocMat.Utility;


namespace Lagrange.XocMat.Command.GroupCommands
{
    public class Sign : Command
    {
        public override string[] Alias => ["签到"];

        public override string HelpText => "每天都可以签到";

        public override string[] Permissions => [OneBotPermissions.Sign];

        public override async Task InvokeAsync(GroupCommandArgs args)
        {
            try
            {
                Random rand = new Random();
                long num = rand.NextInt64(XocMatSetting.Instance.SignMinCurrency, XocMatSetting.Instance.SignMaxCurrency);
                DB.Manager.Sign result = DB.Manager.Sign.SingIn(args.MemberUin);
                DB.Manager.Currency currency = DB.Manager.Currency.Add(args.MemberUin, num);
                await args.MessageBuilder
                    .Image(await HttpUtils.HttpGetByte($"http://q.qlogo.cn/headimg_dl?dst_uin={args.MemberUin}&spec=640&img_type=png"))
                    .Text($"签到成功！\n")
                    .Text($"[签到时长]：{result.Date}\n")
                    .Text($"[获得{XocMatSetting.Instance.Currency}]：{num}\n")
                    .Text($"[{XocMatSetting.Instance.Currency}总数]：{currency.Num}")
                    .Reply();
            }
            catch (Exception e)
            {
                await args.Event.Reply(e.Message);
            }
        }
    }
}
