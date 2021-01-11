using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using OpenTK;
using osum.Graphics.Primitives;
using osum.Graphics.Sprites;
using Color = OpenTK.Graphics.Color4;
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
using TextureEnvTarget = OpenTK.Graphics.ES11.All;
#else
using OpenTK.Graphics.OpenGL;

#endif


namespace osum.Graphics.Renderers
{
    /// <summary>
    /// Class to handle drawing of Greg's enhanced sliders.
    /// </summary>
    internal class SliderTrackRenderer : IDisposable
    {
        protected const int MAXRES = 24; // A higher MAXRES produces rounder endcaps at the cost of more vertices

        // Make the quad overhang just slightly to avoid 1px holes between a quad and a wedge from rounding errors.
        protected const float QUAD_OVERLAP_FUDGE = 6.0e-4f;

        // If the peak vertex of a quad is at exactly 0, we get a crack running down the center of horizontal linear sliders.
        // We shift the vertex slightly off to the side to avoid this.
        protected const float QUAD_MIDDLECRACK_FUDGE = 1.0e-4f;

        // Bias to the number of polygons to render in a given wedge. Also ... fixes ... holes.
        protected const float WEDGE_COUNT_FUDGE = 0.25f; // Seems this fudge is needed for osu!m

        // how much to trim off the inside of the texture
        protected const float TEXTURE_SHRINKAGE_FACTOR = 0.0f;

        // how far towards the inside do we slide the texture
#if iOS || ANDROID
        protected const float TEXEL_ORIGIN = 0.25f;
#else
        protected const float TEXEL_ORIGIN = 0.5f;
#endif
        protected const int COLOUR_COUNT = 4;

        protected int numIndices_quad;
        protected int numIndices_cap;
        protected int numPrimitives_quad;
        protected int numPrimitives_cap;
        protected int numVertices_quad;
        protected int numVertices_cap;

#if !NO_PIN_SUPPORT
        protected float[][] coordinates_cap;
        protected float[] vertices_cap;
#endif

        private GCHandle[] coordinates_cap_handle;
        private IntPtr[] coordinates_cap_pointer;

        private GCHandle vertices_cap_handle;
        private IntPtr vertices_cap_pointer;

#if !NO_PIN_SUPPORT
        protected float[][] coordinates_quad;
#endif
        protected static float[] vertices_quad;
        protected static object vertices_lock = new object();

        private GCHandle[] coordinates_quad_handle;
        private IntPtr[] coordinates_quad_pointer;

        protected static GCHandle vertices_quad_handle;
        protected static IntPtr vertices_quad_pointer;

        // initialization
        protected bool am_initted_geom;
        private bool boundEvents;

        // size of the sprite sheet if this were a retina screen
        private int retinaHeight;

        // ptexture info of where in the sheet our tracks are
        private int sheetY;

        // cache actual texel coordinates for x-axis since they are always the same
        private float sheetStart, sheetEnd;

        private pTexture trackTexture;

        static SliderTrackRenderer()
        {
            vertices_quad = new[]
            {
                -QUAD_OVERLAP_FUDGE, -1, 0,
                1 + QUAD_OVERLAP_FUDGE, -1, 0,
                -QUAD_OVERLAP_FUDGE, QUAD_MIDDLECRACK_FUDGE, 1,
                1 + QUAD_OVERLAP_FUDGE, QUAD_MIDDLECRACK_FUDGE, 1,
                -QUAD_OVERLAP_FUDGE, 1, 0,
                1 + QUAD_OVERLAP_FUDGE, 1, 0
            };

#if !NO_PIN_SUPPORT
            vertices_quad_handle = GCHandle.Alloc(vertices_quad, GCHandleType.Pinned);
            vertices_quad_pointer = vertices_quad_handle.AddrOfPinnedObject();
#else
            vertices_quad_pointer = Marshal.AllocHGlobal(vertices_quad.Length *  sizeof(float));
            unsafe
            {
                float* vertices_quad_p = (float*) vertices_quad_pointer.ToPointer();
                for (int i = 0; i < vertices_quad.Length; i++)
                    vertices_quad_p[i] = vertices_quad[i];
            }
#endif
        }

