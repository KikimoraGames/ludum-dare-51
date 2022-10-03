using Godot;
using GodotOnReady.Attributes;
using System;

namespace Game
{
    public partial class AnimationController : Node
    {
        [OnReadyGet]
        private AnimationPlayer head;
        [OnReadyGet]
        private AnimationPlayer meatbag;
        [OnReadyGet]
        private AnimationPlayer body;

        public string CurrentAnimation => body.CurrentAnimation;

        public void Play(string animation)
        {
            meatbag.Play(animation);
            body.Play(animation);
            if (head.CurrentAnimation != "attack" || head.CurrentAnimation != "bark_loop")
                head.Play(animation);
        }


        public void Bark()
        {
            head.Play("bark_loop");
        }

        public void BarkDone()
        {
            head.Play(body.CurrentAnimation);
            head.Seek(body.CurrentAnimationPosition);
        }

        public void Attack()
        {
            head.Play("attack");
        }

        public void AttackDone()
        {
            head.Play(body.CurrentAnimation);
            head.Seek(body.CurrentAnimationPosition);
        }
    }
}