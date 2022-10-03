using Godot;
using GodotOnReady.Attributes;
using System;

namespace Game
{
    public partial class Player : KinematicBody2D
    {
        private const string ACTION_JUMP = "action_jump";
        private const uint PLATFORM_PHYSICS_LAYER = 1u << 30;

        [Signal]
        public delegate void on_horizontal_direction_change(float dir);

        [Export]
        public float InvulerableAfterStunSeconds { get; private set; } = 0.5f;
        [Export]
        public float LongFallRecoveryTimeSeconds { get; private set; } = 0.2f;
        [Export]
        public float MovementSpeedPixelsPerSecond { get; private set; } = 250;
        [Export]
        public Curve MovementHorizontalSpeedEaseCurve { get; private set; }
        [Export]
        public float MovemementHorizontalMaxHoldSeconds { get; private set; } = 0.1f;
        [Export]
        public float JumpSpeedPixelsPerSecond { get; private set; } = 800f;
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
        [Export]
        private PackedScene boneblockScene;
        [Export]
        public int BlockBoneCost { get; private set; } = 1;
        [Export]
        public float BlockPlacementVerticalDistancePixels { get; private set; } = 64;
        [Export]
        public float BlockPlacementVerticalMaxSpeedPixelsPerSecond { get; private set; } = 500;
        [Export]
        public Curve BlockPlacementDistanceSpeedEaseCurve { get; private set; }

        [OnReadyGet("BarkEffect")]
        private CanvasItem barkEffect;
        [OnReadyGet("GoopTracker")]
        private GoopTracker goopTracker;
        [OnReadyGet("GoopParticles")]
        private Particles2D goopParticles;
        [OnReadyGet("GoopSolidsEmitter")]
        private GoopSolidsEmitter goopSolidsEmitter;

        [OnReadyGet("AnimationController")]
        private AnimationController animationController;
        [OnReadyGet("DropDownParticles")]
        private Particles2D dropDownParticles;

        public bool IsJumpButtonHeld { get; private set; } = false;
        public bool IsJumping { get; private set; } = false;
        public bool IsInAir { get; private set; } = false;
        public bool HasDash { get; private set; } = true;
        public bool IsPlacingBlock { get; private set; } = false;
        public bool IsFallingThrough { get; private set; } = false;

        public bool HasPowerForDash => PlayerPower.Instance.CurrentPower >= DashPowerCost;
        public bool HasBonesForBlock => PlayerBonemass.Instance.CurrentBones >= BlockBoneCost;

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

        private float blockPlacementDistanceCovered = 0f;

        public bool IsSleeping { get; private set; } = false;
        public bool IsStunned { get; private set; } = false;
        public bool IsInvulnerable => invulnerabilityTime > 0f;
        public bool CanBeStunned => !IsStunned && !IsInvulnerable;
        private Vector2 lastFrameVelocity;
        private float stunnedForSeconds = 0f;
        private float invulnerabilityTime = -1f;

        private static Vector2 DirectionClamp(Vector2 v)
        {
            v = Mathf.Abs(v.x) > Mathf.Abs(v.y) ? new Vector2(v.x, 0) : new Vector2(0, v.y);
            return v.Normalized();
        }

        [OnReady]
        private void PlayStatic() => animationController.Play("static");

        public void JumpPressed()
        {
            var inputVelocity = DirectionClamp(InputProcessor.Instance.InputVelocity);
            if (inputVelocity.y > 0.5)
            {
                HandleJumpThroughPlatform();
                return;
            }
            animationController.Play("up");
            IsJumping = true;
            IsJumpButtonHeld = true;
            jumpHangTimeCurrent = 0f;
            jumpHeldSinceMsec = Time.GetTicksMsec();
            jumpStartHorizontalMomentumVelocity = lastFrameVelocity.x;
            jumpCurrentHorizontalMomentumVelocity = jumpStartHorizontalMomentumVelocity;
        }

