using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using osum.GameModes.SongSelect;

namespace osum.GameModes.Play
{
    class Tutorial : Player
    {
        BackButton backButton;

        internal override void Initialize()
        {
            MainMenu.InitializeBgm();

            backButton = new BackButton(delegate { Director.ChangeMode(OsuMode.MainMenu); });
            spriteManager.Add(backButton);

            base.Initialize();
        }

        protected override void initializeUIElements()
        {
            //base.initializeUIElements();
        }
    }
}
