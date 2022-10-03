using System.Collections.Generic;
using Godot;

namespace Game.UI.ButtonPrompts
{
    internal interface IButtonPrompt
    {
        float FPS { get; }
        string Action { get; set; }
        bool Visible { get; set; }
        TileSet TileSet { get; }
        void SetProcess(bool b);
        string OnlyShowFor { get; set; }
        Color SelfModulate { get; set; }
    }
    internal class ButtonPrompt
    {
        private AtlasTexture texture;
        private IButtonPrompt prompt;

        private readonly List<int> tileIDs = new List<int>();
        private int currentListIndex = 0;
        private float currentFrameTime = 0;
        private bool isDisabled;

        public ButtonPrompt(IButtonPrompt prompt, AtlasTexture texture)
        {
            this.prompt = prompt;
            this.texture = texture;
        }

        public bool Disabled
        {
            get => isDisabled;
            set
            {
                isDisabled = value;
                if (!value)
                    RefreshIcon();
                else
                    SetDisabledIcon();
            }
        }

        public void OnReady()
        {
            texture.Atlas = prompt.TileSet.TileGetTexture((int)prompt.TileSet.GetTilesIds()[0]);
            RefreshIcon();
            if (!prompt.Visible)
                prompt.SetProcess(false);
        }

        public void OnProcess(float delta)
        {
            if (tileIDs.Count <= 1 || prompt.FPS == 0 || isDisabled)
                return;

            currentFrameTime += delta;
            if (currentFrameTime < 1.0f / prompt.FPS)
                return;

            currentFrameTime = 0;
            currentListIndex = (currentListIndex + 1) % tileIDs.Count;
            SetTexture(tileIDs[currentListIndex]);
        }

        public void OnInputDeviceChanged(string deviceType, int id)
        {
            if (isDisabled)
                SetDisabledIcon();
            else
                RefreshIcon();
        }

        private void RefreshIcon()
        {
            var actionKey = InputDeviceDetector.GetActionForDevice(prompt.Action);
            if (!string.IsNullOrEmpty(prompt.OnlyShowFor))
            {
                if (!actionKey.Contains(prompt.OnlyShowFor))
                    prompt.SelfModulate = new Color(prompt.SelfModulate, 0f);
                else
                    prompt.SelfModulate = new Color(prompt.SelfModulate, 1f);
            }
            tileIDs.Clear();
            var startVisible = prompt.Visible;
            foreach (var k in new string[] { $"{actionKey}_unpressed", $"{actionKey}_pressed" })
            {
                var tileID = prompt.TileSet.FindTileByName(k);
                if (tileID == -1)
                    continue;
                tileIDs.Add(tileID);
            }

            if (tileIDs.Count == 0)
            {
                GD.PrintErr($"No graphic found for {actionKey}");
                prompt.Visible = false;
                prompt.SetProcess(false);
                return;
            }

            prompt.Visible = startVisible;
            if (tileIDs.Count == 1)
            {
                SetTexture(tileIDs[0]);
                tileIDs.Clear();
                prompt.SetProcess(false);
                return;
            }

            currentListIndex = 0;
            currentFrameTime = 0;
            SetTexture(tileIDs[0]);
            prompt.SetProcess(startVisible);
        }

        private void SetDisabledIcon()
        {
            var actionKey = InputDeviceDetector.GetActionForDevice(prompt.Action);
            SetTexture($"{actionKey}_disabled");
            prompt.SetProcess(false);
        }

        private void SetTexture(string tileName)
        {
            var tileID = prompt.TileSet.FindTileByName(tileName);
            if (tileID == -1)
                return;
            SetTexture(tileID);
        }

        private void SetTexture(int tileID)
            => texture.Region = prompt.TileSet.TileGetRegion(tileID);
    }
}