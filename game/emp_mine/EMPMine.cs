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

        private bool isDetonating = false;
        public void BodyEnteredDetectionArea(PhysicsBody2D b)
        {
            if (!(b is Player p))
                return;

            animationPlayer.Play("blink");
        }

        public void BodyExitedDetectionArea(PhysicsBody2D b)
        {
            if (!(b is Player))
                return;

            animationPlayer.Play("off");
        }

        public void BodyEnteredNearDetectionArea(PhysicsBody2D b)
        {
            if (!(b is Player p))
                return;

            animationPlayer.Play("intense blink");
        }

        public void BodyExitedNearDetectionArea(PhysicsBody2D b)
        {
            if (!(b is Player))
                return;

            animationPlayer.Play("blink");
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

            isDetonating = true;
            animationPlayer.Play("anticipation");
            SetProcess(false);
        }

        public void BodyExitedAttackArea(PhysicsBody2D b)
        {
            if (!(b is Player p))
                return;
            if (isDetonating)
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

            isDetonating = true;
            animationPlayer.Play("anticipation");
            SetProcess(false);
        }

        public void AnimationFinished(string anim)
        {
            if (anim != "anticipation")
                return;

            Events.PlaySFX(playerZapSFX);
            player.Stun(StunForSeconds);
            QueueFree();
        }

        public void Beep() => Events.PlaySFX(beepSFX);
    }
}