        private async void HandleJumpThroughPlatform()
        {
            if (IsFallingThrough)
                return;

            var world = GetWorld2d();
            var collision = world.DirectSpaceState.IntersectRay(GlobalPosition, GlobalPosition + Vector2.Down * 50, collisionLayer: PLATFORM_PHYSICS_LAYER);
            if (collision.Count <= 0)
                return;

            var intersectionPoint = (Vector2)collision["position"];
            var normal = (Vector2)collision["normal"];
            uint oldMask = CollisionMask;
            var newMask = oldMask & ~(PLATFORM_PHYSICS_LAYER);

            CollisionMask = newMask;
            var tree = GetTree();
            IsFallingThrough = true;
            animationController.Play("down");
            var count = 0;
            while (count < 5)
            {
                count++;
                await this.WaitFrames(1, FrameWaiter.PROCESS.PHYSICS);
                if (!IsInsideTree())
                    break;

                var check = world.DirectSpaceState.IntersectRay(GlobalPosition, intersectionPoint, collisionLayer: PLATFORM_PHYSICS_LAYER);
                if (check.Count <= 0)
                    break;

                var newNormal = (Vector2)check["normal"];
                var newIntersection = (Vector2)check["position"];

                if (!newNormal.IsEqualApprox(Vector2.Zero) && (Mathf.Sign(newNormal.y) != Mathf.Sign(normal.y)))
                    break;
            }
            CollisionMask = oldMask;
            IsFallingThrough = false;
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
            animationController.Play("dash");
            var inputVelocity = InputProcessor.Instance.InputVelocity;
            var dashDirection = (inputVelocity.LengthSquared() > 0.1f ? inputVelocity : InputProcessor.Instance.LastNonZeroDirection);
            dashDirection = DirectionClamp(dashDirection);
            DashDirection = dashDirection.Normalized();
            goopSolidsEmitter.EmitSolid(10);
        }

        public void SleepReleased()
        {
            IsSleeping = false;
            animationController.Play("static");
            PlayerPower.Instance.PowerDrainModifier = 1f;
            goopTracker.SetProcess(true);
            goopParticles.Emitting = true;
        }

        public void SleepPressed()
        {
            IsSleeping = true;
            animationController.Play("sit");
            PlayerPower.Instance.PowerDrainModifier = 0f;
            goopTracker.SetProcess(false);
            goopParticles.Emitting = false;
        }

        public async void Bark()
        {
            if (barkEffect.Visible)
                return;

            barkEffect.Visible = true;
            animationController.Bark();
            await this.WaitSeconds(0.6f);
            animationController.BarkDone();
            barkEffect.Visible = false;
        }

        public void BlockPressed()
        {
            IsPlacingBlock = true;
            blockPlacementDistanceCovered = 0f;
            animationController.Play("shit");
            var block = boneblockScene.Instance<Boneblock>();
            block.GlobalPosition = GlobalPosition;
            GetParent().AddChild(block);
            PlayerBonemass.Instance.SpendBones(BlockBoneCost);
        }

        private void DashDone()
        {
            PlayerPower.Instance.PowerDrainModifier = 1.0f;
            PlayerPower.Instance.Add(-DashPowerCost);
            IsDashing = false;
            timeSpentFalling = 0f;
            if (!IsStunned)
                animationController.Play("down");
        }

        private float previousHorizontalInputDirection;
        private float secondsInCurrentHorizontalDirection;

