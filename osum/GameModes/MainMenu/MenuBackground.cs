using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using osum.Graphics.Sprites;
using osum.Graphics.Drawables;
using OpenTK;
using OpenTK.Graphics;
using osum.Graphics.Primitives;
using osum.Helpers;
using osum.Audio;
using osum.Graphics.Skins;
using osum.Graphics;

namespace osum.GameModes
{
    class MenuBackground : SpriteManagerDraggable
    {
        Vector2 centre = new Vector2(320, 200);
        private pQuad yellow;
        private pQuad orange;
        private pQuad blue;
        private pQuad pink;

        Source whoosh;

        public MenuBackground()
        {
            whoosh = AudioEngine.Effect.LoadBuffer(AudioEngine.LoadSample(OsuSamples.MenuWhoosh), 1, false, true);

            EndStopLenience = 0.5f;
            EndBufferZone = 0;
            AutomaticHeight = false;
            Scrollbar = false;
            ExactCoordinates = false;
            LockHorizontal = false;
            CheckSpritesAreOnScreenBeforeRendering = false;

            rectangleLineLeft = new Line(new Vector2(114, 55) - centre, new Vector2(169, 362) - centre);
            rectangleLineRight = new Line(new Vector2(-100, -855) - centre, new Vector2(1200, 250) - centre);

            //if (!GameBase.IsSlowDevice)
            {
                rectBorder = new pQuad(
                    rectangleLineLeft.p1 + new Vector2(-2, -2),
                    new Vector2(444 + 2, 172 - 2) - centre,
                    rectangleLineLeft.p2 + new Vector2(-2, 2),
                    new Vector2(528 + 3, 297 + 2) - centre,
                    true, 0.4f, new Color4(21, 21, 22, 255));
                rectBorder.AlphaBlend = false;
                rectBorder.Field = FieldTypes.StandardSnapCentre;
                rectBorder.Origin = OriginTypes.Centre;
                Add(rectBorder);
            }

            rect = new pQuad(
                rectangleLineLeft.p1,
                new Vector2(444, 172) - centre,
                rectangleLineLeft.p2,
                new Vector2(528, 297) - centre,
                true, 0.42f, new Color4(33, 35, 42, 255));
            rect.Field = FieldTypes.StandardSnapCentre;
            rect.AlphaBlend = false;
            rect.Origin = OriginTypes.Centre;
            rect.Colours = new Color4[] {
                new Color4(40,43,52,255),
                new Color4(38,40,48,255),
                new Color4(41,43,51,255),
                new Color4(29,30,34,255)
            };
            Add(rect);

            pTexture specialTexture = TextureManager.Load(OsuTexture.menu_item_background);
            specialTexture.X++;
            specialTexture.Width -= 2;

            //osuLogo = new pSprite(TextureManager.Load(OsuTexture.menu_logo), new Vector2(-135, 28))
            //{
            //    Colour = new Color4(254, 242, 0, 255),
            //    Field = FieldTypes.StandardSnapCentre,
            //    Origin = OriginTypes.Centre,
            //    Rotation = -rotation_offset,
            //    ScaleScalar = 1 / scale_offset,
            //    Alpha = 0
            //};
            //Add(osuLogo);

            yellow = new pQuad(
                Vector2.Zero,
                Vector2.Zero,
                Vector2.Zero,
                Vector2.Zero,
                true, 0.5f, new Color4(254, 242, 0, 255));
            yellow.Tag = yellow.Colour;
            yellow.HandleClickOnUp = true;
            yellow.Texture = specialTexture;
            yellow.Field = FieldTypes.StandardSnapCentre;
            yellow.Origin = OriginTypes.Centre;
            yellow.OnClick += Option_OnClick;
            yellow.OnHover += Option_OnHover;
            yellow.OnHoverLost += Option_OnHoverLost;
            Add(yellow);

            orange = new pQuad(
                Vector2.Zero,
                Vector2.Zero,
                Vector2.Zero,
                Vector2.Zero,
                true, 0.5f, new Color4(255, 102, 0, 255));
            orange.Tag = orange.Colour;
            orange.HandleClickOnUp = true;
            orange.Texture = specialTexture;
            orange.Field = FieldTypes.StandardSnapCentre;
            orange.Origin = OriginTypes.Centre;
            orange.OnClick += Option_OnClick;
            orange.OnHover += Option_OnHover;
            orange.OnHoverLost += Option_OnHoverLost;

            Add(orange);

            blue = new pQuad(
                Vector2.Zero,
                Vector2.Zero,
                Vector2.Zero,
                Vector2.Zero,
                true, 0.5f, new Color4(0, 192, 245, 255));
            blue.Tag = blue.Colour;
            blue.HandleClickOnUp = true;
            blue.Texture = specialTexture;
            blue.Field = FieldTypes.StandardSnapCentre;
            blue.Origin = OriginTypes.Centre;
            blue.OnClick += Option_OnClick;
            blue.OnHover += Option_OnHover;
            blue.OnHoverLost += Option_OnHoverLost;
            Add(blue);

            pink = new pQuad(
                Vector2.Zero,
                Vector2.Zero,
                Vector2.Zero,
                Vector2.Zero,
                true, 0.5f, new Color4(237, 0, 140, 255));
            pink.Texture = specialTexture;
            pink.Tag = pink.Colour;
            pink.HandleClickOnUp = true;
            pink.Field = FieldTypes.StandardSnapCentre;
            pink.Origin = OriginTypes.Centre;
            pink.OnClick += Option_OnClick;
            pink.OnClick += Option_OnClick;
            pink.OnHover += Option_OnHover;
            pink.OnHoverLost += Option_OnHoverLost;
            Add(pink);

            ScaleScalar = 1.4f;

            pSprite text = new pSprite(TextureManager.Load(OsuTexture.menu_tutorial), new Vector2(-66, 3));
            text.Field = FieldTypes.StandardSnapCentre;
            text.Origin = OriginTypes.Centre;
            text.Rotation = -rotation_offset;
            text.ScaleScalar = 1/scale_offset;
            text.Alpha = 0;
            Add(text);
            textSprites.Add(text);

            text = new pSprite(TextureManager.Load(OsuTexture.menu_play), new Vector2(-48, 23.5f));
            text.Field = FieldTypes.StandardSnapCentre;
            text.Origin = OriginTypes.Centre;
            text.Rotation = -rotation_offset;
            text.ScaleScalar = 1 / scale_offset;
            text.Alpha = 0;
            Add(text);
            textSprites.Add(text);

            text = new pSprite(TextureManager.Load(OsuTexture.menu_store), new Vector2(-43, 48));
            text.Field = FieldTypes.StandardSnapCentre;
            text.Origin = OriginTypes.Centre;
            text.Rotation = -rotation_offset;
            text.ScaleScalar = 1 / scale_offset;
            text.Alpha = 0;
            Add(text);
            textSprites.Add(text);

            text = new pSprite(TextureManager.Load(OsuTexture.menu_options), new Vector2(-44, 74));
            text.Field = FieldTypes.StandardSnapCentre;
            text.Origin = OriginTypes.Centre;
            text.Rotation = -rotation_offset;
            text.ScaleScalar = 1 / scale_offset;
            text.Alpha = 0;
            Add(text);
            textSprites.Add(text);
        }

