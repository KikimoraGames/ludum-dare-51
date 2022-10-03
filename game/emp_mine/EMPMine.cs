using Godot;
using GodotOnReady.Attributes;
using System;


namespace Game
{
    public partial class EMPMine : Node2D
    {
        [Export]
        private float StunForSeconds = 1.5f;

        [OnReadyGet("AnimationPlayer")]
        private AnimationPlayer animationPlayer;

        public void BodyEnteredDetectionArea(PhysicsBody2D b)
        {
            if (!(b is Player p))
                return;

            animationPlayer.Play("player_detected");
        }

        public void BodyExitedDetectionArea(PhysicsBody2D b)
        {
            if (!(b is Player))
                return;

            animationPlayer.Play("static");
        }

        private Player player;

        public void BodyEnteredAttackArea(PhysicsBody2D b)
        {
            if (!(b is Player p))
                return;
            player = p;
            if (!p.CanBeStunned)
            {
                SetProcess(true);
                return;
            }

            p.Stun(StunForSeconds);
            animationPlayer.Play("zap");
        }

        public void BodyExitedAttackArea(PhysicsBody2D b)
        {
            if (!(b is Player p))
                return;
            player = null;
            SetProcess(false);
        }

        public override void _Process(float delta)
        {
            base._Process(delta);
            if (player == null)
                return;

            if (!player.CanBeStunned)
                return;

            player.Stun(StunForSeconds);
            animationPlayer.Play("zap");
            SetProcess(false);
        }

        public void AnimationFinished(string anim)
        {
            if (anim != "zap")
                return;

            QueueFree();
        }
    }
}