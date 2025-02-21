using Lagrange.XocMat.DB.Manager;

namespace Lagrange.XocMat.Internal.Database;

public class SuperAdminGroup : Group
{
    public override List<string> TotalPermissions => ["*"];

    public SuperAdminGroup() : base("superadmin")
    {

    }

    public override bool HasPermission(string permission) => true;
}
