using Android.Views;
using Microsoft.Maui.Controls.Compatibility.Platform.Android;
using Microsoft.Maui.Controls;
using System.Linq;
using Microsoft.Maui.Controls.Platform;

[assembly: ResolutionGroupName("MyApp")]
[assembly: ExportEffect(typeof(BrickBreaker.Platforms.Android.TouchEffectDroid), nameof(BrickBreaker.TouchEffect))]
namespace BrickBreaker.Platforms.Android
{
    public class TouchEffectDroid : PlatformEffect
    {
        private GestureDetector gestureRecognizer;
        private TouchListener touchListener;

        protected override void OnAttached()
        {
            touchListener = new TouchListener(Element as VisualElement, this);
            gestureRecognizer = new GestureDetector(touchListener);
            Control.Touch += (s, e) => gestureRecognizer.OnTouchEvent(e.Event);
        }

        protected override void OnDetached()
        {
            Control.Touch -= (s, e) => gestureRecognizer.OnTouchEvent(e.Event);
        }

        public void OnTouchAction(Element element, TouchActionEventArgs args)
        {
            var touchEffect = (TouchEffect)element.Effects.FirstOrDefault(e => e is TouchEffect);
            touchEffect?.OnTouchAction(element, args);
        }
    }

    // Реалізація слухача торкань для Android
    public class TouchListener : GestureDetector.SimpleOnGestureListener
    {
        private readonly VisualElement element;
        private readonly TouchEffectDroid touchEffect;

        public TouchListener(VisualElement element, TouchEffectDroid touchEffect)
        {
            this.element = element;
            this.touchEffect = touchEffect;
        }

        public override bool OnDown(MotionEvent e)
        {
            return true;
        }

        public override bool OnSingleTapUp(MotionEvent e)
        {
            var touchAction = new TouchActionEventArgs(e.EventTime, TouchActionType.Released, new Point(e.GetX(), e.GetY()), true);
            touchEffect.OnTouchAction(element, touchAction);
            return base.OnSingleTapUp(e);
        }

        public override bool OnScroll(MotionEvent e1, MotionEvent e2, float distanceX, float distanceY)
        {
            var touchAction = new TouchActionEventArgs(e2.EventTime, TouchActionType.Moved, new Point(e2.GetX(), e2.GetY()), true);
            touchEffect.OnTouchAction(element, touchAction);
            return base.OnScroll(e1, e2, distanceX, distanceY);
        }

        public override void OnLongPress(MotionEvent e)
        {
            var touchAction = new TouchActionEventArgs(e.EventTime, TouchActionType.Pressed, new Point(e.GetX(), e.GetY()), true);
            touchEffect.OnTouchAction(element, touchAction);
            base.OnLongPress(e);
        }
    }
}
