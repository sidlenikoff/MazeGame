using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MazeGame
{
    internal class MovingTile : IMovingTile
    {
        public enum Directions
        { S, SW, W, NW, N, NE, E, SE }

        public Directions Direction { get; protected set; }

        public TileTypes Type { get; protected set; }

        public float Height { get; protected set; }

        public PointF Position { get; protected set; }

        public Image Image { get; protected set; }

        public int CurrentSpeed { get; protected set; }

        public float TileSize { get; protected set; }

        public int DefaultSpeed { get; protected set; }

        public MovingTile(float _height, int defaultSpeed,PointF _position, Image image, float tileSize)
        {
            Height = _height;
            Position = _position;
            Image = image;
            TileSize = tileSize;
            DefaultSpeed = defaultSpeed;
            SetSpeed(defaultSpeed);
            SetDirection(Directions.S);
        }

        public void SetDirection(Directions direction)
        {
            Direction = direction;
        }

        public void MoveTo(PointF point)
        {
            this.Position = point;
        }

        public bool CheckCollision(ICollidiableTile collidiableTile)
        {
            Point movingDirection = GetMovingDirectionVector();
            PointF pointA = new PointF(this.Position.X + CurrentSpeed * movingDirection.X,
                this.Position.Y + CurrentSpeed * movingDirection.Y);
            PointF pointB = collidiableTile.Position;
            PointF ldc = new PointF(pointB.X + collidiableTile.TileSize, pointB.Y + collidiableTile.TileSize);
            bool isWalkable = false;
            if (collidiableTile as EnemyTile != null)
                isWalkable = (collidiableTile as EnemyTile).isFreezed;
            return  ((pointA.X <= ldc.X || pointA.X + this.TileSize >= pointB.X)
                    && (Math.Abs(pointA.Y - pointB.Y) < this.TileSize - 8)
                    &&
                    ((pointA.Y + this.TileSize >= pointB.Y || pointA.Y >= ldc.Y))
                    && Math.Abs(pointA.X - pointB.X) < this.TileSize - 8) && !isWalkable;
        }
        public void Collide(IMovingTile tile) {}

        public void SetSpeed(int Speed)
        {
            CurrentSpeed = Speed;
        }

        public void Move()
        {
            Point directionVector = GetMovingDirectionVector();
            PointF newPosition = new PointF(this.Position.X + CurrentSpeed * directionVector.X,
                this.Position.Y + CurrentSpeed * directionVector.Y);
            this.Position = newPosition;
            SetSpeed(DefaultSpeed);
        }

        protected Point GetMovingDirectionVector()
        {
            Point directionVector = new Point(0, 0);
            switch (Direction)
            {
                case Directions.S:
                    directionVector.X = 1;
                    directionVector.Y = 1;
                    break;
                case Directions.SW:
                    directionVector.Y = 1;
                    break;
                case Directions.W:
                    directionVector.X = -1;
                    directionVector.Y = 1;
                    break;
                case Directions.NW:
                    directionVector.X = -1;
                    break;
                case Directions.N:
                    directionVector.X = -1;
                    directionVector.Y = -1;
                    break;
                case Directions.NE:
                    directionVector.Y = -1;
                    break;
                case Directions.E:
                    directionVector.X = 1;
                    directionVector.Y = -1;
                    break;
                case Directions.SE:
                    directionVector.X = 1;
                    break;
                default:
                    break;
            }
            return directionVector;
        }
    }
}
