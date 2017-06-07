using System;
using System.Configuration;
using System.IO;

namespace SQLSERVERLOG
{
    public class Utility
    {
        private const string keyConnStr = "ConnStr";

        public string TableDefineSql { get { return "GetDBLog.sql"; } }
        public string DBLogSql { get { return "GetTableDefine"; } }

        public string GetConfigConnStr()
        {
            var connStr = ConfigurationManager.AppSettings[keyConnStr];
            return connStr;
        }

        public string GetSQLFromFile(string fileName)
        {
            var sqlStr = string.Empty;
            using (var read = new StreamReader(AppDomain.CurrentDomain.BaseDirectory + "SQL\\" + fileName))
            {
                sqlStr = read.ToString().Trim();
            }
            return null;
        }
    }
}
