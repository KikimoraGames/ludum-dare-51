
using Godot;
using GodotOnReady.Attributes;

namespace Game
{
    public partial class CurrentPowerLabelIndicator : Label
    {
        [Export]
        private Color okColor;
        [Export]
        private Color badColor;


        [OnReadyGet]
        private AnimationPlayer animationPlayer;

        [OnReady]
        private void ConnectToPower()
        {
            PlayerPower.Instance.Connect(nameof(PlayerPower.on_power_changed), this, nameof(OnPowerChanged));
            OnPowerChanged(PlayerPower.Instance.CurrentPower);
        }

        [OnReady]
        private void PivotOffsetSetup()
        {
            Connect("resized", this, nameof(OnResized));
            OnResized();
        }

        private void OnResized()
        {
            RectPivotOffset = RectSize / 2;
        }

        private void OnPowerChanged(float p)
        {
            var percent = p / PlayerPower.MAXIMUM_POWER;
            Text = p.ToString("00.00");

            if (percent > 0.75f)
            {
                animationPlayer.Play("RESET");
                Visible = false;
                SelfModulate = okColor;
                return;
            }

            if (percent < 0.75)
            {
                Visible = true;
            }

            if (percent < 0.5)
            {
                SelfModulate = badColor;
                animationPlayer.Play("beat");
            }
        }
    }
}