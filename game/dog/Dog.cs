using Godot;
using GodotOnReady.Attributes;
using System;

namespace Game
{
    public partial class Dog : Node2D
    {
        [OnReadyGet]
        private AnimationPlayer animationPlayer;
        [OnReadyGet]
        private SFXChannel2D sfxChannel;

        [Export]
        private float barkEverySeconds { get; set; } = 2f;
        [Export]
        private RandomSFXContainer barkSFX;

        private float startingBarkTimer = 0f;

        [OnReady]
        private void BarkOffset()
        {
            startingBarkTimer += GD.Randf();
        }

        public override void _Process(float delta)
        {
            base._Process(delta);
            startingBarkTimer += delta;
            if (startingBarkTimer > barkEverySeconds)
            {
                Bark();
                startingBarkTimer = 0f;
            }
        }

        public void Bark()
        {
            animationPlayer.Play("bark");
            sfxChannel.Play(barkSFX);
        }


    }
}