        List<pSprite> textSprites = new List<pSprite>();

        void Option_OnHoverLost(object sender, EventArgs e)
        {
            pDrawable d = sender as pDrawable;

            //if (d == yellow)
            //    osuLogo.FadeColour((Color4)d.Tag, 600);

            d.FadeColour((Color4)d.Tag, 600);
            //d.FadeColour(ColourHelper.Darken(d.Colour, 0.5f), 50);
        }

        void Option_OnHover(object sender, EventArgs e)
        {
            pDrawable d = sender as pDrawable;

            //if (d == yellow)
            //    osuLogo.FadeColour(Color4.White, 150);

            d.FadeColour(Color4.White, 150);
            //d.FadeColour(ColourHelper.Lighten(d.Colour, 0.5f),50);
        }

        public override void Dispose()
        {
            if (whoosh != null)
            {
                whoosh.Reserved = false;
                whoosh.Disposable = true;
                whoosh = null;
            }
            base.Dispose();
        }

        void Option_OnClick(object sender, EventArgs e)
        {
            if (!IsAwesome)
                return;

            pDrawable d = sender as pDrawable;

            d.Colour = Color4.White;
            d.FadeColour((Color4)d.Tag, 600);

            ScaleTo(1.3f, 600, EasingTypes.None);
            MoveTo(new Vector2(-75, 14), 600, EasingTypes.None);

            AudioEngine.PlaySample(OsuSamples.MenuHit);

            if (sender == yellow)
                Director.ChangeMode(OsuMode.Tutorial);
            else if (sender == orange)
                Director.ChangeMode(OsuMode.SongSelect);
            else if (sender == blue)
                Director.ChangeMode(OsuMode.Store);
            else
            {
                Director.ChangeMode(OsuMode.Options);
            }
        }

