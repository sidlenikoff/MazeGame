using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MazeGame
{
    public partial class Form1 : Form
    {
        List<ITile> tiles;
        int mapSize = 16;
        float tileSize = 42;

        int maxSpeed = 7;
        CharachterTile charachter;

        Image water_cube;
        Image cube;
        Image ground;
        Image main_character;
        Image bomb;
        Image freeze_ball;
        Image enemy;
        Image fire_ball;
        Image bomb_explosion;
        Image balloons;

        Bitmap Level;

        ExitTile mazeExit;

        CancellationTokenSource tokenSource;
        Task timerTask;
        Task enemyTask;

        TimeSpan timeLeft;
        TimeSpan DefaultTime;

        public Form1()
        {
            InitializeComponent();
            this.KeyPreview = true;
            InitialiszeImages();
            InitializeMap();

            BallTasks = new List<Task>();
            CharactherBallsTasks = new List<Task>();

            charachter = new CharachterTile(tileSize, maxSpeed,
                new PointF(MazeGenerator.Start.X * tileSize, MazeGenerator.Start.Y * tileSize),main_character, tileSize);
            charachter.Shoot += characther_shoot;

            tiles.Add(charachter);

            mainPanel.Invalidate();
            mainPanel.Focus();
            tokenSource = new CancellationTokenSource();
            foreach (var t in tiles)
            {
                if (t as EnemyTile != null)
                {
                    enemyTask = enemyLive(t as EnemyTile, tokenSource.Token);
                    (t as EnemyTile).Shoot += enemy_Shoot;
                }
            }

            DefaultTime = TimeSpan.FromSeconds(30);
            timeLeft = DefaultTime;
            timerTask = TimerTick(tokenSource.Token);
        }

        void InitialiszeImages()
        {
            water_cube = Image.FromFile(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                   "assets", "water_floor.png"));
            cube = Image.FromFile(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                               "assets", "cube.png"));
            ground = Image.FromFile(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                               "assets", "ground_stroke.png"));
            main_character = Image.FromFile(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                               "assets", "Main_character_stand.png"));
            bomb = Image.FromFile(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                               "assets", "bomb.png"));
            freeze_ball = Image.FromFile(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                               "assets", "freeze_ball.png"));
            fire_ball = Image.FromFile(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                               "assets", "fire_ball.png"));
            enemy = Image.FromFile(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                               "assets", "Enemy_character_stand.png"));
            bomb_explosion = Image.FromFile(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                               "assets", "bomb_explosion.gif"));
            balloons = Image.FromFile(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                               "assets", "balloons.gif"));
        }

        private void InitializeMap()
        {
            int[,] map16x16 = MazeGenerator.Generate(mapSize, mapSize);
            if (tiles == null) tiles = new List<ITile>();
            tiles.Clear();
            for (int i = 0; i < mapSize; i++)
            {
                for (int j = 0; j < mapSize; j++)
                {
                    if (map16x16[i, j] == 1)
                    {
                        tiles.Add(new WallTile(tileSize, new PointF(tileSize * i, tileSize * j), cube, tileSize));
                    }
                    else if (map16x16[i, j] == 0)
                    {
                        tiles.Add(new GroundTile(0, new PointF(tileSize * i, tileSize * j), ground, tileSize));
                    }
                    else if (map16x16[i, j] == 2)
                    {
                        tiles.Add(new GroundTile(0, new PointF(tileSize * i, tileSize * j), ground, tileSize));
                        tiles.Add(new WaterTile(0, new PointF(tileSize * i, tileSize * j), water_cube, tileSize));
                    }
                    else if (map16x16[i, j] == 3)
                    {
                        tiles.Add(new GroundTile(0, new PointF(tileSize * i, tileSize * j), ground, tileSize));
                        tiles.Add(new EnemyTile(tileSize, maxSpeed, new PointF(tileSize * i, tileSize * j), enemy, tileSize));
                    }

                }
            }
            tiles.Add(new WallTile(tileSize, new PointF((MazeGenerator.Start.X + 1) * tileSize, (MazeGenerator.Start.Y) * tileSize), cube, tileSize));
            mazeExit = new ExitTile(tileSize, new PointF((MazeGenerator.Exit.X - 1) * tileSize, (MazeGenerator.Exit.Y) * tileSize), bomb, tileSize);
            mazeExit.ExitReached += OnEndGame;
            tiles.Add(mazeExit);
        }

        PointF CartesianToIsometric(PointF cartPt)
        {
            return new PointF(cartPt.X - cartPt.Y, (cartPt.X + cartPt.Y) / 2);
        }

        #region Enemy Logic

        List<Task> BallTasks;
        async Task ProvideBall(EnemyTile enemy, CancellationToken token)
        {
            var ball = new FireBall(tileSize, maxSpeed * 2, enemy.Position, fire_ball, tileSize);
            tiles.Add(ball);
            ball.SetDirection(enemy.Direction);
            bool isMoving = true;
            while (true)
            {
                if (token.IsCancellationRequested)
                    return;
                await Task.Delay(30);
                foreach (var t in tiles)
                {
                    if (t as EnemyTile == null || t as EnemyTile != enemy)
                    {
                        if ((t as ICollidiableTile) != null && ball.CheckCollision(t as ICollidiableTile))
                        {
                            (t as ICollidiableTile).Collide(ball);
                            if (t.Type == TileTypes.Wall || t.Type == TileTypes.Character)
                            {
                                if (t as CharachterTile != null)
                                {
                                    ball.Collide(t as CharachterTile);
                                    healthLabel.Text = $"HP: {(t as CharachterTile).Health}";
                                }
                                tiles.Remove(ball);
                                isMoving = false;
                                break;
                            }
                        }
                    }
                }
                if (isMoving)
                {
                    ball.Move();
                    mainPanel.Invalidate();
                }
                else
                {
                    enemy.Balls_Count++;
                    if (charachter.Health <= 0)
                        OnEndGame(false);
                    break;
                }
            }
        }

        private void enemy_Shoot(EnemyTile enemy)
        {
            BallTasks.Add(ProvideBall(enemy, tokenSource.Token));
        }

        private async Task enemyLive(EnemyTile enemy, CancellationToken token)
        {
            while (true)
            {
                if (token.IsCancellationRequested)
                    return;


                await Task.Delay(200);

                enemy.ClearVision();
                foreach (var t in tiles)
                {
                    enemy.AddVisibleTile(t);
                    if ((t as EnemyTile) == null || (t as EnemyTile) != enemy)
                    {
                        if ((t as ICollidiableTile) != null && enemy.CheckCollision(t as ICollidiableTile))
                        {
                            if (t as EnemyTile != null)
                                (t as EnemyTile).Collide(enemy);
                            (t as ICollidiableTile).Collide(enemy);
                        }
                    }
                }
                if (enemy.CurrentSpeed == 0)
                {
                    enemy.Turn();
                }
                enemy.Move();
            }
        }

        #endregion
        
        #region Game Logic

        bool isCancelCalled = false;

        private void AnimateWin()
        {
            if(pictureBox1.Image != null) pictureBox1.Image.Dispose();
            pictureBox1.Dock = DockStyle.Fill;
            pictureBox1.Image = balloons;
            pictureBox1.Visible = true;
            timerText.Visible = false;
            MessageBox.Show("Вы победели", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            pictureBox1.Image = null;
            pictureBox1.Visible = false;
            timerText.Visible = true;
        }

        private void AnimateLoss()
        {
            timerText.Visible = false;
            pictureBox1.Dock = DockStyle.None;
            pictureBox1.Height = 300;
            pictureBox1.Width = 300;
            pictureBox1.Location = new Point(timerText.Location.X - pictureBox1.Width / 2,
              timerText.Location.Y - pictureBox1.Height / 2);

            pictureBox1.Image = bomb_explosion;
            pictureBox1.Visible = true;
            tiles.Remove(mazeExit);
            MessageBox.Show("Вы проиграли", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);

            pictureBox1.Image = null;
            pictureBox1.Visible = false;
            pictureBox1.Dock = DockStyle.Fill;
            timerText.Visible = true;
        }

        private async void OnEndGame(bool isSuccess)
        {
            tokenSource.Cancel();
            if (timerTask != null) await timerTask;
            if (enemyTask != null) await enemyTask;
            while(BallTasks.Count > 0)
            {
                var t = Task.WaitAny(BallTasks.ToArray());
                BallTasks.RemoveAt(t);
            }
            while (CharactherBallsTasks.Count > 0)
            {
                var t = Task.WaitAny(CharactherBallsTasks.ToArray());
                CharactherBallsTasks.RemoveAt(t);
            }

            if (isSuccess)
                AnimateWin();
            else
                AnimateLoss();

            ResetGame();
        }

        private void ResetGame()
        {
            InitializeMap();

            arrowaState[0] = false;
            arrowaState[1] = false;
            arrowaState[2] = false;
            arrowaState[3] = false;

            charachter.MoveTo(new PointF(MazeGenerator.Start.X * tileSize, MazeGenerator.Start.Y * tileSize));
            charachter.Health = 100;
            healthLabel.Text = $"HP: {charachter.Health}";
            tiles.Add(charachter);

            timeLeft = DefaultTime;
            if (tokenSource != null) tokenSource.Dispose();
            tokenSource = new CancellationTokenSource();

            foreach (var t in tiles)
            {
                if (t as EnemyTile != null)
                {
                    enemyTask = enemyLive(t as EnemyTile, tokenSource.Token);
                    (t as EnemyTile).Shoot += enemy_Shoot;
                }
            }

            timerTask = TimerTick(tokenSource.Token);
            isCancelCalled = false;
        }

        private void OnTimerEnd()
        {
            OnEndGame(false);
            
        }

        private async Task TimerTick(CancellationToken token)
        {
            int delta = 2;
            int delay = 50;
            while (true)
            {
                if (token.IsCancellationRequested)
                    return;

                await Task.Delay(delay);

                timeLeft = timeLeft.Subtract(TimeSpan.FromMilliseconds(delay));
                timerText.Text = String.Format("{0}:{1:00}", timeLeft.Minutes, timeLeft.Seconds);

                if (timeLeft <= TimeSpan.Zero)
                {
                    OnTimerEnd();
                    return;
                }

                mazeExit.ChangeHeight(delta);
                mainPanel.Invalidate();

                if (mazeExit.Height >= tileSize * 2) delta = -2;
                else if (mazeExit.Height <= tileSize * 1.5) delta = 2;
            }
        }


        #endregion
      
        #region Character Actions

        void ChangeCharacterDirection()
        {
            if (arrowaState[2])
            {
                charachter.SetDirection(CharachterTile.Directions.NW);
            }
            if (arrowaState[3])
            {
                charachter.SetDirection(CharachterTile.Directions.SE);
            }
            if (arrowaState[0])
            {
                if (arrowaState[2]) 
                    charachter.SetDirection(CharachterTile.Directions.N);
                else if (arrowaState[3]) 
                    charachter.SetDirection(CharachterTile.Directions.E);
                else 
                    charachter.SetDirection(CharachterTile.Directions.NE);
            }
            if (arrowaState[1])
            {
                if (arrowaState[2]) 
                    charachter.SetDirection(CharachterTile.Directions.W);
                else if (arrowaState[3]) 
                    charachter.SetDirection(CharachterTile.Directions.S);
                else 
                    charachter.SetDirection(CharachterTile.Directions.SW);
            }
        }
        void MovePlayer()
        {
            if (!isCancelCalled)
            {
                foreach (var t in tiles)
                {
                    if (t as CharachterTile == null)
                    {
                        if ((t as ICollidiableTile) != null && charachter.CheckCollision(t as ICollidiableTile))
                        {
                            if (t as EnemyTile != null)
                                (t as EnemyTile).Collide(charachter);
                            else
                            {
                                (t as ICollidiableTile).Collide(charachter);
                                if (t as ExitTile != null)
                                {
                                    isCancelCalled = true;
                                    return;
                                }
                            }
                        }
                    }
                }
                charachter.Move();
                mainPanel.Invalidate();
            }
        }

        List<Task> CharactherBallsTasks;
        async Task CharachterFire(CharachterTile charachter, CancellationToken token)
        {
            var ball = new FreezeBall(tileSize, maxSpeed * 3, charachter.Position, freeze_ball, tileSize);
            tiles.Add(ball);
            ball.SetDirection(charachter.Direction);
            bool isMoving = true;
            while (true)
            {
                if (token.IsCancellationRequested)
                    return;
                await Task.Delay(5);
                foreach (var t in tiles)
                {
                    if ((t as ICollidiableTile) != null && ball.CheckCollision(t as ICollidiableTile))
                    {
                        (t as ICollidiableTile).Collide(ball);
                        if (t as EnemyTile != null)
                            ball.Collide(t as EnemyTile);
                        if (t.Type == TileTypes.Wall || t.Type == TileTypes.Enemy)
                        {
                            tiles.Remove(ball);
                            isMoving = false;
                            break;
                        }
                    }
                }
                if (isMoving)
                {
                    ball.Move();
                    mainPanel.Invalidate();
                }
                else break;
            }
        }

        private void characther_shoot()
        {
            CharactherBallsTasks.Add(CharachterFire(charachter, tokenSource.Token));
        }
        #endregion

        #region Form Events Handlers
        private void mainPanel_Paint(object sender, PaintEventArgs e)
        {
            if (Level != null) Level.Dispose();

            Level = new Bitmap(mainPanel.Width, mainPanel.Height);

            float maxY = tiles.Max(a => CartesianToIsometric(a.Position).Y);
            float maxX = tiles.Max(a => CartesianToIsometric(a.Position).X);

            float offsetX = mainPanel.Width / 2 - maxX / 2;
            float offsetY = mainPanel.Height / 2 - maxY / 2;

            tiles = tiles.OrderBy(a => CartesianToIsometric(a.Position).Y + a.Height)
                            .ThenBy(a => CartesianToIsometric(a.Position).X).ToList();
            using (var g = Graphics.FromImage(Level))
            {
                foreach (var t in tiles)
                {
                    PointF _isoCoord = CartesianToIsometric(t.Position);

                    g.DrawImage(t.Image, mainPanel.Width / 2 + _isoCoord.X - tileSize,
                         _isoCoord.Y - t.Height + offsetY);

                }
            }
            PointF isoCoord = CartesianToIsometric(mazeExit.Position);
            e.Graphics.DrawImage(Level, 0, 0);
            timerText.Location = new Point((int)(mainPanel.Width / 2 + isoCoord.X - tileSize/2),
                         (int)(isoCoord.Y - mazeExit.Height + offsetY + tileSize / 2 + 9));
        }

        bool[] arrowaState = { false, false, false, false };
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Space)
                charachter?.Fire();
            if (e.KeyCode == Keys.Up)
                arrowaState[0] = true;
            if (e.KeyCode == Keys.Down)
                arrowaState[1] = true;
            if (e.KeyCode == Keys.Left)
                arrowaState[2] = true;
            if (e.KeyCode == Keys.Right)
                arrowaState[3] = true;
            if (arrowaState[0] || arrowaState[1] || arrowaState[2] || arrowaState[3])
            {
                ChangeCharacterDirection();
                MovePlayer();
            }
            e.Handled = true;
        }

        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Up)
                arrowaState[0] = false;
            if (e.KeyCode == Keys.Down)
                arrowaState[1] = false;
            if (e.KeyCode == Keys.Left)
                arrowaState[2] = false;
            if (e.KeyCode == Keys.Right)
                arrowaState[3] = false;
            if (arrowaState[0] || arrowaState[1] || arrowaState[2] || arrowaState[3])
            {
                ChangeCharacterDirection();
                MovePlayer();
            }
            e.Handled = true;
        }

        private void Form1_Leave(object sender, EventArgs e)
        {
            arrowaState[0] = false;
            arrowaState[1] = false;
            arrowaState[2] = false;
            arrowaState[3] = false;
        }

        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            mainPanel.Invalidate();
        }
        #endregion
    }
}
