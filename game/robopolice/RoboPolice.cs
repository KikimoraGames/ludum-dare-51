using Godot;
using GodotOnReady.Attributes;
using System;

namespace Game
{
    public partial class RoboPolice : PathFollow2D
    {
        [Export]
        private float MovementSpeedPixelsPerSecond = 200;
        [Export]
        private float StunForSeconds = 1.5f;
        [Export]
        private RandomSFXContainer playerDetectedSFX;
        [Export]
        private RandomSFXContainer playerZapSFX;


        [OnReadyGet("SpriteHolder")]
        private SpriteHolder spriteHolder;
        [OnReadyGet("SpriteHolder/Sprite")]
        private Sprite sprite;


        [OnReady]
        private void SetCurrentMovementSpeed() => currentTrackFollowMovementSpeed = MovementSpeedPixelsPerSecond;

        private Vector2 globalPositionWhenPlayerTrackingStarted;
        private bool IsTrackingPlayer = false;
        private bool IsReturningToPatrol = false;
        private bool IsZapping = false;
        private float currentTrackFollowMovementSpeed;
        private Player player;

        public override void _Process(float delta)
        {
            base._Process(delta);
            if (IsZapping)
                return;

            if (IsTrackingPlayer)
            {
                if (player.CanBeStunned)
                {
                    var dir = GlobalPosition.DirectionTo(player.GlobalPosition);
                    spriteHolder.OnDirectionChange(Mathf.Sign(dir.x));
                    GlobalPosition += dir * MovementSpeedPixelsPerSecond * delta;
                    return;
                }

                IsTrackingPlayer = false;
                IsReturningToPatrol = true;
                return;
            }

            if (IsReturningToPatrol)
            {
                if (player == null || !player.CanBeStunned)
                {
                    var dir = GlobalPosition.DirectionTo(globalPositionWhenPlayerTrackingStarted);
                    spriteHolder.OnDirectionChange(Mathf.Sign(dir.x));
                    GlobalPosition += dir * MovementSpeedPixelsPerSecond * delta;
                    IsReturningToPatrol = GlobalPosition.DistanceSquaredTo(globalPositionWhenPlayerTrackingStarted) > 5f;
                    return;
                }

                IsTrackingPlayer = true;
                IsReturningToPatrol = false;
                return;
            }

            if (!IsTrackingPlayer && !IsReturningToPatrol)
                FollowPath(delta);
        }

        private void FollowPath(float delta)
        {
            Offset += currentTrackFollowMovementSpeed * delta;
            if (UnitOffset >= 1.0f || Mathf.IsZeroApprox(UnitOffset))
            {
                currentTrackFollowMovementSpeed *= -1;
                spriteHolder.OnDirectionChange(Mathf.Sign(currentTrackFollowMovementSpeed));
            }
        }

        public void BodyEnteredDetectionArea(PhysicsBody2D b)
        {
            if (!(b is Player p))
                return;

            globalPositionWhenPlayerTrackingStarted = GlobalPosition;
            player = p;
            IsTrackingPlayer = true;
            Events.PlaySFX(playerDetectedSFX);
        }

        public void BodyExitedDetectionArea(PhysicsBody2D b)
        {
            if (!(b is Player))
                return;

            player = null;
            if (IsTrackingPlayer == true)
            {
                IsTrackingPlayer = false;
                IsReturningToPatrol = true;
            }
        }

        public void BodyEnteredAttackArea(PhysicsBody2D b)
        {
            if (!(b is Player p))
                return;
            if (!p.CanBeStunned)
                return;
            IsZapping = true;
            Events.PlaySFX(playerZapSFX);
            p.Stun(StunForSeconds);
            var tween = CreateTween();
            tween.TweenProperty(sprite, "scale", Vector2.One * 20, 0.15f).AsRelative().SetTrans(Tween.TransitionType.Bounce).SetEase(Tween.EaseType.In);
            tween.TweenProperty(sprite, "scale", sprite.Scale, 0.1f).SetTrans(Tween.TransitionType.Linear).SetEase(Tween.EaseType.In);
            tween.TweenCallback(this, nameof(ZapAnimationDone));
        }

        private void ZapAnimationDone()
        {
            IsZapping = false;
            IsTrackingPlayer = false;
            IsReturningToPatrol = true;
        }

    }
}