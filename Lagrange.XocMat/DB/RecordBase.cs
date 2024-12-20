using Lagrange.XocMat.Enumerates;
using Lagrange.XocMat.Extensions;
using LinqToDB;
using LinqToDB.Data;

namespace Lagrange.XocMat.DB;

public abstract class RecordBase<T> where T : RecordBase<T>
{
    public class Context : DataConnection
    {
        public ITable<T> Records => this.GetTable<T>();

        private static string GetProvider()
        {
            return XocMatAPI.DB.GetSqlType() switch
            {
                SqlType.Mysql => ProviderName.MySql,
                SqlType.Sqlite => ProviderName.SQLiteMS,
                _ => "",
            };
        }
        public Context(string tableName) : base(GetProvider(), RecordBase<T>.ConnectionString)
        {
            MappingSchema.AddScalarType(typeof(string), new LinqToDB.SqlQuery.SqlDataType(DataType.NVarChar, 255));
            this.CreateTable<T>(tableName, tableOptions: TableOptions.CreateIfNotExists);
        }
    }

    internal static Context GetContext(string tableName)
    {
        return new(tableName);
    }

    // ReSharper disable once StaticMemberInGenericType
    protected static string ConnectionString = XocMatAPI.DB.ConnectionString.Replace(",Version=3", "");
}