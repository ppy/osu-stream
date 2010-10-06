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

		internal override void Initialize()
        {
            const int initial_display = 1300;
            
            pSprite whiteLayer =
                new pSprite(pTexture.FromRawBytes(new byte[] { 255, 255, 255, 255 }, 1, 1), FieldTypes.Standard, OriginTypes.TopLeft, ClockTypes.Game, Vector2.Zero, 1, false, Color4.White);
            whiteLayer.Scale = new Vector2(GameBase.WindowBaseSize.Width, GameBase.WindowBaseSize.Height) / GameBase.SpriteRatioToWindowBase;
            spriteManager.Add(whiteLayer);

            whiteLayer.Transform(new Transformation(TransformationType.Fade,0,0.2f,300,500));
            whiteLayer.Transform(new Transformation(TransformationType.Fade, 0.2f, 1, initial_display - 100, initial_display));
            whiteLayer.Transform(new Transformation(TransformationType.Fade, 1, 0, initial_display, initial_display + 500));
            
            pSprite menuBackground =
                new pSprite(TextureManager.Load("menu-background"), FieldTypes.StandardSnapCentre, OriginTypes.Centre,
                            ClockTypes.Game, Vector2.Zero, 0, true, Color.White);
            spriteManager.Add(menuBackground);

            osuLogo = new pSprite(TextureManager.Load("menu-osu"), FieldTypes.StandardSnapCentre, OriginTypes.Centre, ClockTypes.Game, Vector2.Zero, 1, true, Color4.White);
            spriteManager.Add(osuLogo);
			
            osuLogo.Transform(new Transformation(TransformationType.Fade, 0, 1, initial_display, initial_display));
            menuBackground.Transform(new Transformation(TransformationType.Fade, 0, 1, initial_display, initial_display));

			
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
            AudioEngine.PlaySample(OsuSamples.MenuHit);

            osuLogo.Transform(new Transformation(TransformationType.Scale, 1, 4f, Clock.Time, Clock.Time + 1000, EasingTypes.In));
			
			Director.ChangeMode(OsuMode.SongSelect, new FadeTransition());

            AudioEngine.Music.Play();
        }

        public override void Update()
        {
			base.Update();

            osuLogo.Rotation += (float)(GameBase.ElapsedMilliseconds * 0.0001);
        }

        public override void Draw()
        {
            base.Draw();
			
            if (!Director.IsTransitioning)
			    osuLogo.ScaleScalar = 1 + AudioEngine.Music.CurrentVolume/100;
        }
    }
}