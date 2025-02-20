using Lagrange.Core.Common.Interface.Api;
using Lagrange.XocMat.Attributes;
using Lagrange.XocMat.Command.CommandArgs;
using Lagrange.XocMat.Configuration;
using Lagrange.XocMat.Extensions;
using Lagrange.XocMat.Permission;
using System.IO.Compression;
using System.Text;

namespace Lagrange.XocMat.Command.InternalCommands;

public class ExportPlayerFile : Command
{
    public override string[] Name => ["导出存档"];

    public override string HelpText => "导出服务器存档";

    public override string Permission => OneBotPermissions.ExportFile;

    public override async Task InvokeAsync(GroupCommandArgs args)
    {
        if (UserLocation.Instance.TryGetServer(args.EventArgs.Chain.GroupMemberInfo!.Uin, args.EventArgs.Chain.GroupUin!.Value, out var server) && server != null)
        {
            if (args.Parameters.Count == 1)
            {
                List<string> names = [];
                if (args.Parameters[0] != "all")
                    names.Add(args.Parameters[0]);
                var files = await server.ExportPlayer(names);
                if (!files.PlayerFiles.Any(x => x.Active))
                {
                    await args.EventArgs.Reply("没有可以导出的存档!", true);
                    return;
                }
                var sb = new StringBuilder();
                var zipName = $"[{server.Name}]人物存档[{DateTime.Now:yyyy_MM_dd_HH_mm_ss}].zip";
                using var ms = new MemoryStream();
                using var zip = new ZipArchive(ms, ZipArchiveMode.Create);
                foreach (var file in files.PlayerFiles)
                {
                    if (!file.Active)
                    {
                        sb.AppendLine($"存档{file.Name}.plr导出失败，未找到存档!");
                    }
                    else
                    {
                        var entry = zip.CreateEntry(file.Name + ".plr");
                        using var stream = entry.Open();
                        stream.Write(file.Buffer);
                        if (files.PlayerFiles.Count == 1)
                            stream.Flush();

                    }
                }

                if (sb.Length > 0)
                    await args.EventArgs.Reply(sb.ToString().Trim());
                await args.Bot.GroupFSUpload(args.EventArgs.Chain.GroupUin!.Value, new(ms.GetBuffer(), zipName));
            }
            else
            {
                await args.EventArgs.Reply($"语法错误 正确语法:\n{args.CommamdPrefix}{args.Name} [名称 or all]", true);
            }
        }
        else
        {
            await args.EventArgs.Reply("服务器无效或未切换服务器!");
        }
    }
}
