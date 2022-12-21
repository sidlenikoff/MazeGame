using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MazeGame
{
    internal class FreezeBall : MovingTile
    {
        public FreezeBall(float _height, int defaultSpeed, PointF _position, Image image, float tileSize)
            : base(_height, defaultSpeed, _position, image, tileSize)
        {
            Type = TileTypes.Ball;
        }

        public void Collide(EnemyTile tile)
        {
            tile.Freeze(2000);
        }
    }
}
