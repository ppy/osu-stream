using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
using osu_common.Helpers;
using OpenTK;
using osum.Helpers;
#else
using OpenTK.Input;
using OpenTK.Graphics.OpenGL;
using osum.Input;
using osum.Helpers;
using osu_common.Helpers;
using OpenTK;
#endif

namespace osum.Graphics.Sprites
{
    internal partial class pDrawable : IDrawable, IDisposable
    {
        internal bool IsClickable { get { return onClick != null; } }

        internal bool HandleClickOnUp;

        private bool handleInput;
        internal bool HandleInput
        {
            get { return handleInput; }
            set
            {
                handleInput = value;

                if (inputIsHovering && handleInput)
                //might have a pending unhover state animation to apply.
                {
                    inputIsHovering = false;
                    if (onHoverLost != null)
                        onHoverLost(this, null);
                }
            }
        }

        bool inputEventsBound;

        private event EventHandler onClick;
        internal event EventHandler OnClick
        {
            add { onClick += value; updateInputBindings(); HandleInput = true; }
            remove { onClick -= value; updateInputBindings(); }
        }

        private event EventHandler onHover;
        internal event EventHandler OnHover
        {
            add { onHover += value; updateInputBindings(); HandleInput = true; }
            remove { onHover -= value; updateInputBindings(); }
        }

        private event EventHandler onHoverLost;
        internal event EventHandler OnHoverLost
        {
            add { onHoverLost += value; }
            remove { onHoverLost -= value; }
        }

        internal void UnbindAllEvents()
        {
            onClick = null;
            onHover = null;
            onHoverLost = null;

            updateInputBindings();
        }

        private void updateInputBindings()
        {
            bool needEventsBound = onClick != null || onHover != null;

            if (needEventsBound == inputEventsBound) return;

            inputEventsBound = needEventsBound;

            if (needEventsBound)
            {
                InputManager.OnDown += InputManager_OnDown;
                InputManager.OnMove += InputManager_OnMove;
                InputManager.OnUp += InputManager_OnUp;
            }
            else
            {
                InputManager.OnDown -= InputManager_OnDown;
                InputManager.OnMove -= InputManager_OnMove;
                InputManager.OnUp -= InputManager_OnUp;
            }
        }

        bool inputIsHovering;

        bool inputCheckHover(Vector2 position)
        {
            if (Alpha == 0)
                return false;

            Box2 rect = DisplayRectangle;

            return rect.Left < position.X &&
                rect.Right >= position.X &&
                rect.Top < position.Y &&
                rect.Bottom >= position.Y;
        }

        void inputUpdateHoverState(TrackingPoint trackingPoint)
        {
            if (!HandleInput)
            {
                if (trackingPoint.HoveringObject == this)
                    trackingPoint.HoveringObject = null;
                return;
            }

            bool isNowHovering =
                (trackingPoint.HoveringObject == null || trackingPoint.HoveringObject == this) &&
                inputCheckHover(trackingPoint.WindowPosition);

            if (inputIsHovering)
                trackingPoint.HoveringObject = this;
            else if (trackingPoint.HoveringObject == this)
                trackingPoint.HoveringObject = null;

            if (isNowHovering != inputIsHovering)
            {
                inputIsHovering = isNowHovering;

                if (inputIsHovering)
                {
                    if (onHover != null)
                        onHover(this, null);
                }
                else
                {
                    if (onHoverLost != null)
                        onHoverLost(this, null);
                }
            }
        }

        float acceptableUpClick;

        void InputManager_OnMove(InputSource source, TrackingPoint trackingPoint)
        {
            inputUpdateHoverState(trackingPoint);

            if (acceptableUpClick > 0)
                acceptableUpClick -= Math.Abs(trackingPoint.WindowDelta.X) + Math.Abs(trackingPoint.WindowDelta.Y);
        }

        const float HANDLE_UP_MOVEMENT_ALLOWANCE = 5;

        void InputManager_OnDown(InputSource source, TrackingPoint trackingPoint)
        {
            if (!HandleInput) return;

            inputUpdateHoverState(trackingPoint);
            if (inputIsHovering)
            {
                if (!HandleClickOnUp)
                    Click();
                else
                    acceptableUpClick = HANDLE_UP_MOVEMENT_ALLOWANCE;
            }
            else
            {
                acceptableUpClick = 0;
            }
        }

        void InputManager_OnUp(InputSource source, TrackingPoint trackingPoint)
        {
            if (!HandleInput) return;

            if (inputIsHovering && acceptableUpClick > 0)
                Click();

            if (inputIsHovering && HandleInput)
            //check HandleInput again here so we can cancel the unhover for the time being.
            {
                inputIsHovering = false;
                if (onHoverLost != null)
                    onHoverLost(this, null);
            }
        }

        internal void Click()
        {
            if (onClick != null)
                onClick(this, null);
        }
    }
}
