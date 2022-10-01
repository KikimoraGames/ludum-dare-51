using Godot;
using System;

namespace Game
{
    public class SFXChannel3D : AudioStreamPlayer3D
    {
        public void Play(RandomSFXContainer clip)
        =>
            Play(
                clip.Clips[(int)(GD.Randi() % clip.Clips.Count)],
                clip.Bus,
                clip.PitchRandom,
                clip.PitchRange
            );

        public void Play(AudioStream stream, string bus, bool pitchRandom = false, Vector2 pitchRange = default)
        {
            PitchScale = pitchRandom ? (float)GD.RandRange(pitchRange.x, pitchRange.y) : 1;
            Stream = stream;
            Bus = bus;
            Play();
        }
    }
}