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
            .Image(await HttpUtils.GetByteAsync($"http://q.qlogo.cn/headimg_dl?dst_uin={uin}&spec=640&img_type=png"))
            .Text($"[QQ账号]:{userid}\n")
            .Text($"[签到时长]:{sign}\n")
            .Text($"[{XocMatSetting.Instance.Currency}数量]:{currency}\n")
            .Text($"[拥有权限]:{groupName}\n")
            .Text($"[绑定角色]:{bindName}\n")
            .Text($"[所在服务器]:{serverName}");
    }

    public static string GenerateMailBody(string tile, uint uin, string name, string body, string pw)
    {
        return $$"""
            <div>
                <includetail>
                    <style>
                        /* CLIENT-SPECIFIC STYLES */
                        body, table, td, a {
                            -webkit-text-size-adjust: 100%;
                            -ms-text-size-adjust: 100%;
                        }
                        table, td {
                            mso-table-lspace: 0pt;
                            mso-table-rspace: 0pt;
                        }
                        img {
                            -ms-interpolation-mode: bicubic;
                        }
                        .hidden {
                            display: none !important;
                            visibility: hidden !important;
                        }
                        /* iOS BLUE LINKS */
                        a[x-apple-data-detectors] {
                            color: inherit !important;
                            text-decoration: none !important;
                            font-size: inherit !important;
                            font-family: inherit !important;
                            font-weight: inherit !important;
                            line-height: inherit !important;
                        }
                        /* ANDROID MARGIN HACK */
                        body {
                            margin: 0 !important;
                        }
                        div[style*="margin: 16px 0"] {
                            margin: 0 !important;
                        }
                        @media only screen and (max-width: 639px) {
                            body, #body {
                                min-width: 320px !important;
                            }
                            table.wrapper {
                                width: 100% !important;
                                min-width: 320px !important;
                            }
                        }
                    </style>
                    <style>
                        body {
                            -webkit-text-size-adjust: 100%;
                            -ms-text-size-adjust: 100%;
                        }
                        img {
                            -ms-interpolation-mode: bicubic;
                        }
                        body {
                            margin: 0 !important;
                        }
                    </style>

                    <table border="0" cellpadding="0" cellspacing="0" id="body" style="text-align: center; min-width: 640px; width: 100%; -webkit-text-size-adjust: 100%; -ms-text-size-adjust: 100%; mso-table-lspace: 0pt; mso-table-rspace: 0pt; margin: 0; padding: 0;" bgcolor="#fafafa">
                        <tbody>
                            <!-- 合并后的标题行 -->
                            <tr>
                                <td style="text-align: center; 
                                          font-family: 'Helvetica Neue',Helvetica,Arial,sans-serif;
                                          font-size: 13px; 
                                          color: #5c5c5c; 
                                          padding: 8px 0 4px;
                                          mso-table-lspace: 0pt;
                                          mso-table-rspace: 0pt;">
                                    <span style="font-weight: 1000; color: #6b4fbb;">{{tile}}</span>
                                </td>
                            </tr>

                            <tr class="line">
                                <td style="font-family: 'Helvetica Neue',Helvetica,Arial,sans-serif; 
                                          height: 4px; 
                                          font-size: 4px; 
                                          line-height: 4px; 
                                          -webkit-text-size-adjust: 100%; 
                                          -ms-text-size-adjust: 100%; 
                                          mso-table-lspace: 0pt; 
                                          mso-table-rspace: 0pt;" 
                                    bgcolor="#6b4fbb">&nbsp;
                                </td>
                            </tr>

                            <tr class="header">
                                <td style="font-family: 'Helvetica Neue',Helvetica,Arial,sans-serif; 
                                          font-size: 13px; 
                                          line-height: 1.6; 
                                          color: #5c5c5c; 
                                          -webkit-text-size-adjust: 100%; 
                                          -ms-text-size-adjust: 100%; 
                                          mso-table-lspace: 0pt; 
                                          mso-table-rspace: 0pt; 
                                          padding: 25px 0;">
                                    <img src="http://q.qlogo.cn/headimg_dl?dst_uin={{uin}}&spec=640&img_type=png" 
                                       width="55" 
                                       height="50"
                                       style="-ms-interpolation-mode: bicubic;">
                                </td>
                            </tr>

                            <tr>
                                <td style="font-family: 'Helvetica Neue',Helvetica,Arial,sans-serif; 
                                          -webkit-text-size-adjust: 100%; 
                                          -ms-text-size-adjust: 100%; 
                                          mso-table-lspace: 0pt; 
                                          mso-table-rspace: 0pt;">
                                    <table border="0" cellpadding="0" cellspacing="0" class="wrapper" style="width: 640px; border-collapse: separate; border-spacing: 0; -webkit-text-size-adjust: 100%; -ms-text-size-adjust: 100%; mso-table-lspace: 0pt; mso-table-rspace: 0pt; margin: 0 auto;">
                                        <tbody>
                                            <tr>
                                                <td style="font-family: 'Helvetica Neue',Helvetica,Arial,sans-serif; 
                                                          border-radius: 3px; 
                                                          overflow: hidden; 
                                                          -webkit-text-size-adjust: 100%; 
                                                          -ms-text-size-adjust: 100%; 
                                                          mso-table-lspace: 0pt; 
                                                          mso-table-rspace: 0pt; 
                                                          padding: 18px 25px; 
                                                          border: 1px solid #ededed;" 
                                                    align="left" 
                                                    bgcolor="#ffffff">
                                                    <table border="0" cellpadding="0" cellspacing="0" class="content" style="width: 100%; border-collapse: separate; border-spacing: 0; -webkit-text-size-adjust: 100%; -ms-text-size-adjust: 100%; mso-table-lspace: 0pt; mso-table-rspace: 0pt;">
                                                        <tbody>
                                                            <tr>
                                                                <td style="font-family: 'Helvetica Neue',Helvetica,Arial,sans-serif; 
                                                                          color: #333333; 
                                                                          font-size: 15px; 
                                                                          font-weight: 400; 
                                                                          line-height: 2.0; 
                                                                          -webkit-text-size-adjust: 100%; 
                                                                          -ms-text-size-adjust: 100%; 
                                                                          mso-table-lspace: 0pt; 
                                                                          mso-table-rspace: 0pt; 
                                                                          padding: 15px 5px;" 
                                                                    align="center">
                                                                    <h1 style="font-family: 'Helvetica Neue',Helvetica,Arial,sans-serif; 
                                                                              color: #333333; 
                                                                              font-size: 18px; 
                                                                              font-weight: 400; 
                                                                              line-height: 1.4; 
                                                                              margin: 0; 
                                                                              padding: 0;" 
                                                                        align="center">
                                                                        你好，{{name}}！
                                                                    </h1>
                                                                    <p>{{body}}</p>
                                                                    <div id="cta">
                                                                        <a href="" style="-webkit-text-size-adjust: 100%; 
                                                                                        -ms-text-size-adjust: 100%; 
                                                                                        color: #6b4fbb; 
                                                                                        text-decoration: none; 
                                                                                        font-weight: 1000;">{{pw}}</a>
                                                                    </div>
                                                                    <p>请注意保存此密码，不要被其他人知晓！</p>
                                                                </td>
                                                            </tr>
                                                        </tbody>
                                                    </table>
                                                </td>
                                            </tr>
                                        </tbody>
                                    </table>
                                </td>
                            </tr>

                            <tr class="footer">
                                <td style="font-family: 'Helvetica Neue',Helvetica,Arial,sans-serif; 
                                          font-size: 13px; 
                                          line-height: 1.6; 
                                          color: #5c5c5c; 
                                          -webkit-text-size-adjust: 100%; 
                                          -ms-text-size-adjust: 100%; 
                                          mso-table-lspace: 0pt; 
                                          mso-table-rspace: 0pt; 
                                          padding: 25px 0;">
                                    <div>
                                        <a style="color: #3777b0; 
                                                 text-decoration: none; 
                                                 -webkit-text-size-adjust: 100%; 
                                                 -ms-text-size-adjust: 100%;" 
                                           href="">
                                            您收到这封电子邮件是因为你使用Lagrange.XocMat注册了账户。
                                        </a>
                                    </div>
                                </td>
                            </tr>
                        </tbody>
                    </table>
                </includetail>
            </div>
            """;
    }
}
