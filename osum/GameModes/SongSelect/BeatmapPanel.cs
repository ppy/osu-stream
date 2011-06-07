using System;
using osum.Graphics.Sprites;
using osum.GameplayElements.Beatmaps;
using System.IO;
using OpenTK;
using OpenTK.Graphics;
using osum.Helpers;
using System.Text.RegularExpressions;
using osum.Graphics.Drawables;
using osum.Graphics.Renderers;
using osum.Graphics.Skins;
using osum.Audio;
namespace osum.GameModes.SongSelect
{
    internal class BeatmapPanel : pSpriteCollection
    {
        internal Beatmap Beatmap;

        internal pSprite s_BackingPlate;
        internal pSprite s_BackingPlate2;
        internal pText s_Text;
        internal pText s_TextArtist;
        internal pSprite s_Thumbnail;

        float base_depth = 0.6f;

        static Color4 colourNormal = new Color4(50, 50, 50, 255);
        static Color4 colourHover = new Color4(28, 139, 242, 255);

        internal const int PANEL_HEIGHT = 60;
        public static Color4 BACKGROUND_COLOUR = new Color4(255, 255, 255, 128);

        internal BeatmapPanel(Beatmap beatmap, SongSelectMode select, int index)
        {
            base_depth += 0.001f * index;

            //s_BackingPlate = new pRectangle(Vector2.Zero, new Vector2(GameBase.BaseSizeFixedWidth.Width, PANEL_HEIGHT), true, base_depth, colourNormal);
            s_BackingPlate = new pSprite(TextureManager.Load(OsuTexture.songselect_panel), Vector2.Zero)
            {
                DrawDepth = base_depth,
                Colour = new Color4(255, 255, 255, 160)
            };

            Sprites.Add(s_BackingPlate);

            Beatmap = beatmap;

            s_BackingPlate.OnClick += delegate { select.onSongSelected(this, null); };

            s_BackingPlate.HandleClickOnUp = true;

            s_BackingPlate.OnHover += delegate
            {
                s_BackingPlate.Transform(new Transformation(TransformationType.Fade, s_BackingPlate.Alpha, 0.01f, Clock.ModeTime, Clock.ModeTime + 100));
                s_BackingPlate2.FadeColour(BeatmapPanel.BACKGROUND_COLOUR, 100);

                AudioEngine.PlaySample(OsuSamples.MenuClick);
            };
            s_BackingPlate.OnHoverLost += delegate
            {
                s_BackingPlate2.FadeColour(Color4.Transparent, 100);
                s_BackingPlate.FadeIn(100);
            };

            s_Text = new pText(string.Empty, 32, Vector2.Zero, Vector2.Zero, base_depth + 0.02f, true, Color4.White, false);
            s_Text.Bold = true;
            s_Text.Offset = new Vector2(74, 14);
            Sprites.Add(s_Text);

            s_TextArtist = new pText(string.Empty, 56, Vector2.Zero, Vector2.Zero, base_depth + 0.04f, true, BACKGROUND_COLOUR, false);
            s_TextArtist.TextAlignment = TextAlignment.Right;
            s_TextArtist.Origin = OriginTypes.TopRight;
            s_TextArtist.Field = FieldTypes.StandardSnapRight;
            s_TextArtist.Offset = new Vector2(0, 7);
            Sprites.Add(s_TextArtist);

            if (beatmap != null)
            {
                string filename = Path.GetFileNameWithoutExtension(beatmap.BeatmapFilename);

                Regex r = new Regex(@"(.*) - (.*) \((.*)\)");
                Match m = r.Match(filename);
                s_Text.Text = m.Groups[2].Value;
                s_TextArtist.Text = m.Groups[1].Value;
            }

            s_Thumbnail = new pSprite(TextureManager.Load(OsuTexture.songselect_thumbnail), Vector2.Zero) { DrawDepth = base_depth + 0.02f };
            s_Thumbnail.Offset = new Vector2(2, 2);
            Sprites.Add(s_Thumbnail);

            s_BackingPlate2 = new pSprite(TextureManager.Load(OsuTexture.songselect_panel_selected), Vector2.Zero)
            {
                DrawDepth = base_depth + 0.01f,
                Colour = new Color4(255, 255, 255, 0)
            };

            Sprites.Add(s_BackingPlate2);
        }
    }
}

