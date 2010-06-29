using System;
using System.Collections.Generic;
using osum.Graphics.Skins;
using osum.Helpers;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace osum.Graphics.Sprites
{
    internal class pSpriteText : pSprite
    {
        internal List<Vector2> renderCoordinates = new List<Vector2>();
        internal List<pTexture> renderTextures = new List<pTexture>();

        internal bool TextConstantSpacing;
        internal string TextFont = "default";

        internal int SpacingOverlap;

        // pushed from pSprite
        internal string Text;
        internal bool TextChanged;
        internal Vector2 lastMeasure;

        internal pSpriteText(string text, string fontname, int spacingOverlap, FieldTypes fieldType, OriginTypes originType, ClockTypes clockType,
                             Vector2 startPosition, float drawDepth, bool alwaysDraw, Color4 colour)
            : base(null, fieldType, originType, clockType, startPosition, drawDepth, alwaysDraw, colour)
        {
            Text = text;
            texture = null;
            TextChanged = true;
            //Type = SpriteTypes.SpriteText;
            TextFont = fontname;
            SpacingOverlap = spacingOverlap;
        }

        internal Vector2 MeasureText()
        {
            if (TextChanged)
                refreshRenderArray();

            UpdateTextureAlignment();

            return lastMeasure;
        }

        internal override void UpdateTextureAlignment()
        {
            switch (Origin)
            {
                case OriginTypes.Centre:
                    originVector = lastMeasure * 0.5F;
                    break;
                case OriginTypes.TopCentre:
                    originVector.X = lastMeasure.X * 0.5F;
                    break;
                case OriginTypes.TopRight:
                    originVector.X = lastMeasure.X;
                    break;
                case OriginTypes.BottomCentre:
                    originVector.X = lastMeasure.X / 2;
                    originVector.Y = lastMeasure.Y;
                    break;
                case OriginTypes.BottomRight:
                    originVector.X = lastMeasure.X;
                    originVector.Y = lastMeasure.Y;
                    break;
            }
        }

        private void refreshRenderArray()
        {
            TextChanged = false;

            renderTextures.Clear();
            renderCoordinates.Clear();

            int currentX = 0;
            int height = 0;

            int width = 0;

            string text = Text;

            for (int i = 0; i < text.Length; i++)
            {
                pTexture tex;

                currentX -= (TextConstantSpacing || i == 0 ? 0 : SpacingOverlap);

                int x = currentX;

                switch (text[i])
                {
                    case ' ':
                        currentX += SkinManager.Load(TextFont + "-dot").Width;
                        continue;
                    case ',':
                        tex = SkinManager.Load(TextFont + "-comma");
                        currentX += tex.Width;
                        break;
                    case '.':
                        tex = SkinManager.Load(TextFont + "-dot");
                        currentX += tex.Width;
                        break;
                    case '%':
                        tex = SkinManager.Load(TextFont + "-percent");
                        currentX += tex.Width;
                        break;
                    default:
                        tex = SkinManager.Load(TextFont + "-" + text[i]);
                        if (!TextConstantSpacing)
                            currentX += tex.Width;
                        break;
                }

                renderTextures.Add(tex);
                if (TextConstantSpacing)
                    renderCoordinates.Add(new Vector2(currentX - x, 0));
                else
                    renderCoordinates.Add(new Vector2(x, 0));

                if (height == 0)
                    height = tex.Height;
            }

            if (TextConstantSpacing)
            {
                //float last = 0;
                int charWidth = SkinManager.Load(TextFont + "-5").Width;

                currentX = 0;

                for (int i = 0; i < renderCoordinates.Count; i++)
                {
                    float special = renderCoordinates[i].X;

                    if (special == 0)
                    {
                        renderCoordinates[i] = new Vector2(currentX + Math.Max(0, (charWidth - renderTextures[i].Width) / 2), 0);
                        currentX += charWidth - SpacingOverlap;
                    }
                    else
                    {
                        renderCoordinates[i] = new Vector2(currentX, 0);
                        currentX += (int)special - SpacingOverlap;
                    }
                }
            }

            width = currentX;

            lastMeasure = new Vector2(width, height);

            //DrawWidth = (int)Math.Round(lastMeasure.X);
            //DrawHeight = (int)Math.Round(lastMeasure.Y);
        }

        public override void Draw()
        {
            // either call base.Draw() or duplicate code here

            /*
            Vector2 tmp = Position;
            for (int i = 0; i < renderCoordinates.Count; i++)
            {
                texture = renderTextures[i];
                Position = tmp + renderCoordinates[i];
                base.Draw();
            }
            Position = tmp;
            texture = null;
            */

            if (AlwaysDraw)
            {
                GL.BlendFunc(BlendingFactorSrc.SrcAlpha, blending);
                for (int i = 0; i < renderCoordinates.Count; i++)
                {
                    // note: no srcRect calculation
                    renderTextures[i].TextureGl.Draw(Position + renderCoordinates[i], originVector, Colour, Scale, Rotation, null, SpriteEffect.None);
                }
            }
        }
    }
}
