using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace SQLSERVERLOG
{
    public class Db : IDb
    {
        private IUtility _Utility = new Utility();

        public IList<T> GetByTable<T>(string tableName, string sql) where T : class, new()
        {
            var connStr = _Utility.GetConfigConnStr();
            sql = sql.Replace("<tableName>", tableName);

            return ExecSQL<T>(connStr, sql);
        }
        public IList<T> GetData<T>(string sql) where T : class, new()
        {
            var connStr = _Utility.GetConfigConnStr();
            return ExecSQL<T>(connStr, sql);
        }
        private IList<T> ExecSQL<T>(string connStr, string Sql) where T:class, new()
        {
            var result = new List<T>();
            using (var connection = new SqlConnection(connStr))
            {
                connection.Open();
                using (var cmd = new SqlCommand(Sql))
                {
                    cmd.Connection = connection;
                    var reader = cmd.ExecuteReader();
                   
                    while (reader.Read())
                    {
                        var model = new T();
                        for (var i = 0; i < reader.FieldCount; i++)
                        {
                            var colName = reader.GetName(i);
                            
                            foreach(var property in model.GetType().GetProperties())
                            {
                                if (colName.Equals(property.Name, StringComparison.OrdinalIgnoreCase))
                                    property.SetValue(model, reader[i]);
                            }
                        }
                        result.Add(model);
                    }
                }
            }
            return result;
        }
    }

    public interface IDb
    {
        IList<T> GetByTable<T>(string tableName, string sql) where T : class, new();
        IList<T> GetData<T>(string sql) where T : class, new();
    }
}
