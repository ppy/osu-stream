using System;
using System.Runtime.InteropServices;
using OpenTK;
using OpenTK.Graphics;
using osum.Helpers;
using osum.Graphics.Sprites;
using Color = OpenTK.Graphics.Color4;
#if IPHONE
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
using ErrorCode = OpenTK.Graphics.ES11.All;
using TextureEnvParameter = OpenTK.Graphics.ES11.All;
using TextureEnvTarget =  OpenTK.Graphics.ES11.All;
using osum.Graphics;
using osu.Graphics.Primitives;
using System.Collections.Generic;
using System.Drawing;
#else
using OpenTK.Input;
using OpenTK.Graphics.OpenGL;
using System.Drawing;
using osum.Input;
#endif

namespace osu.Graphics.Renderers
{
    /// <summary>
    /// Class to handle drawing of Greg's enhanced sliders.
    /// </summary>
    internal class SliderTrackRenderer
    {
        private const int MAXRES = 24; // A higher MAXRES produces rounder endcaps at the cost of more vertices
        private const int TEX_WIDTH = 128; // Please keep power of two

        // Make the quad overhang just slightly to avoid 1px holes between a quad and a wedge from rounding errors.
        private const float QUAD_OVERLAP_FUDGE = 3.0e-4f;

        // If the peak vertex of a quad is at exactly 0, we get a crack running down the center of horizontal linear sliders.
        // We shift the vertex slightly off to the side to avoid this.
        private const float QUAD_MIDDLECRACK_FUDGE = 1.0e-4f;

        // Bias to the number of polygons to render in a given wedge. Also ... fixes ... holes.
        private const float WEDGE_COUNT_FUDGE = 0.0f; // Seems this fudge is unneeded YIPEE

        private int bytesPerVertex;
        private int numIndices_quad;
        private int numIndices_cap;
        private int numPrimitives_quad;
        private int numPrimitives_cap;
        private int numVertices_quad;
        private int numVertices_cap;

        private TextureGl[] textures_ogl;

        private TextureGl grey_ogl;

        private TextureGl multi_ogl;

        private bool toon;
        private Color border_colour;

        private Vector3[] vertices_ogl;

        private bool am_initted_geom = false;
        private bool am_initted_tex = false;

        /// <summary>
        /// Performs all advanced computation needed to draw sliders in a particular beatmap.
        /// </summary>
        /// <param name="device">Shared GraphicsDevice</param>
        /// <param name="content">Shared ContentManager</param>
        /// <param name="outer_colours">Array of colours for the outside of the track. There should be one element for each combo colour in the map.</param>
        /// <param name="inner_colours">Array of colours for the inside of the track. There should be one element for each combo colour in the map.</param>
        /// <param name="border_colour">Single colour for the track's border.</param>
        /// <param name="toon">If true, the track gradient is made of four solid colours instead of a smooth gradient.</param>
        /// <param name="compute_geometry">If true, meshes will be computed, as opposed to keeping the ones from before. Leave false if you know this isn't the first time a map is being loaded.</param>
        internal void Init(Color[] outer_colours, Color[] inner_colours, Color border_colour)
        {
            this.border_colour = border_colour;

            int iColours = inner_colours.Length;
            if (outer_colours.Length != iColours) throw new ArgumentException("Outer colours and inner colours must match!");

            {
                if (!am_initted_geom) // TODO: Vertex buffers
                {
                    numVertices_quad = 6;
                    numPrimitives_quad = 4;
                    numIndices_quad = 6;

                    numVertices_cap = MAXRES + 2;
                    numPrimitives_cap = MAXRES;
                    numIndices_cap = 3 * MAXRES;

                    glCalculateCapMesh();

                    am_initted_geom = true;
                }

                if (iColours == 0) // Temporary catch for i332-triggered hard crash
                {
                    textures_ogl = new TextureGl[1];
                    textures_ogl[0] = glRenderSliderTexture(new Color(255, 255, 255, 255), new Color(255, 255, 255, 64), new Color(0, 0, 0, 64));
                }
                else
                {
                    textures_ogl = new TextureGl[iColours];

                    for (int x = 0; x < iColours; x++)
                    {
                        textures_ogl[x] = glRenderSliderTexture(border_colour, inner_colours[x], outer_colours[x]);
                    }
                }

                Color grey1, grey2;
                ComputeSliderColour(Color.Gray, out grey1, out grey2);
                //grey_ogl = glRenderSliderTexture(border_colour, grey1, grey2);
                multi_ogl = textures_ogl[0]; // Should be unneeded if things go right.

                am_initted_tex = true;
            }
        }

