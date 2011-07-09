using System;
using System.Runtime.InteropServices;
using OpenTK;
using OpenTK.Graphics;
using osum.Helpers;
using osum.Graphics.Sprites;
using Color = OpenTK.Graphics.Color4;
#if iOS
using OpenTK.Graphics.ES11;
using MonoTouch.Foundation;
using MonoTouch.ObjCRuntime;
using MonoTouch.OpenGLES;

using TextureTarget = OpenTK.Graphics.ES11.All;
using TextureParameterName = OpenTK.Graphics.ES11.All;
using EnableCap = OpenTK.Graphics.ES11.All;
using ArrayCap = OpenTK.Graphics.ES11.All;
using BlendingFactorSrc = OpenTK.Graphics.ES11.All;
using BlendingFactorDest = OpenTK.Graphics.ES11.All;
using PixelStoreParameter = OpenTK.Graphics.ES11.All;
using VertexPointerType = OpenTK.Graphics.ES11.All;
using ColorPointerType = OpenTK.Graphics.ES11.All;
using DepthFunction = OpenTK.Graphics.ES11.All;
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
using ErrorCode = OpenTK.Graphics.ES11.All;
using TextureEnvParameter = OpenTK.Graphics.ES11.All;
using TextureEnvTarget =  OpenTK.Graphics.ES11.All;
#else
using OpenTK.Input;
using OpenTK.Graphics.OpenGL;
#endif

using osum.Graphics;
using osum;
using System.Collections.Generic;
using osum.GameplayElements;
using osum.Graphics.Skins;
using osum.Graphics.Primitives;
using System.Drawing;


namespace osum.Graphics.Renderers
{
    /// <summary>
    /// Class to handle drawing of Greg's enhanced sliders.
    /// </summary>
    internal class SliderTrackRenderer : IDisposable
    {
        protected const int MAXRES = 24; // A higher MAXRES produces rounder endcaps at the cost of more vertices
        protected const int TEX_WIDTH = 128; // Please keep power of two

        // Make the quad overhang just slightly to avoid 1px holes between a quad and a wedge from rounding errors.
        protected const float QUAD_OVERLAP_FUDGE = 6.0e-4f;

        // If the peak vertex of a quad is at exactly 0, we get a crack running down the center of horizontal linear sliders.
        // We shift the vertex slightly off to the side to avoid this.
        protected const float QUAD_MIDDLECRACK_FUDGE = 1.0e-4f;

        // Bias to the number of polygons to render in a given wedge. Also ... fixes ... holes.
        protected const float WEDGE_COUNT_FUDGE = 0.2f; // Seems this fudge is needed for osu!m

        // how much to trim off the inside of the texture
        protected const float TEXTURE_SHRINKAGE_FACTOR = 2.0f;

        // how far towards the inside do we slide the texture
#if iOS
        protected const float TEXEL_ORIGIN = 0.25f;
#else
        protected const float TEXEL_ORIGIN = 0.5f;
#endif

        protected int numIndices_quad;
        protected int numIndices_cap;
        protected int numPrimitives_quad;
        protected int numPrimitives_cap;
        protected int numVertices_quad;
        protected int numVertices_cap;

        protected TextureGl[] textures_ogl;

        protected Color border_colour;

        protected float[] coordinates_cap;
        protected float[] vertices_cap;

        protected float[] coordinates_quad;
        protected float[] vertices_quad;

        // initialization
        protected bool am_initted_geom = false;
        protected bool am_initted_tex = false;
        bool boundEvents;


