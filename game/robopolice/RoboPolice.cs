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
        [OnReadyGet("Sprite")]
        private Sprite sprite;


        [OnReady]
        private void SetCurrentMovementSpeed() => currentTrackFollowMovementSpeed = MovementSpeedPixelsPerSecond;

        private Vector2 globalPositionWhenPlayerTrackingStarted;
        private bool IsTrackingPlayer = false;
        private bool IsReturningToPatrol = false;
        private bool IsZapping = false;
        private float currentTrackFollowMovementSpeed;
        private Player player;

        public string State()
        {
            var s = "";
            if (IsZapping)
                s += " ZAP";
            if (IsTrackingPlayer)
                s += " TRACK";
            if (IsReturningToPatrol)
                s += " PATROL";
            if (!IsTrackingPlayer && !IsReturningToPatrol)
                s += " FOLLOW";
            return s;
        }
        public override void _Process(float delta)
        {
            base._Process(delta);
            GD.Print(State());
            if (IsZapping)
                return;

            if (IsTrackingPlayer)
            {
                if (player == null || !player.IsStunned)
                {
                    var dir = GlobalPosition.DirectionTo(player.GlobalPosition);
                    GlobalPosition += dir * MovementSpeedPixelsPerSecond * delta;
                    return;
                }

                IsTrackingPlayer = false;
                IsReturningToPatrol = true;
                return;
            }

            if (IsReturningToPatrol)
            {
                if (player == null || player.IsStunned)
                {
                    var dir = GlobalPosition.DirectionTo(globalPositionWhenPlayerTrackingStarted);
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
                currentTrackFollowMovementSpeed *= -1;
        }

        public void BodyEnteredDetectionArea(PhysicsBody2D b)
        {
            if (!(b is Player p))
                return;

            globalPositionWhenPlayerTrackingStarted = GlobalPosition;
            player = p;
            IsTrackingPlayer = true;
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
            IsZapping = true;
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