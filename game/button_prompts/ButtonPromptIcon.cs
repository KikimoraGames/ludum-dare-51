using Godot;
using System;
using System.Collections.Generic;

namespace Game.UI.ButtonPrompts
{
    public class ButtonPromptIcon : TextureRect, IButtonPrompt
    {
        [Export]
        public string OnlyShowFor { get; set; } = "";
        [Export]
        private TileSet tileset;
        [Export]
        public string Action { get; set; }
        [Export]
        private float fps;

        public float FPS
        {
            get => fps;
            set
            {
                fps = value;
                if (value > 0)
                    SetProcess(true);
            }
        }

        private AtlasTexture atlas;

        public bool Disabled
        {
            get => prompt.Disabled;
            set => prompt.Disabled = value;
        }

        bool IButtonPrompt.Visible
        {
            get
            {
                return Visible;
            }
            set
            {
                Visible = value;
            }
        }

        Color IButtonPrompt.SelfModulate
        {
            get => SelfModulate;
            set => SelfModulate = value;
        }

        float IButtonPrompt.FPS => FPS;

        TileSet IButtonPrompt.TileSet => tileset;

        private ButtonPrompt prompt;

        public override void _Ready()
        {
            prompt = new ButtonPrompt(this, (AtlasTexture)Texture);
            Connect("visibility_changed", this, nameof(OnVisibilityChanged));
            prompt.OnReady();
        }

        private void OnVisibilityChanged()
        {
            if (IsVisibleInTree())
            {
                InputDeviceDetector.Connect(this, nameof(OnInputDeviceChanged));
                OnInputDeviceChanged(InputDeviceDetector.CurrentDeviceType, InputDeviceDetector.CurrentDeviceID);
            }
            else
                InputDeviceDetector.Disconnect(this, nameof(OnInputDeviceChanged));
        }

        public override void _Process(float delta) => prompt.OnProcess(delta);
        private void OnInputDeviceChanged(string deviceType, int id) => prompt.OnInputDeviceChanged(deviceType, id);

        void IButtonPrompt.SetProcess(bool b) => base.SetProcess(b);
    }
}