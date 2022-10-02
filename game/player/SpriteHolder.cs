using Godot;
using System;

namespace Game
{
    public class SpriteHolder : Node2D
    {
        [Export]
        private float flipSeconds = 0.15f;

        public void OnDirectionChange(float dir)
        {
            if (Mathf.IsZeroApprox(dir))
                return;
            if (Mathf.Sign(Scale.x) == dir)
                return;

            var scale = new Vector2(dir, 1f);
            var tween = CreateTween();
            tween.TweenProperty(this, "scale", scale, flipSeconds).SetTrans(Tween.TransitionType.Expo).SetEase(Tween.EaseType.Out);
        }
    }
}