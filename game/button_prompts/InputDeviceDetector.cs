using Godot;
using System;

namespace Game.UI
{
    public class InputDeviceDetector : GodotSingleton<InputDeviceDetector>
    {
        public const string DEVICE_TYPE_GAMEPAD_GENERIC = "generic";
        public const string DEVICE_TYPE_GAMEPAD_XBOX = "xbox";
        public const string DEVICE_TYPE_GAMEPAD_PS = "ps";
        public const string DEVICE_TYPE_KEYBOARD_MOUSE = "kbm";

        [Signal]
        public delegate void input_device_changed(string device_type, int device_id);

        public static int CurrentDeviceID { get; private set; }
        public static string CurrentDeviceType { get; private set; }

        public override void _Ready()
        {
            base._Ready();
            PauseMode = PauseModeEnum.Process;
            CurrentDeviceType = DEVICE_TYPE_KEYBOARD_MOUSE;
        }

        public override void _Input(InputEvent e)
        {
            var dt = GetDeviceType(e);
            if (string.IsNullOrEmpty(dt))
                return;
            if (CurrentDeviceType == dt && CurrentDeviceID == e.Device)
                return;

            CurrentDeviceID = e.Device;
            CurrentDeviceType = dt;
            EmitSignal(nameof(input_device_changed), CurrentDeviceType, CurrentDeviceID);
        }

        public static string GetActionForDevice(string action)
        {
            var al = InputMap.GetActionList(action);
            foreach (InputEvent e in al)
            {
                switch (CurrentDeviceType)
                {
                    case DEVICE_TYPE_KEYBOARD_MOUSE:
                        if (e is InputEventKey ek)
                            return $"{CurrentDeviceType}_key_{OS.GetScancodeString(ek.PhysicalScancode).ToLower()}"; // TODO: https://github.com/godotengine/godot/pull/56015    

                        if (e is InputEventMouseButton eb)
                            return $"{CurrentDeviceType}_mouse_{((ButtonList)eb.ButtonIndex).ToString().ToLower()}";

                        break;
                    default:
                        if (e is InputEventJoypadButton ej)
                            return $"{CurrentDeviceType}_{JoyButtonToString(ej.ButtonIndex)}";


                        if (e is InputEventJoypadMotion em)
                            return $"{CurrentDeviceType}_{JoyAxisToString(em.Axis)}";

                        break;


                }
            }
#if DEBUG
            GD.PushError($"Unknown input event {action} {al} for {CurrentDeviceType};");
#endif
            return null;
        }



        public static void Connect(Godot.Object target, string method)
        {
            if (Instance.IsConnected(nameof(input_device_changed), target, method))
                return;
            Instance.Connect(nameof(input_device_changed), target, method, binds: null, flags: 0);
        }

        public static void Disconnect(Godot.Object target, string method)
        {
            if (Instance.IsConnected(nameof(input_device_changed), target, method))
                Instance.Disconnect(nameof(input_device_changed), target, method);
        }

        private static string GetDeviceType(InputEvent e)
        {
            if (e is InputEventMouseMotion)
                return null;

            if (e is InputEventMouseButton || e is InputEventKey)
                return DEVICE_TYPE_KEYBOARD_MOUSE;

            if (!IsJoypadEventValid(e))
                return null;

            return JoyToType(e);
        }

        private static bool IsJoypadEventValid(InputEvent e)
        {
            if (e is InputEventJoypadMotion m && Mathf.Abs(m.AxisValue) < 0.05)
                return false;

            return true;
        }

        public static string JoyToType(InputEvent e)
        {
            switch (Input.GetJoyName(e.Device))
            {
                case "PS4 Controller":
                    return DEVICE_TYPE_GAMEPAD_PS;
                case "XInput Gamepad":
                    return DEVICE_TYPE_GAMEPAD_XBOX;
                default:
                    return DEVICE_TYPE_GAMEPAD_GENERIC;
            }
        }

        public static string JoyButtonToString(int idx)
        {
            switch ((JoystickList)idx)
            {
                case JoystickList.XboxA:
                    return "face_down";
                case JoystickList.XboxB:
                    return "face_right";
                case JoystickList.XboxY:
                    return "face_up";
                case JoystickList.XboxX:
                    return "face_left";
                case JoystickList.DpadDown:
                    return "dpad_down";
                case JoystickList.DpadRight:
                    return "dpad_right";
                case JoystickList.DpadUp:
                    return "dpad_up";
                case JoystickList.DpadLeft:
                    return "dpad_left";
                case JoystickList.Start:
                    return "start";
                case JoystickList.Select:
                    return "select";
                case JoystickList.L:
                    return "shoulder_left";
                case JoystickList.R:
                    return "shoulder_right";
                case JoystickList.L2:
                    return "trigger_left";
                case JoystickList.R2:
                    return "trigger_right";
                case JoystickList.L3:
                    return "stick_click_left";
                case JoystickList.R3:
                    return "stick_click_right";
                default:
                    throw new InvalidOperationException($"Unknown joystick button {idx} -> {((JoystickList)idx).ToString()}");
            }
        }

        private static string JoyAxisToString(int axis)
        {
            switch ((JoystickList)axis)
            {
                case JoystickList.Axis0:
                    return "analog_left_horizontal";
                case JoystickList.Axis1:
                    return "analog_right_vertical";
                case JoystickList.Axis2:
                    return "analog_right_horizontal";
                case JoystickList.Axis3:
                    return "analog_right_vertical";
                default:
                    throw new InvalidOperationException($"Unknown joystick axis {axis} -> {((JoystickList)axis).ToString()}");
            }
        }
    }
}