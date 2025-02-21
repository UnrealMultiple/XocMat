namespace Lagrange.XocMat.Extensions;

public static class IEnumerableExtension
{
    public static IEnumerable<IEnumerable<T>> Split<T>(this IEnumerable<T> source, int size)
    {
        if (size <= 0)
            throw new ArgumentException("The size must be greater than 0!");
        List<T> list = new List<T>(size);
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

    public static string JoinToString<T>(this IEnumerable<T> source, string separator)
    {
        return string.Join(separator, source);
    }
}