        /// <summary>
        /// Performs all advanced computation needed to draw sliders in a particular beatmap.
        /// </summary>
        /// <param name="device">Shared GraphicsDevice</param>
        /// <param name="content">Shared ContentManager</param>
        /// <param name="outer_colours">Array of colours for the outside of the track. There should be one element for each combo colour in the map.</param>
        /// <param name="inner_colours">Array of colours for the inside of the track. There should be one element for each combo colour in the map.</param>
        /// <param name="border_colour">Single colour for the track's border.</param>
        /// <param name="compute_geometry">If true, meshes will be computed, as opposed to keeping the ones from before. Leave false if you know this isn't the first time a map is being loaded.</param>
        internal void Init(Color[] outer_colours, Color[] inner_colours, Color border_colour)
        {
            this.border_colour = border_colour;

            int iColours = inner_colours.Length;
            if (outer_colours.Length != iColours) throw new ArgumentException("Outer colours and inner colours must match!");

            {
                numVertices_quad = 6;
                numPrimitives_quad = 4;
                numIndices_quad = 6;

                numVertices_cap = MAXRES + 2;
                numPrimitives_cap = MAXRES;
                numIndices_cap = 3 * MAXRES;

                glCalculateCapMesh();
                CalculateQuadMesh();

                am_initted_geom = true;

                if (textures_ogl != null)
                {
                    foreach (TextureGl t in textures_ogl)
                        t.Dispose();
                }

                textures_ogl = new TextureGl[iColours];

                for (int x = 0; x < iColours; x++)
                    textures_ogl[x] = glRenderSliderTexture(border_colour, inner_colours[x], outer_colours[x]);

                am_initted_tex = true;
            }
        }

        /// <summary>
        /// The cap mesh is a half cone.
        /// </summary>
        private void glCalculateCapMesh()
        {
            vertices_cap = new float[(numVertices_cap) * 3];
            coordinates_cap = new float[(numVertices_cap) * 2];

            float maxRes = (float)MAXRES;
            float step = MathHelper.Pi / maxRes;

            // the commented out lines are already set 0 from initialization.
            // they are kept for completeness.
            //vertices_cap[0] = 0.0f;
            //vertices_cap[1] = 0.0f;
            vertices_cap[2] = 1.0f;

            //vertices_cap[3] = 0.0f;
            vertices_cap[4] = -1.0f;
            //vertices_cap[5] = 0.0f;

            coordinates_cap[0] = 1.0f + (TEXEL_ORIGIN - TEXTURE_SHRINKAGE_FACTOR) / TEX_WIDTH;
            //coordinates_cap[1] = 0.0f;

            coordinates_cap[2] = TEXEL_ORIGIN / TEX_WIDTH;
            //coordinates_cap[3] = 0.0f;

            for (int z = 1; z < MAXRES; z++)
            {
                float angle = (float)z * step;
                vertices_cap[z * 3 + 3] = (float)(Math.Sin(angle));
                vertices_cap[z * 3 + 4] = -(float)(Math.Cos(angle));
                //vertices_cap[z * 3 + 5] = 0.0f;

                coordinates_cap[z * 2 + 2] = TEXEL_ORIGIN / TEX_WIDTH;
                //coordinates_cap[z * 2 + 3] = 0.0f;
            }

            //vertices_cap[MAXRES * 3 + 3] = 0.0f;
            vertices_cap[MAXRES * 3 + 4] = 1.0f;
            //vertices_cap[MAXRES * 3 + 5] = 0.0f;

            coordinates_cap[MAXRES * 2 + 2] = TEXEL_ORIGIN / TEX_WIDTH;
            //coordinates_cap[MAXRES * 2 + 3] = 0.0f;
        }

        private void CalculateQuadMesh()
        {
            coordinates_quad = new[]{ TEXEL_ORIGIN / TEX_WIDTH, 0,
                                    TEXEL_ORIGIN / TEX_WIDTH, 0,
                                    1.0f + (TEXEL_ORIGIN - TEXTURE_SHRINKAGE_FACTOR) / TEX_WIDTH, 0,
                                    1.0f + (TEXEL_ORIGIN - TEXTURE_SHRINKAGE_FACTOR) / TEX_WIDTH, 0,
                                    TEXEL_ORIGIN / TEX_WIDTH, 0,
                                    TEXEL_ORIGIN / TEX_WIDTH, 0};

            vertices_quad = new[]{-QUAD_OVERLAP_FUDGE, -1, 0,
                            1 + QUAD_OVERLAP_FUDGE, -1, 0,
                            -QUAD_OVERLAP_FUDGE, QUAD_MIDDLECRACK_FUDGE, 1,
                            1 + QUAD_OVERLAP_FUDGE, QUAD_MIDDLECRACK_FUDGE, 1,
                            -QUAD_OVERLAP_FUDGE, 1, 0,
                            1 + QUAD_OVERLAP_FUDGE, 1, 0};
        }