        /// <summary>
        /// Performs all advanced computation needed to draw sliders in a particular beatmap.
        /// </summary>
        internal void Init()
        {
            numVertices_quad = 6;
            numPrimitives_quad = 4;
            numIndices_quad = 6;

            numVertices_cap = MAXRES + 2;
            numPrimitives_cap = MAXRES;
            numIndices_cap = 3 * MAXRES;

            pTexture texture = TextureManager.Load(OsuTexture.tracks);

            float retinaWidth = texture.TextureGl.TextureWidth;
            retinaHeight = texture.TextureGl.TextureHeight;
            float sheetX = texture.X;
            sheetY = texture.Y;

            float texel_origin = GameBase.SpriteSheetResolution == 960 ? TEXEL_ORIGIN : 0.375f;

            sheetStart = (2.0f + sheetX + texel_origin) / retinaWidth;
            sheetEnd = (-3.0f + sheetX + texture.Width + texel_origin - TEXTURE_SHRINKAGE_FACTOR) / retinaWidth;

            CalculateCapMesh();
            CalculateQuadMesh();

            am_initted_geom = true;

            trackTexture = TextureManager.Load(OsuTexture.tracks);
        }

        /// <summary>
        /// The cap mesh is a half cone.
        /// </summary>
        private void CalculateCapMesh()
        {
#if !NO_PIN_SUPPORT
            vertices_cap = new float[(numVertices_cap) * 3];
            coordinates_cap = new float[COLOUR_COUNT][];

            coordinates_cap_handle = new GCHandle[COLOUR_COUNT];
#else
            vertices_cap_pointer = Marshal.AllocHGlobal(numVertices_cap * 3 * sizeof(float));
#endif
            coordinates_cap_pointer = new IntPtr[COLOUR_COUNT];


            float maxRes = MAXRES;
            float step = MathHelper.Pi / maxRes;
#if NO_PIN_SUPPORT
                float* vertices_cap = (float*)vertices_cap_pointer.ToPointer();

                vertices_cap[0] = 0.0f;
                vertices_cap[1] = 0.0f;
                vertices_cap[3] = 0.0f;
                vertices_cap[5] = 0.0f;
#endif
            vertices_cap[2] = 1.0f;
            vertices_cap[4] = -1.0f;


            for (int z = 1; z < MAXRES; z++)
            {
                float angle = z * step;
                vertices_cap[z * 3 + 3] = (float)(Math.Sin(angle));
                vertices_cap[z * 3 + 4] = -(float)(Math.Cos(angle));
#if NO_PIN_SUPPORT
                    vertices_cap[z * 3 + 5] = 0.0f;
#endif
            }
#if NO_PIN_SUPPORT
                vertices_cap[MAXRES * 3 + 3] = 0.0f;
                vertices_cap[MAXRES * 3 + 5] = 0.0f;
#endif
            vertices_cap[MAXRES * 3 + 4] = 1.0f;


            for (int x = 0; x < COLOUR_COUNT; x++)
            {
                float y = (2.0f + 4.0f * x + sheetY + TEXEL_ORIGIN) / retinaHeight;

#if NO_PIN_SUPPORT
                    IntPtr this_coordinates_pointer = Marshal.AllocHGlobal(numVertices_cap * 2 * sizeof(float));
                    float* this_coordinates = (float*)this_coordinates_pointer.ToPointer();
#else
                float[] this_coordinates = new float[(numVertices_cap) * 2];
#endif
                this_coordinates[0] = sheetEnd;
                this_coordinates[1] = y;

                this_coordinates[2] = sheetStart;
                this_coordinates[3] = y;

                for (int z = 1; z < MAXRES; z++)
                {
                    this_coordinates[z * 2 + 2] = sheetStart;
                    this_coordinates[z * 2 + 3] = y;
                }

                this_coordinates[MAXRES * 2 + 2] = sheetStart;
                this_coordinates[MAXRES * 2 + 3] = y;

#if !NO_PIN_SUPPORT
                coordinates_cap[x] = this_coordinates;
                coordinates_cap_handle[x] = GCHandle.Alloc(coordinates_cap[x], GCHandleType.Pinned);
                coordinates_cap_pointer[x] = coordinates_cap_handle[x].AddrOfPinnedObject();
#else
                    coordinates_cap_pointer[x] = this_coordinates_pointer;
#endif
            }

#if !NO_PIN_SUPPORT
            vertices_cap_handle = GCHandle.Alloc(vertices_cap, GCHandleType.Pinned);
            vertices_cap_pointer = vertices_cap_handle.AddrOfPinnedObject();
#endif
        }

