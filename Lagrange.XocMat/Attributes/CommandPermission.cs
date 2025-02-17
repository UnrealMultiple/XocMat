namespace Lagrange.XocMat.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public class CommandPermission(params string[] perms) : Attribute
{
    public string[] Permissions { get; init; } = perms;
}