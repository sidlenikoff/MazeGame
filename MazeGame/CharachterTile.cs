using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MazeGame
{
    internal class CharachterTile : MovingTile
    {
        private Image FullImage;

        public Action Shoot;

        public int Health { get; set; }

        public CharachterTile(float _height, int defaultSpeed, PointF _position, Image fullImage, float tileSize) 
            : base(_height,defaultSpeed,_position,null,tileSize)
        {
            Type = TileTypes.Character;
            Height = _height;
            Position = _position;
            FullImage = fullImage;
            TileSize = tileSize;
            Health = 100;
            SetDirection(Directions.S);
        }

        public new void Collide(IMovingTile tile) 
        {
            tile.SetSpeed(0);
        }
        bool isTakingDamage = false;
        public async void GetDamage(int damage)
        {
            if (Health >= damage)
            {
                isTakingDamage = true;
                Image = GetImage();
                Health -= damage;
                await Task.Delay(2);
                isTakingDamage = false;
                Image = GetImage();
            }
        }

        public void Fire()
        {
            Shoot?.Invoke();
        }

        public new void SetDirection(Directions direction)
        {
            Direction = direction;
            this.Image = GetImage();
        }

        private Image GetImage()
        {
            Image img = null;
            Rectangle cloneRect = new Rectangle(64* (int)Direction, 0, 64, 64);
            if (isTakingDamage)
                cloneRect.Y = 64;
            using (var bmp = new Bitmap(FullImage))
                img = bmp.Clone(cloneRect, bmp.PixelFormat);
            return img;
        }
    }
}
