using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MazeGame
{
    internal interface ICollidiableTile : ITile
    {
        void Collide(IMovingTile tile);
    }
}
