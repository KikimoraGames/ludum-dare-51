using Godot;

namespace Game
{
    public partial class PlayerPower : GodotSingleton<PlayerPower>
    {
        public const float MAXIMUM_POWER = 10f;

        [Signal]
        public delegate void on_power_changed(float power);

        public float PowerDrainModifier { get; set; } = 1.0f;

        private float currentPower = 10;

        public override void _Process(float delta)
        {
            base._Process(delta);
            currentPower = Mathf.Clamp(currentPower - delta * PowerDrainModifier, 0, MAXIMUM_POWER);
            EmitSignal(nameof(on_power_changed), currentPower);
            if (Mathf.IsZeroApprox(currentPower))
                SetProcess(false);
        }

        public void Add(float power)
        {
            currentPower = Mathf.Clamp(currentPower + power, 0, MAXIMUM_POWER);
            EmitSignal(nameof(on_power_changed), currentPower);
        }
    }
}