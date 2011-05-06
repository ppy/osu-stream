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

        private void updateTrackingPosition(Point position)
        {
            if (trackingPoints.Count == 0)
                trackingPoints.Add(new TrackingPoint(position));
            else
                trackingPoints[0].Location = position;
        }

        void mouse_Move(object sender, MouseMoveEventArgs e)
        {
            updateTrackingPosition(e.Position);

            //because mouse movement is continuous, we want to track a new hover object every position change.
            trackingPoints[0].HoveringObject = null;

            TriggerOnMove(trackingPoints[0]);
        }

        List<MouseButton> pressedButtons = new List<MouseButton>();

        void mouse_ButtonUp(object sender, MouseButtonEventArgs e)
        {
            //because mouse movement is continuous, we want to track a new hover object every position change.
            trackingPoints[0].HoveringObject = null;

            pressedButtons.Remove(e.Button);

            TriggerOnUp(trackingPoints[0]);
        }

        void mouse_ButtonDown(object sender, MouseButtonEventArgs e)
        {
            pressedButtons.Add(e.Button);

            TriggerOnDown(trackingPoints[0]);
        }
    }
}
