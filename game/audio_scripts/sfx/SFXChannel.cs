using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;

namespace Game
{
    public class SFXChannel : Node
    {
        [Export]
        private string defaultBus;

        private readonly List<AudioStreamPlayer> channels = new List<AudioStreamPlayer>();
        private readonly SortedList<ulong, AudioStreamPlayer> channelsPlaying = new SortedList<ulong, AudioStreamPlayer>();
        private readonly System.Collections.Generic.HashSet<string> containersPlaying = new HashSet<string>();

        public override void _Ready()
        {
            channels.AddRange(new Array<AudioStreamPlayer>(GetTree().GetNodesInGroup("sfx_channels")));
            Events.Connect(nameof(Events.request_sfx_play), this, nameof(Play));
            Events.Connect(nameof(Events.request_sfx_rand_play), this, nameof(PlayRandom));
        }

        public void PlayRandom(RandomSFXContainer clip)
        {
            if (clip.IsSingleInstance)
            {
                if (containersPlaying.Contains(clip.ResourcePath))
                    return;
                containersPlaying.Add(clip.ResourcePath);
                ClearTimedSingleInstance(clip.ResourcePath, clip.SingleInstanceTime);
            }

            Play(clip.Clips[(int)(GD.Randi() % clip.Clips.Count)], clip.Bus, clip.PitchRandom, clip.PitchRange);
        }
        public void Play(AudioStream clip, string onBus) => Play(clip, onBus, false, default);

        public void Play(AudioStream clip, string onBus, bool pitchRandom, Vector2 pitchRange)
        {
            var c = GetChannel();
            var id = Time.GetTicksUsec();
            c.Stream = clip;
            c.Bus = string.IsNullOrEmpty(onBus) ? defaultBus : onBus;
            channelsPlaying.Add(id, c);
            if (pitchRandom)
                c.PitchScale = (float)GD.RandRange(pitchRange.x, pitchRange.y);
            else
                c.PitchScale = 1;
            c.Play();
            WaitUntilDone(id, c);
        }

        private AudioStreamPlayer GetChannel()
        {
            foreach (var c in channels)
            {
                if (c.Playing)
                    continue;

                return c;
            }
            var id = channelsPlaying.Keys[0];
            var ch = channelsPlaying[id];
            channelsPlaying.Remove(id);
            return ch;
        }

        private async void ClearTimedSingleInstance(string instanceID, float time)
        {
            await this.WaitSeconds(time);
            if (containersPlaying.Contains(instanceID))
                containersPlaying.Remove(instanceID);
        }

        private async void WaitUntilDone(ulong id, AudioStreamPlayer p)
        {
            var n = p.Name;
            p.Name += " Active";
            await ToSignal(p, "finished");
            p.Name = n;
            p.Stream = null;
            channelsPlaying.Remove(id);
        }
    }
}