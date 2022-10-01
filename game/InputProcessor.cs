using Godot;
using GodotOnReady.Attributes;
using System;

namespace Game
{
    public partial class InputProcessor : GodotSingleton<InputProcessor>
    {
        public static string ACTION_JUMP = "action_jump";
        public static string ACTION_SLEEP = "action_sleep";

        [OnReadyGet]
        private Player Player { get; set; }

        [OnReadyGet]
        private CameraStack GameplayCameraStack { get; set; }

        [OnReadyGet]
        private CameraStack ZoomCameraStack { get; set; }

        public static bool IgnoreInput = false;

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



            if (e.IsActionPressed(ACTION_JUMP))
            {
                GetTree().SetInputAsHandled();
                if (Player.IsInAir && Player.CoyoteTime > 1.0f)
                    return;

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
        }

        public static Vector2 InputVelocity
        {
            get
            {
                if (IgnoreInput)
                    return Vector2.Zero;
                return new Vector2(Input.GetAxis("move_left", "move_right"), 0);
            }
        }
    }
}