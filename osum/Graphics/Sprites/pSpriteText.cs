using System;
using System.Collections.Generic;
using osum.Graphics.Skins;
using osum.Graphics;
using osum.Helpers;

using OpenTK;
using OpenTK.Graphics;

#if iOS
using OpenTK.Graphics.ES11;
using MonoTouch.Foundation;
using MonoTouch.ObjCRuntime;
using MonoTouch.OpenGLES;

using TextureTarget = OpenTK.Graphics.ES11.All;
using TextureParameterName = OpenTK.Graphics.ES11.All;
using EnableCap = OpenTK.Graphics.ES11.All;
using BlendingFactorSrc = OpenTK.Graphics.ES11.All;
using BlendingFactorDest = OpenTK.Graphics.ES11.All;
using PixelStoreParameter = OpenTK.Graphics.ES11.All;
using VertexPointerType = OpenTK.Graphics.ES11.All;
using ColorPointerType = OpenTK.Graphics.ES11.All;
using ClearBufferMask = OpenTK.Graphics.ES11.All;
using TexCoordPointerType = OpenTK.Graphics.ES11.All;
using BeginMode = OpenTK.Graphics.ES11.All;
using MatrixMode = OpenTK.Graphics.ES11.All;
using PixelInternalFormat = OpenTK.Graphics.ES11.All;
using PixelFormat = OpenTK.Graphics.ES11.All;
using PixelType = OpenTK.Graphics.ES11.All;
using ShaderType = OpenTK.Graphics.ES11.All;
using VertexAttribPointerType = OpenTK.Graphics.ES11.All;
using ProgramParameter = OpenTK.Graphics.ES11.All;
using ShaderParameter = OpenTK.Graphics.ES11.All;
#else
using OpenTK.Input;
using OpenTK.Graphics.OpenGL;
using osum.Input;
#endif


namespace osum.Graphics.Sprites
{
    internal class pSpriteText : pSprite
    {
        internal List<Vector2> renderCoordinates = new List<Vector2>();
        internal List<pTexture> renderTextures = new List<pTexture>();

        internal bool TextConstantSpacing = true;
        internal string TextFont = "default";

        internal int SpacingOverlap;

        // pushed from pSprite
        private string text;
        internal string Text
        {
            get { return text; }
            set
            {
                if (text == value) return;
                
                text = value;
                textChanged = true;

                MeasureText();
            }
        }

        private bool textChanged;
        private Vector2 lastMeasure;
        private OsuTexture osuTextureFont;

        internal pSpriteText(string text, string fontname, int spacingOverlap, FieldTypes fieldType, OriginTypes originType, ClockTypes clockType,
                             Vector2 startPosition, float drawDepth, bool alwaysDraw, Color4 colour)
            : base(null, fieldType, originType, clockType, startPosition, drawDepth, alwaysDraw, colour)
        {
            TextFont = fontname;

            try
            {
                osuTextureFont = (OsuTexture)Enum.Parse(typeof(OsuTexture), TextFont + "_0");
				
				TextureManager.Load(osuTextureFont);
				//preload
            }
            catch
            {
            }

            SpacingOverlap = spacingOverlap;

            //this will trigger a render call here
            Text = text;
        }

        internal Vector2 MeasureText()
        {
            if (text == null) return Vector2.Zero;

            if (textChanged)
                refreshTexture();

            UpdateTextureAlignment();

            return lastMeasure;
        }

        internal override void UpdateTextureAlignment()
        {
            switch (Origin)
            {
                case OriginTypes.Centre:
                    OriginVector = lastMeasure * 0.5F;
                    break;
                case OriginTypes.TopCentre:
                    OriginVector.X = lastMeasure.X * 0.5F;
                    break;
                case OriginTypes.TopRight:
                    OriginVector.X = lastMeasure.X;
                    break;
                case OriginTypes.BottomCentre:
                    OriginVector.X = lastMeasure.X / 2;
                    OriginVector.Y = lastMeasure.Y;
                    break;
                case OriginTypes.BottomRight:
                    OriginVector.X = lastMeasure.X;
                    OriginVector.Y = lastMeasure.Y;
                    break;
                case OriginTypes.BottomLeft:
                    OriginVector.Y = lastMeasure.Y;
                    break;
            }
        }


        Dictionary<char,pTexture> textureCache = new Dictionary<char, pTexture>();

		pTexture textureFor(char c)
		{
			pTexture tex = null;
			
			if (textureCache.TryGetValue(c, out tex) && tex.TextureGl != null && tex.TextureGl.Id >= 0)
					//the extra two conditions are only required for the fps counter between modes.
                {
					
                }
                else
                {
                    int offset = c - '0';
					
					switch (c)
                    {
                        case ',':
							offset = 10;
                            break;
                        case '.':
							offset = 11;
							break;
                        case '%':
							offset = 12;
							break;
						case 'x':
							offset = 13;
							break;
                    }
					
					if (osuTextureFont != OsuTexture.None)
                        tex = TextureManager.Load((OsuTexture)(osuTextureFont + offset));
                    else
                        tex = TextureManager.Load(TextFont + "-" + c);

                    textureCache[c] = tex;
                }
			
			return tex;
		}

        /// <summary>
        /// Updates the array of each character which is to be displayed.
        /// </summary>
        private void refreshTexture()
        {
            textChanged = false;

            renderTextures.Clear();
            renderCoordinates.Clear();

            int currentX = 0;
            int height = 0;

            int width = 0;

            string text = Text;

            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];

                currentX -= (TextConstantSpacing || i == 0 ? 0 : SpacingOverlap);

                int x = currentX;
				
				pTexture tex = textureFor(c);
                
				
				if (!TextConstantSpacing || c < '0' || c > '9')
                        currentX += tex.Width;

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
                int charWidth = textureFor('5').Width;

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

            UpdateTextureAlignment();
        }

        public override bool Draw()
        {
            if (AlwaysDraw || Transformations.Count != 0)
            {
                if (Alpha != 0)
                {
                    int i = 0;
                    foreach (pTexture tex in renderTextures)
                    {
                        // note: no srcRect calculation
                        if (tex.TextureGl != null)
                            tex.TextureGl.Draw(FieldPosition + renderCoordinates[i] * Scale.X * GameBase.SpriteToNativeRatio, OriginVector, AlphaAppliedColour, FieldScale, Rotation,
                                new Box2(tex.X, tex.Y, tex.X + tex.Width, tex.Y + tex.Height));
                        i++;
                    }

                    return true;
                }
                
            }

            return false;
        }
    }
}
