using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using osum.Graphics.Sprites;
using osum.Graphics.Drawables;
using OpenTK;
using OpenTK.Graphics;
using osum.Graphics.Skins;
using osum.Helpers;
using osum.Audio;
using osum.GameplayElements;

namespace osum.GameModes.SongSelect
{
    class BackButton : pSprite
    {
        pSprite arrow;

        SpriteManager sm = new SpriteManager();

        EventHandler Action;
        const float offset = 30;

        int colourIndex;
        private double elapsedRotation;

        public BackButton(EventHandler action)
            : base(TextureManager.Load(OsuTexture.songselect_back_hexagon), FieldTypes.StandardSnapBottomLeft,
                OriginTypes.Centre, ClockTypes.Mode, new Vector2(offset, offset), 0.99f, true, TextureManager.DefaultColours[0])
        {
            AlwaysDraw = true;
            Alpha = 1;
            Action = action;
            HandleInput = true;

            HandleClickOnUp = true;

            OnClick += OnBackgroundOnClick;
            OnHover += delegate { FadeColour(ColourHelper.Lighten(Colour,0.5f), 100); };
            OnHoverLost += delegate { FadeColour(ColourHelper.Darken(Colour, 0.2f), 100); };
            arrow = new pSprite(TextureManager.Load(OsuTexture.songselect_back_arrow), FieldTypes.StandardSnapBottomLeft, OriginTypes.Centre, ClockTypes.Mode, new Vector2(offset + 15, offset + 18), 1, true, Color4.White);
            sm.Add(arrow);
        }

        void OnBackgroundOnClick(object sender, EventArgs e)
        {
            AudioEngine.PlaySample(OsuSamples.MenuBack);

            Transform(new TransformationBounce(Clock.ModeTime - 300, Clock.ModeTime + 700, 1, 1, 2));
            arrow.Transform(new TransformationBounce(Clock.ModeTime - 300, Clock.ModeTime + 700, 1, 1, 2));

            Action(sender, e);
        }

        public override void Dispose()
        {
            sm.Dispose();
            base.Dispose();
        }

        public override bool Draw()
        {
            if (!base.Draw())
                return false;

            sm.Draw();

            return true;
        }

        internal override bool IsOnScreen
        {
            get
            {
                return true;
            }
        }

        public override void Update()
        {
            base.Update();

            elapsedRotation += GameBase.ElapsedMilliseconds;
            arrow.Rotation += (float)(Math.Cos((elapsedRotation) / 1000f) * 0.0001 * GameBase.ElapsedMilliseconds);

            if (Transformations.Count == 0 && !IsHovering)
            {
                colourIndex = (colourIndex + 1) % TextureManager.DefaultColours.Length;
                FadeColour(TextureManager.DefaultColours[colourIndex],10000);
            }

            arrow.Alpha = this.Alpha;

            sm.Update();

            Rotation += (float)GameBase.ElapsedMilliseconds * 0.0005f;
        }
    }
}
