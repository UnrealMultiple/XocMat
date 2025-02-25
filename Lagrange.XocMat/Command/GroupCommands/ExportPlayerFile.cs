using System.IO.Compression;
using System.Text;
using Lagrange.Core.Common.Interface.Api;
using Lagrange.XocMat.Command.CommandArgs;
using Lagrange.XocMat.Configuration;
using Lagrange.XocMat.Extensions;
using Lagrange.XocMat.Internal;
using Microsoft.Extensions.Logging;

namespace Lagrange.XocMat.Command.GroupCommands;

public class ExportPlayerFile : Command
{
    public override string[] Alias => ["导出存档"];
    public override string HelpText => "导出服务器存档";

    public override string[] Permissions => [OneBotPermissions.ExportFile];

    public override async Task InvokeAsync(GroupCommandArgs args, ILogger log)
    {
        if (UserLocation.Instance.TryGetServer(args.MemberUin, args.GroupUin, out Terraria.TerrariaServer? server) && server != null)
        {
            if (args.Parameters.Count == 1)
            {
                List<string> names = [];
                if (args.Parameters[0] != "all")
                    names.Add(args.Parameters[0]);
                Internal.Socket.Action.Response.ExportPlayer files = await server.ExportPlayer(names);
                if (!files.PlayerFiles.Any(x => x.Active))
                {
                    await args.Event.Reply("没有可以导出的存档!", true);
                    return;
                }
                StringBuilder sb = new StringBuilder();
                string zipName = $"[{server.Name}]人物存档[{DateTime.Now:yyyy_MM_dd_HH_mm_ss}].zip";
                using MemoryStream ms = new MemoryStream();
                using ZipArchive zip = new ZipArchive(ms, ZipArchiveMode.Create);
                foreach (Internal.Socket.Internet.PlayerFile file in files.PlayerFiles)
                {
                    if (!file.Active)
                    {
                        sb.AppendLine($"存档{file.Name}.plr导出失败，未找到存档!");
                    }
                    else
                    {
                        ZipArchiveEntry entry = zip.CreateEntry(file.Name + ".plr");
                        using Stream stream = entry.Open();
                        stream.Write(file.Buffer);
                        if (files.PlayerFiles.Count == 1)
                            stream.Flush();

                    }
                }

                if (sb.Length > 0)
                    await args.Event.Reply(sb.ToString().Trim());
                await args.Bot.GroupFSUpload(args.GroupUin, new(ms.GetBuffer(), zipName));
            }
            else
            {
                await args.Event.Reply($"语法错误 正确语法:\n{args.CommamdPrefix}{args.Name} [名称 or all]", true);
            }
        }
        else
        {
            await args.Event.Reply("服务器无效或未切换服务器!");
        }
    }
}