        int awesomeStartTime = -1;
        private Line rectangleLineLeft;
        private TransformationBounce awesomeTransformation;
        private Line rectangleLineRight;
        const int duration = 3000;

        const float rotation_offset = 0.35f;
        const float scale_offset = 4.2f;

        internal void BeAwesome()
        {
            GameBase.Scheduler.Add(delegate
            {
                if (whoosh != null) whoosh.Play();

                ScaleTo(scale_offset, duration / 2, EasingTypes.InOut);
                MoveTo(new Vector2(75, -44), duration / 2, EasingTypes.InOut);
                RotateTo(rotation_offset, duration / 2, EasingTypes.InOut);

                rectBorder.FadeOut(duration);
            }, 200);

            awesomeStartTime = Clock.ModeTime;
            awesomeTransformation = new TransformationBounce(Clock.ModeTime, Clock.ModeTime + duration / 3, 1, 0.6f, 6);

            if (textSprites != null) textSprites.ForEach(s => s.FadeIn(500));
        }

        bool first = true;
        private pQuad rectBorder;
        private pQuad rect;
        public override void Update()
        {
            if (awesomeTransformation != null || first)
            {
                if (awesomeTransformation != null)
                    awesomeTransformation.Update(ClockingNow);

                float progress = awesomeTransformation == null ? 0 : awesomeTransformation.CurrentFloat;
                yellow.p1 = rectangleLineLeft.PositionAt(0.575f + 0.08f * progress);
                yellow.p2 = rectangleLineRight.PositionAt(0.575f + 0.08f * progress);
                yellow.p3 = rectangleLineLeft.PositionAt(0.58f + 0.12f * progress);
                yellow.p4 = rectangleLineRight.PositionAt(0.58f + 0.12f * progress);

                orange.p1 = rectangleLineLeft.PositionAt(0.69f + 0.02f * progress);
                orange.p2 = rectangleLineRight.PositionAt(0.69f + 0.02f * progress);
                orange.p3 = rectangleLineLeft.PositionAt(0.73f + 0.02f * progress);
                orange.p4 = rectangleLineRight.PositionAt(0.73f + 0.02f * progress);

                blue.p1 = rectangleLineLeft.PositionAt(0.785f - 0.025f * progress);
                blue.p2 = rectangleLineRight.PositionAt(0.785f - 0.025f * progress);
                blue.p3 = rectangleLineLeft.PositionAt(0.79f + 0.01f * progress);
                blue.p4 = rectangleLineRight.PositionAt(0.79f + 0.01f * progress);

                pink.p1 = rectangleLineLeft.PositionAt(0.82f - 0.01f * progress);
                pink.p2 = rectangleLineRight.PositionAt(0.82f - 0.01f * progress);
                pink.p3 = rectangleLineLeft.PositionAt(0.825f + 0.03f * progress);
                pink.p4 = rectangleLineRight.PositionAt(0.825f + 0.03f * progress);

                Rotation *= 0.9f;

                if (awesomeTransformation != null && awesomeTransformation.Terminated)
                    awesomeTransformation = null;
                first = false;
            }

            base.Update();
        }

        public bool IsBeingAwesome { get { return awesomeTransformation != null; } }
        public bool IsAwesome { get { return awesomeStartTime >= 0 && Clock.ModeTime - awesomeStartTime > 50; } }
    }
}
