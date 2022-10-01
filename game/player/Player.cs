using Godot;
using GodotOnReady.Attributes;
using System;

namespace Game
{
    public partial class Player : KinematicBody2D
    {
        private static string ACTION_JUMP = "action_jump";

        [Export]
        public float MovementSpeedPixelsPerSecond { get; private set; } = 150;
        [Export]
        public float JumpSpeedPixelsPerSecond { get; private set; } = 500f;
        [Export]
        public Curve JumpSpeedHoldTimeModifierCurve { get; private set; }
        [Export]
        public float MaxJumpHeldDownTimeSeconds { get; private set; } = 0.25f;
        [Export]
        public float FallingSpeedPixelsPerSecond { get; private set; } = 200f;
        [Export]
        public float CoyoteTimeSeconds { get; private set; } = 0.2f;

        private bool IsJumping { get; set; } = false;
        private bool IsInAir { get; set; } = false;
        private ulong jumpHeldSinceMsec;
        private ulong isInAirSinceMsec;

        private float CoyoteTime => ((Time.GetTicksMsec() - isInAirSinceMsec) / 1000f) / CoyoteTimeSeconds;
        public override void _UnhandledInput(InputEvent e)
        {
            base._UnhandledInput(e);
            if (e.IsEcho())
                return;

            if (e.IsActionPressed(ACTION_JUMP))
            {
                if (!IsInAir || CoyoteTime <= 1.0f)
                {
                    GetTree().SetInputAsHandled();
                    IsJumping = true;
                    jumpHeldSinceMsec = Time.GetTicksMsec();
                }
            }

            if (e.IsActionReleased(ACTION_JUMP))
            {
                GetTree().SetInputAsHandled();
                IsJumping = false;
            }
        }

        public override void _PhysicsProcess(float delta)
        {
            base._PhysicsProcess(delta);
            var horizontalVelocity = new Vector2(Input.GetAxis("move_left", "move_right"), 0);
            var verticalVelocity = Vector2.Up;
            if (IsJumping)
            {
                var jumpHeldFor = ((Time.GetTicksMsec() - jumpHeldSinceMsec) / 1000f) / MaxJumpHeldDownTimeSeconds;
                if (jumpHeldFor <= 1.0f)
                    verticalVelocity *= JumpSpeedPixelsPerSecond * JumpSpeedHoldTimeModifierCurve.Interpolate(jumpHeldFor);
                else
                    IsJumping = false;
            }
            else
                verticalVelocity *= -FallingSpeedPixelsPerSecond;

            horizontalVelocity *= MovementSpeedPixelsPerSecond;
            MoveAndSlide(horizontalVelocity + verticalVelocity, Vector2.Up, true);
            if (!IsOnFloor() && !IsInAir)
            {
                IsInAir = true;
                if (!IsJumping)
                {
                    isInAirSinceMsec = Time.GetTicksMsec();
                }
            }

            if (IsOnFloor())
            {
                IsInAir = false;
            }

            GD.Print(CoyoteTime);
        }
    }
}