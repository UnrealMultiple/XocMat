using Lagrange.XocMat.Command.CommandArgs;
using Lagrange.XocMat.Extensions;
using Lagrange.XocMat.Internal;
using Lagrange.XocMat.Utility.Images;
using Microsoft.Extensions.Logging;
using System.Reflection;


namespace Lagrange.XocMat.Command.GroupCommands;

public class HelpCommand : Command
{
    public override string[] Alias => ["help", "菜单", "帮助"];

    public override string HelpText => "获取命令列表";

    public override string[] Permissions => [OneBotPermissions.Help];

    public override async Task InvokeAsync(GroupCommandArgs args, ILogger log)
    {
        var commands = XocMatAPI.CommandManager!.Commands
            .Where(i => IsMethodOverridden(i.GetType(), nameof(i.InvokeAsync), [typeof(GroupCommandArgs), typeof(ILogger)]));

        var keyboard = new Core.Message.Entity.KeyboardData();
        var row = new Core.Message.Entity.Row();
        keyboard.Rows.Add(row);
        for (var i = 0; i < commands.Count(); i++)
        {
            var command = commands.ElementAt(i);
            if (i != 0 && i % 4 != 0)
            {
                row.Buttons.Add(new Core.Message.Entity.Button()
                {
                    Id = command.Alias.JoinToString(""),
                    RenderData = new Core.Message.Entity.RenderData()
                    {
                        Label = command.Alias.First(),
                        Style = 1
                    },
                    Action = new()
                    {
                        Type = 2,
                        Data = args.CommandPrefix + command.Alias.First(),
                        Permission = new()
                        {
                            Type = 2
                        }
                    }
                });
            }
            else
            { 
                row = new Core.Message.Entity.Row();
                keyboard.Rows.Add(row);
            }
        }
        await args.MessageBuilder.Keyboard(keyboard).Reply();
        //if (!commands.Any())
        //{
        //    await args.Event.Reply("当前无指令可用!", true);
        //    return;
        //}
        //var builder = MenuBuilder.Create()
        //    .SetMemberUin(args.MemberUin);
        //foreach (var command in commands)
        //{
        //    builder.AddCell(args.CommandPrefix + command.Alias.First(), command.HelpText);
        //}
        //await args.MessageBuilder.Image(builder.Build()).Reply();
    }

    public override async Task InvokeAsync(FriendCommandArgs args, ILogger log)
    {
        var commands = XocMatAPI.CommandManager!.Commands
            .Where(i => IsMethodOverridden(i.GetType(), nameof(i.InvokeAsync), [typeof(FriendCommandArgs), typeof(ILogger)]));
        if (!commands.Any())
        {
            await args.Event.Reply("当前无指令可用!", true);
            return;
        }
        var builder = MenuBuilder.Create()
            .SetMemberUin((uint)args.Account.UserId);
        foreach (var command in commands)
        {
            builder.AddCell(args.CommandPrefix + command.Alias.First(), command.HelpText);
        }
        await args.MessageBuilder.Image(builder.Build()).Reply();
    }

    public static bool IsMethodOverridden(Type derivedType, string methodName, Type[] parameterTypes)
    {
        Type? baseType = derivedType.BaseType;
        MethodInfo? derivedMethod = derivedType.GetMethod(methodName, parameterTypes);
        if (derivedMethod == null)
        {
            return false; // 方法不存在
        }

        MethodInfo[]? baseMethods = baseType?.GetMethods().Where(m => m.Name == methodName).ToArray();
        return baseMethods != null && baseMethods.Any(baseMethod =>
            MethodParametersMatch(baseMethod, parameterTypes) &&
            derivedMethod.DeclaringType != baseMethod.DeclaringType);
    }

    private static bool MethodParametersMatch(MethodInfo method, Type[] parameterTypes)
    {
        return method.GetParameters().Select(p => p.ParameterType).SequenceEqual(parameterTypes);
    }
}

