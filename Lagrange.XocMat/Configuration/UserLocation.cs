using Lagrange.XocMat.Attributes;
using Lagrange.XocMat.Terraria;
using Newtonsoft.Json;

namespace Lagrange.XocMat.Configuration;

[ConfigSeries]
public class UserLocation : JsonConfigBase<UserLocation>
{
    [JsonProperty("服务器位置")]
    public Dictionary<uint, string> Location { get; set; } = [];

    protected override string Filename => "UserLocation";

    public void Change(uint id, TerrariaServer server)
    {
        Change(id, server.Name);
    }

    public void Change(uint id, string Name)
    {
        Location[id] = Name;
        Save();
    }

    public bool TryGetServer(uint id, uint groupid, out TerrariaServer? terrariaServer)
    {
        if (Location.TryGetValue(id, out var name) && !string.IsNullOrEmpty(name))
        {
            var server = XocMatSetting.Instance.GetServer(name, groupid);
            if (server != null)
            {
                terrariaServer = server;
                return true;
            }
        }
        terrariaServer = null;
        return false;
    }
}
