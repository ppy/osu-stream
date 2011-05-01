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
namespace osum.GameModes.SongSelect
{
    internal class BeatmapPanel : pSpriteCollection
    {
        internal Beatmap Beatmap;

        internal pDrawable s_BackingPlate;
        internal pText s_Text;
        internal pSprite s_Thumbnail;

        float base_depth = 0.6f;

        static Color4 colourNormal = new Color4(50, 50, 50, 255);
        static Color4 colourHover = new Color4(28, 139, 242, 255);

        internal const int PANEL_HEIGHT = 60;

        internal BeatmapPanel(Beatmap beatmap, SongSelectMode select, int index)
        {
            base_depth += 0.001f * index;

            s_BackingPlate = new pRectangle(Vector2.Zero, new Vector2(GameBase.BaseSize.Width, PANEL_HEIGHT), true, base_depth, colourNormal);
            Sprites.Add(s_BackingPlate);

            Beatmap = beatmap;

            s_BackingPlate.OnClick += delegate { select.SongSelected(this, null); };

            s_BackingPlate.HandleClickOnUp = true;

            s_BackingPlate.OnHover += delegate { s_BackingPlate.FadeColour(colourHover, 80); };
            s_BackingPlate.OnHoverLost += delegate { s_BackingPlate.FadeColour(colourNormal, 80); };

            string filename = Path.GetFileNameWithoutExtension(beatmap.BeatmapFilename);

            Regex r = new Regex(@"(.*) - (.*) \((.*)\)");
            Match m = r.Match(filename);


            s_Text = new pText(m.Groups[2].Value, 32, Vector2.Zero, new Vector2(GameBase.BaseSize.Width, PANEL_HEIGHT), base_depth + 0.01f, true, Color4.White, false);
            s_Text.Bold = true;
            s_Text.Offset = new Vector2(74, 14);
            if (s_Text.Texture != null)
                Sprites.Add(s_Text);

            s_Text = new pText(m.Groups[1].Value, 56, Vector2.Zero, new Vector2(256, 60), base_depth + 0.01f, true, new Color4(255, 255, 255, 128), false);
            s_Text.TextAlignment = TextAlignment.Right;
            s_Text.Origin = OriginTypes.TopRight;
            s_Text.Offset = new Vector2(GameBase.BaseSize.Width, 10);
            Sprites.Add(s_Text);

            s_Thumbnail = new pSprite(TextureManager.Load(OsuTexture.songselect_thumbnail), Vector2.Zero) { DrawDepth = base_depth + 0.02f };
            s_Thumbnail.Offset = new Vector2(2, 2);
            Sprites.Add(s_Thumbnail);
        }

        internal void MoveTo(Vector2 location)
        {
            Sprites.ForEach(s => s.MoveTo(location, 150));
        }
    }
}

