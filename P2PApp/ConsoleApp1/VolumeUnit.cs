using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PApp
{
    public enum Volume
    {
        Byte = 0,
        KB = 1,
        MB = 2,
        GB = 3
    }

    public static class VolumeUnit
    {
        public static string ToAutoByte(this long bytesize)
        {
            var temp = bytesize;
            for (int i = 0; i < bytesize.ToString().Length / 3; i++)
            {
                temp /= 1024;
            }
            string result = string.Format("{0:0,0}", temp) + ((Volume)(bytesize.ToString().Length / 3)).ToString();
            return result;
        }

        public static string ToKByte(this long bytesize)
        {
            var kbSize = (bytesize / 1024).ToString();
            string result = string.Format("{0:0,0}", long.Parse(kbSize)) + "KB";
            return result;
        }

        public static string ToMByte(this long bytesize)
        {
            var mbSize = (bytesize / 1024 / 1024).ToString();
            string result = string.Format("{0:0,0}", long.Parse(mbSize)) + "MB";
            return result;
        }

        public static string ToGByte(this long bytesize)
        {
            var gbSize = (bytesize / 1024 / 1024).ToString();
            string result = string.Format("{0:0,0}", long.Parse(gbSize)) + "GB";
            return result;
        }
    }
}
