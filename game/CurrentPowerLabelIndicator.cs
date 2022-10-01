
using Godot;
using GodotOnReady.Attributes;

namespace Game
{
    public partial class CurrentPowerLabelIndicator : Label
    {

        [OnReady]
        private void ConnectToPower()
        {
            PlayerPower.Instance.Connect(nameof(PlayerPower.on_power_changed), this, nameof(OnPowerChanged));
        }

        private void OnPowerChanged(float p)
        {
            Text = p.ToString("00.00");
        }
    }
}