        /// <summary>
        /// This overload computes the outer/inner colours and AA widths, and has a hardcoded shadow colour.
        /// </summary>
        private TextureGl glRenderSliderTexture(Color border, Color InnerColour, Color OuterColour)
        {
            // This formula is used to keep sliders smooth for a wide variety of screen resolutions and circle sizes.
            // In most cases, it's set to equal a nice ratio of screen pixels, but it's constrained to within two values.
            // When circles are very large, the hitcircle(overlay).png textures become a bit fuzzy, so sharp sliders would look unnatural.
            // When they are tiny, we get the opposite problem. I confine it at 1/16th of a circle-radius, which is almost as wide as its border.
            float aa_width = Math.Min(Math.Max(0.25f / DifficultyManager.HitObjectRadius, 0.015625f), 0.0625f);

            Color shadow = new Color(0, 0, 0, 0.5f);

            GL.Clear(Constants.COLOR_DEPTH_BUFFER_BIT);

            return glRenderSliderTexture(shadow, border, InnerColour, OuterColour, aa_width);
        }

        /// <summary>
        /// Helper function to turn a single colour into a lighter and darker shade for use with the slider's gradient.
        /// </summary>
        /// <param name="colour">HitObject colour</param>
        /// <param name="InnerColour">Track center</param>
        /// <param name="OuterColour">Track edges</param>
        internal static void ComputeSliderColour(Color colour, out Color InnerColour, out Color OuterColour)
        {
            Color col = new Color(colour.R, colour.G, colour.B, 200 / 255f); // Weird opengl transparency issue
            InnerColour = ColourHelper.Lighten2(col, 0.5f);
            OuterColour = ColourHelper.Darken(col, 0.1f);
        }

        /// <summary>
        /// Draws a slider to the active device using a cached texture.
        /// </summary>
        /// <param name="lineList">List of lines to use</param>
        /// <param name="radius">Width of the slider</param>
        /// <param name="ColourIndex">Current combo colour index between 0 and 4; -1 for grey; -2 for Tag Multi override.</param>
        /// <param name="prev">The last line which was rendered in the previous iteration, or null if this is the first iteration.</param>
        internal void Draw(List<Line> lineList, float radius, int ColourIndex, Line prev)
        {
            GL.Color4((byte)255, (byte)255, (byte)255, (byte)255);

            switch (ColourIndex)
            {
                /*case -1: // Grey
                    DrawOGL(lineList, radius, grey_ogl, prev);
                    break;
                case -2: // Multi custom
                    DrawOGL(lineList, radius, multi_ogl, prev);
                    break;*/
                default:
                    DrawOGL(lineList, radius, textures_ogl[ColourIndex], prev, true);
                    break;
            }
        }

        internal void Initialize()
        {
            if (!boundEvents)
            {
                GameBase.OnScreenLayoutChanged += GameBase_OnScreenLayoutChanged;
                boundEvents = true;
            }

            List<Color> innerColours, outerColours;

            innerColours = new List<Color>(5);
            outerColours = new List<Color>(5);

            foreach (LineTextureInfo l in lineTextureCache)
                l.Dispose();
            lineTextureCache.Clear();

            // Automatically calculate some lighter/darker shades to use for the slider track.
            // In the long-term, I'd like these colours to be made skinnable.
            foreach (Color col in TextureManager.DefaultColours)
            {
                Color Inner, Outer;
                ComputeSliderColour(col, out Inner, out Outer);

                innerColours.Add(Inner);
                outerColours.Add(Outer);
            }

            Init(outerColours.ToArray(), innerColours.ToArray(), Color.White);
        }

