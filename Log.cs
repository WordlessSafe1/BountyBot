using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BountyBot
{
    public static class Log
    {
        const int logTypeLength = 10;
        const int tagLength = 5;
        public static void Out(string logType, string tag, ConsoleColor tagColor, string message)
        {
            if (logType.Length > logTypeLength)
                logType = logType[..(logTypeLength - 1)];
            logType = logType.PadRight(logTypeLength, ' ');
            if (tag.Length > tagLength)
                tag = tag[..(tagLength - 1)];
            tag = tag.PadRight(tagLength, ' ');
            Console.Write("[{0:yyyy-MM-dd H:mm:ss zzz}] [Custom.{1}] ", DateTime.Now, logType);
            Console.ForegroundColor = tagColor;
            Console.Write("[{0}] ", tag);
            Console.ResetColor();
            Console.WriteLine(message);
        }
    }
}
