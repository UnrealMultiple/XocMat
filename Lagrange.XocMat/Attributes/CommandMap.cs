namespace Lagrange.XocMat.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public class CommandMap(params string[] names) : Attribute
{
    public List<string> Name { get; init; } = [.. names];
}
