using Godot;

namespace Game
{
    public class GodotSingleton<T> : Node where T : GodotSingleton<T>
    {
        private static T instance;
        public static T Instance
        {
            get
            {
                if (instance == null)
                    return null;
                return instance;
            }
        }

        public override void _EnterTree()
        {
            base._EnterTree();
            instance = (T)this;
        }

        public override void _ExitTree()
        {
            base._ExitTree();
            if (instance == this)
                instance = null;
        }
    }
}