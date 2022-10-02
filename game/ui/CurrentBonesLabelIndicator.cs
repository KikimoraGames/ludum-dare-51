
using Godot;
using GodotOnReady.Attributes;

namespace Game
{
    public partial class CurrentBonesLabelIndicator : Label
    {

        [OnReady]
        private void ConnectTo()
        {
            PlayerBonemass.Instance.Connect(nameof(PlayerBonemass.on_bones_changed), this, nameof(OnChange));
            OnChange(PlayerBonemass.Instance.CurrentBones);
        }

        private void OnChange(int b)
        {
            Text = b.ToString();
        }
    }
}