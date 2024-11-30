using Lagrange.Core.Message;
using Lagrange.Core.Message.Entity;
using Lagrange.XocMat.Extensions;
using Lagrange.XocMat.Utility;
using System.Reflection;

namespace Lagrange.XocMat.Commands;

internal static class CommandUtils
{
    public static async ValueTask SendImagsEmoji(string url, CommandArgs args)
    {
        var at = args.EventArgs.Chain
            .Where(c => c is MentionEntity).Select(c => ((MentionEntity)c));

        long target = -1;
        if (at.Any())
        {
            target = at.First().Uin;
        }
        else
        {
            if (args.Parameters.Count > 0)
            {
                _ = long.TryParse(args.Parameters[0], out target);
            }
        }
        if (target != -1)
            await args.EventArgs.Reply(MessageBuilder.Group(args.EventArgs.Chain.GroupUin!.Value).Image(await HttpUtils.HttpGetByte(url + "?QQ=" + target)));
    }
    public static bool ParseBool(string str)
    {
        return str switch
        {
            "true" or "开启" or "开" => true,
            "false" or "关闭" or "关" => false,
            _ => false,
        };
    }
    public static bool ClassConstructParamIsZerp(this ConstructorInfo[] constructors)
    {
        return constructors.Any(ctor => ctor.GetParameters().Length == 0);
    }

    public static bool CommandParamPares(this MethodInfo method, Type type)
    {
        if (method != null)
        {
            var param = method.GetParameters();
            if (param.Length == 1)
            {
                return param[0].ParameterType == type;
            }
        }
        return false;
    }

    public static async Task<MessageBuilder> GetAccountInfo(uint groupid, uint uin, string groupName)
    {
        var userid = uin;
        var serverName = XocMatAPI.UserLocation.TryGetServer(userid, groupid, out var server) ? server?.Name ?? "NULL" : "NULL";
        var bindUser = XocMatAPI.TerrariaUserManager.GetUserById(userid, serverName);
        var bindName = bindUser.Count == 0 ? "NULL" : string.Join(",", bindUser.Select(x => x.Name)); ;
        var signInfo = XocMatAPI.SignManager.Query(groupid, userid);
        var sign = signInfo != null ? signInfo.Date : 0;
        var currencyInfo = XocMatAPI.CurrencyManager.Query(groupid, userid);
        var currency = currencyInfo != null ? currencyInfo.num : 0;
        return MessageBuilder.Group(groupid)
            .Image(await HttpUtils.HttpGetByte($"http://q.qlogo.cn/headimg_dl?dst_uin={uin}&spec=640&img_type=png"))
            .Text($"[QQ账号]:{userid}\n")
            .Text($"[签到时长]:{sign}\n")
            .Text($"[星币数量]:{currency}\n")
            .Text($"[拥有权限]:{groupName}\n")
            .Text($"[绑定角色]:{bindName}\n")
            .Text($"[所在服务器]:{serverName}");
    }
}
