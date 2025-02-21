
using System.Data;
using Lagrange.XocMat.Enumerates;

namespace Lagrange.XocMat.Extensions;

/// <summary>
/// Database extensions
/// </summary>
public static class DatabaseExtension
{

    public static SqlType GetSqlType(this IDbConnection conn)
    {
        string name = conn.GetType().Name;
        return name == "SqliteConnection" || name == "SQLiteConnection"
            ? SqlType.Sqlite
            : name == "MySqlConnection" ? SqlType.Mysql : SqlType.Unknown;
    }
}
