using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MazeGame
{
    enum TileTypes { Ground, Character, Wall, WaterWall, Ball, Enemy }
    internal interface ITile
    {
        TileTypes Type { get; }
        float Height { get; }
        PointF Position { get; }
        Image Image { get; }

        float TileSize { get; }
    }
}
