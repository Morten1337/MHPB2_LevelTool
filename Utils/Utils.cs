using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MHPB2_LevelTool
{
    class Utils
    {
        public static bool is_flag_set(uint src, uint flag)
        {
            if ((src & flag) != 0)
                return true;
            else
                return false;
        }

        public static string GetStringFromByteArray(byte[] array)
        {
            string returnString = "";
            foreach (byte element in array)
            {
                if (element == 0)
                    break;

                returnString += (char)element;
            }
            return returnString;
        }
    }
}
