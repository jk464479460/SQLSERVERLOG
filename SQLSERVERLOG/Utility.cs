using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace SQLSERVERLOG
{
    public class Utility : IUtility
    {
        private const string keyConnStr = "ConnStr";

        public string DBLogSql { get { return "GetDBLog.sql"; } }

        public string TableDefineSql { get { return "GetTableDefine.sql"; } }

        public string PageSql
        {
            get
            {
                return "Page.sql";
            }
        }

        public string GetConfigConnStr()
        {
            var connStr = ConfigurationManager.AppSettings[keyConnStr];
            return connStr;
        }

        public IList<Datacolumn> GetDatacolumn(IList<TableDefine> tableSchema)
        {
            var result = new List<Datacolumn>();
            foreach (var item in tableSchema)
            {
                if(!item.TypeName.Equals("sysname", StringComparison.OrdinalIgnoreCase))
                    result.Add(new Datacolumn(item.ColName, GetDbType(item.TypeName), item.MaxLength));
            }
            return result;
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

        public void TranslateData(byte[] data, Datacolumn[] columns)
        {
            //http://blog.csdn.net/jinjazz/archive/2008/08/07/2783872.aspx
            //行数据从第5个字节开始
            short index = 4;
            //先取定长字段
            foreach (Datacolumn c in columns)
            {
                switch (c.DataType)
                {
                    case SqlDbType.NChar:
                        //读取定长字符串，需要根据表结构指定长度
                        c.Value = System.Text.Encoding.Default.GetString(data, index, c.Length);
                        index += c.Length;
                        break;
                    case SqlDbType.Char:
                        //读取定长字符串，需要根据表结构指定长度
                        c.Value = System.Text.Encoding.Default.GetString(data, index, c.Length);
                        index += c.Length;
                        break;
                    case SqlDbType.DateTime:
                        //读取datetime字段，sql为8字节保存
                        System.DateTime date = new DateTime(1900, 1, 1);
                        //前四位1/300秒保存
                        int second = BitConverter.ToInt32(data, index);
                        date = date.AddSeconds(second / 300);
                        index += 4;
                        //后四位1900-1-1的天数
                        int days = BitConverter.ToInt32(data, index);
                        date = date.AddDays(days);
                        index += 4;
                        c.Value = date;
                        break;
                    case SqlDbType.Int:
                        //读取int字段,为4个字节保存
                        c.Value = BitConverter.ToInt32(data, index);
                        index += 4;
                        break;
                    default:
                        //忽略不定长字段和其他不支持以及不愿意考虑的字段
                        break;
                }
            }
            //跳过三个字节
            index += 3;
            //取变长字段的数量,保存两个字节
            short varColumnCount = BitConverter.ToInt16(data, index);
            index += 2;
            //接下来,每两个字节保存一个变长字段的结束位置,
            //所以第一个变长字段的开始位置可以算出来
            short startIndex = (short)(index + varColumnCount * 2);
            //第一个变长字段的结束位置也可以算出来
            short endIndex = BitConverter.ToInt16(data, index);
            //循环变长字段列表读取数据
            foreach (Datacolumn c in columns)
            {
                switch (c.DataType)
                {
                    case SqlDbType.VarChar:
                        //根据开始和结束位置，可以算出来每个变长字段的值
                        c.Value = System.Text.Encoding.Default.GetString(data, startIndex, endIndex - startIndex);
                        //下一个变长字段的开始位置
                        startIndex = endIndex;
                        //获取下一个变长字段的结束位置
                        index += 2;
                        endIndex = BitConverter.ToInt16(data, index);
                        break;
                    case SqlDbType.NVarChar:
                        if(data.Length - startIndex >= endIndex - startIndex)
                            c.Value = System.Text.Encoding.Default.GetString(data, startIndex, endIndex - startIndex);
                        startIndex = endIndex;
                        index += 2;
                        if(data.Length > index)
                        endIndex = BitConverter.ToInt16(data, index);
                        break;
                    default:
                        //忽略定长字段和其他不支持以及不愿意考虑的字段
                        break;
                }
            }
            //获取完毕
        }

        //LOP_MODIFY_ROW
        public void ModifyRow(IList<PageInfo> pageSlotInfoList, IList<TableDefine> tableSchema, int offSet,byte[] data0, byte[] data1)
        {
            if (pageSlotInfoList.Count == 0) throw new NotSupportedException();
            if (tableSchema.Count == 0) throw new NotSupportedException();

            var currentFieldOffset = -9999;
            var currentField = new PageInfo();

            foreach (var pageSlot in pageSlotInfoList)
            {
                var arrObject = pageSlot.Object.Split(' ');
                if (arrObject.Length < 6) continue;
                if (string.IsNullOrEmpty(arrObject[5])) continue;
                
                var tempOffset= ConvHexStrtoDEC(arrObject[5]);
                if (offSet >= tempOffset && currentFieldOffset<=tempOffset)
                {
                    currentFieldOffset = tempOffset;
                    currentField = pageSlot;
                } 
            }

            if (currentFieldOffset == -9999)
                throw new NotSupportedException();

            var relativeOffsetInField = offSet - currentFieldOffset;
            var colType = tableSchema.Where(x => x.ColName.Equals(currentField.Field, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();

            if (colType == null) throw new NotSupportedException();

            
            switch (colType.TypeName)
            {
                case "datetime":
                    TranslateDT(relativeOffsetInField, currentField, data0, data1);
                    break;
                case "char":
                case "varchar":
                    var currentValueOfColumn = currentField.Value;
                    var data0Str = Encoding.Default.GetString(data0);
                    var arr=currentValueOfColumn.ToArray();
                    var sb = new StringBuilder();

                    for (var i = 0; i < data0Str.Length; i++)
                    {
                        arr[relativeOffsetInField + i] = data0Str[i];
                    }
                    sb.Append($"{currentField.Field}\n");
                    sb.Append($"\t{string.Join("",arr)} \t{currentValueOfColumn}");
                    Trace.WriteLine(sb.ToString());
                    currentField.Value=string.Join("", arr);
                    break;
            }
        }

        public string ConvDECtoHexStr(int hex)
        {
            var hexStr = Convert.ToString(hex, 16);
            if (hexStr.Length % 2 != 0) hexStr = "0" + hexStr;
            return hexStr;
        }

        public int ConvHexStrtoDEC(string hex)
        {
            var res = Convert.ToInt32(hex, 16);
            return res;
        }

        public IList<PageInfo> FilterPageBySlot(int parentSlot, int slot, IList<PageInfo> pageInfo)
        {
            var result = new List<PageInfo>();
            foreach(var page in pageInfo)
            {
                var parentObject = page.ParentObject;
                var obj = page.Object;

                var arr = parentObject.Split(' ');
                if (arr.Length < 2) continue;
                if (!arr[0].Equals("slot", StringComparison.OrdinalIgnoreCase)) continue;
                if (!arr[1].Equals($"{parentSlot}", StringComparison.OrdinalIgnoreCase)) continue;

                arr = obj.Split(' ');
                if (arr.Length < 2) continue;
                if (!arr[0].Equals("slot", StringComparison.OrdinalIgnoreCase)) continue;
                if (!arr[1].Equals($"{slot}", StringComparison.OrdinalIgnoreCase)) continue;
                result.Add(page);
            }
            return result;
        }

        public byte[] strToToHexByte(string hexString)
        {
            hexString = hexString.Replace("0x", "");

            hexString = hexString.Replace(" ", "");
            if ((hexString.Length % 2) != 0)
                hexString += " ";
            byte[] returnBytes = new byte[hexString.Length / 2];
            for (int i = 0; i < returnBytes.Length; i++)
            {
                var subHexStr = hexString.Substring(i * 2, 2);
                returnBytes[i] = Convert.ToByte(subHexStr.Replace(" ", ""), 16);
            }

            return returnBytes;
        }

        #region local
        private SqlDbType GetDbType(string name)
        {
            switch (name)
            {
                case "int":
                    return SqlDbType.Int;
                case "char":
                    return SqlDbType.Char;
                case "varchar":
                    return SqlDbType.VarChar;
                case "datetime":
                    return SqlDbType.DateTime;
                case "nchar":
                    return SqlDbType.NChar;
                case "nvarchar":
                    return SqlDbType.NVarChar;
            }
            throw new NotSupportedException();
        }

        private void TranslateDT(int relativeOffsetInField, PageInfo currentField, byte[] data0, byte[] data1)
        {
            var result = "00";
            var result2 = "00";
            var skipIndex = 0;
            var skipIndex1 = 0;
            var afterValue = DateTime.Parse(currentField.Value);

            if (data0.Length > 4) skipIndex = 4;
            if (data1.Length > 4) skipIndex1 = 4;

            var sb = new StringBuilder();
            var currentValueOfColumn = currentField.Value;

            if (relativeOffsetInField == 0) //seconds
            {
                for (var i = (skipIndex == 4 ? skipIndex - 1 : data0.Length - 1); i >= 0; i--)
                {
                    result = $"{result}{ConvDECtoHexStr(data0[i])}";
                }

                for (var i = (skipIndex1 == 4 ? skipIndex1 - 1 : data1.Length - 1); i >= 0; i--)
                {
                    result2 = $"{result2}{ConvDECtoHexStr(data1[i])}";
                }

                var diffDays = Convert.ToInt64(result2, 16) - Convert.ToInt64(result, 16);

                var beforeValue = afterValue.AddSeconds(-diffDays / 300);

                if (skipIndex != 4 || skipIndex1 != 4)
                {
                    sb.Append($"{currentField.Field}\n");
                    sb.Append($"\t{beforeValue} \t{currentValueOfColumn}");
                    Trace.WriteLine(sb.ToString());
                }

                currentField.Value = beforeValue.ToString("yyyy-MM-dd HH:mm:ss");
                afterValue = DateTime.Parse(currentField.Value);
            }
            if (relativeOffsetInField == 4 || skipIndex == 4 || skipIndex1 == 4) //days
            {
                result = "00";
                result2 = "00";
                for (var i = data0.Length - 1; i >= skipIndex; i--)
                {
                    result = $"{result}{ConvDECtoHexStr(data0[i])}";
                }

                for (var i = data1.Length - 1; i >= skipIndex1; i--)
                {
                    result2 = $"{result2}{ConvDECtoHexStr(data1[i])}";
                }

                var diffDays = Convert.ToInt64(result2, 16) - Convert.ToInt64(result, 16);
                var beforeValue = afterValue.AddDays(-diffDays);

                sb.Append($"{currentField.Field}\n");
                sb.Append($"\t{beforeValue} \t{currentValueOfColumn}");
                Trace.WriteLine(sb.ToString());
                currentField.Value = beforeValue.ToString("yyyy-MM-dd HH:mm:ss");
            }
        }

        #endregion
    }

    public interface IUtility
    {
        string TableDefineSql { get; }
        string DBLogSql { get; }
        string PageSql { get; }

        string GetConfigConnStr();
        string GetSQLFromFile(string fileName);

        void TranslateData(byte[] data, Datacolumn[] columns);
        IList<Datacolumn> GetDatacolumn(IList<TableDefine> tableSchema);
        void ModifyRow(IList<PageInfo> pageSlotInfoList, IList<TableDefine> tableSchema, int offSet, byte[] data0, byte[] data1);

        IList<PageInfo> FilterPageBySlot(int parentSlot, int slot, IList<PageInfo> pageInfo);

        string ConvDECtoHexStr(int hex);
        int ConvHexStrtoDEC(string hex);
        byte[] strToToHexByte(string hexString);
    }
}
