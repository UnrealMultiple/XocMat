using Lagrange.XocMat.Command.CommandArgs;
using Lagrange.XocMat.Configuration;
using Lagrange.XocMat.Enumerates;
using Lagrange.XocMat.Extensions;
using Lagrange.XocMat.Internal;

namespace Lagrange.XocMat.Command.GroupCommands;

public class GenerateMap : Command
{
    public override string[] Alias => ["生成地图"];
    public override string HelpText => "生成地图";
    public override string[] Permissions => [OneBotPermissions.GenerateMap];

    public override async Task InvokeAsync(GroupCommandArgs args)
    {
        ImageType type = ImageType.Jpg;
        if (args.Parameters.Count > 0 && args.Parameters[0] == "-p")
            type = ImageType.Png;
        if (UserLocation.Instance.TryGetServer(args.MemberUin, args.GroupUin, out Terraria.TerrariaServer? server) && server != null)
        {
            Internal.Socket.Action.Response.MapImage api = await server.MapImage(type);
            if (api.Status)
            {
                string tempDir = Path.Combine(Environment.CurrentDirectory, "TempImage");
                if (!Directory.Exists(tempDir))
                {
                    Directory.CreateDirectory(tempDir);
                }
                string fileName = Guid.NewGuid().ToString() + ".jpg";
                string path = Path.Combine(tempDir, fileName);
                System.IO.File.WriteAllBytes(path, api.Buffer);
                await args.MessageBuilder.Image(api.Buffer).Reply();
                File.Delete(path);
            }
            else
            {
                await args.Event.Reply(api.Message);
            }

        }
        else
        {
            await args.Event.Reply("未切换服务器或服务器无效!", true);
        }
    }
}
