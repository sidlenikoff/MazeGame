using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MazeGame
{
    internal class GroundTile : ITile
    {
        public TileTypes Type { get; private set; }

        public float Height { get; private set; }

        public PointF Position { get; private set; }

        public Image Image { get; private set; }

        public float TileSize { get; set; }

        public GroundTile(float height, PointF poistion, Image image, float tileSize)
        {
            Type = TileTypes.Ground;
            Height = height;
            Position = poistion;
            Image = image;
            TileSize = tileSize;
        }
    }
}
