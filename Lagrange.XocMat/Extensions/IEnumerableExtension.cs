namespace Lagrange.XocMat.Extensions;

public static class IEnumerableExtension
{
    private static readonly Random _random = new Random();

    public static IEnumerable<IEnumerable<T>> Split<T>(this IEnumerable<T> source, int size)
    {
        if (size <= 0)
            throw new ArgumentException("The size must be greater than 0!");
        var list = new List<T>(size);
        foreach (T? item in source)
        {
            list.Add(item);
            if (list.Count == size)
            {
                yield return list;
                list = new List<T>(size);
            }
        }
        if (list.Count > 0)
            yield return list;
    }

    public static T Rand<T>(this IEnumerable<T> source)
    {
        return source.ElementAt(_random.Next(0, source.Count()));
    }

    public static string JoinToString<T>(this IEnumerable<T> source, string separator)
    {
        return string.Join(separator, source);
    }

    public static string JoinToString<T>(this IEnumerable<T> source, string separator, Func<T, string> func)
    {
        return source.Select(func).JoinToString(separator);
    }
}
