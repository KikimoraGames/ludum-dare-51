
using Godot;
using GodotOnReady.Attributes;

namespace Game
{
    public partial class CurrentHumansLabelIndicator : Label
    {

        [OnReady]
        private void ConnectTo()
        {
            PlayerBonemass.Instance.Connect(nameof(PlayerBonemass.on_humans_changed), this, nameof(OnChange));
            OnChange(PlayerBonemass.Instance.CurrentHumans);
        }

        private void OnChange(int b)
        {
            Text = $"{b.ToString()}/{PlayerBonemass.Instance.HumansNeededForBone}";
        }
    }
}