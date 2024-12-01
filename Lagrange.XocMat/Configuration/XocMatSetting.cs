using Lagrange.XocMat.Attributes;
using Lagrange.XocMat.Terraria;
using System.Text.Json.Serialization;

namespace Lagrange.XocMat.Configuration;

[ConfigSeries]
public class XocMatSetting : JsonConfigBase<XocMatSetting>
{
    [JsonPropertyName("指令前缀")]
    public List<string> CommamdPrefix { get; init; } = [];

    [JsonPropertyName("权限所有者")]
    public long OwnerId { get; set; } = 523321293;

    [JsonPropertyName("数据库类型")]
    public string DbType { get; set; } = "sqlite";

    [JsonPropertyName("Sqlite路径")]
    public string DbPath { get; set; } = "Lagrange.XocMat.sqlite";

    [JsonPropertyName("数据库地址")]
    public string DbHost { get; init; } = "127.0.0.1";

    [JsonPropertyName("数据库端口")]
    public int DbPort { get; init; } = 3306;

    [JsonPropertyName("数据库名称")]
    public string DbName { get; init; } = "Mirai";

    [JsonPropertyName("数据库用户名")]
    public string DbUserName { get; init; } = "Mirai";

    [JsonPropertyName("数据库密码")]
    public string DbPassword { get; init; } = "";

    [JsonPropertyName("Bot用户默认权限组")]
    public string DefaultPermGroup { get; init; } = "default";

    [JsonPropertyName("邮箱STMP地址")]
    public string MailHost { get; init; } = "";

    [JsonPropertyName("STMP邮箱")]
    public string SenderMail { get; init; } = "";

    [JsonPropertyName("STMP授权码")]
    public string SenderPwd { get; init; } = "";

    [JsonPropertyName("TShockSocket通信端口")]
    public int SocketProt { get; init; } = 6000;

    [JsonPropertyName("获得星币最大数")]
    public int SignMaxCurrency { get; init; } = 700;

    [JsonPropertyName("获得星币最小数")]
    public int SignMinCurrency { get; init; } = 400;

    [JsonPropertyName("服务器列表")]
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
