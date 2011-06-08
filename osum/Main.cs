using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace osum
{
    public class Application
    {
        static GameBase game;

        static void Main(string[] args)
        {
#if iOS
            game = new GameBaseIphone();
#else
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            game = new GameBaseDesktop();
#endif
        }

#if MONO
        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            File.WriteAllText("error.txt", e.ExceptionObject.ToString() + "\n\n" + OpenTK.Graphics.OpenGL.GL.GetString(OpenTK.Graphics.OpenGL.StringName.Version));
        }
#endif
    }
}