        /// <summary>
        /// The cap mesh is a half cone.
        /// </summary>
        private void glCalculateCapMesh()
        {
            vertices_ogl = new Vector3[numVertices_cap - 1];

            float maxRes = (float)MAXRES;
            float step = MathHelper.Pi / maxRes;

            vertices_ogl[0] = new Vector3(0.0f, -1.0f, 0.0f);

            for (int z = 1; z < MAXRES; z++)
            {
                float angle = (float)z * step;
                vertices_ogl[z] = new Vector3((float)(Math.Sin(angle)), -(float)(Math.Cos(angle)), 0.0f);
            }

            vertices_ogl[MAXRES] = new Vector3(0.0f, 1.0f, 0.0f);
        }

        private void glDrawQuad()
        {
            // Todo: vertex buffers

            GL.Begin(BeginMode.TriangleStrip);

            GL.TexCoord2(0.0f, 0.0f);
            GL.Vertex3(-QUAD_OVERLAP_FUDGE, -1.0f, 0.0f);
            GL.Vertex3(1.0f + QUAD_OVERLAP_FUDGE, -1.0f, 0.0f);

            GL.TexCoord2((float)TEX_WIDTH, 0.0f);
            GL.Vertex3(-QUAD_OVERLAP_FUDGE, QUAD_MIDDLECRACK_FUDGE, 1.0f);
            GL.Vertex3(1.0f + QUAD_OVERLAP_FUDGE, QUAD_MIDDLECRACK_FUDGE, 1.0f);

            GL.TexCoord2(0.0f, 0.0f);
            GL.Vertex3(-QUAD_OVERLAP_FUDGE, 1.0f, 0.0f);
            GL.Vertex3(1.0f + QUAD_OVERLAP_FUDGE, 1.0f, 0.0f);

            GL.End();
        }

        private void glDrawHalfCircle(int count)
        {
            if (count > 0)
            {
                // Todo: vertex buffers

                GL.Begin(BeginMode.TriangleFan);

                GL.TexCoord2((float)TEX_WIDTH, 0.0f);
                GL.Vertex3(0.0f, 0.0f, 1.0f);

                GL.TexCoord2(0.0f, 0.0f);
                for (int x = 0; x <= count; x++)
                {
                    Vector3 v = vertices_ogl[x];
                    GL.Vertex3(v.X, v.Y, v.Z);
                }

                GL.End();
            }
        }

