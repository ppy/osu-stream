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
using System.IO;
using OpenTK.Graphics.OpenGL;

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

            pSprite whiteLayer = pSprite.FullscreenWhitePixel;
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

            explosions.ForEach(s => {
                if (s.Transformations.Count == 0)
                    s.Transform(new TransformationBounce(Clock.ModeTime, Clock.ModeTime + 900, s.ScaleScalar, 0.1f, 2));
            });
		}

		public override void Draw()
		{
			base.Draw();

            float da = (float) (Math.PI/20);
            float startAngle = (float) (-Math.PI/2);
            
            float endAngle = (float)((((float)Clock.Time / 3000) % 2) * (2 * Math.PI) + startAngle);

            int parts = (int)((endAngle - startAngle) / da);
            
            float[] vertices = new float[parts * 2 + 2];
            float[] colours = new float[parts * 4 + 4];

            float radius = 200;

            float xsc = 200;
            float ysc = 200;

            vertices[0] = xsc;
            vertices[1] = ysc;

            float a = startAngle;
            for (int v = 1; v < parts + 1; v++)
            {
                vertices[v * 2] = (float)(xsc + Math.Cos(a)*radius);
                vertices[v * 2 + 1] = (float)(ysc + Math.Sin(a)*radius);
                a += da;

                colours[v * 4] = 1;
                colours[v * 4 + 1] = 1;
                colours[v * 4 + 2] = 1;
                colours[v * 4 + 3] = 1;
            }

            GL.EnableClientState(EnableCap.ColorArray);
            GL.EnableClientState(EnableCap.VertexArray);

		    GL.VertexPointer(2,VertexPointerType.Float, 0, vertices);
            GL.ColorPointer(4, ColorPointerType.Float, 0,colours);
            GL.DrawArrays(BeginMode.TriangleFan, 0, parts + 1);
			
			if (!Director.IsTransitioning)
				osuLogo.ScaleScalar = 1 + AudioEngine.Music.CurrentVolume/100;
		}
	}
}