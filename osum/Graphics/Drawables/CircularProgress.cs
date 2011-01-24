using System;
using osum.Graphics.Sprites;
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
using osum.Graphics.Drawables;
using osum.Helpers;
#else
using OpenTK.Input;
using OpenTK.Graphics.OpenGL;
using System.Drawing;
using osum.Input;
#endif


namespace osum.Graphics.Drawables
{
	internal class CircularProgress : pDrawable
	{
		public CircularProgress()
		{
			AlwaysDraw = true;
		}
		
		public override void Dispose()
		{
		}
		
		public override void Draw()
		{
			base.Draw();
			
			float resolution = 0.1f;
            float startAngle = (float) (-Math.PI/2);
            
            float endAngle = (float)((((float)Clock.Time / 1000) % 1) * (2 * Math.PI) + startAngle);

            int parts = (int)((endAngle - startAngle) / resolution);
			
			float da = (endAngle - startAngle) / parts;
            
            float[] vertices = new float[parts * 2 + 2];
            float[] colours = new float[parts * 4 + 4];

            float radius = 1000 * ((Clock.Time / 1000f) % 1);

            float xsc = 512;
            float ysc = 384;

            vertices[0] = xsc;
            vertices[1] = ysc;
			
			colours[0] = 1;
			colours[1] = 1;
			colours[2] = 1;
			colours[3] = 0.5f;

            float a = startAngle;
            for (int v = 1; v < parts + 1; v++)
            {
                vertices[v * 2] = (float)(xsc + Math.Cos(a)*radius);
                vertices[v * 2 + 1] = (float)(ysc + Math.Sin(a)*radius);
                a += da;

                colours[v * 4] = 1;
                colours[v * 4 + 1] = 1;
                colours[v * 4 + 2] = 1;
                colours[v * 4 + 3] = 0.5f;
            }

            GL.EnableClientState(EnableCap.ColorArray);
            //GL.EnableClientState(EnableCap.VertexArray);

		    //GL.VertexPointer(2,VertexPointerType.Float, 0, vertices);
            GL.ColorPointer(4, ColorPointerType.Float, 0,colours);
            GL.DrawArrays(BeginMode.TriangleFan, 0, parts + 1);
            
            GL.DisableClientState(EnableCap.ColorArray);
            //GL.DisableClientState(EnableCap.VertexArray);
		}
	}
}