        public override void _PhysicsProcess(float delta)
        {
            base._PhysicsProcess(delta);
            invulnerabilityTime = Mathf.Clamp(invulnerabilityTime, -1f, invulnerabilityTime - delta);
            if (IsSleeping)
                return;

            if (IsStunned)
            {
                stunnedForSeconds -= delta;
                if (stunnedForSeconds < 0)
                {
                    IsStunned = false;
                    invulnerabilityTime = InvulerableAfterStunSeconds;
                }
            }

            var inputVelocity = InputProcessor.Instance.InputVelocity;
            var horizontalVelocity = Vector2.Right * inputVelocity;
            var horizontalDirection = Mathf.Sign(horizontalVelocity.x);
            if (previousHorizontalInputDirection != horizontalDirection && !Mathf.IsZeroApprox(horizontalDirection))
            {
                previousHorizontalInputDirection = horizontalDirection;
                EmitSignal(nameof(on_horizontal_direction_change), previousHorizontalInputDirection);
                secondsInCurrentHorizontalDirection = 0;
            }
            else
                secondsInCurrentHorizontalDirection += delta;


            var verticalVelocity = Vector2.Up;

            if (IsDashing)
            {
                HandleDashing(delta);
                return;
            }

            if (IsPlacingBlock)
            {
                HandlePlacingBlock(delta);
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


            horizontalVelocity *= MovementSpeedPixelsPerSecond * MovementHorizontalSpeedEaseCurve.Interpolate(secondsInCurrentHorizontalDirection / MovemementHorizontalMaxHoldSeconds);
            if (IsInAir)
                horizontalVelocity += Vector2.Right * jumpCurrentHorizontalMomentumVelocity;

            if (IsStunned)
                horizontalVelocity = Vector2.Zero;
            lastFrameVelocity = MoveAndSlide(horizontalVelocity + verticalVelocity, Vector2.Up, true, infiniteInertia: false);
            var isOnFloor = IsOnFloor();
            if (isOnFloor && !IsInAir && !IsStunned)
            {
                var currAnim = animationController.CurrentAnimation;
                if (horizontalVelocity.LengthSquared() > 0.1f)
                {
                    if (currAnim != "walk")
                        animationController.Play("walk");
                }
                else
                {
                    if (currAnim != "static" && currAnim != "recovery")
                        animationController.Play("static");
                }
            }

            if (!isOnFloor && !IsInAir)
            {
                IsInAir = true;
                if (!IsJumping)
                    isInAirSinceMsec = Time.GetTicksMsec();
            }

            if (isOnFloor && IsInAir)
            {
                if (timeSpentFalling > 0.4f)
                {
                    dropDownParticles.Restart();
                    if (!IsStunned && timeSpentFalling > 0.65f)
                    {
                        animationController.Play("recovery");
                        IsStunned = true;
                        stunnedForSeconds = LongFallRecoveryTimeSeconds;
                    }
                }
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
                if (animationController.CurrentAnimation != "down")
                    animationController.Play("down");
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

            var collision = MoveAndCollide(dashVelocity * delta, infiniteInertia: false, testOnly: true);
            if (collision != null)
            {
                dashDistanceCovered += DashDistance;
                return;
            }

            MoveAndSlide(dashVelocity, Vector2.Up, false, infiniteInertia: false);
            var newPosition = GlobalPosition;
            dashDistanceCovered += oldPosition.DistanceTo(newPosition);
        }

        private void HandlePlacingBlock(float delta)
        {
            if (blockPlacementDistanceCovered > BlockPlacementVerticalDistancePixels)
            {
                BlockPlacementDone();
                return;
            }

            var oldPosition = GlobalPosition;
            var velocity = Vector2.Up * BlockPlacementVerticalMaxSpeedPixelsPerSecond * BlockPlacementDistanceSpeedEaseCurve.Interpolate(blockPlacementDistanceCovered / BlockPlacementVerticalDistancePixels);

            var collision = MoveAndCollide(velocity * delta, infiniteInertia: false, testOnly: true);
            if (collision != null)
            {
                BlockPlacementDone();
                return;
            }

            MoveAndSlide(velocity, Vector2.Up, false, infiniteInertia: false);
            var newPosition = GlobalPosition;
            blockPlacementDistanceCovered += oldPosition.DistanceTo(newPosition);
        }

        private void BlockPlacementDone()
        {
            IsPlacingBlock = false;
            animationController.Play("static");
        }

        public async void Stun(float stunForSeconds)
        {

            animationController.Play("stun");
            IsStunned = true;
            stunnedForSeconds = stunForSeconds;
            if (IsDashing)
                DashDone();

            IsJumping = false;
            IsPlacingBlock = false;
            Engine.TimeScale = 0.1f;
            Events.ShakeCamera(Vector2.One * 50f, 0.5f);
            await this.WaitSeconds(0.1f);
            Engine.TimeScale = 1f;
        }
    }
}