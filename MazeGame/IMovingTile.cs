using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MazeGame
{
    internal interface IMovingTile : ICollidiableTile
    {
        int CurrentSpeed { get; }
        int DefaultSpeed { get; }
        void Move();
        void SetSpeed(int Speed);
        bool CheckCollision(ICollidiableTile collidiableTile);
    }
}
