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
        [OnReadyGet]
        private Button goodbyeButton;


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
            transitionTween.TweenProperty(levelTransitionEffectMaterial, "shader_param/radius", -0.1f, 0.25f).From(1.1f).SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.Out);
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
            if (idx == -2)
            {
                Goodbye();
                return;
            }

            var nextLevel = idx + 1;
            if (nextLevel >= levels.Count)
            {
                LoadLevel(outroScene, -2);
                return;
            }

            LoadLevel(levels[nextLevel], nextLevel);
        }

        private void OnLevelFailed(int idx)
        {
            Engine.TimeScale = 1f;
            if (idx == -1)
                LoadLevel(introScene, idx);
            else
                LoadLevel(levels[idx], idx);
        }

        private async void Goodbye()
        {
            var transitionTween = CreateTween();
            transitionTween.TweenProperty(levelTransitionEffectMaterial, "shader_param/radius", -0.1f, 2f).From(1.2f).SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.Out);
            await ToSignal(transitionTween, "finished");

            goodbyeButton.Visible = true;
            transitionTween = CreateTween();
            transitionTween.TweenProperty(goodbyeButton, "modulate", new Color(1f, 1f, 1f, 1f), 2f).From(new Color(1f, 1f, 1f, 0f)).SetTrans(Tween.TransitionType.Linear).SetEase(Tween.EaseType.Out);
            await ToSignal(transitionTween, "finished");
            goodbyeButton.Disabled = false;
            goodbyeButton.GrabFocus();
        }

        private void _on_GoodbyeButton_pressed() => GetTree().Quit();
    }
}