using osum.Support;

#if !iOS
using osum.Support.Desktop;
#endif

namespace osum
{
    public class Application
    {
        private static void Main(string[] args)
        {
#if iOS
            GameBase game = new GameBaseIphone();
            game.Run();
#else
            GameBase game = new GameBaseDesktop();
            game.Run();
#endif
        }
    }
}

