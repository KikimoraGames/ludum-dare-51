using Godot;
using GodotOnReady.Attributes;
using System;

namespace Game
{
    public partial class HumanEatenViewController : Node
    {
        private readonly string[] surnames = new string[] {
            "Smith",
            "Johnson",
            "Williams",
            "Brown",
            "Jones",
            "Miller",
            "Davis",
            "Garcia",
            "Rodriguez",
            "Wilson",
            "Martinez",
            "Anderson",
            "Taylor",
            "Thomas",
            "Hernandez",
            "Moore",
            "Martin",
            "Jackson",
            "Thompson",
            "White",
            "Lopez",
            "Lee",
            "Gonzalez",
            "Harris",
            "Clark",
            "Lewis",
            "Robinson",
            "Walker",
            "Perez",
            "Hall",
            "Young",
            "Allen",
            "Sanchez",
            "Wright",
            "King",
            "Scott",
            "Green",
            "Baker",
            "Adams",
            "Nelson",
            "Hill",
            "Ramirez",
            "Campbell",
            "Mitchell",
            "Roberts",
            "Carter",
            "Phillips",
            "Evans",
            "Turner",
            "Torres",
            "Parker",
            "Collins",
            "Edwards",
            "Stewart",
            "Flores",
            "Morris",
            "Nguyen",
            "Murphy",
            "Rivera",
            "Cook",
            "Rogers",
            "Morgan",
            "Peterson",
            "Cooper",
            "Reed",
            "Bailey",
            "Bell",
            "Gomez",
            "Kelly",
            "Howard",
            "Ward",
            "Cox",
            "Diaz",
            "Richardson",
            "Wood",
            "Watson",
            "Brooks",
            "Bennett",
            "Gray",
            "James",
            "Reyes",
            "Cruz",
            "Hughes",
            "Price",
            "Myers",
            "Long",
            "Foster",
            "Sanders",
            "Ross",
            "Morales",
            "Powell",
            "Sullivan",
            "Russell",
            "Ortiz",
            "Jenkins",
            "Gutierrez",
            "Perry",
            "Butler",
            "Barnes",
            "Fisher",
            "Henderson",
            "Orioli",
            "Lundgren",
            "Modig",
            "P. Aratchi"
         };

        [Export]
        private float secondsBeforeDisapper = 2f;
        [OnReadyGet]
        private Sprite hatSprite;
        [OnReadyGet]
        private Sprite moustacheSprite;
        [OnReadyGet]
        private Label nameLabel;
        [OnReadyGet]
        private TextureRect viewer;
        [OnReadyGet]
        private Viewport viewport;
        [OnReadyGet]
        private Timer dissapearTimer;
        [OnReadyGet]
        private Control mainControl;
        private SceneTreeTween currentDisapperTween;

        [OnReady]
        private void ConnectTimer()
        {
            if (secondsBeforeDisapper > 0)
                dissapearTimer.Connect("timeout", this, nameof(OnTimeout));

            mainControl.Modulate = new Color(mainControl.Modulate, 0f);
        }

        [OnReady]
        private void ConnectListener()
        {
            var vpt = new ViewportTexture();
            viewer.Texture = viewport.GetTexture();
            Events.Connect(nameof(Events.human_eaten), this, nameof(OnHumanEaten));
        }

        private void OnHumanEaten(Texture hat, Color moustacheColor, Color hatColor)
        {
            if (hat == null)
                hatSprite.Visible = false;
            else
            {
                hatSprite.Visible = true;
                hatSprite.Texture = hat;
                hatSprite.SelfModulate = hatColor;
            }

            moustacheSprite.SelfModulate = moustacheColor;
            string title = "";
            switch (GD.Randi() % 6)
            {
                case 0:
                    title = "Mr. ";
                    break;
                case 1:
                    title = "Ms. ";
                    break;
                case 2:
                    title = "Mrs. ";
                    break;
                case 3:
                    title = "Dr. ";
                    break;
                case 4:
                    title = "Sir ";
                    break;
                case 5:
                    title = "Lady ";
                    break;
            }
            nameLabel.Text = title + surnames[(int)(GD.Randi() % surnames.Length)];

            if (secondsBeforeDisapper > 0)
                dissapearTimer.Start(secondsBeforeDisapper);
            if (currentDisapperTween != null && currentDisapperTween.IsRunning())
                currentDisapperTween.Kill();
            mainControl.Modulate = new Color(mainControl.Modulate, 1f);
            var tween = CreateTween();
            tween.TweenProperty(viewer, "rect_scale", Vector2.One * 1.25f, 0.15f).From(Vector2.One).SetTrans(Tween.TransitionType.Back).SetEase(Tween.EaseType.Out);
            tween.TweenProperty(viewer, "rect_scale", Vector2.One, 0.1f).SetTrans(Tween.TransitionType.Linear).SetDelay(0.1f);
        }

        private void OnTimeout()
        {
            if (currentDisapperTween != null && currentDisapperTween.IsRunning())
                return;

            currentDisapperTween = CreateTween();
            currentDisapperTween.TweenProperty(mainControl, "modulate:a", 0f, 1f).SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.In);
        }
    }
}