using Godot;
using System;
using System.Collections.Generic;

namespace Game
{
    public class RandomSFXContainer : Resource
    {
        [Export]
        public List<AudioStream> Clips { get; protected set; }
        [Export]
        public string Bus { get; protected set; }

        [Export]
        public bool PitchRandom { get; protected set; } = false;
        [Export]
        public Vector2 PitchRange { get; protected set; } = Vector2.Zero;

        [Export]
        public bool IsSingleInstance { get; protected set; } = false;
        [Export]
        public float SingleInstanceTime { get; protected set; } = 0f;
    }
}