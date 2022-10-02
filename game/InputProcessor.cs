using Godot;
using GodotOnReady.Attributes;
using System;

namespace Game
{
    public partial class InputProcessor : GodotSingleton<InputProcessor>
    {
        public static string ACTION_JUMP = "action_jump";
        public static string ACTION_SLEEP = "action_sleep";
        public static string ACTION_BLOCK = "action_boneblock";
        public static string ACTION_BARK = "action_bark";
        [OnReadyGet]
        private Player Player { get; set; }

        [OnReadyGet]
        private CameraStack GameplayCameraStack { get; set; }

        [OnReadyGet]
        private CameraStack ZoomCameraStack { get; set; }

        public bool IgnoreInput = false;

        public Vector2 LastNonZeroDirection { get; private set; } = Vector2.Right;

        [OnReady]
        private void SetupStackTargets()
        {
            GameplayCameraStack.SetTarget(Player);
            ZoomCameraStack.SetTarget(Player);
        }

        public override void _UnhandledInput(InputEvent e)
        {
            base._UnhandledInput(e);
            if (IgnoreInput)
                return;

            if (e.IsEcho())
                return;

            if (e.IsActionPressed(ACTION_BARK))
            {
                GetTree().SetInputAsHandled();
                Player.Bark();
                return;
            }

            if (e.IsActionPressed(ACTION_JUMP))
            {
                GetTree().SetInputAsHandled();
                if (Player.IsPlacingBlock)
                    return;

                if (Player.IsInAir && Player.CoyoteTime > 1.0f)
                {
                    if (Player.HasDash && Player.HasPowerForDash)
                        Player.DashPressed();
                    return;
                }

                Player.JumpPressed();
                return;
            }

            if (e.IsActionReleased(ACTION_JUMP))
            {
                GetTree().SetInputAsHandled();
                Player.JumpReleased();
                return;
            }

            if (e.IsActionPressed(ACTION_SLEEP))
            {
                GetTree().SetInputAsHandled();
                if (Player.IsInAir)
                    return;
                if (Player.IsPlacingBlock)
                    return;

                if (Player.IsSleeping)
                {
                    GameplayCameraStack.Priority = 11;
                    Player.SleepReleased();
                }
                else
                {
                    Player.SleepPressed();
                    GameplayCameraStack.Priority = 0;
                }
                return;
            }


            if (e.IsActionPressed(ACTION_BLOCK))
            {
                GetTree().SetInputAsHandled();
                if (Player.IsInAir)
                    return;
                if (Player.IsSleeping)
                    return;
                if (Player.IsPlacingBlock)
                    return;
                if (!Player.HasBonesForBlock)
                    return;

                Player.BlockPressed();
                return;
            }
        }

        public Vector2 InputVelocity
        {
            get
            {
                if (IgnoreInput)
                    return Vector2.Zero;
                if (Player.IsSleeping)
                    return Vector2.Zero;
                var v = new Vector2(Input.GetAxis("move_left", "move_right"), Input.GetAxis("look_up", "look_down"));
                if (v.LengthSquared() > 0.1f)
                    LastNonZeroDirection = v;
                return v;
            }
        }
    }
}