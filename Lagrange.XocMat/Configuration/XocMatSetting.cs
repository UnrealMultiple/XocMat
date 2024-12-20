using Lagrange.XocMat.Attributes;
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

    [JsonProperty("数据库类型")]
    public string DbType { get; set; } = "sqlite";

    [JsonProperty("Sqlite路径")]
    public string DbPath { get; set; } = "Lagrange.XocMat.sqlite";

    [JsonProperty("数据库地址")]
    public string DbHost { get; init; } = "127.0.0.1";

    [JsonProperty("数据库端口")]
    public int DbPort { get; init; } = 3306;

    [JsonProperty("数据库名称")]
    public string DbName { get; init; } = "Mirai";

    [JsonProperty("数据库用户名")]
    public string DbUserName { get; init; } = "Mirai";

    [JsonProperty("数据库密码")]
    public string DbPassword { get; init; } = "";

    [JsonProperty("Bot用户默认权限组")]
    public string DefaultPermGroup { get; init; } = "default";

    [JsonProperty("邮箱STMP地址")]
    public string MailHost { get; init; } = "";

    [JsonProperty("STMP邮箱")]
    public string SenderMail { get; init; } = "";

    [JsonProperty("STMP授权码")]
    public string SenderPwd { get; init; } = "";

    [JsonProperty("TShockSocket通信端口")]
    public int SocketProt { get; init; } = 6000;

    [JsonProperty("获得货币最大数")]
    public int SignMaxCurrency { get; init; } = 700;

    [JsonProperty("获得货币最小数")]
    public int SignMinCurrency { get; init; } = 400;

    [JsonProperty("货币名称")]
    public string Currency { get; init; } = "星币";

    [JsonProperty("服务器列表")]
    public List<TerrariaServer> Servers { get; init; } = [];

    protected override string Filename => "XocMat";

    protected override string? ReloadMsg => "[XocMat]: config reload successfully!\n";

    public TerrariaServer? GetServer(string name)
    {
        return Servers.Find(x => x.Name == name);
    }

    public TerrariaServer? GetServer(string name, uint groupid)
    {
        return Servers.Find(x => x.Name == name && x.Groups.Contains(groupid));
    }
}
