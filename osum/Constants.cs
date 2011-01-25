#if IPHONE
using OpenTK.Graphics.ES11;
#else
using OpenTK.Graphics.OpenGL;
#endif


namespace osum
{
    public static class Constants
    {
        public const double SIXTY_FRAME_TIME = (double)1000 / 60;

#if IPHONE
        public const int COLOR_BUFFER_BIT = (int)All.ColorBufferBit;
        public const int COLOR_DEPTH_BUFFER_BIT = (int)(All.ColorBufferBit | All.DepthBufferBit);
        
#else
        public const ClearBufferMask COLOR_BUFFER_BIT = ClearBufferMask.ColorBufferBit;
        public const ClearBufferMask COLOR_DEPTH_BUFFER_BIT = ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit;
#endif
    }
}
