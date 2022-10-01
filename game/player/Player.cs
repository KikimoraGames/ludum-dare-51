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
        public float JumpHangTimeSeconds { get; private set; } = 0.1f;
        [Export]
        public Curve JumpSpeedHoldTimeModifierCurve { get; private set; }
        [Export]
        public Curve JumpMomentumFalloffCurve { get; private set; }
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
        public Curve DashDistanceSpeedEaseCurve { get; private set; }
        [Export]
        public float DashSpeedPixelsPerSecond { get; private set; } = 1000f;
        [Export]
        public float DashHangTimeSeconds { get; private set; } = 0.15f;
        [Export]
        public float CoyoteTimeSeconds { get; private set; } = 0.2f;
        [Export]
        public float DashPowerCost { get; private set; } = 1f;

        public bool IsJumpButtonHeld { get; private set; } = false;
        public bool IsJumping { get; private set; } = false;
        public bool IsInAir { get; private set; } = false;
        public bool HasDash { get; private set; } = true;
        public bool HasPowerForDash => PlayerPower.Instance.CurrentPower >= DashPowerCost;
        public bool IsDashing { get; private set; } = false;
        public Vector2 DashDirection { get; private set; } = Vector2.Zero;

        private ulong jumpHeldSinceMsec;
        private float jumpDistanceCovered = 0f;
        private float jumpHangTimeCurrent = 0f;
        private float jumpStartHorizontalMomentumVelocity;
        private float jumpCurrentHorizontalMomentumVelocity;

        private ulong isInAirSinceMsec;

        public float CoyoteTime => ((Time.GetTicksMsec() - isInAirSinceMsec) / 1000f) / CoyoteTimeSeconds;
        private float timeSpentFalling = 0f;

        private float dashDistanceCovered = 0f;
        private float dashHangTimeCurrent = 0f;

        public bool IsSleeping { get; private set; } = false;
        private Vector2 lastFrameVelocity;

        public void JumpPressed()
        {
            IsJumping = true;
            IsJumpButtonHeld = true;
            jumpHangTimeCurrent = 0f;
            jumpHeldSinceMsec = Time.GetTicksMsec();
            jumpStartHorizontalMomentumVelocity = lastFrameVelocity.x;
            jumpCurrentHorizontalMomentumVelocity = jumpStartHorizontalMomentumVelocity;
        }

        public void JumpReleased()
        {
            IsJumpButtonHeld = false;
            timeSpentFalling = 0f;
        }

        public void DashPressed()
        {
            HasDash = false;
            IsJumping = false;
            IsDashing = true;
            dashDistanceCovered = 0f;
            dashHangTimeCurrent = 0f;
            PlayerPower.Instance.PowerDrainModifier = 0.0f;

            var inputVelocity = InputProcessor.Instance.InputVelocity;
            var dashDirection = (inputVelocity.LengthSquared() > 0.1f ? inputVelocity : InputProcessor.Instance.LastNonZeroDirection).Normalized();
            dashDirection = Mathf.Abs(dashDirection.x) > Mathf.Abs(dashDirection.y) ? new Vector2(dashDirection.x, 0) : new Vector2(0, dashDirection.y);
            DashDirection = dashDirection.Normalized();
        }

        private void DashDone()
        {
            PlayerPower.Instance.PowerDrainModifier = 1.0f;
            PlayerPower.Instance.Add(-DashPowerCost);
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
                HandleDashing(delta);
                return;
            }

            if (IsJumping)
                verticalVelocity = HandleJumping(verticalVelocity, delta);
            else
            {
                timeSpentFalling += delta;
                var fallSpeed = MinFallingSpeedPixelsPerSecond + MaxFallingSpeedPixelsPerSecond * FallSpeedEaseCurve.Interpolate(timeSpentFalling / MaxFallingSpeedReachTimeSeconds);
                verticalVelocity *= -fallSpeed;
            }


            horizontalVelocity *= MovementSpeedPixelsPerSecond;
            if (IsInAir)
                horizontalVelocity += Vector2.Right * jumpCurrentHorizontalMomentumVelocity;

            lastFrameVelocity = MoveAndSlide(horizontalVelocity + verticalVelocity, Vector2.Up, true);
            var isOnFloor = IsOnFloor();
            if (!isOnFloor && !IsInAir)
            {
                IsInAir = true;
                if (!IsJumping)
                    isInAirSinceMsec = Time.GetTicksMsec();
            }

            if (isOnFloor)
            {
                timeSpentFalling = 0f;
                IsInAir = false;
                HasDash = true;
            }
        }

        private Vector2 HandleJumping(Vector2 verticalVelocity, float delta)
        {
            var jumpHeldFor = ((Time.GetTicksMsec() - jumpHeldSinceMsec) / 1000f) / MaxJumpHeldDownTimeSeconds;
            jumpCurrentHorizontalMomentumVelocity = jumpStartHorizontalMomentumVelocity * JumpMomentumFalloffCurve.Interpolate(jumpHeldFor);
            if (IsJumpButtonHeld && jumpHeldFor <= 1.0f)
                return verticalVelocity * JumpSpeedPixelsPerSecond * JumpSpeedHoldTimeModifierCurve.Interpolate(jumpHeldFor);
            jumpHangTimeCurrent += delta;
            if (jumpHangTimeCurrent >= JumpHangTimeSeconds)
            {
                IsJumping = false;
                timeSpentFalling = 0f;
            };

            return verticalVelocity;
        }

        private void HandleDashing(float delta)
        {
            var oldPosition = GlobalPosition;
            var dashVelocity = DashDirection * DashSpeedPixelsPerSecond * DashDistanceSpeedEaseCurve.Interpolate(dashDistanceCovered / DashDistance);
            if (dashDistanceCovered > DashDistance)
            {
                dashHangTimeCurrent += delta;
                if (dashHangTimeCurrent >= DashHangTimeSeconds)
                    DashDone();
                return;
            }

            var collision = MoveAndCollide(dashVelocity * delta, testOnly: true);
            if (collision != null)
            {
                dashDistanceCovered += DashDistance;
                return;
            }

            MoveAndSlide(dashVelocity, Vector2.Up, false);
            var newPosition = GlobalPosition;
            dashDistanceCovered += oldPosition.DistanceTo(newPosition);
            return;
        }

        public void SleepReleased()
        {
            IsSleeping = false;
            PlayerPower.Instance.PowerDrainModifier = 1f;
        }

        public void SleepPressed()
        {
            IsSleeping = true;
            PlayerPower.Instance.PowerDrainModifier = 0f;
        }
    }
}