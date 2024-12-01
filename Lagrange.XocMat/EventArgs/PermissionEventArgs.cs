using Lagrange.XocMat.DB.Manager;
using Lagrange.XocMat.Enumerates;

namespace Lagrange.XocMat.EventArgs;

public class PermissionEventArgs : System.EventArgs
{
    public Account Account { get; }

    public string permission { get; }

    public UserPermissionType UserPermissionType { get; set; }

    public PermissionEventArgs(Account account, string perm, UserPermissionType userPermissionType)
    {
        Account = account;
        permission = perm;
        UserPermissionType = userPermissionType;
    }
}
