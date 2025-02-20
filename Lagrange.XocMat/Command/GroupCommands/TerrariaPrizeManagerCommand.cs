﻿using Lagrange.XocMat.Attributes;
using Lagrange.XocMat.Command.CommandArgs;
using Lagrange.XocMat.Configuration;
using Lagrange.XocMat.Extensions;
using Lagrange.XocMat.Permission;
using Lagrange.XocMat.Utility;


namespace Lagrange.XocMat.Command.InternalCommands;

public class TerrariaPrizeManagerCommand : Command
{
    public override string[] Name => [ "prize" ];

    public override string HelpText => "管理Terraria奖品";

    public override string Permission => OneBotPermissions.TerrariaPrizeAdmin;

    public override async Task InvokeAsync(GroupCommandArgs args)
    {
        if (args.Parameters.Count == 5 && args.Parameters[0].ToLower() == "add")
        {
            if (!int.TryParse(args.Parameters[1], out int id) || id < 1)
            {
                await args.EventArgs.Reply("请输入一个正确的物品ID", true);
                return;
            }
            if (!int.TryParse(args.Parameters[2], out int max) || max < 0)
            {
                await args.EventArgs.Reply("请输入一个正确最大数", true);
                return;
            }
            if (!int.TryParse(args.Parameters[3], out int min) || min < 0)
            {
                await args.EventArgs.Reply("请输入一个正确最小数", true);
                return;
            }
            if (!int.TryParse(args.Parameters[4], out int rate) || rate < 0)
            {
                await args.EventArgs.Reply("请输入一个正确概率", true);
                return;
            }
            var item = SystemHelper.GetItemById(id);
            if (item == null)
            {
                await args.EventArgs.Reply("没有找到此物品", true);
                return;
            }
            TerrariaPrize.Instance.Add(item.Name, id, rate, max, min);
            await args.EventArgs.Reply("添加成功", true);
            TerrariaPrize.Save();
        }
        else if (args.Parameters.Count == 2 && args.Parameters[0].ToLower() == "del")
        {
            if (!int.TryParse(args.Parameters[1], out int index))
            {
                await args.EventArgs.Reply("请输入一个正确序号", true);
                return;
            }
            var prize = TerrariaPrize.Instance.GetPrize(index);
            if (prize == null)
            {
                await args.EventArgs.Reply("该奖品不存在!", true);
                return;
            }
            TerrariaPrize.Instance.Remove(prize);
            await args.EventArgs.Reply("删除成功", true);
            TerrariaPrize.Save();
        }
        else
        {
            await args.EventArgs.Reply($"语法错误,正确语法:\n" +
                $"{args.CommamdPrefix}{args.Name} add [物品id] [最大数] [最小数] [概率]\n" +
                $"{args.CommamdPrefix}{args.Name} del [奖品序号]");
        }
    }
}
