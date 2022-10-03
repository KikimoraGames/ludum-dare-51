using Godot;
using System;

namespace Game
{
    public class AutomaticPivotOffsetTextureRect : TextureRect
    {
        public override void _Ready()
        {
            Connect("resized", this, nameof(OnResized));
            OnResized();
        }

        private void OnResized()
        {
            RectPivotOffset = RectSize / 2;
        }
    }
}