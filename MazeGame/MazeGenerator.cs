using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static MazeGame.CharachterTile;
using static MazeGame.MazeGenerator;

namespace MazeGame
{
    
    public static class MazeGenerator
    {   
        private static int[,] Maze { get; set; }
        private static int Width;
        private static int Height;
        private static Random random= new Random();

        public static Point Start;
        public static Point Exit;

        static Random rnd = new Random();

        static List<Point> Path = new List<Point>();

        public static int[,] Generate(int width, int height)
        {
            Maze = new int[width, height];
            Width = width;
            Height = height;


            for (int i = 0; i < Width; i++)
                for (int j = 0; j < Height; j++)
                    Maze[i, j] = -1;

            List<Point> Walls = new List<Point>();

            Point currentCell = new Point(random.Next(1, width - 1), random.Next(1, height - 1));
            Maze[currentCell.Y, currentCell.X] = 0;

            AddWalls(currentCell, ref Walls);

            while(Walls.Count > 0)
            {
                int wallIndex = random.Next(Walls.Count);
                Point Wall = Walls[wallIndex];
                if (CanMakePassege(Wall))
                {
                    Maze[Wall.Y, Wall.X] = 0;
                    AddWalls(Wall, ref Walls);
                    Walls.Remove(Wall);
                    continue;
                }
                Walls.Remove(Wall);
            }

            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                    if (Maze[i, j] == -1) Maze[i, j] = 1;
            }


            SetWallsAtBorders();
            SetStartAndExitPoints();

            int path = BFS(Maze, Exit, Start);
            if (path == -1) Generate(width, height);
            List<Point> walkable = new List<Point>();
            for(int i = 0; i < Width; i++)
            {
                for(int j = 0; j < Height; j++)
                {
                    if (Maze[i,j] == 0)
                        walkable.Add(new Point(i, j));
                }
            }

            walkable = walkable.OrderBy(x => Guid.NewGuid()).ToList();
           int enemy_count = rnd.Next(3,5);
            for(int i = 0; i < enemy_count; i++)
            {
                int ind = rnd.Next(Path.Count);
                if(Path[ind] == Exit) 
                    rnd.Next(Path.Count);
                Maze[Path[ind].X, Path[ind].Y] = 3;
            }
           for (int i = 0; i < 6; i++)
                Maze[walkable[i].X, walkable[i].Y] = 2;

            return Maze;
        }
        
        private static void SetWallsAtBorders()
        {
            for (int i = 0; i < Width; i++)
            {
                Maze[i, 0] = 1;
                Maze[i, Height - 1] = 1;
            }
            for (int i = 0; i < Height; i++)
            {
                Maze[0, i] = 1;
                Maze[Width - 1, i] = 1;
            }
        }

        private static void SetStartAndExitPoints()
        {
            for (int i = 0; i < Width; i++)
            {
                if (Maze[1, i] == 0)
                {
                    Maze[0, i] = 0;
                    Exit = new Point(0, i);
                    break;
                }
            }

            for (int i = Width - 1; i >= 0; i--)
            {
                if (Maze[Height - 2, i] == 0)
                {
                    Maze[Height - 1, i] = 0;
                    Start = new Point(Height - 1, i);
                    break;
                }
            }
        }
        private static void AddWalls(Point Cell, ref List<Point> Walls)
        {
            if (Cell.Y + 1 > 0 && Cell.Y + 1 < Height && Maze[Cell.Y + 1, Cell.X] != 0)
            {
                Maze[Cell.Y + 1, Cell.X] = 1;
                Point newWall = new Point(Cell.X, Cell.Y + 1);
                if (!Walls.Contains(newWall))
                    Walls.Add(newWall);
            }
            if (Cell.Y - 1 > 0 && Cell.Y - 1 < Height && Maze[Cell.Y - 1, Cell.X] != 0)
            {
                Maze[Cell.Y - 1, Cell.X] = 1;
                Point newWall = new Point(Cell.X, Cell.Y - 1);
                if (!Walls.Contains(newWall))
                    Walls.Add(newWall);
            }
            if (Cell.X + 1 > 0 && Cell.X + 1 < Width && Maze[Cell.Y, Cell.X + 1] != 0)
            {
                Maze[Cell.Y, Cell.X + 1] = 1;
                Point newWall = new Point(Cell.X + 1, Cell.Y);
                if (!Walls.Contains(newWall))
                    Walls.Add(newWall);
            }
            if (Cell.X - 1 > 0 && Cell.X - 1 < Width && Maze[Cell.Y, Cell.X - 1] != 0)
            {
                Maze[Cell.Y, Cell.X - 1] = 1;
                Point newWall = new Point(Cell.X - 1, Cell.Y);
                if (!Walls.Contains(newWall))
                    Walls.Add(newWall);
            }
        }