        /// <summary>
        /// Render a gradient into a 256x1 texture.
        /// </summary>
        private TextureGl glRenderSliderTexture(Color shadow, Color border, Color InnerColour, Color OuterColour, float aa_width, bool toon)
        {
            GL.PushAttrib(AttribMask.EnableBit);
            

            GL.Viewport(0, 0, TEX_WIDTH, 1);
            GL.Disable(EnableCap.DepthTest);

            GL.MatrixMode(MatrixMode.Modelview);

            GL.LoadIdentity();

            GL.MatrixMode(MatrixMode.Projection);

            GL.LoadIdentity();
            GL.Ortho(0.0d, 1.0d, 1.0d, 0.0d, -1.0d, 1.0d);

            GL.Clear(ClearBufferMask.ColorBufferBit);

            {
                GL.Begin(BeginMode.LineStrip);

                GL.Color4(0, 0, 0, 0);
                GL.Vertex2(0.0f, 0.0f);

                GL.Color4(shadow.R, shadow.G, shadow.B, shadow.A);
                GL.Vertex2(0.078125f - aa_width, 0.0f);

                GL.Color4(border.R, border.G, border.B, border.A);
                GL.Vertex2(0.078125f + aa_width, 0.0f);
                GL.Vertex2(0.1875f - aa_width, 0.0f);

                GL.Color4(OuterColour.R, OuterColour.G, OuterColour.B, OuterColour.A);
                GL.Vertex2(0.1875f + aa_width, 0.0f);

                GL.Color4(InnerColour.R, InnerColour.G, InnerColour.B, InnerColour.A);
                GL.Vertex2(1.0f, 0.0f);

                GL.End();
            }

            TextureGl result = new TextureGl(TEX_WIDTH, 1);
            int[] textures = new int[1];

            GL.GenTextures(1, textures);
            GL.Enable((EnableCap)TextureGl.SURFACE_TYPE);

            GL.BindTexture(TextureGl.SURFACE_TYPE, textures[0]);
            GL.TexParameter(TextureGl.SURFACE_TYPE, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureGl.SURFACE_TYPE, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

            GL.CopyTexImage2D(TextureGl.SURFACE_TYPE, 0, PixelInternalFormat.Rgba, 0, 0, TEX_WIDTH, 1, 0);
            GL.Disable((EnableCap)TextureGl.SURFACE_TYPE);

            result.SetData(textures[0]);

            GL.PopAttrib();

            //restore viewport (can make this more efficient but not much point?)
            GameBase.Instance.SetupScreen();

            return result;
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
            float aa_width = Math.Min(Math.Max(0.25f / (DifficultyManager.HitObjectRadius * GameBase.GamefieldRatio), 0.015625f), 0.0625f);

            Color shadow = new Color(0, 0, 0, 128);

            return glRenderSliderTexture(shadow, border, InnerColour, OuterColour, aa_width, toon);
        }

        /// <summary>
        /// Recomputes the textures used on Tag Multi custom colour sliders.
        /// </summary>
        internal void SetTagCustomColour(Color color)
        {
            Color inner, outer;
            ComputeSliderColour(color, out inner, out outer);

            multi_ogl = glRenderSliderTexture(border_colour, inner, outer);
        }

        /// <summary>
        /// Helper function to turn a single colour into a lighter and darker shade for use with the slider's gradient.
        /// </summary>
        /// <param name="colour">HitObject colour</param>
        /// <param name="InnerColour">Track center</param>
        /// <param name="OuterColour">Track edges</param>
        internal static void ComputeSliderColour(Color colour, out Color InnerColour, out Color OuterColour)
        {
            Color col = new Color(colour.R, colour.G, colour.B, 230/255f); // Weird opengl transparency issue
            InnerColour = ColourHelper.Lighten2(col, 0.5f);
            OuterColour = ColourHelper.Darken(col, 0.1f);
        }

        /// <summary>
        /// Draws a slider to the active device using a cached texture.
        /// </summary>
        /// <param name="lineList">List of lines to use</param>
        /// <param name="globalRadius">Width of the slider</param>
        /// <param name="ColourIndex">Current combo colour index between 0 and 4; -1 for grey; -2 for Tag Multi override.</param>
        /// <param name="prev">The last line which was rendered in the previous iteration, or null if this is the first iteration.</param>
        internal void Draw(List<Line> lineList, float globalRadius, int ColourIndex, Line prev)
        {
            if (!am_initted_tex || !am_initted_geom) initialize();

            {
                switch (ColourIndex)
                {
                    case -1: // Grey
                        DrawOGL(lineList, globalRadius, grey_ogl, prev);
                        break;
                    case -2: // Multi custom
                        DrawOGL(lineList, globalRadius, multi_ogl, prev);
                        break;
                    default:
                        if ((ColourIndex > textures_ogl.Length) || (ColourIndex < 0))
                        {
#if DEBUG
                            throw new ArgumentOutOfRangeException("Colour index outside the range of the collection.");
#else
                            DrawOGL(lineList, globalRadius, grey_ogl, prev);
#endif
                        }
                        else DrawOGL(lineList, globalRadius, textures_ogl[ColourIndex], prev);
                        break;
                }
            }
        }

        private void initialize()
        {
            List<Color> innerColours, outerColours;
            
            {
                innerColours = new List<Color>(5);
                outerColours = new List<Color>(5);

                // Automatically calculate some lighter/darker shades to use for the slider track.
                // In the long-term, I'd like these colours to be made skinnable.
                foreach (Color col in SkinManager.DefaultColours)
                {
                    Color Inner, Outer;
                    ComputeSliderColour(col, out Inner, out Outer);

                    innerColours.Add(Inner);
                    outerColours.Add(Outer);
                }
            }

            Init(outerColours.ToArray(), innerColours.ToArray(), Color.White);
        }

        /// <summary>
        /// Draws a slider to the active device. Its texture is rendered on the fly.
        /// </summary>
        /// <param name="lineList">List of lines to use</param>
        /// <param name="globalRadius">Width of the slider</param>
        /// <param name="colour">Single colour of the track</param>
        /// <param name="BorderColour">ruoloCredroB</param>
        /// <param name="prev">The last line which was rendered in the previous iteration, or null if this is the first iteration.</param>
        /// <param name="viewport">(OpenGL only) The rectangle we restore the projection matrix to.</param>
        internal void Draw(List<Line> lineList, float globalRadius, Color colour, Color BorderColour, Line prev, Rectangle projection)
        {
            if (!am_initted_tex || !am_initted_geom) initialize();

            Color Inner, Outer;
            ComputeSliderColour(colour, out Inner, out Outer);

            TextureGl tex = glRenderSliderTexture(BorderColour, Inner, Outer);

            GL.Viewport(0, 0, projection.Width, projection.Height);

            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(projection.Left, projection.Right, projection.Top, projection.Bottom, -1, 1); // Bottom and top are flipped but this is the way peppy did it so is expected

            DrawOGL(lineList, globalRadius, tex, prev);

            tex.Dispose(); // hmmmm do we wait for GC or kill it off the bat?
            //we kill it ;) -peppy
        }

        /// <summary>
        /// Core drawing method in OpenGL
        /// </summary>
        /// <param name="lineList">List of lines to use</param>
        /// <param name="globalRadius">Width of the slider</param>
        /// <param name="texture">Texture used for the track</param>
        /// <param name="prev">The last line which was rendered in the previous iteration, or null if this is the first iteration.</param>
        private void DrawOGL(List<Line> lineList, float globalRadius, TextureGl texture, Line prev)
        {
            GL.PushAttrib(AttribMask.EnableBit);

            GL.Disable(EnableCap.CullFace);
            GL.Disable(EnableCap.Blend);
            GL.Enable(EnableCap.DepthTest);
            GL.DepthMask(true);
            GL.DepthFunc(DepthFunction.Lequal);

            GL.Enable((EnableCap)TextureGl.SURFACE_TYPE);
            GL.Color3(255, 255, 255);

            // Select The Modelview Matrix
            GL.MatrixMode(MatrixMode.Modelview);
            // Reset The Modelview Matrix
            GL.LoadIdentity();

            GL.BindTexture(TextureGl.SURFACE_TYPE, texture.Id);
            GL.TexParameter(TextureGl.SURFACE_TYPE, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureGl.SURFACE_TYPE, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

            int count = lineList.Count;

            for (int x = 1; x < count; x++)
            {
                DrawLineOGL(prev, lineList[x - 1], lineList[x], globalRadius);

                prev = lineList[x - 1];
            }

            DrawLineOGL(prev, lineList[count - 1], null, globalRadius);

            GL.LoadIdentity();

            GL.PopAttrib();
        }

        private void DrawLineOGL(Line prev, Line curr, Line next, float globalRadius)
        {
            // Quad
            Matrix4 matrix = new Matrix4(curr.rho, 0, 0, 0, // Scale-X
                                        0, globalRadius, 0, 0, // Scale-Y
                                        0, 0, 1, 0,
                                        0, 0, 0, 1) * curr.WorldMatrix();

            GL.LoadMatrix(ref matrix);

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

                GL.LoadMatrix(ref matrix);

            }
            else
            {
                matrix = new Matrix4(globalRadius, 0, 0, 0,
                                    0, globalRadius, 0, 0,
                                    0, 0, 1, 0,
                                    0, 0, 0, 1) * curr.EndWorldMatrix();
                GL.LoadMatrix(ref matrix);
            }

            glDrawHalfCircle(end_triangles);

            // Cap on start
            bool hasStartCap = false;

            if (prev == null) hasStartCap = true;
            else if (curr.p1 != prev.p2) hasStartCap = true;

            if (hasStartCap)
            {
                // Catch for Darrinub and other slider inconsistencies. (Redpoints seem to be causing some.)
                // Render a complete beginning cap if this Line isn't connected to the end of the previous line.

                matrix = new Matrix4(-globalRadius, 0, 0, 0,
                                    0, -globalRadius, 0, 0,
                                    0, 0, 1, 0,
                                    0, 0, 0, 1) * curr.WorldMatrix();

                GL.LoadMatrix(ref matrix);
                glDrawHalfCircle(numPrimitives_cap);
            }
        }
    }
}