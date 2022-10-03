using Godot;
using GodotOnReady.Attributes;
using System;

namespace Game
{
    public partial class IntroLevel : Level
    {
        [OnReadyGet]
        private Node billboardCamera;

        [OnReadyGet]
        private Control title;

        [OnReadyGet]
        private Control awaken;

        protected override void OnLevelReady()
        {
            base.OnLevelReady();
            InputProcessor.Instance.ForceSleep();
        }
        public override void Begin()
        {
            InputProcessor.Instance.IgnoreInput = false;
        }

        public override void _Input(InputEvent e)
        {
            base._Input(e);
            if (InputProcessor.Instance.IgnoreInput)
                return;

            if (e.IsActionPressed("action_sleep"))
                awaken.Visible = false;
        }

        protected async override void OnHumanDestroyed()
        {
            billboardCamera.Set("priority", 12);
            PlayerPower.Instance.LevelOverride = 0f;
            await this.WaitSeconds(5f);
            title.Visible = true;
            var tween = CreateTween();
            tween.TweenProperty(title, "rect_scale", Vector2.One, 1f).From(Vector2.Zero).SetTrans(Tween.TransitionType.Back).SetEase(Tween.EaseType.Out);
            await ToSignal(tween, "finished");
            await this.WaitSeconds(3f);
            base.OnHumanDestroyed();

        }

    }
}