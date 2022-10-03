using Godot;
using GodotOnReady.Attributes;
using System;

namespace Game
{
    public partial class OutroLevel : Level
    {
        public override void Begin()
        {
            InputProcessor.Instance.IgnoreInput = false;
        }

        private void _on_Area2D_body_entered(Node _)
        {
            PlayerPower.Instance.LevelOverride = 1f;
        }

        protected override void OnPowerChanged(float p)
        {
            if (p > 0f)
                return;

            InputProcessor.Instance.IgnoreInput = true;
            EmitSignal(nameof(level_completed));
        }
    }
}