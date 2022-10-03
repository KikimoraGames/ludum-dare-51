using Godot;

namespace Game
{
    public partial class PlayerBonemass : GodotSingleton<PlayerBonemass>
    {
        [Export]
        public int MaximumBones { get; private set; } = 100;
        [Export]
        public int HumansNeededForBone { get; private set; } = 1;

        [Signal]
        public delegate void on_bones_changed(int bones);
        [Signal]
        public delegate void on_humans_changed(int humans);


        public int CurrentBones { get; private set; } = 0;
        public int CurrentHumans { get; private set; } = 0;

        public void AddHuman(int h)
        {
            var newH = CurrentHumans + h;
            var b = newH / HumansNeededForBone;
            var rem = newH % HumansNeededForBone;
            CurrentBones = Mathf.Clamp(CurrentBones + b, 0, MaximumBones);
            CurrentHumans = rem;
            EmitSignal(nameof(on_bones_changed), CurrentBones);
            EmitSignal(nameof(on_humans_changed), CurrentHumans);
        }

        public void SpendBones(int b)
        {
            CurrentBones = Mathf.Clamp(CurrentBones - b, 0, MaximumBones);
            EmitSignal(nameof(on_bones_changed), CurrentBones);
        }
    }
}