        List<LineTextureInfo> lineTextureCache = new List<LineTextureInfo>();

        internal TextureGl CreateLineTexture(Color innerColour, Color outerColour, Color borderColour, float radius)
        {
            float aa_width = 0.08f;

            Color shadow = new Color(1, 0, 0, 1);

            LineTextureInfo search = new LineTextureInfo(innerColour, outerColour, borderColour, aa_width);

            LineTextureInfo texInfo = lineTextureCache.Find(t => t.Equals(search));

            if (texInfo == null)
            {
                texInfo = search;
                texInfo.SetTexture(glRenderSliderTexture(shadow, borderColour, innerColour, outerColour, aa_width));
                lineTextureCache.Add(texInfo);
            }

            return texInfo.Texture;
        }

        /// <summary>
        /// Draws a slider to the active device. Its texture is rendered on the fly.
        /// </summary>
        /// <param name="lineList">List of lines to use</param>
        /// <param name="radius">Width of the slider</param>
        /// <param name="colour">Single colour of the track</param>
        /// <param name="borderColour">ruoloCredroB</param>
        /// <param name="prev">The last line which was rendered in the previous iteration, or null if this is the first iteration.</param>
        /// <param name="viewport">(OpenGL only) The rectangle we restore the projection matrix to.</param>
        internal void Draw(List<Line> lineList, float radius, Color innerColour, Color outerColour, Color borderColour, Color4 tint)
        {

            TextureGl tex = CreateLineTexture(innerColour, outerColour, borderColour, radius);

            GL.Color4(tint.R, tint.G, tint.B, tint.A);

            DrawOGL(lineList, radius, tex, null, false);
        }

        #region IDisposable Members

        public void Dispose()
        {
            GameBase.OnScreenLayoutChanged -= GameBase_OnScreenLayoutChanged;
        }

        void GameBase_OnScreenLayoutChanged()
        {
            Initialize();
        }

        #endregion

        protected void glDrawQuad()
        {
            GL.TexCoordPointer(2, TexCoordPointerType.Float, 0, coordinates_quad);
            GL.VertexPointer(3, VertexPointerType.Float, 0, vertices_quad);
            GL.DrawArrays(BeginMode.TriangleStrip, 0, 6);
        }

        protected void glDrawHalfCircle(int count)
        {
            GL.TexCoordPointer(2, TexCoordPointerType.Float, 0, coordinates_cap);
            GL.VertexPointer(3, VertexPointerType.Float, 0, vertices_cap);

            GL.DrawArrays(BeginMode.TriangleFan, 0, count + 2);
        }

