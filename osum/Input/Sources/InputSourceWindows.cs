using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using OpenTK.Input;

namespace osum.Input
{
    internal static class InputManager
    {
        private static MouseDevice mouseDevice;

        internal static Vector2 MousePosition
        {
            get { return new Vector2(mouseDevice.X, mouseDevice.Y); }
        }

        internal static event EventHandler<MouseButtonEventArgs> MouseDown;
        internal static event EventHandler<MouseButtonEventArgs> MouseUp;
        internal static event EventHandler<MouseButtonEventArgs> MouseClick;

        internal static void Initialise(MouseDevice mouse)
        {
            mouseDevice = mouse;
            mouseDevice.ButtonDown += HandleMouseDown;
            mouseDevice.ButtonUp += HandleMouseUp;
        }

        private static MouseButton buttonState;

        private static void HandleMouseDown(object sender, MouseButtonEventArgs e)
        {
            buttonState |= e.Button;
            if (MouseDown != null)
                MouseDown(sender, e);
        }

        private static void HandleMouseUp(object sender, MouseButtonEventArgs e)
        {
            if ((buttonState & e.Button) > 0)
            {
                if (MouseClick != null)
                    MouseClick(sender, e);
            }

            buttonState &= ~e.Button;

            if (MouseUp != null)
                MouseUp(sender, e);
        }
    }
}
