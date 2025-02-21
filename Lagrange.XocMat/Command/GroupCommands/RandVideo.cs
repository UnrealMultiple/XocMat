using Lagrange.XocMat.Command.CommandArgs;
using Lagrange.XocMat.Extensions;
using Lagrange.XocMat.Utility;

namespace Lagrange.XocMat.Command.GroupCommands;

public class RandVideo : Command
{
    public override string[] Alias => ["randv"];
    public override string HelpText => "Ëæ»úÊÓÆ”";
    public override string[] Permissions => ["onebot.video.rand"];

    public override async Task InvokeAsync(GroupCommandArgs args)
    {
        await args.MessageBuilder.Video(await HttpUtils.HttpGetByte("https://www.yujn.cn/api/heisis.php")).Reply();
    }
}
