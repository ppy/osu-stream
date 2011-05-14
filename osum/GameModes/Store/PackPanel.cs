using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using osum.Graphics.Sprites;
using OpenTK.Graphics;
using osum.Graphics.Drawables;
using OpenTK;
using System.IO;
using System.Text.RegularExpressions;
using osum.Graphics.Renderers;
using osum.Graphics.Skins;
using osum.Helpers;

namespace osum.GameModes.Store
{
    internal class PackPanel : pSpriteCollection
    {
        internal pDrawable s_BackingPlate;
        internal pText s_Text;
        internal pText s_TextArtist;
        internal pSprite s_Thumbnail;

        float base_depth = 0.6f;

        static Color4 colourNormal = new Color4(50, 50, 50, 255);
        static Color4 colourHover = new Color4(28, 139, 242, 255);
        static Color4 colourHover2 = new Color4(0, 77, 164, 255);

        internal const int PANEL_HEIGHT = 60;

        internal float Height = PANEL_HEIGHT;
        

        public PackPanel(string packTitle, string price, EventHandler action)
        {
            //base_depth += 0.001f * index;

            s_BackingPlate = new pRectangle(Vector2.Zero, new Vector2(GameBase.BaseSize.Width, PANEL_HEIGHT), true, base_depth, colourNormal);
            Sprites.Add(s_BackingPlate);


            s_BackingPlate.OnClick += delegate {
                s_BackingPlate.FadeColour(colourHover2, 80);
                s_BackingPlate.HandleInput = false;

                if (!isPreviewing)
                    songPreviewBacks[0].Click();
            };
            s_BackingPlate.OnClick += action;

            s_BackingPlate.HandleClickOnUp = true;

            s_BackingPlate.OnHover += delegate { s_BackingPlate.FadeColour(colourHover2, 80); };
            s_BackingPlate.OnHoverLost += delegate { s_BackingPlate.FadeColour(colourNormal, 80); };

            s_Text = new pText(packTitle, 32, Vector2.Zero, new Vector2(GameBase.BaseSize.Width, PANEL_HEIGHT), base_depth + 0.01f, true, Color4.White, false);
            s_Text.Bold = true;
            s_Text.Offset = new Vector2(74, 14);
            if (s_Text.Texture != null)
                Sprites.Add(s_Text);

            s_TextArtist = new pText(price, 56, Vector2.Zero, Vector2.Zero, base_depth + 0.01f, true, new Color4(255, 255, 255, 128), false);
            s_TextArtist.TextAlignment = TextAlignment.Right;
            s_TextArtist.Origin = OriginTypes.TopRight;
            s_TextArtist.Offset = new Vector2(GameBase.BaseSize.Width, 7);
            Sprites.Add(s_TextArtist);

            s_Thumbnail = new pSprite(TextureManager.Load(OsuTexture.songselect_thumbnail), Vector2.Zero) { DrawDepth = base_depth + 0.02f };
            s_Thumbnail.Offset = new Vector2(2, 2);
            Sprites.Add(s_Thumbnail);
        }

        List<pDrawable> songPreviewBacks = new List<pDrawable>();
        List<pSprite> songPreviewButtons = new List<pSprite>();

        bool isPreviewing;

        internal void ResetPreviews()
        {
            isPreviewing = false;

            foreach (pDrawable p in songPreviewBacks)
            {
                p.FadeColour(Color4.Black, 200);
                p.TagNumeric = 0;
            }

            foreach (pSprite p in songPreviewButtons)
                p.Texture = TextureManager.Load(OsuTexture.songselect_audio_preview);

            s_BackingPlate.FadeColour(colourNormal, 200);
            s_BackingPlate.HandleInput = true;
        }

        internal void Add(string filename)
        {
            pSprite preview = new pSprite(TextureManager.Load(OsuTexture.songselect_audio_preview), Vector2.Zero) { DrawDepth = base_depth + 0.02f };
            preview.Offset = new Vector2(68, Height + 3);
            Sprites.Add(preview);
            songPreviewButtons.Add(preview);

            pRectangle back = new pRectangle(Vector2.Zero, new Vector2(GameBase.BaseSize.Width, 40), true, base_depth, Color4.Black);
            back.HandleClickOnUp = true;

            back.OnHover += delegate { if (back.TagNumeric != 1) back.FadeColour(new Color4(40,40,40,255),200); };
            back.OnHoverLost += delegate { if (back.TagNumeric != 1) back.FadeColour(Color4.Black,200); };

            back.OnClick += delegate(object sender, EventArgs e) {

                bool isPausing = back.TagNumeric == 1;

                StoreMode.ResetAllPreviews();

                if (isPausing) return;

                back.FadeColour(colourHover,0);
                back.Transform(new Transformation(TransformationType.VectorScale, new Vector2(back.Scale.X,0), back.Scale,Clock.ModeTime, Clock.ModeTime + 200, EasingTypes.In));
                back.TagNumeric = 1;
                preview.Texture = TextureManager.Load(OsuTexture.songselect_audio_preview_pause);

                isPreviewing = true;

                s_BackingPlate.Click(false);
            };

            songPreviewBacks.Add(back);

            back.Origin = OriginTypes.CentreLeft;
            back.Offset = new Vector2(0, Height + 45/2);
            Sprites.Add(back);

            Regex r = new Regex(@"(.*) - (.*) \((.*)\)");
            Match m = r.Match(Path.GetFileNameWithoutExtension(filename));

            pText artist = new pText(m.Groups[1].Value, 26, Vector2.Zero, Vector2.Zero, base_depth + 0.01f, true, Color4.SkyBlue, false);
            artist.Bold = true;
            artist.Offset = new Vector2(110, Height + 6);
            Sprites.Add(artist);

            pText title = new pText(m.Groups[2].Value, 26, Vector2.Zero, Vector2.Zero, base_depth + 0.01f, true, Color4.White, false);

            title.Offset = new Vector2(120 + artist.MeasureText().X / GameBase.BaseToNativeRatio, Height + 6);
            Sprites.Add(title);


            Height += 45;
        }
    }
}
