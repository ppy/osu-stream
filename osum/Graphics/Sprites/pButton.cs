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
namespace osum.GameModes.SongSelect
{
    internal class pButton : pSpriteCollection
    {
        internal Beatmap Beatmap;

        internal pDrawable s_BackingPlate;
        internal pText s_Text;

        float base_depth = 0.5f;

        Color4 colour;
        Color4 colourNormal = new Color4(28, 139, 242, 255);
        Color4 colourHover = new Color4(27, 197, 241, 255);

        internal const int PANEL_HEIGHT = 80;

        EventHandler action;

        public bool Enabled = true;

        bool pendingUnhover = false;

        internal pButton(string text, Vector2 position, Vector2 size, Color4 colour, EventHandler action)
        {
            this.action = action;

            Colour = colour;

            s_BackingPlate = new pRectangle(position, size, true, base_depth, colourNormal);
            Sprites.Add(s_BackingPlate);

            s_BackingPlate.OnClick += s_BackingPlate_OnClick;

            s_BackingPlate.HandleClickOnUp = true;

            s_BackingPlate.OnHover += delegate
            {
                if (!Enabled) return;
                s_BackingPlate.FadeColour(colourHover, 150);

                pendingUnhover = true;
            };
            s_BackingPlate.OnHoverLost += delegate
            {
                if (!Enabled || !pendingUnhover) return;
                s_BackingPlate.FadeColour(colourNormal, 150);
            };

            s_Text = new pText(text, 25, position, size, base_depth + 0.01f, true, Color4.White, false);
            s_Text.TextAlignment = TextAlignment.Centre;
            s_Text.Bold = true;

            if (s_Text.Texture != null)
                Sprites.Add(s_Text);
        }

        internal Color4 Colour
        {
            get { return colour; }
            set
            {
                colour = value;

                colourNormal = ColourHelper.Darken(colour, 0.2f);
                colourHover = colour;

                if (s_BackingPlate != null)
                    s_BackingPlate.Colour = colourNormal;
            }
        }

        void s_BackingPlate_OnClick(object sender, EventArgs e)
        {
            if (!Enabled) return;

            s_BackingPlate.FlashColour(Color4.White, 500);
            pendingUnhover = false;

            if (action != null)
                action(this, null);
        }

        public Vector2 Position
        {
            get
            {
                return s_BackingPlate.Position;
            }
        }
    }
}

