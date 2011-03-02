using System;
using osum.Graphics.Sprites;
using osum.GameplayElements.Beatmaps;
using System.IO;
using OpenTK;
using OpenTK.Graphics;
using osum.Helpers;
using System.Text.RegularExpressions;
namespace osum.GameModes.SongSelect
{
    internal class BeatmapPanel : pSpriteCollection
    {
        internal Beatmap Beatmap;

        internal pSprite s_BackingPlate;
        internal pText s_Text;

        float base_depth = 0.5f;

        static Color4 colourNormal = new Color4(28, 139, 242, 255);
        static Color4 colourHover = new Color4(27, 197, 241, 255);

        internal const int PANEL_HEIGHT = 80;

        internal BeatmapPanel(Beatmap beatmap, SongSelectMode select)
        {
            s_BackingPlate = pSprite.FullscreenWhitePixel;
            s_BackingPlate.Alpha = 1;
            s_BackingPlate.AlwaysDraw = true;
            s_BackingPlate.Colour = colourNormal;
            s_BackingPlate.Scale.Y = PANEL_HEIGHT;
            s_BackingPlate.DrawDepth = base_depth;
            SpriteCollection.Add(s_BackingPlate);

            Beatmap = beatmap;

            s_BackingPlate.OnClick += delegate { select.SongSelected(this, null); };

            s_BackingPlate.HandleClickOnUp = true;

            s_BackingPlate.OnHover += delegate { s_BackingPlate.FadeColour(colourHover, 150); };
            s_BackingPlate.OnHoverLost += delegate { s_BackingPlate.FadeColour(colourNormal, 150); };

            string filename = Path.GetFileNameWithoutExtension(beatmap.BeatmapFilename);

            Regex r = new Regex(@"(.*) - (.*) \((.*)\)");
            Match m = r.Match(filename);


            s_Text = new pText(m.Groups[1].Value + " - " + m.Groups[2].Value, 25, Vector2.Zero, new Vector2(GameBase.BaseSize.Width, PANEL_HEIGHT), base_depth + 0.01f, true, Color4.White, false);
            s_Text.Bold = true;
            s_Text.Offset = new Vector2(10, 0);
            if (s_Text.Texture != null)
                SpriteCollection.Add(s_Text);

            s_Text = new pText(m.Groups[4].Value, 20, Vector2.Zero, new Vector2(GameBase.BaseSize.Width - 120, 60), base_depth + 0.01f, true, Color4.White, false);
            s_Text.Offset = new Vector2(10, 28);
            SpriteCollection.Add(s_Text);

            s_Text = new pText("by " + m.Groups[3].Value, 18, Vector2.Zero, new Vector2(GameBase.BaseSize.Width - 120, 60), base_depth + 0.01f, true, Color4.White, false);
            s_Text.Origin = OriginTypes.TopRight;
            s_Text.Offset = new Vector2(GameBase.BaseSize.Width - 10, 28);
            SpriteCollection.Add(s_Text);
        }

        internal void MoveTo(Vector2 location)
        {
            SpriteCollection.ForEach(s => s.MoveTo(location, 150));
        }
    }
}

