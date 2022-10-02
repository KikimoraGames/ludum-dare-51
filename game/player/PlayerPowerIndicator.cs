using Godot;
using GodotOnReady.Attributes;
using System;

namespace Game
{
    public partial class PlayerPowerIndicator : Node
    {
        [OnReadyGet]
        private Sprite meatbagSprite;
        [Export]
        private float minYScale = 0.5f;
        [Export]
        private float maxYScale = 1.5f;

        private float previousPower;
        private bool isTweening;

        [OnReady]
        private void ConnectToPower()
        {
            PlayerPower.Instance.Connect(nameof(PlayerPower.on_power_changed), this, nameof(OnPowerChanged));
            OnPowerChanged(PlayerPower.Instance.CurrentPower);
        }

        private void OnPowerChanged(float p)
        {
            if (isTweening)
                return;

            var scale = new Vector2(1.0f, 1.0f);
            scale.y = Mathf.Lerp(minYScale, maxYScale, p / PlayerPower.MAXIMUM_POWER);
            if (previousPower < p)
            {
                isTweening = true;
                var tween = CreateTween();
                tween.TweenProperty(meatbagSprite, "scale", new Vector2(1.33f, maxYScale * 1.33f), 0.25f).SetTrans(Tween.TransitionType.Bounce).SetEase(Tween.EaseType.Out);
                tween.TweenProperty(meatbagSprite, "scale", scale, 0.15f).SetTrans(Tween.TransitionType.Linear).SetEase(Tween.EaseType.Out);
                tween.TweenCallback(this, nameof(TweenFinished));
            }
            else
                meatbagSprite.Scale = scale;

            previousPower = p;
        }

        private void TweenFinished() => isTweening = false;
    }
}