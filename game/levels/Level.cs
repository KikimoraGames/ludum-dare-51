using Godot;
using GodotOnReady.Attributes;
using System;

namespace Game
{
    public partial class Level : Node
    {
        [Signal]
        public delegate void level_complete();
        [Signal]
        public delegate void level_failed();

        [OnReadyGet("UI")]
        private CanvasLayer ui;

        [OnReady]
        private void SetUIVisible() => ui.Show();


        [OnReady]
        private void ConnectToHumanDeath()
        {
            Events.Connect(nameof(Events.human_destroyed), this, nameof(OnHumanDestroyed));
        }

        [OnReady]
        protected virtual void OnLevelReady()
        {
            InputProcessor.Instance.IgnoreInput = true;
            PlayerPower.Instance.PowerDrainModifier = 0f;
            if (this.IsDebugMainScene())
                Begin();
        }

        public void Unload()
        {
            InputProcessor.Instance.IgnoreInput = true;
        }

        public void Begin()
        {
            PlayerPower.Instance.PowerDrainModifier = 1f;
            InputProcessor.Instance.IgnoreInput = false;
        }

        private void OnHumanDestroyed()
        {
            var hs = GetTree().GetNodesInGroup<Human>("humans", this);
            if (hs.Count <= 0)
            {
                EmitSignal(nameof(level_complete));
                return;
            }

            foreach (var h in hs)
            {
                if (IsInstanceValid(h) && !h.IsQueuedForDeletion())
                    return;
            }

            EmitSignal(nameof(level_complete));
        }
    }
}