using Godot;
using GodotOnReady.Attributes;
using System;


namespace Game
{
    public partial class EMPMine : Node2D
    {
        [Export]
        private float StunForSeconds = 1.5f;
        [Export]
        private RandomSFXContainer beepSFX;
        [Export]
        private RandomSFXContainer playerZapSFX;

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

            Events.PlaySFX(playerZapSFX);
            p.Stun(StunForSeconds);
            animationPlayer.Play("zap");
            SetProcess(false);
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

            Events.PlaySFX(playerZapSFX);
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

        public void Beep() => Events.PlaySFX(beepSFX);
    }
}