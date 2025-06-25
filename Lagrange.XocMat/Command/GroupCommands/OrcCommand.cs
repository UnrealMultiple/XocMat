using Lagrange.Core.Common.Interface.Api;
using Lagrange.Core.Message.Entity;
using Lagrange.XocMat.Command.CommandArgs;
using Lagrange.XocMat.DB.Manager;
using Lagrange.XocMat.Extensions;
using Microsoft.Extensions.Logging;
using System.Text;

namespace Lagrange.XocMat.Command.GroupCommands;

public class OrcCommand : Command
{
    public override string[] Alias => ["ocr"];

    public override string[] Permissions => ["xocmat.ocr"];

    public override string HelpText => "提取图片内容。";

    public override async Task InvokeAsync(GroupCommandArgs args, ILogger log)
    {
        var forward = args.Event.Chain.GetEntity<ForwardEntity>();
        if (forward is null || MessageRecord.Query(forward.MessageId)?.GetEntity<ImageEntity>() is not ImageEntity image)
        {
            await args.Event.Reply("未能找到图片!", true);
            return;
        }
        var result = await args.Bot.OcrImage(image.ImageUrl);
        if (result is null || result.Texts.Count == 0)
        {
            await args.Event.Reply("未能识别到任何文字!", true);
            return;
        }
        var str = result.Texts.Aggregate(new StringBuilder(), (sb, text) => sb.AppendLine(text.Text)).ToString().Trim();
        await args.MessageBuilder.Text($"OCR Result:\n {str}").Reply();
    }
}
