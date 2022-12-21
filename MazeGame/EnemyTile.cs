using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MazeGame
{
    internal class EnemyTile : MovingTile
    {
        public bool isFreezed { get; private set; }
        public Action<EnemyTile> Shoot { get; set; }
        private Image FullImage;

        public List<ITile> Vision { get; private set; }

        Random rnd = new Random();

        public int Balls_Count { get; set; }

        public EnemyTile(float _height,int defaultSpeed, PointF _position, Image fullImage, float tileSize) 
            : base(_height, defaultSpeed,_position, null, tileSize)
        {
            Type = TileTypes.Enemy;
            isFreezed = false;
            FullImage = fullImage;
            Image = GetImage();
            Vision = new List<ITile>();
            Balls_Count = 2;
        }
        private Image GetImage()
        {
            Image img = null;
            Rectangle cloneRect = new Rectangle(64 * (int)Direction, 0, 64, 64);
            if (isFreezed)
                cloneRect.Y = 64;
            using (var bmp = new Bitmap(FullImage))
                img = bmp.Clone(cloneRect, bmp.PixelFormat);
            return img;
        }

        public void AddVisibleTile(ITile tile)
        {
            Point direction = GetMovingDirectionVector();
            if (tile as ICollidiableTile != null)
            {
                if ((Math.Abs(this.Position.Y - tile.Position.Y) < this.TileSize/2 && direction.X != 0 && direction.Y == 0 &&
                     (tile.Position.X - this.Position.X) / direction.X > 0))
                {
                    Vision.Add(tile);
                }
                else if (Math.Abs(this.Position.X - tile.Position.X) < this.TileSize / 2 && direction.Y != 0 && direction.X == 0 &&
                     (tile.Position.Y - this.Position.Y) / direction.Y > 0)
                {
                    Vision.Add(tile);
                }
            }
        }

        bool isShooting = false;
        public new void Move()
        {
            if (!isFreezed)
            {
                Point directionVector = GetMovingDirectionVector();
                bool needShoot = true;
                bool findCharacther = false;
                if (needShoot)
                {
                    foreach (var t in Vision)
                    {
                        if (t as CharachterTile != null)
                        {
                            SetSpeed(0);
                            Fire();
                            findCharacther = true;
                        }
                    }
                }
                if(findCharacther)
                {
                    foreach (var t in Vision)
                    {
                        Console.WriteLine($"{t} - {t.Position}");
                    }
                    Console.WriteLine();
                }
                if (!findCharacther) isShooting = false;

                if (!isShooting)
                {
                    directionVector = GetMovingDirectionVector();
                    PointF newPosition = new PointF(this.Position.X + CurrentSpeed * directionVector.X,
                        this.Position.Y + CurrentSpeed * directionVector.Y);

                    this.Position = newPosition;
                    SetSpeed(DefaultSpeed);
                }
            }
        }

        public void ClearVision()
        {
            Vision.Clear();
        }

        void Fire()
        {
            if (Balls_Count > 0)
            {
                isShooting = true;
                Balls_Count--;
                Shoot?.Invoke(this);
            }
        }

        public void Turn()
        {
            if (!isShooting && !isFreezed)
            {
                int current_direction = (int)Direction;
                current_direction = (current_direction + rnd.Next(8) + 1) % 8;
                SetDirection(current_direction);
            }
        }

        public void SetDirection(int direction)
        {
            switch (direction)
            {
                case 0:
                    SetDirection(Directions.S);
                    break;
                case 1:
                    SetDirection(Directions.SW);
                    break;
                case 2:
                    SetDirection(Directions.W);
                    break;
                case 3:
                    SetDirection(Directions.NW);
                    break;
                case 4:
                    SetDirection(Directions.N);
                    break;
                case 5:
                    SetDirection(Directions.NE);
                    break;
                case 6:
                    SetDirection(Directions.E);
                    break;
                case 7:
                    SetDirection(Directions.SE);
                    break;
                default:
                    break;
            }
        }

        public new void SetDirection(Directions direction)
        {
            Direction = direction;
            this.Image = GetImage();
        }

        public new void Collide(IMovingTile tile)
        {
            tile.SetSpeed(0);
        }

        public async void Freeze(int time)
        {
            int prevSpeed = CurrentSpeed;
            SetSpeed(0);
            isFreezed = true;
            Image = GetImage();
            await Task.Delay(time);
            isFreezed = false;
            Image = GetImage();
            SetSpeed(prevSpeed);
        }
    }
}