        private unsafe void CalculateQuadMesh(float* dest, float radius, Line prev, Line cur, Line next, bool flip_start, bool flip_end)
        {
            if (prev != null) prev.Recalc();
            if (cur != null) cur.Recalc();
            if (next != null) next.Recalc();
            Vector2 p = cur.unitAngle * radius;
            dest[6] = cur.p1.X;
            dest[7] = cur.p1.Y;
            dest[9] = cur.p2.X;
            dest[10] = cur.p2.Y;

            if (prev == null)
            {
                dest[0] = cur.p1.X - cur.unitAngle.Y * radius;
                dest[1] = cur.p1.Y + cur.unitAngle.X * radius;
                dest[12] = cur.p1.X + cur.unitAngle.Y * radius;
                dest[13] = cur.p1.Y - cur.unitAngle.X * radius;
            }
            else
            {
                Vector2 p0 = -(prev.unitAngle) * radius;
                Vector2 p1 = p;
                float ratiox = p1.X / p0.X;
                float ratioy = p1.Y / p0.Y;

                dest[0] = cur.p1.X;
                dest[1] = cur.p1.Y;
                dest[12] = cur.p1.X;
                dest[13] = cur.p1.Y;

                if (!flip_start)
                {
                    if (p0.X < 0.00390625f)
                    {
                        dest[0] -= prev.unitAngle.Y * radius;
                    }
                    else
                    {
                        dest[0] -= (ratiox <= 0.99609375f)
                            ? (p1.Y + p0.Y * ratiox) / (1 - ratiox)
                            : (cur.unitAngle.Y + prev.unitAngle.Y) * radius * 0.5f;
                    }

                    if (p0.Y < 0.00390625f)
                    {
                        dest[1] += prev.unitAngle.X * radius;
                    }
                    else
                    {
                        dest[1] += (ratioy <= 0.99609375f)
                            ? (p1.X + p0.X * ratioy) / (1 - ratioy)
                            : (cur.unitAngle.X + prev.unitAngle.X) * radius * 0.5f;
                    }

                    dest[12] += cur.unitAngle.Y * radius;
                    dest[13] -= cur.unitAngle.X * radius;
                }
                else
                {
                    dest[0] -= cur.unitAngle.Y * radius;
                    dest[1] += cur.unitAngle.X * radius;

                    if (p0.X < 0.00390625f)
                    {
                        dest[12] += prev.unitAngle.Y * radius;
                    }
                    else
                    {
                        dest[12] += (ratiox <= 0.99609375f)
                            ? (p1.Y + p0.Y * ratiox) / (1 - ratiox)
                            : (cur.unitAngle.Y + prev.unitAngle.Y) * radius * 0.5f;
                    }

                    if (p0.Y < 0.00390625f)
                    {
                        dest[13] -= prev.unitAngle.X * radius;
                    }
                    else
                    {
                        dest[13] -= (ratioy <= 0.99609375f)
                            ? (p1.X + p0.X * ratioy) / (1 - ratioy)
                            : (cur.unitAngle.X + prev.unitAngle.X) * radius * 0.5f;
                    }
                }
            }

            if (next == null)
            {
                dest[3] = cur.p2.X - cur.unitAngle.Y * radius;
                dest[4] = cur.p2.Y + cur.unitAngle.X * radius;
                dest[15] = cur.p2.X + cur.unitAngle.Y * radius;
                dest[16] = cur.p2.Y - cur.unitAngle.X * radius;
            }
            else
            {
                Vector2 p0 = -p;
                Vector2 p1 = next.unitAngle * radius;
                float ratiox = p1.X / p0.X;
                float ratioy = p1.Y / p0.Y;

                dest[3] = cur.p2.X;
                dest[4] = cur.p2.Y;
                dest[15] = cur.p2.X;
                dest[16] = cur.p2.Y;

                if (!flip_end)
                {
                    if (p0.X < 0.00390625f)
                    {
                        dest[3] -= cur.unitAngle.Y * radius;
                    }
                    else
                    {
                        dest[3] -= (ratiox <= 0.99609375f)
                            ? (p1.Y + p0.Y * ratiox) / (1 - ratiox)
                            : (next.unitAngle.Y + cur.unitAngle.Y) * radius * 0.5f;
                    }

                    if (p0.Y < 0.00390625f)
                    {
                        dest[4] += cur.unitAngle.X * radius;
                    }
                    else
                    {
                        dest[4] += (ratioy <= 0.99609375f)
                            ? (p1.X + p0.X * ratioy) / (1 - ratioy)
                            : (next.unitAngle.X + cur.unitAngle.X) * radius * 0.5f;
                    }

                    dest[15] += cur.unitAngle.Y * radius;
                    dest[16] -= cur.unitAngle.X * radius;
                }
                else
                {
                    dest[3] -= cur.unitAngle.Y * radius;
                    dest[4] += cur.unitAngle.X * radius;

                    if (p0.X < 0.00390625f)
                    {
                        dest[15] += cur.unitAngle.Y * radius;
                    }
                    else
                    {
                        dest[15] += (ratiox <= 0.99609375f)
                            ? (p1.Y + p0.Y * ratiox) / (1 - ratiox)
                            : (next.unitAngle.Y + cur.unitAngle.Y) * radius * 0.5f;
                    }

                    if (p0.Y < 0.00390625f)
                    {
                        dest[16] -= cur.unitAngle.X * radius;
                    }
                    else
                    {
                        dest[16] -= (ratioy <= 0.99609375f)
                            ? (p1.X + p0.X * ratioy) / (1 - ratioy)
                            : (next.unitAngle.X + cur.unitAngle.X) * radius * 0.5f;
                    }
                }
            }
        }

