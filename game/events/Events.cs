using Godot;
using System;

namespace Game
{
    public class Events : GodotSingleton<Events>
    {
        [Signal]
        public delegate void request_sfx_rand_play(RandomSFXContainer sfx);
        [Signal]
        public delegate void request_sfx_play(AudioStream sfx, string bus);
        [Signal]
        public delegate void camera_impulse_sent(Vector2 velocity, float duration);
        [Signal]
        public delegate void human_eaten(Texture hat, Color moustacheColor, Color hatColor);

        public static void Emit(string signal, params object[] args)
        {
            Instance.EmitSignal(signal, args);
        }

        public static void Connect(string signal, Godot.Object target, string method, Godot.Object.ConnectFlags flags = 0)
        {
            Instance.Connect(signal, target, method, binds: null, flags: (uint)flags);
        }

        public static void PlaySFX(AudioStream sfx, string bus) => Emit(nameof(request_sfx_play), sfx, bus);
        public static void PlaySFX(RandomSFXContainer container) => Emit(nameof(request_sfx_rand_play), container);
        public static void ShakeCamera(Vector2 velocity, float duration) => Emit(nameof(camera_impulse_sent), velocity, duration);
        public static void HumanEaten(Texture hat, Color moustacheColor, Color hatColor) => Emit(nameof(human_eaten), hat, moustacheColor, hatColor);
    }
}
