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
            head.Play(animation);
            meatbag.Play(animation);
            body.Play(animation);
        }
        public void Queue(string animation)
        {
            head.Queue(animation);
            meatbag.Queue(animation);
            body.Queue(animation);
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
    }
}