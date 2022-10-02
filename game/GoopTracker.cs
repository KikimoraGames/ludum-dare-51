using Godot;
using GodotOnReady.Attributes;
using System;

namespace Game
{
    public partial class GoopTracker : Node
    {
        [Export]
        private int goopVertical = 3;

        [OnReadyGet]
        private Node2D player;

        [OnReadyGet("RayCast2D")]
        private RayCast2D raycast;

        [OnReadyGet("GoopTileMap")]
        private TileMap goopTilemap;


        public override void _Process(float delta)
        {
            base._Process(delta);
            var isColliding = raycast.IsColliding();
            raycast.GlobalPosition = player.GlobalPosition;
            if (!isColliding)
                return;
            var p = raycast.GetCollisionPoint();
            var cell = new Vector2(p.x / goopTilemap.CellSize.x, p.y / goopTilemap.CellSize.y);
            for (int x = -2; x < 2; x++)
            {
                for (int y = 0; y < goopVertical; y++)
                {
                    goopTilemap.SetCell((int)cell.x + x, (int)cell.y + y, 0);
                }
            }
        }
    }
}