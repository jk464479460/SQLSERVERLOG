using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SQLSERVERLOG;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace UnitTestProject1
{
    [TestClass]
    public class UnitTest1
    {
        private IUtility _Utility = new Utility();

        [TestMethod]
        public void TestMethod1()
        {
            IDb db = new Db();
            var dt=db.GetByTable<DBLog>("log_test", _Utility.GetSQLFromFile(_Utility.DBLogSql));
        }

        [TestMethod]
        public void TestTableDefine()
        {
            IDb db = new Db();
            var dt = db.GetByTable<TableDefine>("log_test", _Utility.GetSQLFromFile(_Utility.TableDefineSql));
        }

        [TestMethod]
        public void TestGetDatacolumnByTableDefine()
        {
            IDb db = new Db();
            var dt = db.GetByTable<TableDefine>("log_test", _Utility.GetSQLFromFile(_Utility.TableDefineSql));
            var datacolumns = _Utility.GetDatacolumn(dt);
        }

        [TestMethod]
        public void TestDataRecovery()
        {
            IDb db = new Db();
            var logData = db.GetByTable<DBLog>("log_test", _Utility.GetSQLFromFile(_Utility.DBLogSql));

            var dt = db.GetByTable<TableDefine>("log_test", _Utility.GetSQLFromFile(_Utility.TableDefineSql));

            foreach(var log in logData)
            {
                var dbName = "test";
                var pageId = Convert.ToInt32(log.PageId.Split(':')[1], 16);
                var sql = _Utility.GetSQLFromFile(_Utility.PageSql);
                sql = sql.Replace("<pageId>", $"{pageId}");
                sql = sql.Replace("<db>", dbName);
                var pageData = db.GetData<PageInfo>(sql);
                pageData = _Utility.FilterPageBySlot(log.SlotId, log.SlotId, pageData);
                if (log.Operation.Equals("LOP_MODIFY_ROW"))
                {
                    _Utility.ModifyRow(pageData, dt, log.Offset, log.R0, log.R1);
                    continue;
                }
                var bytesArr = log.R0;
                var datacolumns = _Utility.GetDatacolumn(dt);
                _Utility.TranslateData(bytesArr, ((List<Datacolumn>)datacolumns).ToArray());

                var sb = new StringBuilder();
                sb.Append($"Operation: {log.Operation}{Environment.NewLine}");
                foreach(var item in datacolumns)
                {
                    sb.Append($"name: {item.Name}  value: {item.Value}{Environment.NewLine}");
                }
                Trace.WriteLine(sb.ToString());
            }
        }

        [TestMethod]
        public void TestDBpage()
        {
            IDb db = new Db();
            var dbName = "test";
            var pageId= "89";
            var sql = _Utility.GetSQLFromFile(_Utility.PageSql);
            sql = sql.Replace("<pageId>", pageId);
            sql = sql.Replace("<db>", dbName);
            var pageData = db.GetData<PageInfo>(sql);
        }

        [TestMethod]
        public void TestFilterPageBySlot()
        {
            IDb db = new Db();
            var logData = db.GetByTable<DBLog>("log_test", _Utility.GetSQLFromFile(_Utility.DBLogSql));

            var dbName = "test";
            var pageId = "314";
            var sql = _Utility.GetSQLFromFile(_Utility.PageSql);
            sql = sql.Replace("<pageId>", pageId);
            sql = sql.Replace("<db>", dbName);
            var pageData = db.GetData<PageInfo>(sql);
            var dbLog = new DBLog();
            if (logData.Count > 0) dbLog = logData[0];
            var result=_Utility.FilterPageBySlot(dbLog.SlotId, dbLog.SlotId, pageData);
        }

    }
}
