using System;
using System.Data;
using System.Data.SqlClient;

namespace SQLSERVERLOG
{
    public class Db : IDb
    {
        private IUtility _Utility = new Utility();

        public DataTable GetLogByTable(string tableName)
        {
            var sql = _Utility.GetSQLFromFile(_Utility.DBLogSql);
            var connStr = _Utility.GetConfigConnStr();
            sql = sql.Replace("<tableName>", tableName);

            return ExecSQL(connStr, sql);
        }

        private DataTable ExecSQL(string connStr, string Sql)
        {
            var dt = new DataTable();
            using (var connection = new SqlConnection(connStr))
            {
                connection.Open();
                using (var cmd = new SqlCommand(Sql))
                {
                    cmd.Connection = connection;
                    var reader = cmd.ExecuteReader();

                    for (var i = 0; i < reader.FieldCount; i++)
                    {
                        dt.Columns.Add(reader.GetName(i));
                    }
                    while (reader.Read() && reader.HasRows)
                    {
                        var row = dt.NewRow();
                        for (var i = 0; i < reader.FieldCount; i++)
                        {
                            if (reader.GetFieldType(i).Name == "Byte[]")
                            {
                                var strOfBytes = string.Empty;
                                if (string.IsNullOrEmpty(Convert.ToString(reader[i]))) continue;
                                var bytes = (byte[])reader[i];
                                for (var by = 0; by < bytes.Length; by++)
                                    strOfBytes = strOfBytes + bytes[by];
                                //row[i] = BitConverter.ToUInt64(bytes, 0);
                            }
                            else
                            {
                                row[i] = reader[i];
                            }
                          
                        }
                        dt.Rows.Add(row);
                    }
                }

                return dt;
            }
        }
    }

    public interface IDb
    {
        DataTable GetLogByTable(string tableName);
    }
}
