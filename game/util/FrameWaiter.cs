using System.Threading;
using Godot;

namespace Game
{
    public class FrameWaiter : Node
    {
        [Signal]
        public delegate void completed(bool success);

        public enum PROCESS
        {
            IDLE = 0,
            PHYSICS = 1,
        }

        private PROCESS p;
        private int frameCount;
        private CancellationToken token;
        private PauseModeEnum pauseModeEnum;
        private long createdOnFrame;

        public FrameWaiter(int frameCount, PROCESS p = PROCESS.IDLE, PauseModeEnum pauseModeEnum = PauseModeEnum.Inherit, CancellationToken token = default) : base()
        {
            this.frameCount = frameCount;
            this.p = p;
            this.token = token;
            this.pauseModeEnum = pauseModeEnum;
        }

        public override void _Ready()
        {
            createdOnFrame = GetTree().GetFrame();
            PauseMode = pauseModeEnum;
            switch (p)
            {
                case PROCESS.IDLE:
                    SetProcess(true);
                    SetPhysicsProcess(false);
                    break;
                case PROCESS.PHYSICS:
                    SetProcess(false);
                    SetPhysicsProcess(true);
                    break;
            }
        }

        public override void _Process(float _) => Tick();


        public override void _PhysicsProcess(float _) => Tick();

        private void Tick()
        {
            if (GetTree().GetFrame() == createdOnFrame)
                return;
            if (token.IsCancellationRequested)
            {
                EmitSignal(nameof(completed), false);
                QueueFree();
                SetProcess(false);
                SetPhysicsProcess(false);
                return;
            }

            frameCount -= 1;
            if (frameCount <= 0)
            {
                EmitSignal(nameof(completed), true);
                QueueFree();
                SetProcess(false);
                SetPhysicsProcess(false);
            }
        }
    }
}