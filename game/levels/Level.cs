using Godot;
using GodotOnReady.Attributes;
using System;

namespace Game
{
    public partial class Level : Node
    {
        [Signal]
        public delegate void level_completed();
        [Signal]
        public delegate void level_failed();

        [OnReadyGet("UI")]
        private CanvasLayer ui;

        private bool isFailEmitted;

        [OnReady]
        private void SetUIVisible() => ui.Show();


        [OnReady]
        private void ConnectToImportantSignals()
        {
            Events.Connect(nameof(Events.human_destroyed), this, nameof(OnHumanDestroyed));
            PlayerPower.Instance.Connect(nameof(PlayerPower.on_power_changed), this, nameof(OnPowerChanged));
        }

        [OnReady]
        protected virtual void OnLevelReady()
        {
            InputProcessor.Instance.IgnoreInput = true;
            PlayerPower.Instance.LevelOverride = 0f;
            if (this.IsDebugMainScene())
                Begin();
        }

        public void Unload()
        {
            InputProcessor.Instance.IgnoreInput = true;
        }

        public virtual void Begin()
        {
            PlayerPower.Instance.LevelOverride = 1f;
            Engine.TimeScale = 1f;
            InputProcessor.Instance.IgnoreInput = false;
        }

        protected virtual void OnHumanDestroyed()
        {
            var hs = GetTree().GetNodesInGroup<Human>("humans", this);
            if (hs.Count <= 0)
            {
                EmitSignal(nameof(level_completed));
                return;
            }

            foreach (var h in hs)
            {
                if (IsInstanceValid(h) && !h.IsQueuedForDeletion())
                    return;
            }

            EmitSignal(nameof(level_completed));
        }

        protected virtual void OnPowerChanged(float p)
        {
            if (isFailEmitted)
                return;


            if (p > 0f)
                return;

            isFailEmitted = true;
            EmitSignal(nameof(level_failed));
        }
    }
}