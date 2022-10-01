using Godot;
using GodotOnReady.Attributes;
using System;

namespace Game
{
    public partial class Player : KinematicBody2D
    {
        private static string ACTION_JUMP = "action_jump";

        [Export]
        public float MovementSpeedPixelsPerSecond { get; private set; } = 250;
        [Export]
        public float JumpSpeedPixelsPerSecond { get; private set; } = 500f;
        [Export]
        public Curve JumpSpeedHoldTimeModifierCurve { get; private set; }
        [Export]
        public float JumpHorizontalSpeedPixelsPerSecond { get; private set; } = 200;
        [Export]
        public float MaxJumpHeldDownTimeSeconds { get; private set; } = 0.25f;
        [Export]
        public float MinFallingSpeedPixelsPerSecond { get; private set; } = 150f;
        [Export]
        public float MaxFallingSpeedPixelsPerSecond { get; private set; } = 250f;
        [Export]
        public float MaxFallingSpeedReachTimeSeconds { get; private set; } = 0.1f;
        [Export]
        public Curve FallSpeedEaseCurve { get; private set; }
        [Export]
        public float DashDistance { get; private set; } = 200f;
        [Export]
        public float DashSpeedPixelsPerSecond { get; private set; } = 1000f;

        [Export]
        public float CoyoteTimeSeconds { get; private set; } = 0.2f;

        public bool IsJumping { get; private set; } = false;
        public bool IsInAir { get; private set; } = false;
        public bool HasDash { get; private set; } = true;
        public bool IsDashing { get; private set; } = false;
        public Vector2 DashDirection { get; private set; } = Vector2.Zero;

        private ulong jumpHeldSinceMsec;
        private ulong isInAirSinceMsec;

        public float CoyoteTime => ((Time.GetTicksMsec() - isInAirSinceMsec) / 1000f) / CoyoteTimeSeconds;
        private float timeSpentFalling = 0f;

        private float dashDistanceCovered = 0f;

        public bool IsSleeping { get; private set; } = false;

        public void JumpPressed()
        {
            IsJumping = true;
            jumpHeldSinceMsec = Time.GetTicksMsec();
        }

        public void JumpReleased()
        {
            IsJumping = false;
            timeSpentFalling = 0f;
        }

        // TODO: Dash hang time, 4 directional dash
        public void DashPressed()
        {
            HasDash = false;
            IsJumping = false;
            IsDashing = true;
            var inputVelocity = InputProcessor.Instance.InputVelocity;
            DashDirection = (inputVelocity.LengthSquared() > 0.1f ? inputVelocity : InputProcessor.Instance.LastNonZeroDirection).Normalized();
            dashDistanceCovered = 0f;
        }

        private void DashDone()
        {
            IsDashing = false;
            timeSpentFalling = 0f;
        }

        public override void _PhysicsProcess(float delta)
        {
            base._PhysicsProcess(delta);
            var inputVelocity = InputProcessor.Instance.InputVelocity;
            var horizontalVelocity = Vector2.Right * inputVelocity;
            var verticalVelocity = Vector2.Up;
            if (IsDashing)
            {
                var oldPosition = GlobalPosition;
                var dashVelocity = DashDirection * DashSpeedPixelsPerSecond;

                var collision = MoveAndCollide(dashVelocity * delta, testOnly: true);
                if (collision != null)
                {
                    DashDone();
                    return;
                }
                MoveAndSlide(dashVelocity, Vector2.Up, false);
                var newPosition = GlobalPosition;
                dashDistanceCovered += oldPosition.DistanceTo(newPosition);
                if (dashDistanceCovered > DashDistance)
                    DashDone();
                return;
            }

            if (IsJumping)
            {
                var jumpHeldFor = ((Time.GetTicksMsec() - jumpHeldSinceMsec) / 1000f) / MaxJumpHeldDownTimeSeconds;
                if (jumpHeldFor <= 1.0f)
                    verticalVelocity *= JumpSpeedPixelsPerSecond * JumpSpeedHoldTimeModifierCurve.Interpolate(jumpHeldFor);
                else
                {
                    IsJumping = false;
                    timeSpentFalling = 0f;
                }
            }
            else
            {
                timeSpentFalling += delta;
                var fallSpeed = MinFallingSpeedPixelsPerSecond + MaxFallingSpeedPixelsPerSecond * FallSpeedEaseCurve.Interpolate(timeSpentFalling / MaxFallingSpeedReachTimeSeconds);
                verticalVelocity *= -fallSpeed;
            }

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
                timeSpentFalling = 0f;
                IsInAir = false;
                HasDash = true;
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