

namespace Lagrange.XocMat.DB;

public static class Db
{
    public static RecordBase<T>.Context Context<T>(string tableName) where T : RecordBase<T>
    {
        return RecordBase<T>.GetContext(tableName);
    }
}