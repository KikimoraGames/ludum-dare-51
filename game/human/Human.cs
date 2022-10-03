using Godot;
using GodotOnReady.Attributes;
using System;

namespace Game
{
    public partial class Human : Node2D
    {
        [Export]
        private float PowerOnEat = 10;
        [Export]
        private float MovementSpeedPixelsPerSecond = 200;

        [Export]
        private Godot.Collections.Array<Color> approvedColors;

        [Export(PropertyHint.Range, "0,1,0.01")]
        private float moustacheChance = 0.25f;
        [Export(PropertyHint.Range, "0,1,0.01")]
        private float hatChance = 0.33f;

        [OnReadyGet("Path2D/PathFollow2D/SpriteHolder/SpriteOffset/Moustache")]
        private Sprite moustacheSprite;
        [OnReadyGet("Path2D/PathFollow2D/SpriteHolder/SpriteOffset/Hat1")]
        private Sprite hat1Sprite;
        [OnReadyGet("Path2D/PathFollow2D/SpriteHolder/SpriteOffset/Hat2")]
        private Sprite hat2Sprite;
        [OnReadyGet("Path2D/PathFollow2D/SpriteHolder")]
        private SpriteHolder spriteHolder;

        [OnReadyGet("Path2D/PathFollow2D")]
        private PathFollow2D pathFollow;
        [OnReadyGet("AnimationPlayer")]
        private AnimationPlayer animationPlayer;
        [OnReadyGet("Path2D/PathFollow2D/SFXChannel2D")]
        private SFXChannel2D sfxChannel;

        [Export]
        private RandomSFXContainer onConsumeSFX;
        [Export]
        public RandomSFXContainer footstepsSFX;

        private float currentTrackFollowMovementSpeed = 0;

        [OnReady]
        private void RandomizeAppereance()
        {
            var roll = GD.Randf();
            moustacheSprite.SelfModulate = new Color(approvedColors[(int)(GD.Randi() % approvedColors.Count)], roll < moustacheChance ? 1.0f : 0.0f);

            roll = GD.Randf();
            if (roll > hatChance)
                return;

            roll = GD.Randf();

            if (roll > 0.5)
            {
                hat1Sprite.Visible = true;
                hat1Sprite.SelfModulate = new Color(approvedColors[(int)(GD.Randi() % approvedColors.Count)], 1.0f);
                return;
            }

            hat2Sprite.Visible = true;
            hat2Sprite.SelfModulate = new Color(approvedColors[(int)(GD.Randi() % approvedColors.Count)], 1.0f);
        }

        public override void _Process(float delta)
        {
            base._Process(delta);
            FollowPath(delta);
        }

        private void FollowPath(float delta)
        {
            pathFollow.Offset += currentTrackFollowMovementSpeed * delta;
            if (pathFollow.UnitOffset >= 1.0f || Mathf.IsZeroApprox(pathFollow.UnitOffset))
            {
                currentTrackFollowMovementSpeed *= -1;
                spriteHolder.OnDirectionChange(Mathf.Sign(currentTrackFollowMovementSpeed));
            }
        }

        private void PlayerDetectionAreaEntered(PhysicsBody2D b)
        {
            if (!(b is Player p))
                return;
            if (currentTrackFollowMovementSpeed > 0)
                return;
            currentTrackFollowMovementSpeed = MovementSpeedPixelsPerSecond * -Mathf.Sign(spriteHolder.GlobalPosition.DirectionTo(p.GlobalPosition).x);
            animationPlayer.Play("run");
        }
        private bool isEaten;
        private async void PlayerCollisionAreaEntered(PhysicsBody2D b)
        {
            if (isEaten)
                return;

            if (!(b is Player p))
                return;
            isEaten = true;
            EmitHumanEaten();
            p.AnimationController.Attack();
            PlayerPower.Instance.Add(PowerOnEat);
            PlayerBonemass.Instance.AddHuman(1);
            Engine.TimeScale = 0.5f;
            Events.ZoomCamera(0.8f, 0.2f, Tween.TransitionType.Expo, Tween.EaseType.In);
            Events.PlaySFX(onConsumeSFX);
            p.humanEatenParticles.GlobalPosition = pathFollow.GlobalPosition;
            p.humanEatenParticles.Restart();
            await this.WaitSeconds(0.1f);
            Engine.TimeScale = 1f;
            p.AnimationController.AttackDone();
            QueueFree();
            Events.Emit(nameof(Events.human_destroyed));
        }

        private void EmitHumanEaten()
        {
            Texture hatTexture = null;
            Color hatColor = new Color(1, 1, 1, 0);

            if (hat1Sprite.Visible)
            {
                hatTexture = hat1Sprite.Texture;
                hatColor = hat1Sprite.SelfModulate;
            }
            if (hat2Sprite.Visible)
            {
                hatTexture = hat2Sprite.Texture;
                hatColor = hat2Sprite.SelfModulate;
            }

            Events.HumanEaten(hatTexture, moustacheSprite.SelfModulate, hatColor);
        }
        public void FootstepSFX() => sfxChannel.Play(footstepsSFX);
    }
}