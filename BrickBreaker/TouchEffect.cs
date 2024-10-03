using System;
using Microsoft.Maui.Controls;

namespace BrickBreaker
{
    public class TouchEffect : RoutingEffect
    {
        public event TouchActionEventHandler TouchAction;

        public TouchEffect() : base("MyApp.TouchEffect") { }

        public bool Capture { set; get; }

        public void OnTouchAction(Element element, TouchActionEventArgs args)
        {
            TouchAction?.Invoke(element, args);
        }
    }

    public delegate void TouchActionEventHandler(object sender, TouchActionEventArgs args);

    public class TouchActionEventArgs : EventArgs
    {
        public long Id { get; private set; }
        public TouchActionType Type { get; private set; }
        public Point Location { get; private set; }
        public bool IsInContact { get; private set; }

        public TouchActionEventArgs(long id, TouchActionType type, Point location, bool isInContact)
        {
            Id = id;
            Type = type;
            Location = location;
            IsInContact = isInContact;
        }
    }

    public enum TouchActionType
    {
        Entered,
        Pressed,
        Moved,
        Released,
        Exited,
        Cancelled
    }
}
