//  GLES.cs
//  Author: Dean Herbert <pe@ppy.sh>
//  Copyright (c) 2010 2010 Dean Herbert
using System;
namespace osum.Graphics
{
    public static class GLES
    {
        [System.Security.SuppressUnmanagedCodeSecurity()]
        internal unsafe delegate void GenFramebuffersOES(Int32 n, UInt32* framebuffers);

        public static unsafe void GenFramebuffers(Int32 n, Int32* framebuffers)
        {
            //GenFramebuffersOES((Int32)n, (UInt32*)framebuffers);
        }
    }
}

