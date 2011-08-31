using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using osum.Graphics.Sprites;
using osum.GameplayElements.Scoring;
using osum.GameModes.Play.Components;
using OpenTK;
using OpenTK.Graphics;

namespace osum.GameModes.Play
{
    class PreviewPlayer : Player
    {
        pText t_currentStream;

        public override void Initialize()
        {
            base.Initialize();

            t_currentStream = new pText(HitObjectManager.ActiveStream.ToString(), 64, new Vector2(20, 20), 1, true, Color4.White);
            t_currentStream.Field = FieldTypes.StandardSnapBottomRight;
            t_currentStream.Origin = OriginTypes.BottomRight;
            t_currentStream.TextShadow = true;
            spriteManager.Add(t_currentStream);
        }

        protected override void initializeUIElements()
        {
            healthBar = new HealthBar();
            healthBar.SetCurrentHp(200);

            comboCounter = new ComboCounter();
            streamSwitchDisplay = new StreamSwitchDisplay();
            countdown = new CountdownDisplay();

            progressDisplay = new ProgressDisplay();
        }

        protected override void loadBeatmap()
        {
            base.loadBeatmap();
            
            Difficulty = GameplayElements.Difficulty.Easy;
            //force back to stream difficulty, as it may be modified during load to get correct AR etc. variables.
        }

        protected override void hitObjectManager_OnStreamChanged(GameplayElements.Difficulty newStream)
        {
            base.hitObjectManager_OnStreamChanged(newStream);

            t_currentStream.Text = HitObjectManager.ActiveStream.ToString();
        }
    }
}