        private static bool CanMakePassege(Point Wall)
        {
            bool f = false;
            if (Wall.Y - 1 >= 0 && Wall.Y + 1 < Height)
            {
                if (Maze[Wall.Y + 1, Wall.X] == -1 && Maze[Wall.Y - 1, Wall.X] == 0
                    || Maze[Wall.Y + 1, Wall.X] == 0 && Maze[Wall.Y - 1, Wall.X] == -1)
                {
                    f = true;
                }
            }
            if (Wall.X - 1 >= 0 && Wall.X + 1 < Width)
            {
                if (Maze[Wall.Y, Wall.X + 1] == -1 && Maze[Wall.Y, Wall.X - 1] == 0
                    || Maze[Wall.Y, Wall.X + 1] == 0 && Maze[Wall.Y, Wall.X - 1] == -1)
                {
                    f = true;
                }
            }

            if (f && GetVisitedNeigbours(Wall) < 2)
            {
                return true;
            }
            else return false;
        }

        private static int GetVisitedNeigbours(Point Wall)
        {
            int s_cells = 0;
            if (Wall.Y - 1 >= 0 && Maze[Wall.Y - 1, Wall.X] == 0)
                s_cells++;
            if (Wall.Y + 1 < Height && Maze[Wall.Y + 1, Wall.X] == 0)
                s_cells++;
            if (Wall.X + 1 < Width && Maze[Wall.Y, Wall.X + 1] == 0)
                s_cells++;
            if (Wall.X - 1 >= 0 && Maze[Wall.Y, Wall.X - 1] == 0)
                s_cells++;
            return s_cells;
        }


        static bool isValid(int row, int col)
        {
            // return true if row number and
            // column number is in range
            return (row >= 0) && (row < Height) && (col >= 0) && (col < Width);
        }

        static int[] rowNum = { -1, 0, 0, 1 };
        static int[] colNum = { 0, -1, 1, 0 };

        // A Data Structure for queue used in BFS
        private class queueNode
        {
            // The coordinates of a cell
            public Point pt;

            // cell's distance of from the source
            public int dist;

            public queueNode(Point pt, int dist)
            {
                this.pt = pt;
                this.dist = dist;
            }
        };

        static int CheckTurns(int[,] distences, Point dest)
        {
            Point currentDirection, previousDirection = new Point(-2, -2);
            if (Path != null) Path.Clear();
            int turns_count = 0;
            Queue<Point> points = new Queue<Point>();
            points.Enqueue(dest);
            while (points.Count > 0)
            {
                Point cur = points.Dequeue();
                for (int i = 0; i < 4; i++)
                {
                    int row = cur.X + rowNum[i];
                    int col = cur.Y + colNum[i];
                    currentDirection = new Point(rowNum[i], colNum[i]);
                    if (isValid(row, col) && distences[row, col] + 1 == distences[cur.X, cur.Y])
                    {
                        if (currentDirection != previousDirection)
                        {
                            turns_count++;
                            Console.WriteLine($"{cur.X},{cur.Y}({distences[cur.X, cur.Y]})");
                            previousDirection = currentDirection;
                        }
                        Path.Add(new Point(row, col));
                        points.Enqueue(new Point(row, col));
                        break;
                    }
                }
            }
            return --turns_count;
        }

        // function to find the shortest path between
        // a given source cell to a destination cell.
        static int BFS(int[,] mat, Point src, Point dest)
        {
            // check source and destination cell
            // of the matrix have value 1
            if (mat[src.X, src.Y] == 1 || mat[dest.X, dest.Y] == 1)
                return -1;

            bool[,] visited = new bool[Height, Width];
            int[,] distences = new int[Height, Width];
            // Mark the source cell as visited
            visited[src.X, src.Y] = true;

            // Create a queue for BFS
            Queue<queueNode> q = new Queue<queueNode>();

            // Distance of source cell is 0
            queueNode s = new queueNode(src, 0);
            q.Enqueue(s); // Enqueue source cell

            // Do a BFS starting from source cell
            while (q.Count != 0)
            {
                queueNode curr = q.Peek();
                Point pt = curr.pt;

                // If we have reached the destination cell,
                // we are done
                if (pt.X == dest.X && pt.Y == dest.Y)
                {
                    if (CheckTurns(distences, pt) <= 2) return -1;
                    return curr.dist;
                }

                // Otherwise dequeue the front cell
                // in the queue and enqueue
                // its adjacent cells
                q.Dequeue();

                for (int i = 0; i < 4; i++)
                {
                    int row = pt.X + rowNum[i];
                    int col = pt.Y + colNum[i];
                    // if adjacent cell is valid, has path
                    // and not visited yet, enqueue it.
                    if (isValid(row, col) && mat[row, col] != 1 && !visited[row, col])
                    {
                        // mark cell as visited and enqueue it
                        visited[row, col] = true;
                        queueNode Adjcell = new queueNode(new Point(row, col), curr.dist + 1);
                        distences[row, col] = curr.dist + 1;
                        q.Enqueue(Adjcell);
                    }
                }
            }
            // Return -1 if destination cannot be reached
            return -1;
        }
    }
}
