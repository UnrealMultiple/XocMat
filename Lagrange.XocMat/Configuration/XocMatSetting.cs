using Lagrange.XocMat.Attributes;
using Lagrange.XocMat.EventArgs;
using Lagrange.XocMat.Terraria;
using Newtonsoft.Json;

namespace Lagrange.XocMat.Configuration;

[ConfigSeries]
public class XocMatSetting : JsonConfigBase<XocMatSetting>
{
    [JsonProperty("指令前缀")]
    public List<string> CommamdPrefix { get; init; } = [];

    [JsonProperty("权限所有者")]
    public long OwnerId { get; set; } = 523321293;

    [JsonProperty("缓存消息数量")]
    public int MaxCacheMessage { get; init; } = 10000;

    [JsonProperty("删除缓存数量")]
    public int DeleteCacheMessage { get; init; } = 1000;

    [JsonProperty("Sqlite路径")]
    public string DbPath { get; set; } = "Lagrange.XocMat.sqlite";

    [JsonProperty("用户默认权限组")]
    public string DefaultPermGroup { get; init; } = "default";

    [JsonProperty("邮箱STMP地址")]
    public string MailHost { get; init; } = "";

    [JsonProperty("STMP邮箱")]
    public string SenderMail { get; init; } = "";

    [JsonProperty("STMP授权码")]
    public string SenderPwd { get; init; } = "";

    [JsonProperty("通信端口")]
    public int SocketProt { get; init; } = 6000;

    [JsonProperty("获得货币最大数")]
    public int SignMaxCurrency { get; init; } = 700;

    [JsonProperty("获得货币最小数")]
    public int SignMinCurrency { get; init; } = 400;

    [JsonProperty("货币名称")]
    public string Currency { get; init; } = "星币";

    [JsonProperty("重复签到提示")]
    public string RepeatCheckinNotice { get; init; } = "你已经签到过了你个傻逼!";

    [JsonProperty("服务器列表")]
    public List<TerrariaServer> Servers { get; init; } = [];

    protected override string Filename => "XocMat";

    public TerrariaServer? GetServer(string name)
    {
        return Servers.Find(x => x.Name == name);
    }

    public TerrariaServer? GetServer(string name, uint groupid)
    {
        return Servers.Find(x => x.Name == name && x.Groups.Contains(groupid));
    }

    protected override async ValueTask OnReload(ReloadEventArgs args)
    {
        var identitys = Servers.ToDictionary(s => s.Name, s => s.ConnectIdentity);
        await base.OnReload(args);
        foreach (var server in Instance.Servers)
        {
            server.ConnectIdentity = identitys.GetValueOrDefault(server.Name, string.Empty);
        }
    }
}
