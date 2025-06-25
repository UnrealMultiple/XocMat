using Lagrange.Core.Message.Entity;
using Lagrange.XocMat.Command.CommandArgs;
using Lagrange.XocMat.Extensions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lagrange.XocMat.Command.GroupCommands;

public class ButtonCommand : Command
{
    public override string[] Alias => ["b"];

    public override string[] Permissions => ["button"];

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
}
