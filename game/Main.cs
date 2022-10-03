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
            LoadLevel(introScene, -1);
        }

        private async void LoadLevel(PackedScene level, int idx)
        {
            if (currentLoadedLevel != null)
                currentLoadedLevel.Unload();

            var l = level.Instance<Level>();
            l.Connect(nameof(Level.level_completed), this, nameof(OnLevelComplete), new Godot.Collections.Array { idx });
            l.Connect(nameof(Level.level_failed), this, nameof(OnLevelFailed), new Godot.Collections.Array { idx });


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
            await this.WaitSeconds(1f);
            transitionTween = CreateTween();
            transitionTween.TweenProperty(levelTransitionEffectMaterial, "shader_param/radius", 1.1f, 0.15f).SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.Out);
            currentLoadedLevel = l;
            await ToSignal(transitionTween, "finished");


            currentLoadedLevel.Begin();
        }

        private void OnLevelComplete(int idx)
        {
            LoadLevel(levels[(idx + 1) % levels.Count], (idx + 1) % levels.Count);
        }

        private void OnLevelFailed(int idx)
        {
            Engine.TimeScale = 1f;
            if (idx == -1)
                LoadLevel(introScene, idx);
            else
                LoadLevel(levels[idx], idx);
        }
    }
}