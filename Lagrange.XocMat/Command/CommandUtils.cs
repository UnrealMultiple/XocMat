using Lagrange.Core.Message;
using Lagrange.XocMat.Configuration;
using Lagrange.XocMat.DB.Manager;
using Lagrange.XocMat.Utility;

namespace Lagrange.XocMat.Command;

internal static class CommandUtils
{
    public static bool ParseBool(string str)
    {
        return str switch
        {
            "true" or "开启" or "开" => true,
            "false" or "关闭" or "关" => false,
            _ => false,
        };
    }


    public static async Task<MessageBuilder> GetAccountInfo(uint groupid, uint uin, string groupName)
    {
        uint userid = uin;
        string serverName = UserLocation.Instance.TryGetServer(userid, groupid, out Terraria.TerrariaServer? server) ? server?.Name ?? "NULL" : "NULL";
        List<TerrariaUser> bindUser = TerrariaUser.GetUserById(userid, serverName);
        string bindName = bindUser.Count == 0 ? "NULL" : string.Join(",", bindUser.Select(x => x.Name)); ;
        Sign? signInfo = Sign.Query(userid);
        long sign = signInfo != null ? signInfo.Date : 0;
        Currency? currencyInfo = Currency.Query(userid);
        long currency = currencyInfo != null ? currencyInfo.Num : 0;
        return MessageBuilder.Group(groupid)
            .Image(await HttpUtils.HttpGetByte($"http://q.qlogo.cn/headimg_dl?dst_uin={uin}&spec=640&img_type=png"))
            .Text($"[QQ账号]:{userid}\n")
            .Text($"[签到时长]:{sign}\n")
            .Text($"[{XocMatSetting.Instance.Currency}数量]:{currency}\n")
            .Text($"[拥有权限]:{groupName}\n")
            .Text($"[绑定角色]:{bindName}\n")
            .Text($"[所在服务器]:{serverName}");
    }
}
