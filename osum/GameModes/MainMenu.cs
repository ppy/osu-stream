using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using osum.GameModes;
using osum.Graphics.Sprites;
using osum.Graphics.Skins;
using osum.Helpers;
using OpenTK;
using OpenTK.Graphics;
using System.Drawing;
using osum.Audio;
using osum.Support;

namespace osum.GameModes
{
    class MainMenu : GameMode
    {
		pSprite osuLogo;

        int sampleTest;
		
		internal override void Initialize()
        {
            pSprite menuBackground =
                new pSprite(SkinManager.Load("menu-background"), FieldTypes.StandardSnapCentre, OriginTypes.Centre,
                            ClockTypes.Game, Vector2.Zero, 0, true, Color.White);
            spriteManager.Add(menuBackground);

            osuLogo = new pSprite(SkinManager.Load("menu-osu"), FieldTypes.StandardSnapCentre, OriginTypes.Centre, ClockTypes.Game, Vector2.Zero, 1, true, Color4.White);
            spriteManager.Add(osuLogo);
			
			osuLogo.Transform(new Transformation(TransformationType.Rotation,0,200,0,200000));

			
            sampleTest = AudioEngine.Effect.Load("Skins/Default/normal-hitclap.wav");

            //AudioEngine.Music.Load("test.mp3");

            InputManager.OnDown += new InputHandler(InputManager_OnDown);
        }
		
		public override void Dispose ()
		{
			InputManager.OnDown -= new InputHandler(InputManager_OnDown);
			
			base.Dispose();
		}
		
        void InputManager_OnDown(InputSource source, TrackingPoint point)
        {
            AudioEngine.Effect.PlayBuffer(sampleTest);

            osuLogo.Transform(new Transformation(TransformationType.Scale, 1, 4f, Clock.Time, Clock.Time + 1000, EasingTypes.In));
			
			Director.ChangeMode(OsuMode.Play, new FadeTransition());

            AudioEngine.Music.Play();
        }

        public override void Update()
        {
			base.Update();
        }

        public override void Draw()
        {
            base.Draw();
			
            if (!Director.IsTransitioning)
			    osuLogo.ScaleScalar = 1 + AudioEngine.Music.CurrentVolume/100;
        }
    }
}