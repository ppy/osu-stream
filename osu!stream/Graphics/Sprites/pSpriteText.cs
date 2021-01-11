#if iOS || ANDROID
using OpenTK.Graphics.ES11;
#if iOS
using Foundation;
using ObjCRuntime;
using OpenGLES;
#endif

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
using OpenTK.Graphics.OpenGL;
#endif
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using OpenTK;
using OpenTK.Graphics;
using osum.Helpers;


namespace osum.Graphics.Sprites
{
    internal class pSpriteText : pSprite
    {
        internal List<int> renderCoordinates = new List<int>();
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
            get => textArray;
            set
            {
                if (value.Length != textArray.Length)
                    textArray = new char[value.Length];

                for (int i = 0; i < textArray.Length; i++)
                    textArray[i] = value[i];

                if (textArray.Length > MAX_LENGTH)
                    throw new Exception("STRING TOO LONG");

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

                if (value?.Length > MAX_LENGTH)
                    throw new Exception($"STRING TOO LONG ({value})");

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
        private readonly OsuTexture osuTextureFont;

        private const int MAX_LENGTH = 9; // 100.00% (7) 1,000,000 (9)

        internal pSpriteText(string text, string fontname, int spacingOverlap, FieldTypes fieldType, OriginTypes originType, ClockTypes clockType,
            Vector2 startPosition, float drawDepth, bool alwaysDraw, Color4 colour)
            : base(null, fieldType, originType, clockType, startPosition, drawDepth, alwaysDraw, colour)
        {
            TextFont = fontname;

            try
            {
                osuTextureFont = (OsuTexture)Enum.Parse(typeof(OsuTexture), TextFont + "_0");

                Texture = TextureManager.Load(osuTextureFont);
                //preload
            }
            catch
            {
            }

            SpacingOverlap = spacingOverlap;

            //this will trigger a render call here
            Text = text;

            const int coordinates_per_char = 12;

#if !NO_PIN_SUPPORT
            coordinates = new float[MAX_LENGTH * coordinates_per_char];
            vertices = new float[MAX_LENGTH * coordinates_per_char];

            handle_vertices = GCHandle.Alloc(vertices, GCHandleType.Pinned);
            handle_coordinates = GCHandle.Alloc(coordinates, GCHandleType.Pinned);

            handle_vertices_pointer = handle_vertices.AddrOfPinnedObject();
            handle_coordinates_pointer = handle_coordinates.AddrOfPinnedObject();
#else
            handle_vertices_pointer = Marshal.AllocHGlobal(MAX_LENGTH * coordinates_per_char * sizeof(float));
            handle_coordinates_pointer = Marshal.AllocHGlobal(MAX_LENGTH * coordinates_per_char * sizeof(float));
#endif
        }

        private bool isDisposed;

        public override void Dispose()
        {
            if (isDisposed) return;

#if !NO_PIN_SUPPORT
            handle_vertices.Free();
            handle_coordinates.Free();
#else
            Marshal.FreeHGlobal(handle_coordinates_pointer);
            Marshal.FreeHGlobal(handle_vertices_pointer);
#endif

            base.Dispose();
            isDisposed = true;
        }

        internal Vector2 MeasureText()
        {
            if (textArray == null) return Vector2.Zero;

            if (textChanged)
                refreshTexture();

            return lastMeasure;
        }

        internal override void UpdateOriginVector()
        {
            Vector2 origin = Vector2.Zero;

            switch (Origin)
            {
                case OriginTypes.Centre:
                    origin = lastMeasure * 0.5F;
                    break;
                case OriginTypes.TopCentre:
                    origin.X = lastMeasure.X * 0.5F;
                    break;
                case OriginTypes.TopRight:
                    origin.X = lastMeasure.X;
                    break;
                case OriginTypes.CentreRight:
                    origin = new Vector2(lastMeasure.X, lastMeasure.Y * 0.5f);
                    break;
                case OriginTypes.BottomCentre:
                    origin = new Vector2(lastMeasure.X / 2, lastMeasure.Y);
                    break;
                case OriginTypes.BottomRight:
                    origin = lastMeasure;
                    break;
                case OriginTypes.BottomLeft:
                    origin.Y = lastMeasure.Y;
                    break;
            }

            if (!exactCoordinatesOverride)
            {
                if (origin.X % 2 != 0) origin.X--;
                if (origin.Y % 2 != 0) origin.Y--;
            }

            OriginVector = origin;
        }

        public override pDrawable Clone()
        {
            pDrawable cl = base.Clone();
            return cl;
        }


        private readonly Dictionary<char, pTexture> textureCache = new Dictionary<char, pTexture>();
        public float ZeroAlpha = 1;

        private pTexture textureFor(char c)
        {
            pTexture tex;

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
                    tex = TextureManager.Load(osuTextureFont + offset);
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
            updateDrawCache = true;

            renderTextures.Clear();
            renderCoordinates.Clear();

            if (textArray == null)
                return;

            int currentX = 0;
            int height = 0;

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
                    renderCoordinates.Add(currentX - x);
                else
                    renderCoordinates.Add(x);

                if (height == 0)
                    height = tex.Height;
            }

            if (TextConstantSpacing)
            {
                pTexture spacingTexture = textureFor('6');
                int charWidth = spacingTexture?.Width ?? 0;

                currentX = 0;

                bool exact = ExactCoordinates;

                for (int i = 0; i < renderCoordinates.Count; i++)
                {
                    int x = renderCoordinates[i];

                    if (x == 0)
                    {
                        x = currentX + Math.Max(0, (charWidth - renderTextures[i].Width) / 2);
                        currentX += charWidth - SpacingOverlap;
                    }
                    else
                    {
                        int oldX = x;
                        x = currentX;
                        currentX += oldX - SpacingOverlap;
                    }

                    if (!exactCoordinatesOverride)
                        if (x % 2 != 0)
                            x++;

                    renderCoordinates[i] = x;
                }
            }

            lastMeasure = new Vector2(currentX, height);
        }

