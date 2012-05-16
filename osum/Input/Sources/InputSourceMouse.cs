using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Input;
using System.Drawing;

namespace osum.Input.Sources
{
    class InputSourceMouse : InputSource
    {
        MouseDevice mouse;

        public InputSourceMouse(MouseDevice mouse) : base()
        {
            this.mouse = mouse;


            mouse.ButtonDown += mouse_ButtonDown;
            mouse.ButtonUp += new EventHandler<MouseButtonEventArgs>(mouse_ButtonUp);
            mouse.Move += new EventHandler<MouseMoveEventArgs>(mouse_Move);
        }

        void mouse_Move(object sender, MouseMoveEventArgs e)
        {
            if (trackingPoints.Count > 0)
            {
                trackingPoints[0].Location = e.Position;
                TriggerOnMove(trackingPoints[0]);
            }
        }

        List<MouseButton> pressedButtons = new List<MouseButton>();

        void mouse_ButtonUp(object sender, MouseButtonEventArgs e)
        {
            pressedButtons.Remove(e.Button);

            if (trackingPoints.Count > 0)
                TriggerOnUp(trackingPoints[0]);

            if (pressedButtons.Count == 0)
                trackingPoints.Clear();
        }

        void mouse_ButtonDown(object sender, MouseButtonEventArgs e)
        {
            pressedButtons.Add(e.Button);
            TriggerOnDown(new TrackingPoint(e.Position));
        }
    }
}