        private void CalculateQuadMesh()
        {
#if !NO_PIN_SUPPORT

            coordinates_quad = new float[COLOUR_COUNT][];
            coordinates_quad_handle = new GCHandle[COLOUR_COUNT];
#endif
            coordinates_quad_pointer = new IntPtr[COLOUR_COUNT];


            for (int x = 0; x < COLOUR_COUNT; x++)
            {
                float y = (2.0f + 4.0f * x + sheetY + TEXEL_ORIGIN) / retinaHeight;
#if !NO_PIN_SUPPORT
                coordinates_quad[x] = new[]
                {
                    sheetStart, y,
                    sheetStart, y,
                    sheetEnd, y,
                    sheetEnd, y,
                    sheetStart, y,
                    sheetStart, y
                };


                coordinates_quad_handle[x] = GCHandle.Alloc(coordinates_quad[x], GCHandleType.Pinned);
                coordinates_quad_pointer[x] = coordinates_quad_handle[x].AddrOfPinnedObject();
#else
                IntPtr coordinates_quad_p = Marshal.AllocHGlobal(12 * sizeof(float));
                coordinates_quad_pointer[x] = coordinates_quad_p;
                unsafe
                {
                    float* coordinates_quad = (float*)coordinates_quad_p.ToPointer();
                    coordinates_quad[0] = sheetStart;   coordinates_quad[1] = y;
                    coordinates_quad[2] = sheetStart;   coordinates_quad[3] = y;
                    coordinates_quad[4] = sheetEnd;     coordinates_quad[5] = y;
                    coordinates_quad[6] = sheetEnd;     coordinates_quad[7] = y;
                    coordinates_quad[8] = sheetStart;   coordinates_quad[9] = y;
                    coordinates_quad[10] = sheetStart;   coordinates_quad[11] = y;
                }
#endif
            }
        }

        /// <summary>
        /// Draws a slider to the active device using a cached texture.
        /// </summary>
        /// <param name="lineList">List of lines to use</param>
        /// <param name="radius">Width of the slider</param>
        /// <param name="ColourIndex">Current combo colour index between 0 and 4; -1 for grey; -2 for Tag Multi override.</param>
        /// <param name="prev">The last line which was rendered in the previous iteration, or null if this is the first iteration.</param>
        /// <param name="next">The next line which will be rendered in the next iteration, or null if this is the last iteration.</param>
        internal void Draw(List<Line> lineList, float radius, int ColourIndex, Line prev, Line next)
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
                    DrawOGL(lineList, radius, prev, next, true, ColourIndex);
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

            Init();
        }

        #region IDisposable Members

        public void Dispose()
        {
#if !NO_PIN_SUPPORT
            if (vertices_cap_handle.IsAllocated)
            {
                for (int i = 0; i < COLOUR_COUNT; i++)
                {
                    coordinates_cap_handle[i].Free();
                    coordinates_quad_handle[i].Free();
                }

                vertices_cap_handle.Free();
            }
#else
            for (int i = 0; i < COLOUR_COUNT; i++)
            {
                Marshal.FreeHGlobal(coordinates_cap_pointer[i]);
                Marshal.FreeHGlobal(coordinates_quad_pointer[i]);
            }
            Marshal.FreeHGlobal(vertices_cap_pointer);
#endif

            GameBase.OnScreenLayoutChanged -= GameBase_OnScreenLayoutChanged;
        }

        private void GameBase_OnScreenLayoutChanged()
        {
            Initialize();
        }

        #endregion

        protected void glDrawQuad(int ColourIndex)
        {
            GL.TexCoordPointer(2, TexCoordPointerType.Float, 0, coordinates_quad_pointer[ColourIndex]);
            GL.VertexPointer(3, VertexPointerType.Float, 0, vertices_quad_pointer);

            GL.DrawArrays(BeginMode.TriangleStrip, 0, 6);
        }

