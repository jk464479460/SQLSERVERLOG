using System;
using System.Configuration;
using System.IO;

namespace SQLSERVERLOG
{
    public class Utility: IUtility
    {
        private const string keyConnStr = "ConnStr";

        public string DBLogSql { get { return "GetDBLog.sql"; } }
        public string TableDefineSql { get { return "GetTableDefine"; } }

        public string GetConfigConnStr()
        {
            var connStr = ConfigurationManager.AppSettings[keyConnStr];
            return connStr;
        }

        public string GetSQLFromFile(string fileName)
        {
            var sqlStr = string.Empty;
            using (var read = new StreamReader(AppDomain.CurrentDomain.BaseDirectory + "\\SQL\\" + fileName))
            {
                sqlStr = read.ReadToEnd().ToString().Trim();
            }
            return sqlStr;
        }
    }

    public interface IUtility
    {
        string TableDefineSql { get; }
        string DBLogSql { get; }

        string GetConfigConnStr();
        string GetSQLFromFile(string fileName);
    }
}
