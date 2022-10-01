using Godot;

namespace Game
{
    public partial class PlayerPower : GodotSingleton<PlayerPower>
    {
        public const float MAXIMUM_POWER = 10f;

        [Signal]
        public delegate void on_power_changed(float power);

        public float PowerDrainModifier { get; set; } = 1.0f;

        public float CurrentPower { get; private set; } = 10;

        public override void _Process(float delta)
        {
            base._Process(delta);
            CurrentPower = Mathf.Clamp(CurrentPower - delta * PowerDrainModifier, 0, MAXIMUM_POWER);
            EmitSignal(nameof(on_power_changed), CurrentPower);
            if (Mathf.IsZeroApprox(CurrentPower))
                SetProcess(false);
        }

        public void Add(float power)
        {
            CurrentPower = Mathf.Clamp(CurrentPower + power, 0, MAXIMUM_POWER);
            EmitSignal(nameof(on_power_changed), CurrentPower);
        }
    }
}