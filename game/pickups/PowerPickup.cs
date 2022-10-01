using Godot;
using GodotOnReady.Attributes;
using System;

namespace Game
{
    public partial class PowerPickup : Node2D
    {
        [Export]
        private float power = 5;

        [OnReadyGet("Area2D")]
        private Area2D area;

        [OnReady]
        private void SetupConnections()
        {
            area.Connect("body_entered", this, nameof(OnBodyEntered));
        }

        private void OnBodyEntered(PhysicsBody2D body)
        {
            if (!(body is Player))
                return;

            QueueFree();
            PlayerPower.Instance.Add(power);
        }
    }
}