        /// <summary>
        /// Render a gradient into a 256x1 texture.
        /// </summary>
        protected TextureGl glRenderSliderTexture(OpenTK.Graphics.Color4 shadow, OpenTK.Graphics.Color4 border, OpenTK.Graphics.Color4 InnerColour, OpenTK.Graphics.Color4 OuterColour, float aa_width)
        {
            SpriteManager.TexturesEnabled = false;

            GL.PushMatrix();

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();

            //todo: can we do this whole function without changing the viewport? it should be possible i think (and might be much more efficient)
            GL.Viewport(0, 0, TEX_WIDTH, 1);
            GL.Ortho(0.0f, 1.0f, 1.0f, -1.0f, -1.0f, 1.0f);

            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();

            GL.EnableClientState(ArrayCap.ColorArray);

            float[] colours = {0,0,0,0,
                               0,0,0,0,
                               shadow.R, shadow.G, shadow.B, shadow.A,
                               border.R, border.G, border.B, border.A,
                               border.R, border.G, border.B, border.A,
                               OuterColour.R, OuterColour.G, OuterColour.B, OuterColour.A,
                               InnerColour.R, InnerColour.G, InnerColour.B, InnerColour.A,
                               InnerColour.R, InnerColour.G, InnerColour.B, InnerColour.A};

            float[] vertices = { -1.0f, 0,
                                   0, 0,
                0.078125f - aa_width, 0.0f,
                0.078125f + aa_width, 0.0f,
                0.1875f - aa_width, 0.0f,
                0.1875f + aa_width, 0.0f,
                1.0f, 0.0f,
                2.0f, 0};

            GL.VertexPointer(2, VertexPointerType.Float, 0, vertices);
            GL.ColorPointer(4, ColorPointerType.Float, 0, colours);
            GL.DrawArrays(BeginMode.LineStrip, 0, 8);

            GL.DisableClientState(ArrayCap.ColorArray);

            TextureGl result = new TextureGl(TEX_WIDTH, 1);

            int[] textures = new int[1];
            GL.GenTextures(1, textures);
            int textureId = textures[0];

            GL.BindTexture(TextureGl.SURFACE_TYPE, textureId);

            GL.TexParameter(TextureGl.SURFACE_TYPE, TextureParameterName.TextureMinFilter, (int)All.Linear);
            GL.TexParameter(TextureGl.SURFACE_TYPE, TextureParameterName.TextureMagFilter, (int)All.Linear);
            GL.TexParameter(TextureGl.SURFACE_TYPE, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureGl.SURFACE_TYPE, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

            GL.CopyTexImage2D(TextureGl.SURFACE_TYPE, 0, PixelInternalFormat.Rgba, 0, 0, TEX_WIDTH, 1, 0);

            result.SetData(textureId);

            GameBase.Instance.SetViewport();

            GL.PopMatrix();

            return result;
        }

        /// <summary>
        /// Core drawing method in OpenGL
        /// </summary>
        /// <param name="lineList">List of lines to use</param>
        /// <param name="globalRadius">Width of the slider</param>
        /// <param name="texture">Texture used for the track</param>
        /// <param name="prev">The last line which was rendered in the previous iteration, or null if this is the first iteration.</param>
        protected void DrawOGL(List<Line> lineList, float globalRadius, TextureGl texture, Line prev, bool renderingToTexture)
        {
            if (renderingToTexture)
            {
                GL.Disable(EnableCap.Blend);
                GL.DepthMask(true);
                GL.DepthFunc(DepthFunction.Lequal);
                GL.Enable(EnableCap.DepthTest);
            }

            SpriteManager.TexturesEnabled = true;

            GL.MatrixMode(MatrixMode.Modelview);

            texture.Bind();

            int count = lineList.Count;
            for (int x = 1; x < count; x++)
            {
                DrawLineOGL(prev, lineList[x - 1], lineList[x], globalRadius);
                prev = lineList[x - 1];
            }

            if (count > 0)
                DrawLineOGL(prev, lineList[count - 1], null, globalRadius);

            if (renderingToTexture)
            {
                GL.Enable(EnableCap.Blend);
                GL.Disable(EnableCap.DepthTest);
                GL.DepthMask(false);
            }
        }

        protected void DrawLineOGL(Line prev, Line curr, Line next, float globalRadius)
        {
            // Quad
            Matrix4 matrix = new Matrix4(curr.rho, 0, 0, 0, // Scale-X
                                        0, globalRadius, 0, 0, // Scale-Y
                                        0, 0, 1, 0,
                                        0, 0, 0, 1) * curr.WorldMatrix();

            GL.LoadMatrix(new float[]{matrix.M11, matrix.M12, matrix.M13, matrix.M14,
                                    matrix.M21, matrix.M22, matrix.M23, matrix.M24,
                                    matrix.M31, matrix.M32, matrix.M33, matrix.M34,
                                    matrix.M41, matrix.M42, matrix.M43, matrix.M44});

            glDrawQuad();

            int end_triangles;
            bool flip;
            if (next == null)
            {
                flip = false; // totally irrelevant
                end_triangles = numPrimitives_cap;
            }
            else
            {
                float theta = next.theta - curr.theta;

                // keep on the +- pi/2 range.
                if (theta > Math.PI) theta -= (float)(Math.PI * 2);
                if (theta < -Math.PI) theta += (float)(Math.PI * 2);

                if (theta < 0)
                {
                    flip = true;
                    end_triangles = (int)Math.Ceiling((-theta) * MAXRES / Math.PI + WEDGE_COUNT_FUDGE);
                }
                else if (theta > 0)
                {
                    flip = false;
                    end_triangles = (int)Math.Ceiling(theta * MAXRES / Math.PI + WEDGE_COUNT_FUDGE);
                }
                else
                {
                    flip = false; // totally irrelevant
                    end_triangles = 0;
                }
            }
            end_triangles = Math.Min(end_triangles, numPrimitives_cap);

            // Cap on end
            if (flip)
            {
                matrix = new Matrix4(globalRadius, 0, 0, 0,
                                    0, -globalRadius, 0, 0,
                                    0, 0, 1, 0,
                                    0, 0, 0, 1) * curr.EndWorldMatrix();

                GL.LoadMatrix(new float[]{matrix.M11, matrix.M12, matrix.M13, matrix.M14,
                                    matrix.M21, matrix.M22, matrix.M23, matrix.M24,
                                    matrix.M31, matrix.M32, matrix.M33, matrix.M34,
                                    matrix.M41, matrix.M42, matrix.M43, matrix.M44});

            }
            else
            {
                matrix = new Matrix4(globalRadius, 0, 0, 0,
                                    0, globalRadius, 0, 0,
                                    0, 0, 1, 0,
                                    0, 0, 0, 1) * curr.EndWorldMatrix();

                GL.LoadMatrix(new float[]{matrix.M11, matrix.M12, matrix.M13, matrix.M14,
                                    matrix.M21, matrix.M22, matrix.M23, matrix.M24,
                                    matrix.M31, matrix.M32, matrix.M33, matrix.M34,
                                    matrix.M41, matrix.M42, matrix.M43, matrix.M44});
            }

            glDrawHalfCircle(end_triangles);

            // Cap on start
            bool hasStartCap = false;

            if (prev == null) hasStartCap = true;
            else if (curr.p1 != prev.p2) hasStartCap = true;

            //todo: this makes stuff look bad... need to look into it.
            if (hasStartCap)
            {
                // Catch for Darrinub and other slider inconsistencies. (Redpoints seem to be causing some.)
                // Render a complete beginning cap if this Line isn't connected to the end of the previous line.

                matrix = new Matrix4(-globalRadius, 0, 0, 0,
                                    0, -globalRadius, 0, 0,
                                    0, 0, 1, 0,
                                    0, 0, 0, 1) * curr.WorldMatrix();

                GL.LoadMatrix(new float[]{matrix.M11, matrix.M12, matrix.M13, matrix.M14,
                                    matrix.M21, matrix.M22, matrix.M23, matrix.M24,
                                    matrix.M31, matrix.M32, matrix.M33, matrix.M34,
                                    matrix.M41, matrix.M42, matrix.M43, matrix.M44});

                glDrawHalfCircle(numPrimitives_cap);
            }
        }
    }

    public class LineTextureInfo : IEquatable<LineTextureInfo>
    {
        public Color4 Inner;
        public Color4 Outer;
        public Color4 Border;
        public float Width;
        public TextureGl Texture;

        public LineTextureInfo(Color4 inner, Color4 outer, Color4 border, float width)
        {
            Inner = inner;
            Outer = outer;
            Border = border;
            Width = width;
        }

        public void SetTexture(TextureGl texture)
        {
            Texture = texture;
        }

        public void Dispose()
        {
            if (Texture != null)
                Texture.Dispose();
        }

        #region IEquatable<LineTextureInfo> Members

        public bool Equals(LineTextureInfo other)
        {
            return other.Inner == Inner && other.Outer == Outer && other.Border == Border && other.Width == Width;
        }

        #endregion
    }
}