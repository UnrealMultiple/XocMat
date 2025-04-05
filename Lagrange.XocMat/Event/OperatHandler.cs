﻿using Lagrange.XocMat.Command.CommandArgs;
using Lagrange.XocMat.Configuration;
using Lagrange.XocMat.DB.Manager;
using Lagrange.XocMat.Enumerates;
using Lagrange.XocMat.EventArgs;

namespace Lagrange.XocMat.Event;

public static class OperatHandler
{
    public delegate IResult EventCallBack<in IEventArgs, out IResult>(IEventArgs args) where IEventArgs : System.EventArgs;

    public static event EventCallBack<PermissionEventArgs, UserPermissionType>? OnUserPermission;

    public static event EventCallBack<BaseCommandArgs, ValueTask>? OnCommand;

    public static event EventCallBack<GroupCommandArgs, ValueTask>? OnGroupCommand;

    public static event EventCallBack<FriendCommandArgs, ValueTask>? OnFriendCommand;

    public static event EventCallBack<ServerCommandArgs, ValueTask>? OnServerCommand;

    public static event EventCallBack<ReloadEventArgs, ValueTask>? OnReload;

    public static event EventCallBack<GroupMessageForwardArgs, ValueTask>? OnGroupMessageForward;

    internal static UserPermissionType UserPermission(Account account, string prem)
    {
        if (account.UserId == XocMatSetting.Instance.OwnerId)
            return UserPermissionType.Granted;
        if (OnUserPermission == null)
            return UserPermissionType.Denied;
        PermissionEventArgs args = new PermissionEventArgs(account, prem, UserPermissionType.Denied);
        return OnUserPermission(args);
    }

    internal static async ValueTask<bool> MessageForward(GroupMessageForwardArgs args)
    {
        if (OnGroupMessageForward == null)
            return false;
        await OnGroupMessageForward(args);
        return args.Handler;
    }

    private static async ValueTask<bool> GroupCommand(GroupCommandArgs args)
    {
        if (OnGroupCommand == null)
            return false;
        await OnGroupCommand(args);
        return args.Handler;
    }

    private static async ValueTask<bool> FriendCommand(FriendCommandArgs args)
    {
        if (OnFriendCommand == null)
            return false;
        await OnFriendCommand(args);
        return args.Handler;
    }

    internal static async ValueTask<bool> Command<T>(T args) where T : BaseCommandArgs
    {
        if (OnCommand != null) await OnCommand(args);
        return args switch
        {
            GroupCommandArgs group => await GroupCommand(group),
            FriendCommandArgs friend => await FriendCommand(friend),
            ServerCommandArgs server => await ServerUserCommand(server),
            _ => args.Handler
        };
    }


    internal static async ValueTask Reload(ReloadEventArgs args)
    {
        if (OnReload != null)
            await OnReload(args);
    }

    private static async ValueTask<bool> ServerUserCommand(ServerCommandArgs args)
    {
        if (OnServerCommand == null)
            return false;
        await OnServerCommand(args);
        return args.Handler;
    }
}
