using System;
using osum.GameModes.SongSelect;

namespace osum.GameModes.Options
{
    public class Options : GameMode
    {
        BackButton s_ButtonBack;

        public override void Initialize()
        {
            s_ButtonBack = new BackButton(delegate { Director.ChangeMode(OsuMode.MainMenu); });
            spriteManager.Add(s_ButtonBack);
        }
    }
}

