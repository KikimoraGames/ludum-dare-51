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
        public float JumpHorizontalSpeedPixelsPerSecond { get; private set; } = 50;
        [Export]
        public float MaxJumpHeldDownTimeSeconds { get; private set; } = 0.25f;
        [Export]
        public float FallingSpeedPixelsPerSecond { get; private set; } = 200f;
        [Export]
        public float CoyoteTimeSeconds { get; private set; } = 0.2f;

        public bool IsJumping { get; private set; } = false;
        public bool IsInAir { get; private set; } = false;
        private ulong jumpHeldSinceMsec;
        private ulong isInAirSinceMsec;

        public float CoyoteTime => ((Time.GetTicksMsec() - isInAirSinceMsec) / 1000f) / CoyoteTimeSeconds;

        public bool IsSleeping { get; private set; } = false;

        public void JumpPressed()
        {
            IsJumping = true;
            jumpHeldSinceMsec = Time.GetTicksMsec();
        }

        public void JumpReleased()
        {
            IsJumping = false;
        }

        public override void _PhysicsProcess(float delta)
        {
            base._PhysicsProcess(delta);
            var horizontalVelocity = InputProcessor.Instance.InputVelocity;
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

            if (!IsInAir)
                horizontalVelocity *= MovementSpeedPixelsPerSecond;
            else
                horizontalVelocity *= JumpHorizontalSpeedPixelsPerSecond;

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
        }

        public void SleepReleased()
        {
            IsSleeping = false;
        }

        public void SleepPressed()
        {
            IsSleeping = true;
        }
    }
}