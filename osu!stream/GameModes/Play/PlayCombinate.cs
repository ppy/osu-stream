using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using osum.GameModes.Play.Components;
using osum.GameplayElements;
using osum.GameplayElements.Scoring;

namespace osum.GameModes.Play
{
    public class PlayCombinate : Player
    {
        protected override void initializeUIElements()
        {
            if (Difficulty != Difficulty.Easy) healthBar = new HealthBar();
            comboCounter = new ComboCounter();
        }
    }
}
