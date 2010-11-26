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
using osum.Graphics;

namespace osum.GameModes
{
	class MainMenu : GameMode
	{
		pSprite osuLogo;

        List<pSprite> explosions = new List<pSprite>();

        const int initial_display = 1300;

		internal override void Initialize()
		{
			menuBackground =
				new pSprite(TextureManager.Load(@"menu-background"), FieldTypes.StandardSnapCentre, OriginTypes.Centre,
							ClockTypes.Mode, Vector2.Zero, 0, true, Color.White);
			spriteManager.Add(menuBackground);

			osuLogo = new pSprite(TextureManager.Load(@"menu-osu"), FieldTypes.StandardSnapCentre, OriginTypes.Centre, ClockTypes.Mode, Vector2.Zero, 0.9f, true, Color4.White);
            osuLogo.Transform(new TransformationBounce(initial_display, initial_display + 2000, 1, 0.4f, 2));
			spriteManager.Add(osuLogo);
			
            pSprite explosion = new pSprite(TextureManager.Load(@"menu-explosion"), FieldTypes.StandardSnapCentre, OriginTypes.Centre, ClockTypes.Mode, new Vector2(-110,-110), 0.8f, true, new Color4(252,6,127,255));
            explosion.Transform(new TransformationBounce(initial_display + 50, initial_display + 2600, 1, 1f, 7));
            explosions.Add(explosion);
            spriteManager.Add(explosion);

            explosion = new pSprite(TextureManager.Load(@"menu-explosion"), FieldTypes.StandardSnapCentre, OriginTypes.Centre, ClockTypes.Mode, new Vector2(140, 10), 0.8f, true, new Color4(255, 212, 27, 255));
            explosion.Transform(new TransformationBounce(initial_display + 200, initial_display + 2900, 1.4f, 1.4f, 8));
            explosions.Add(explosion);
            spriteManager.Add(explosion);

            explosion = new pSprite(TextureManager.Load(@"menu-explosion"), FieldTypes.StandardSnapCentre, OriginTypes.Centre, ClockTypes.Mode, new Vector2(-120, 60), 0.8f, true, new Color4(29, 209, 255, 255));
            explosion.Transform(new TransformationBounce(initial_display + 400, initial_display + 3200, 1.2f, 1.7f, 5));
            explosions.Add(explosion);
            spriteManager.Add(explosion);


            Transformation fadeIn = new Transformation(TransformationType.Fade, 0, 1, initial_display, initial_display);
            spriteManager.Sprites.ForEach(s => s.Transform(fadeIn));


            pSprite whiteLayer =
    new pSprite(pTexture.FromRawBytes(new byte[] { 255, 255, 255, 255 }, 1, 1), FieldTypes.Standard, OriginTypes.TopLeft, ClockTypes.Mode, Vector2.Zero, 1, false, Color4.White);
            whiteLayer.Scale = new Vector2(GameBase.WindowBaseSize.Width, GameBase.WindowBaseSize.Height) / GameBase.SpriteRatioToWindowBase;
            spriteManager.Add(whiteLayer);

            whiteLayer.Transform(new Transformation(TransformationType.Fade, 0, 0.2f, 300, 500));
            whiteLayer.Transform(new Transformation(TransformationType.Fade, 0.2f, 1, initial_display - 100, initial_display));
            whiteLayer.Transform(new Transformation(TransformationType.Fade, 1, 0, initial_display, initial_display + 1200, EasingTypes.In));

			InputManager.OnDown += new InputHandler(InputManager_OnDown);
		}
		
		public override void Dispose ()
		{
			InputManager.OnDown -= new InputHandler(InputManager_OnDown);
			
			base.Dispose();
		}
		
		void InputManager_OnDown(InputSource source, TrackingPoint point)
		{
			if (!Director.IsTransitioning)
			{
				AudioEngine.PlaySample(OsuSamples.MenuHit);

                osuLogo.Transform(new Transformation(TransformationType.Scale, 1, 4f, Clock.ModeTime, Clock.ModeTime + 1000, EasingTypes.In));
                osuLogo.Transform(new Transformation(TransformationType.Rotation, 0, 1.4f, Clock.ModeTime, Clock.ModeTime + 1000, EasingTypes.In));

				Director.ChangeMode(OsuMode.SongSelect, new FadeTransition());
			}
		}

        double elapsedRotation;
        float startingRotation = 5;
        bool finishedSpinIn;
        private pSprite menuBackground;

		public override void Update()
		{
			base.Update();

            if (Clock.ModeTime > initial_display)
            {
                elapsedRotation += GameBase.ElapsedMilliseconds;
                osuLogo.Rotation += (float) (Math.Cos((elapsedRotation)/1000f)*0.0001 * GameBase.ElapsedMilliseconds);
            }

		    int track = 0;
            explosions.ForEach(s => {
                if (s.Transformations.Count == 0)
                    s.Transform(new TransformationBounce(Clock.Time, Clock.Time + 900, s.ScaleScalar, 0.1f, 2));
            });
		}

		public override void Draw()
		{
			base.Draw();
			
			if (!Director.IsTransitioning)
				osuLogo.ScaleScalar = 1 + AudioEngine.Music.CurrentVolume/100;
		}
	}
}