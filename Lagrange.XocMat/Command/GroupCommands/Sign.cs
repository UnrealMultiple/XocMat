using Lagrange.XocMat.Command.CommandArgs;
using Lagrange.XocMat.Configuration;
using Lagrange.XocMat.Extensions;
using Lagrange.XocMat.Permission;
using Lagrange.XocMat.Utility;


namespace Lagrange.XocMat.Command.InternalCommands
{
    public class Sign : Command
    {
        public override string[] Name => ["签到"];

        public override string HelpText => "每天都可以签到";

        public override string Permission => OneBotPermissions.Sign;

        public override async Task InvokeAsync(GroupCommandArgs args)
        {
            try
            {
                var rand = new Random();
                long num = rand.NextInt64(XocMatSetting.Instance.SignMinCurrency, XocMatSetting.Instance.SignMaxCurrency);
                var result = DB.Manager.Sign.SingIn(args.EventArgs.Chain.GroupUin!.Value, args.EventArgs.Chain.GroupMemberInfo!.Uin);
                var currency = DB.Manager.Currency.Add(args.EventArgs.Chain.GroupUin!.Value, args.EventArgs.Chain.GroupMemberInfo!.Uin, num);
                await args.MessageBuilder
                    .Image(await HttpUtils.HttpGetByte($"http://q.qlogo.cn/headimg_dl?dst_uin={args.EventArgs.Chain.GroupMemberInfo!.Uin}&spec=640&img_type=png"))
                    .Text($"签到成功！\n")
                    .Text($"[签到时长]：{result.Date}\n")
                    .Text($"[获得{XocMatSetting.Instance.Currency}]：{num}\n")
                    .Text($"[{XocMatSetting.Instance.Currency}总数]：{currency.Num}")
                    .Reply();
            }
            catch (Exception e)
            {
                await args.EventArgs.Reply(e.Message);
            }
        }
    }
}
