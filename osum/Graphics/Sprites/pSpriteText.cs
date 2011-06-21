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

        internal bool TextConstantSpacing = false;
        internal string TextFont = "default";

        internal int SpacingOverlap;


        /// <summary>
        /// Optimal storage for memory efficiency.
        /// </summary>
        private char[] textArray;
        internal char[] TextArray
        {
            get { return textArray; }
            set
            {
                if (value.Length != textArray.Length)
                    textArray = new char[value.Length];

                for (int i = 0; i < textArray.Length; i++)
                    textArray[i] = value[i];

                textChanged = true;
            }
        }

        internal void UpdateCharacterAt(int i, char c)
        {
            if (textArray[i] == c)
                return;

            textArray[i] = c;
            textChanged = true;
        }

        internal string Text
        {
            set
            {
                bool sameSizeArray = textArray != null && value.Length == textArray.Length;

                bool sameString = sameSizeArray;

                if (sameSizeArray)
                {
                    for (int i = 0; i < textArray.Length; i++)
                    {
                        if (value[i] != textArray[i])
                        {
                            sameString = false;
                            textArray[i] = value[i];
                        }
                    }

                    if (sameString)
                        return; //strings are equal
                }
                else
                {
                    if (value == null)
                        textArray = null;
                    else
                        textArray = value.ToCharArray();
                }

                textChanged = true;
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
            ExactCoordinates = true;

            //this will trigger a render call here
            Text = text;
        }

        internal Vector2 MeasureText()
        {
            if (textArray == null) return Vector2.Zero;

            if (textChanged)
                refreshTexture();

            return lastMeasure;
        }

        internal override Vector2 OriginVector
        {
            get
            {
                switch (Origin)
                {
                    default:
                        return Vector2.Zero;
                    case OriginTypes.Centre:
                        return lastMeasure * 0.5F;
                    case OriginTypes.TopCentre:
                        return new Vector2(lastMeasure.X * 0.5F, 0);
                    case OriginTypes.TopRight:
                        return new Vector2(lastMeasure.X, 0);
                    case OriginTypes.CentreRight:
                        return new Vector2(lastMeasure.X, lastMeasure.Y * 0.5f);
                    case OriginTypes.BottomCentre:
                        return new Vector2(lastMeasure.X / 2, lastMeasure.Y);
                    case OriginTypes.BottomRight:
                        return lastMeasure;
                    case OriginTypes.BottomLeft:
                        return new Vector2(0, lastMeasure.Y);
                }
            }
        }

        public override pDrawable Clone()
        {
            pDrawable cl = base.Clone();
            pSpriteText st = cl as pSpriteText;
            Console.WriteLine();
            return cl;
        }


        Dictionary<char, pTexture> textureCache = new Dictionary<char, pTexture>();
        public float ZeroAlpha = 1;

        pTexture textureFor(char c)
        {
            pTexture tex = null;

            if (textureCache.TryGetValue(c, out tex) && tex != null && tex.TextureGl != null && tex.TextureGl.Id >= 0)
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

            if (textArray == null)
                return;

            int currentX = 0;
            int height = 0;

            int width = 0;

            for (int i = 0; i < textArray.Length; i++)
            {
                char c = textArray[i];

                currentX -= (TextConstantSpacing || i == 0 ? 0 : SpacingOverlap);

                int x = currentX;

                pTexture tex = textureFor(c);

                if (tex == null) continue;

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
                pTexture spacingTexture = textureFor('6');
                int charWidth = spacingTexture != null ? spacingTexture.Width : 0;

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
        }

        public override void Update()
        {
            if (textChanged) MeasureText();
            base.Update();
        }

        public override bool Draw()
        {
            if (AlwaysDraw || Transformations.Count != 0)
            {
                if (Alpha != 0)
                {
                    if (textChanged) MeasureText();

                    int i = 0;

                    Vector2 pos = FieldPosition;
                    Vector2 scale = FieldScale;
                    Color4 col = AlphaAppliedColour;
                    Color4 colZero = new Color4(col.R * ZeroAlpha, col.G * ZeroAlpha, col.B * ZeroAlpha, col.A * ZeroAlpha);

                    bool isPaddedZero = true;

                    foreach (pTexture tex in renderTextures)
                    {
                        // note: no srcRect calculation
                        if (tex.TextureGl != null)
                        {
                            if (textArray[i] != '0')
                                isPaddedZero = false;

                            if (isPaddedZero)
                                tex.TextureGl.Draw(pos + renderCoordinates[i] * Scale.X * GameBase.SpriteToNativeRatio, OriginVector, colZero, scale, Rotation, new Box2(tex.X, tex.Y, tex.X + tex.Width, tex.Y + tex.Height));
                            else
                                tex.TextureGl.Draw(pos + renderCoordinates[i] * Scale.X * GameBase.SpriteToNativeRatio, OriginVector, col, scale, Rotation, new Box2(tex.X, tex.Y, tex.X + tex.Width, tex.Y + tex.Height));
                        }

                        i++;
                    }

                    return true;
                }

            }

            return false;
        }

        internal int LastInt;
        internal void ShowInt(int number, int padding = 0, bool separators = false, char suffix = (char)0)
        {
            LastInt = number;

            int numberLength = 1;
            while (number / (int)Math.Pow(10, numberLength) > 0)
                numberLength++;

            if (numberLength < padding)
                numberLength = padding;

            int totalLength = numberLength + (suffix > 0 ? 1 : 0) + (separators ? (numberLength - 1) / 3 : 0);

            if (textArray.Length != totalLength)
                //todo: can optimise this to avoid reacllocation when shrinking.
                textArray = new char[totalLength];

            int zero_offset = 48;

            int cChar = 0;

            for (int i = numberLength - 1; i >= 0; i--)
            {
                UpdateCharacterAt(cChar++, (char)(zero_offset + (number / (int)Math.Pow(10, i)) % 10));
                if (separators && i > 0 && (i % 3 == 0))
                    UpdateCharacterAt(cChar++, ',');
            }

            if (suffix > 0)
                UpdateCharacterAt(cChar, suffix);
        }

        internal void ShowDouble(double number, int padding, int accuracy, char suffix)
        {
            int numberLengthLeft = 1;
            while ((int)number / (int)Math.Pow(10, numberLengthLeft) > 0)
                numberLengthLeft++;

            if (numberLengthLeft < padding)
                numberLengthLeft = padding;

            int totalLength = numberLengthLeft + (suffix > 0 ? 1 : 0) + (accuracy > 0 ? 1 + accuracy : 0);

            if (textArray.Length != totalLength)
                //todo: can optimise this to avoid reacllocation when shrinking.
                textArray = new char[totalLength];

            int zero_offset = 48;

            int cChar = 0;

            for (int i = numberLengthLeft - 1; i >= 0; i--)
                UpdateCharacterAt(cChar++, (char)(zero_offset + (number / (int)Math.Pow(10, i)) % 10));

            if (accuracy > 0)
                UpdateCharacterAt(cChar++, '.');

            double decimalPart = number - (int)number;

            for (int i = 0; i < accuracy; i++)
                UpdateCharacterAt(cChar++, (char)(zero_offset + (int)(decimalPart * Math.Pow(10, i + 1)) % 10));

            if (suffix > 0)
                UpdateCharacterAt(cChar, suffix);
        }
    }
}
