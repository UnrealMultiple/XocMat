using Lagrange.XocMat.Command.CommandArgs;
using Lagrange.XocMat.Extensions;
using Lagrange.XocMat.Internal;
using Lagrange.XocMat.Utility.Images;
using Microsoft.Extensions.Logging;

namespace Lagrange.XocMat.Command.GroupCommands;

public class SystemInfo : Command
{
    public override string[] Alias => ["系统信息"];

    public override string HelpText => "查看系统信息!";

    public override string[] Permissions => [OneBotPermissions.ClearMemory];

    public override async Task InvokeAsync(GroupCommandArgs args, ILogger log)
    {
        var builder = ProfileItemBuilder.Create()
            .SetTitle("系统信息")
            .SetMemberUin(args.MemberUin)
            .AddItem("CPU占用率", $"{XocMatAPI.SystemMonitor.CpuUsagePercent:0.0}%")
            .AddItem("内存占用率", $"{XocMatAPI.SystemMonitor.MemoryUsagePercent:0.0}%")
            .AddItem("总内存", $"{XocMatAPI.SystemMonitor.TotalPhysicalMemory / 1024 / 1024} MB")
            .AddItem("占用内存", $"{XocMatAPI.SystemMonitor.UsedPhysicalMemory / 1024 / 1024} MB")
            .AddItem("网络上行", $"{XocMatAPI.SystemMonitor.NetworkUploadKbps:0.0} KB/s")
            .AddItem("网络下行", $"{XocMatAPI.SystemMonitor.NetworkDownloadKbps:0.0} KB/s");
        await args.MessageBuilder.Image(builder.Build()).Reply();
    }
}
