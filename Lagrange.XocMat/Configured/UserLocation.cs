using Lagrange.XocMat.Terraria;
using System.Text.Json.Serialization;

namespace Lagrange.XocMat.Configured;

public class UserLocation
{
    [JsonPropertyName("服务器位置")]
    public Dictionary<uint, string> Location { get; set; } = [];

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
            var server = XocMatAPI.Setting.GetServer(name, groupid);
            if (server != null)
            {
                terrariaServer = server;
                return true;
            }
        }
        terrariaServer = null;
        return false;
    }

    private void Save()
    {
        ConfigHelpr.Write(XocMatAPI.UserLocationPath, XocMatAPI.UserLocation);
    }
}
