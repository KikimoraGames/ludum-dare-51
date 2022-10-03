using Godot;
using Godot.Collections;
using GodotOnReady.Attributes;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Game
{
    public partial class CameraStack : Node2D
    {
        [Export]
        public bool listenToZoomRequests = false;

        protected readonly List<Node> stackCameraModifiers = new List<Node>();
        public Node VirtualCamera { get; private set; }

        [OnReady]
        private void GatherReferences()
        {
            if (GetChildCount() > 2)
                GD.PrintErr("CombatCameraStack has too many children, check your scene setup");

            foreach (var c in new Array<Node>(GetChildren()))
            {
                if (c.Owner == this)
                {
                    TravelStack(c);
                    continue;
                }

                if (c.IsInGroup("vcamera"))
                {
                    VirtualCamera = c;
                }
            }
        }

        [OnReady]
        private void SetCameraInStack()
        {
            VirtualCamera.GetParent().RemoveChild(VirtualCamera);
            stackCameraModifiers[stackCameraModifiers.Count - 1].AddChild(VirtualCamera);
        }

        [OnReady]
        private void ConnectImpulseListener()
        {
            foreach (var n in stackCameraModifiers)
            {
                if (n.HasMethod("on_camera_impulse"))
                    Events.Connect("camera_impulse_sent", n, "on_camera_impulse");
            }
        }

        [OnReady]
        private void ConnectZoomListener()
        {
            if (!listenToZoomRequests)
                return;
            Events.Connect(nameof(Events.camera_zoom_request), this, nameof(ZoomCamera));
        }

        private void TravelStack(Node parent)
        {
            if (parent.IsInGroup("vcamera"))
            {
                VirtualCamera = parent;
                return;
            }

            stackCameraModifiers.Add(parent);
            if (parent.GetChildCount() > 0)
                TravelStack(parent.GetChild(0));
        }

        public int Priority
        {
            get => VirtualCamera.Get<int>("priority");
            set => VirtualCamera.Set("priority", value);
        }

        public void SetTarget(Node2D target)
        {
            foreach (var sm in stackCameraModifiers)
                sm.Set("target", target);
        }

        private async void ZoomCamera(float zoom, float duration, Tween.TransitionType transType, Tween.EaseType easeType)
        {
            await ToZoom(zoom, duration, transType, easeType);
            ZoomResetAsync(0.1f);
        }

        public async void ToZoomAsync(float zoom, float duration, Tween.TransitionType transType = Tween.TransitionType.Linear, Tween.EaseType easeType = Tween.EaseType.InOut)
        => await ToZoom(zoom, duration, transType, easeType);

        public async void ZoomResetAsync(float duration, Tween.TransitionType transType = Tween.TransitionType.Linear, Tween.EaseType easeType = Tween.EaseType.InOut)
           => await ToZoom(1f, duration, transType, easeType);

        internal async Task ToZoom(float zoom, float duration, Tween.TransitionType transType, Tween.EaseType easeType)
        {
            var tween = CreateTween();
            tween.TweenProperty(VirtualCamera, "zoom", zoom * Vector2.One, duration).SetTrans(transType).SetEase(easeType);
            await ToSignal(tween, "finished");
        }
    }
}