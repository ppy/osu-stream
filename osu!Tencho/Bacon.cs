using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace osu_Tencho
{
    internal static class Bacon
    {
        //static readonly List<string> consoleText = new List<string>();
        private static bool requiredUpdate = true;
        static int width;
        static int height;
        private static string statusText = "Initialising...";
        public static bool LoggingEnabled;

        internal static void WriteLine(object s)
        {
            if (!LoggingEnabled)
                return;

            Console.WriteLine(s);
            return;
            //consoleText.Add(s.ToString());
            //requiredUpdate = true;
        }

        internal static void WriteSystem(object s)
        {
            Console.WriteLine("[sys] " + s);
        }

        internal static void WriteLine(string str)
        {
            if (!LoggingEnabled)
                return;
            
            Console.WriteLine(str);
        }

        internal static void WriteLine(string format, params object[] parms)
        {
            if (!LoggingEnabled)
                return;
            
            Console.WriteLine(format, parms);
        }

        internal static void Monitor()
        {
/*            MainThread = new Thread(Run);
            MainThread.IsBackground = true;
            MainThread.Start();*/
        }

        internal static void SetStatus(string status)
        {
            if (!LoggingEnabled)
                return;
            
            Console.WriteLine(status);
            statusText = status;
            requiredUpdate = true;
        }

/*        private static void Run()
        {
            while (true)
            {
                if (requiredUpdate)
                {
                    Console.CursorLeft = 0;
                    Console.CursorTop = 0;

                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.ForegroundColor = ConsoleColor.Gray;

                    int j = Math.Max(0, consoleText.Count - height);
                    for (int i = 0; i < height - 2; i++)
                    {
                        if (j < consoleText.Count)
                            WriteToConsole(consoleText[j++], i);
                        else
                            WriteToConsole("", i);
                    }

                    Console.BackgroundColor = ConsoleColor.DarkBlue;
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    WriteToConsole(statusText,height - 2);

                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.ForegroundColor = ConsoleColor.White;

                    WriteToConsole("", height-1);
                    requiredUpdate = false;
                }
                    
                Thread.Sleep(250);
            }
        }

        private static void WriteToConsole(string s, int line)
        {
            Console.SetCursorPosition(0, line);
            Console.Write(s.PadRight(line == height ? width - 1 : width));
        }*/
    }
}
