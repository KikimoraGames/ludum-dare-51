using Godot;
using System;

namespace Game
{
    public class SpriteHolder : Node2D
    {

        private void OnDirectionChange(float dir)
        {
            var scale = Scale;
            if (Mathf.Sign(scale.x) != dir)
                scale.x = dir;

            var tween = CreateTween();
            tween.TweenProperty(this, "scale", scale, 0.15f).SetTrans(Tween.TransitionType.Expo).SetEase(Tween.EaseType.Out);
        }
    }
}