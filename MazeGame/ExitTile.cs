using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MazeGame
{
    internal class ExitTile : ICollidiableTile
    {
        public TileTypes Type { get; private set; }

        public float Height { get; private set; }

        public PointF Position { get; private set; }

        public Image Image { get; private set; }

        public float TileSize { get; private set; }

        public Action<bool> ExitReached { get; set; }

        public ExitTile(float height, PointF poistion, Image image, float tileSize)
        {
            Type = TileTypes.Wall;
            Height = height;
            Position = poistion;
            Image = image;
            TileSize = tileSize;
        }

        public void ChangeHeight(int delta)
        {
            Height += delta;
        }

        public void MoveTile(PointF position)
        {
            Position = position;
        }

        public void Collide(IMovingTile tile)
        {
            tile.SetSpeed(0);

            if(tile as CharachterTile != null)
                ExitReached?.Invoke(true);
        }
    }
}
