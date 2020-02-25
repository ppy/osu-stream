using System.Collections.Generic;
using OpenTK.Input;

namespace osum.Input.Sources
{
    internal class InputSourceMouse : InputSource
    {
        private MouseDevice mouse;

        public InputSourceMouse(MouseDevice mouse)
        {
            this.mouse = mouse;


            mouse.ButtonDown += mouse_ButtonDown;
            mouse.ButtonUp += mouse_ButtonUp;
            mouse.Move += mouse_Move;
        }

        private void mouse_Move(object sender, MouseMoveEventArgs e)
        {
            if (trackingPoints.Count > 0)
            {
                trackingPoints[0].Location = e.Position;
                TriggerOnMove(trackingPoints[0]);
            }
        }

        private readonly List<MouseButton> pressedButtons = new List<MouseButton>();

        private void mouse_ButtonUp(object sender, MouseButtonEventArgs e)
        {
            pressedButtons.Remove(e.Button);

            if (trackingPoints.Count > 0)
                TriggerOnUp(trackingPoints[0]);

            if (pressedButtons.Count == 0)
                trackingPoints.Clear();
        }

        private void mouse_ButtonDown(object sender, MouseButtonEventArgs e)
        {
            pressedButtons.Add(e.Button);
            TriggerOnDown(new TrackingPoint(e.Position));
        }
    }
}