        protected void glDrawHalfCircle(int count, int ColourIndex)
        {
            GL.TexCoordPointer(2, TexCoordPointerType.Float, 0, coordinates_cap_pointer[ColourIndex]);
            GL.VertexPointer(3, VertexPointerType.Float, 0, vertices_cap_pointer);


            GL.DrawArrays(BeginMode.TriangleFan, 0, count + 2);
        }

        /// <summary>
        /// Core drawing method in OpenGL
        /// </summary>
        /// <param name="lineList">List of lines to use</param>
        /// <param name="globalRadius">Width of the slider</param>
        /// <param name="texture">Texture used for the track</param>
        /// <param name="prev">The last line which was rendered in the previous iteration, or null if this is the first iteration.</param>
        protected void DrawOGL(List<Line> lineList, float globalRadius, Line prev, Line next, bool renderingToTexture, int ColourIndex)
        {
            if (renderingToTexture)
            {
                GL.Disable(EnableCap.Blend);
                //GL.DepthMask(true);
                //GL.DepthFunc(DepthFunction.Lequal);
                //GL.Enable(EnableCap.DepthTest);
            }

            SpriteManager.TexturesEnabled = true;

            GL.MatrixMode(MatrixMode.Modelview);

            trackTexture.TextureGl.Bind();

            int count = lineList.Count;
            bool flip;
            if (count == 0 || prev == null)
            {
                flip = false;
            }
            else
            {
                // this is the first segment this frame so we need to recover the previous flip value
                float theta = lineList[0].theta - prev.theta;

                // keep on the +- pi/2 range.
                if (theta > MathHelper.Pi) theta -= MathHelper.Pi * 2;
                if (theta < -MathHelper.Pi) theta += MathHelper.Pi * 2;
                flip = theta < 0;
            }

            for (int x = 1; x < count; x++)
            {
                DrawLineOGL(prev, lineList[x - 1], lineList[x], globalRadius, ColourIndex, ref flip);
                prev = lineList[x - 1];
            }

            if (count > 0)
                DrawLineOGL(prev, lineList[count - 1], next, globalRadius, ColourIndex, ref flip);

            if (renderingToTexture)
            {
                GL.Enable(EnableCap.Blend);
                //GL.Disable(EnableCap.DepthTest);
                //GL.DepthMask(false);
            }
        }

        protected void DrawLineOGL(Line prev, Line curr, Line next, float globalRadius, int ColourIndex, ref bool flip)
        {
            // find our orientation before anything else since we now need it for quad behaviour.
            int end_triangles;
            bool prev_flip = flip;
            if (next == null)
            {
                flip = false; // totally irrelevant
                end_triangles = numPrimitives_cap;
            }
            else
            {
                float theta = next.theta - curr.theta;

                // keep on the +- pi/2 range.
                if (theta > MathHelper.Pi) theta -= MathHelper.Pi * 2;
                if (theta < -MathHelper.Pi) theta += MathHelper.Pi * 2;

                if (theta < 0)
                {
                    flip = true;
                    end_triangles = (int)Math.Ceiling((-theta) * MAXRES / MathHelper.Pi + WEDGE_COUNT_FUDGE);
                }
                else if (theta > 0)
                {
                    flip = false;
                    end_triangles = (int)Math.Ceiling(theta * MAXRES / MathHelper.Pi + WEDGE_COUNT_FUDGE);
                }
                else
                {
                    flip = false; // totally irrelevant
                    end_triangles = 0;
                }
            }

            end_triangles = Math.Min(end_triangles, numPrimitives_cap);

            // Quad
            lock (vertices_lock)
            {
                unsafe
                {
                    CalculateQuadMesh((float*)vertices_quad_pointer, globalRadius, prev, curr, next, prev_flip, flip);
                }

                GL.LoadIdentity();
                glDrawQuad(ColourIndex);
            }

            // Cap on end
            Matrix4 matrix = curr.EndCapMatrix(globalRadius, flip);
            GL.LoadMatrix(ref matrix.Row0.X);

            glDrawHalfCircle(end_triangles, ColourIndex);

            // Cap on start
            bool hasStartCap = false;

            if (prev == null) hasStartCap = true;
            //else if (curr.p1 != prev.p2) hasStartCap = true; // this causes issues with missing zbuffers so remove for now

            //todo: this makes stuff look bad... need to look into it.
            if (hasStartCap)
            {
                // Catch for Darrinub and other slider inconsistencies. (Redpoints seem to be causing some.)
                // Render a complete beginning cap if this Line isn't connected to the end of the previous line.

                matrix = curr.CapMatrix(globalRadius);
                GL.LoadMatrix(ref matrix.Row0.X);

                glDrawHalfCircle(numPrimitives_cap, ColourIndex);
            }
        }
    }
}