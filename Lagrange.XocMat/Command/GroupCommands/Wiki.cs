using Lagrange.Core.Message.Entity;
using Lagrange.XocMat.Command.CommandArgs;
using Lagrange.XocMat.Extensions;
using Lagrange.XocMat.Internal;
using Microsoft.Extensions.Logging;
using System.Web;

namespace Lagrange.XocMat.Command.GroupCommands;

public class Wiki : Command
{
    public override string[] Alias => ["wiki"];
    public override string HelpText => "Terraria Wiki";
    public override string[] Permissions => [OneBotPermissions.TerrariaWiki];

    public override async Task InvokeAsync(GroupCommandArgs args, ILogger log)
    {
        var Keyboard = new KeyboardData
        {
            Rows =
            [
                new()
                {
                    Buttons =
                    [
                        new()
                        {
                            Id = "VERIFICATION_FORUM",
                            RenderData= new RenderData()
                            {
                                Style = 1,
                                Label = args.Parameters.Count != 0 ? args.Parameters[0] : "Terraria Wiki",
                            },
                            Action = new()
                            {
                                Type = 0,
                                Data = $"https://terraria.wiki.gg/zh/index.php?search={(args.Parameters.Count != 0 ? args.Parameters[0] : "")}",
                                Permission = new()
                                {
                                    Type = 2
                                }
                            }
                        },
                    ]
                },
                new()
                {
                    Buttons =
                    [
                        new()
                        {
                            Id = "search",
                            RenderData= new RenderData()
                            {
                                Style = 1,
                                Label = "再次搜索",
                            },
                            Action = new()
                            {
                                Type = 2,
                                Data = $"{args.CommandPrefix}{args.Name}",
                                Permission = new()
                                {
                                    Type = 2
                                }
                            }
                        },
                    ]
                }
            ],
        };
        args.MessageBuilder.Keyboard(Keyboard);
        await args.MessageBuilder.Reply();
    }

    public override async Task InvokeAsync(FriendCommandArgs args, ILogger log)
    {
        string url = "https://terraria.wiki.gg/zh/index.php?search=";
        string msg = args.Parameters.Count > 0 ? url += HttpUtility.UrlEncode(args.Parameters[0]) : url.Split("?")[0];
        await args.MessageBuilder.Add(new StreamEntity(msg)).Reply();
    }
}
