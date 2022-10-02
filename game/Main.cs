using Godot;
using GodotOnReady.Attributes;
using System;

namespace Game
{
    public partial class Main : Node
    {

        [OnReadyGet("UI")]
        private CanvasLayer ui;

        [OnReady]
        private void SetUIVisible() => ui.Show();
    }
}