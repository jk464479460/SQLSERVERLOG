using System;

namespace Console_0
{
    class Program
    {
        static void Main(string[] args)
        {
            var input = new string[] { "B0" ,"82" ,"CC" ,"00" ,"5A", "A8" };
            System.DateTime date = new DateTime(2017, 12, 31, 12, 24,36);
            date=date.AddDays(-43098);

          

        }

        public static byte[] strToToHexByte(string hexString)
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

        static void T1()
        {
            ////读取datetime字段，sql为8字节保存
            //System.DateTime date = new DateTime(1900, 1, 1);
            ////前四位1/300秒保存
            //int second = BitConverter.ToInt32(data, index);
            //date = date.AddSeconds(second / 300);
            //index += 4;
            ////后四位1900-1-1的天数
            //int days = BitConverter.ToInt32(data, index);
            //date = date.AddDays(days);
            //index += 4;
            //c.Value = date;

            //0x B0 82 CC 00   5A A8

        }
    }
}
