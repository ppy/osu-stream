using osum.GameModes.Play.Components;
using osum.GameplayElements;

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