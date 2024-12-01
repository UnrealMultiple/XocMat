
using Lagrange.XocMat.Enumerates;
using System.Data;

namespace Lagrange.XocMat.Extensions;

/// <summary>
/// Database extensions
/// </summary>
public static class DbExt
{

    public static SqlType GetSqlType(this IDbConnection conn)
    {
        var name = conn.GetType().Name;
        if (name == "SqliteConnection" || name == "SQLiteConnection")
            return SqlType.Sqlite;
        if (name == "MySqlConnection")
            return SqlType.Mysql;
        return SqlType.Unknown;
    }
}
