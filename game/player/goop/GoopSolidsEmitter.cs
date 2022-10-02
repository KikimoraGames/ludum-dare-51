using Godot;
using GodotOnReady.Attributes;
using System;

namespace Game
{
    public partial class GoopSolidsEmitter : Node
    {
        [Export]
        private float EmitEveryPowerLoss { get; set; } = 0.25f;

        [Export]
        private Godot.Collections.Array<PackedScene> goopSolidScenes;

        [OnReadyGet]
        private Node2D player;

        float previousPower;
        [OnReady]
        private void ConnectToPower()
        {
            PlayerPower.Instance.Connect(nameof(PlayerPower.on_power_changed), this, nameof(OnPowerChanged));
            previousPower = PlayerPower.Instance.CurrentPower;
        }

        private void OnPowerChanged(float p)
        {
            var diff = previousPower - p;
            if (diff < EmitEveryPowerLoss)
                return;
            previousPower = p;

            EmitSolid(1);
        }

        public void EmitSolid(int count)
        {
            for (int i = 0; i < count; i++)
            {
                var gs = goopSolidScenes[(int)(GD.Randi() % goopSolidScenes.Count)].Instance<RigidBody2D>();
                gs.GlobalPosition = player.GlobalPosition;
                gs.LinearVelocity = (Vector2.Left * 50f + Vector2.Up * 100f).Rotated((float)(GD.RandRange(-1, 1) * Mathf.Deg2Rad(15)));
                AddChild(gs);
            }
        }
    }
}