        public override void Update()
        {
            if (textChanged) MeasureText();
            base.Update();
        }

#if !NO_PIN_SUPPORT
        private readonly float[] coordinates;
        private readonly float[] vertices;

        private GCHandle handle_vertices;
        private GCHandle handle_coordinates;
#endif

        private readonly IntPtr handle_vertices_pointer;
        private readonly IntPtr handle_coordinates_pointer;

        private Color4 colZeroCached;

        private bool updateDrawCache;
        private Vector2 drawPos;
        private Vector2 drawScale;
        private Color4 drawCol;

        public override bool Draw()
        {
            if (Alpha > 0 && (AlwaysDraw || !noTransformationsLeft) && textArray != null && textArray.Length > 0)
            {
                if (textChanged) MeasureText();

                if (FieldPosition != drawPos)
                {
                    drawPos = FieldPosition;
                    updateDrawCache = true;
                }

                if (drawScale != FieldScale)
                {
                    drawScale = FieldScale;
                    updateDrawCache = true;
                }

                Color4 col = AlphaAppliedColour;
                if (drawCol != col)
                {
                    drawCol = col;
                    updateDrawCache = true;
                }

                if (updateDrawCache)
                {
                    if (ZeroAlpha < 1)
                        colZeroCached = new Color4(drawCol.R * ZeroAlpha, drawCol.G * ZeroAlpha, drawCol.B * ZeroAlpha, drawCol.A * ZeroAlpha);

                    //todo: reimplement padding alpha changes
                    //bool isPaddedZero = true;

                    int i = 0;
                    float coordScale = Scale.X * GameBase.SpriteToNativeRatio;

                    foreach (pTexture tex in renderTextures)
                    {
                        Vector2 thisDrawPos = new Vector2(drawPos.X + renderCoordinates[i] * coordScale, drawPos.Y);

                        unsafe
                        {
                            float* coordinatesP = (float*)handle_coordinates_pointer;
                            float* verticesP = (float*)handle_vertices_pointer;
                            tex.TextureGl.DrawTo(coordinatesP, verticesP, i, thisDrawPos, OriginVector, drawScale, Rotation, new Box2(tex.X, tex.Y, tex.X + tex.Width, tex.Y + tex.Height));
                        }

                        i++;
                        // note: no srcRect calculation
                        /*if (ZeroAlpha == 1)
                            tex.TextureGl.Draw(drawPos, OriginVector, col, scale, Rotation, new Box2(tex.X, tex.Y, tex.X + tex.Width, tex.Y + tex.Height));
                        else
                        {
                            if (textArray[i] != '0')
                                isPaddedZero = false;

                            if (isPaddedZero)
                                tex.TextureGl.Draw(drawPos, OriginVector, colZeroCached, scale, Rotation, new Box2(tex.X, tex.Y, tex.X + tex.Width, tex.Y + tex.Height));
                            else
                                tex.TextureGl.Draw(drawPos, OriginVector, col, scale, Rotation, new Box2(tex.X, tex.Y, tex.X + tex.Width, tex.Y + tex.Height));
                        }*/
                    }

                    updateDrawCache = false;
                }

                Texture.TextureGl.Bind();

                SpriteManager.SetColour(drawCol);

                GL.VertexPointer(2, VertexPointerType.Float, 0, handle_vertices_pointer);
                GL.TexCoordPointer(2, TexCoordPointerType.Float, 0, handle_coordinates_pointer);

                GL.DrawArrays(BeginMode.Triangles, 0, renderTextures.Count * 6);

                return true;
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