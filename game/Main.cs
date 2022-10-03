using Godot;
using GodotOnReady.Attributes;
using System;

namespace Game
{
    public partial class Main : Node
    {
        [Export]
        private PackedScene introScene;
        [Export]
        private PackedScene outroScene;
        [Export]
        private Godot.Collections.Array<PackedScene> levels;

        [OnReadyGet]
        private Control levelTransitionEffect;
        [OnReadyGet]
        private Node currentLevelHolder;


        private Level currentLoadedLevel;

        private Material levelTransitionEffectMaterial;

        [OnReady]
        private void SaveMaterial()
        {
            levelTransitionEffectMaterial = levelTransitionEffect.Material;
        }

        [OnReady]
        private void LoadFirstLevel()
        {
            LoadLevel(levels[0], 0);
        }

        private async void LoadLevel(PackedScene level, int idx)
        {
            if (currentLoadedLevel != null)
                currentLoadedLevel.Unload();

            var l = level.Instance<Level>();
            l.Connect(nameof(Level.level_complete), this, nameof(OnLevelComplete), new Godot.Collections.Array { idx });

            var transitionTween = CreateTween();
            transitionTween.TweenProperty(levelTransitionEffectMaterial, "shader_param/radius", -0.1f, 0.25f).From(1f).SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.Out);
            await ToSignal(transitionTween, "finished");

            if (currentLoadedLevel != null)
            {
                currentLoadedLevel.QueueFree();
                await ToSignal(currentLoadedLevel, "tree_exited");
            }
            var awaiter = ToSignal(l, "ready");
            currentLevelHolder.AddChild(l);
            await awaiter;
            transitionTween = CreateTween();
            transitionTween.TweenProperty(levelTransitionEffectMaterial, "shader_param/radius", 1f, 0.15f).SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.Out);
            currentLoadedLevel = l;
            await ToSignal(transitionTween, "finished");
            await this.WaitSeconds(0.5f);
            currentLoadedLevel.Begin();
        }

        private void OnLevelComplete(int idx)
        {
            LoadLevel(levels[(idx + 1) % levels.Count], (idx + 1) % levels.Count);
        }

    }
}