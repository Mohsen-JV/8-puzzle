using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using System.IO;

namespace _8Puzzle
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region UI code behind
        int[,] Puzzle;
        System.Timers.Timer clock;
        System.DateTime time;
        bool timeStop;
        public MainWindow()
        {
            InitializeComponent();
            Puzzle = new int[3, 3] { { 1, 2, 3 }, { 4, 5, 6 }, { 7, 8, 0 } };
            rowZero = colZero = 2;
            clock = new System.Timers.Timer(100);
            clock.Elapsed += (s, e) => clock.Stop();
            clock.Elapsed += TimeElapse;
            clock.Start();
        }

        void TimeElapse(object sender, System.Timers.ElapsedEventArgs e)
        {
            Monitor.Enter("lock");
            Monitor.Pulse("lock");
            Monitor.Wait("lock");
            Monitor.Exit("lock");
            clock.Start();
        }

        void timer()
        {
            Task t = new Task(() =>
            {
                time = new DateTime(1, 1, 1, 1, 0, 0, 0);
                timeStop = false;
                while (!timeStop)
                {
                    Monitor.Enter("lock");
                    Monitor.Pulse("lock");
                    time=time.AddMilliseconds(100);
                    Dispatcher.Invoke(() => tbTimer.Text = string.Format($"{time.Minute}:{time.Second}:{time.Millisecond}"));
                    Monitor.Wait("lock");
                    Monitor.Exit("lock");
                }
            });
            t.Start();
        }

        private void Rectangle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (num == -1) return;
            int i;
            for (i = 0; i < 9; i++)
                if (GridPuzzle.Children[i] == e.OriginalSource) break;
            Puzzle[i / 3, i % 3] = num;
            (GridPuzzle.Children[i + 9] as Button).Content = num.ToString();
            GridPuzzle.Children[i + 9].Visibility = Visibility.Visible;
            num++;
            if (num == 9)
            {
                num = -1;
                for (i = 0; i < 3; i++)
                    for (int j = 0; j < 3; j++)
                        if (Puzzle[i, j] == 0)
                        {
                            rowZero = i;
                            colZero = j;
                            break;
                        }
                stackButton.IsEnabled = true;
            }
        }

        int num = -1;

        private void EnterNumber_Click(object sender, RoutedEventArgs e)
        {
            num = 1;
            stackButton.IsEnabled = false;
            for (int i = 9; i < 18; i++)
                GridPuzzle.Children[i].Visibility = Visibility.Hidden;
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    Puzzle[i, j] = 0;
        }

        private void OpenPFF_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
            openFileDialog.ShowDialog();
            string[] lines = File.ReadAllLines(openFileDialog.FileName);
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                {
                    Puzzle[i, j] = int.Parse(lines[i].Split(',')[j]);
                    (GridPuzzle.Children[i * 3 + j + 9] as Button).Visibility = Visibility.Visible;
                    if (Puzzle[i, j] == 0)
                    {
                        (GridPuzzle.Children[i * 3 + j + 9] as Button).Visibility = Visibility.Hidden;
                        rowZero = i;
                        colZero = j;
                    }
                    (GridPuzzle.Children[i * 3 + j + 9] as Button).Content = lines[i].Split(',')[j];
                }
        }

        int rowZero, colZero, preRow, preCol;

        private void Shuffle_Click(object sender, RoutedEventArgs e)
        {
            Task t = new Task(ShufflePuzzle);
            t.Start();
            stackButton.IsEnabled = false;
        }

        void ShufflePuzzle()
        {
            var rnd = new Random();
            int num = rnd.Next(180, 310);
            preRow = rowZero;
            preCol = colZero;
            for (int i = 0; i < num; i++)
                while (!move(rnd.Next(1, 5))) ;
            Dispatcher.Invoke(() => stackButton.IsEnabled = true);
        }

        bool move(int dir)
        {
            //1 up - 2 right - 3 down - 4 left
            int row = rowZero, col = colZero;
            switch (dir)
            {
                case 1:
                    row--;
                    if (row == preRow && col == preCol) return false;
                    if (row < 0) return false;
                    Puzzle[rowZero, colZero] = Puzzle[row, col];
                    Puzzle[row, col] = 0;
                    preRow = rowZero;
                    preCol = colZero;
                    rowZero = row;
                    colZero = col;
                    Monitor.Enter("lock");
                    Monitor.Pulse("lock");
                    Monitor.Wait("lock");
                    Dispatcher.Invoke(() =>
                    {
                        GridPuzzle.Children[9 + preRow * 3 + preCol].Visibility = Visibility.Visible;
                        (GridPuzzle.Children[9 + preRow * 3 + preCol] as Button).Content = Puzzle[preRow, preCol].ToString();
                        GridPuzzle.Children[9 + rowZero * 3 + colZero].Visibility = Visibility.Hidden;
                    });
                    Monitor.Exit("lock");
                    break;
                case 2:
                    col++;
                    if (row == preRow && col == preCol) return false;
                    if (2 < col) return false;
                    Puzzle[rowZero, colZero] = Puzzle[row, col];
                    Puzzle[row, col] = 0;
                    preRow = rowZero;
                    preCol = colZero;
                    rowZero = row;
                    colZero = col;
                    Monitor.Enter("lock");
                    Monitor.Pulse("lock");
                    Monitor.Wait("lock");
                    Dispatcher.Invoke(() =>
                    {
                        GridPuzzle.Children[9 + preRow * 3 + preCol].Visibility = Visibility.Visible;
                        (GridPuzzle.Children[9 + preRow * 3 + preCol] as Button).Content = Puzzle[preRow, preCol].ToString();
                        GridPuzzle.Children[9 + rowZero * 3 + colZero].Visibility = Visibility.Hidden;
                    });
                    Monitor.Exit("lock");
                    break;
                case 3:
                    row++;
                    if (row == preRow && col == preCol) return false;
                    if (2 < row) return false;
                    Puzzle[rowZero, colZero] = Puzzle[row, col];
                    Puzzle[row, col] = 0;
                    preRow = rowZero;
                    preCol = colZero;
                    rowZero = row;
                    colZero = col;
                    Monitor.Enter("lock");
                    Monitor.Pulse("lock");
                    Monitor.Wait("lock");
                    Dispatcher.Invoke(() =>
                    {
                        GridPuzzle.Children[9 + preRow * 3 + preCol].Visibility = Visibility.Visible;
                        (GridPuzzle.Children[9 + preRow * 3 + preCol] as Button).Content = Puzzle[preRow, preCol].ToString();
                        GridPuzzle.Children[9 + rowZero * 3 + colZero].Visibility = Visibility.Hidden;
                    });
                    Monitor.Exit("lock");
                    break;
                case 4:
                    col--;
                    if (row == preRow && col == preCol) return false;
                    if (col < 0) return false;
                    Puzzle[rowZero, colZero] = Puzzle[row, col];
                    Puzzle[row, col] = 0;
                    preRow = rowZero;
                    preCol = colZero;
                    rowZero = row;
                    colZero = col;
                    Monitor.Enter("lock");
                    Monitor.Pulse("lock");
                    Monitor.Wait("lock");
                    Dispatcher.Invoke(() =>
                    {
                        GridPuzzle.Children[9 + preRow * 3 + preCol].Visibility = Visibility.Visible;
                        (GridPuzzle.Children[9 + preRow * 3 + preCol] as Button).Content = Puzzle[preRow, preCol].ToString();
                        GridPuzzle.Children[9 + rowZero * 3 + colZero].Visibility = Visibility.Hidden;
                    });
                    Monitor.Exit("lock");
                    break;
                default:
                    break;
            }
            return true;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            int i;
            for (i = 9; i < 18; i++)
                if (GridPuzzle.Children[i] == e.OriginalSource) break;
            int row = (i - 9) / 3, col = (i - 9) % 3;
            if (Math.Abs(row - rowZero) + Math.Abs(col - colZero) == 1)
            {
                GridPuzzle.Children[9 + rowZero * 3 + colZero].Visibility = Visibility.Visible;
                (GridPuzzle.Children[9 + rowZero * 3 + colZero] as Button).Content = (e.OriginalSource as Button).Content;
                Puzzle[rowZero, colZero] = int.Parse((e.OriginalSource as Button).Content.ToString());
                Puzzle[row, col] = 0;
                rowZero = row;
                colZero = col;
                GridPuzzle.Children[9 + rowZero * 3 + colZero].Visibility = Visibility.Hidden;
            }
        }
        #endregion
        #region BFS solve

        public enum Dir : short { up, right, down, left }

        struct BFSNode
        {
            public short rowZ, colZ;
            public Queue<Dir> order;
        }

        private void SolveBFS_Click(object sender, RoutedEventArgs e)
        {
            timer();
            Task t = new Task(BFS);
            GridPuzzle.IsEnabled = false;
            stackButton.IsEnabled = false;
            t.Start();
        }

        void SolveWithOrderPuzzle(BFSNode orderSolve)
        {
            clock.Interval = 500;
            var direction = new[] { new { rd = -1, cd = 0 }, new { rd = 0, cd = 1 }
                                  , new { rd = 1, cd = 0 }, new { rd = 0, cd = -1} };
            foreach (var item in orderSolve.order)
            {
                int r = rowZero + direction[(int)item].rd, c = colZero + direction[(int)item].cd, rz = rowZero, cz = colZero;
                Puzzle[rowZero, colZero] = Puzzle[r, c];
                Puzzle[r, c] = 0;
                rowZero = r;
                colZero = c;
                Monitor.Enter("lock");
                Monitor.Pulse("lock");
                Monitor.Wait("lock");
                Dispatcher.Invoke(() =>
                {
                    GridPuzzle.Children[9 + rz * 3 + cz].Visibility = Visibility.Visible;
                    (GridPuzzle.Children[9 + rz * 3 + cz] as Button).Content = Puzzle[rz, cz].ToString();
                    GridPuzzle.Children[9 + rowZero * 3 + colZero].Visibility = Visibility.Hidden;
                });
                Monitor.Exit("lock");
            }
            clock.Interval = 100;
        }

        void BFS()
        {
            Queue<BFSNode> fringe = new Queue<BFSNode>();
            fringe.Enqueue(new BFSNode() { rowZ = (short)rowZero, colZ = (short)colZero, order = new Queue<Dir>() });
            if (CheckSolvePuzzle(Puzzle))
            {
                Dispatcher.Invoke(() =>
                {
                    GridPuzzle.IsEnabled = true;
                    stackButton.IsEnabled = true;
                });
                return;
            }
            while (true)
            {
                int numofq = fringe.Count;
                for (int i = 0; i < numofq; i++)
                    foreach (var item in expandNode(fringe.Dequeue(), numofq == 1))
                        fringe.Enqueue(item);
                foreach (var item in fringe)
                {
                    if (check(item))
                    {
                        timeStop = true;
                        SolveWithOrderPuzzle(item);
                        Dispatcher.Invoke(() =>
                        {
                            GridPuzzle.IsEnabled = true;
                            stackButton.IsEnabled = true;
                        });
                        return;
                    }
                }
            }
        }

        bool check(BFSNode orderNode)
        {
            orderNode.order = new Queue<Dir>(orderNode.order.ToArray());
            int[,] p = Puzzle.Clone() as int[,];
            int rowz = rowZero, colz = colZero;
            var direction = new[] { new { rd = -1, cd = 0 }, new { rd = 0, cd = 1 }
                                    , new { rd = 1, cd = 0 }, new { rd = 0, cd = -1} };
            for (int i = orderNode.order.Count; i > 0; i--)
            {
                var dir = orderNode.order.Dequeue();
                int r = rowz + direction[(int)dir].rd, c = colz + direction[(int)dir].cd;
                p[rowz, colz] = p[r, c];
                p[r, c] = 0;
                rowz = r;
                colz = c;
            }
            if (CheckSolvePuzzle(p)) return true;
            return false;
        }

        bool CheckSolvePuzzle(int[,] p)
        {
            int[,] sP = new int[,] { { 1, 2, 3 }, { 4, 5, 6 }, { 7, 8, 0 } };
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    if (sP[i, j] != p[i, j]) return false;
            return true;
        }

        IEnumerable<BFSNode> expandNode(BFSNode node, bool isRoot)
        {
            var direction = new[] { new { rd = -1, cd = 0,dir=Dir.up }, new { rd = 0, cd = 1 ,dir=Dir.right}
                                    , new { rd = 1, cd = 0 ,dir=Dir.down}, new { rd = 0, cd = -1,dir=Dir.left } };
            BFSNode tempNode;
            foreach (var d in direction)
            {
                if (isRoot || d.dir != reverseDir(node.order.Last()))
                {
                    if (node.rowZ + d.rd > -1 && node.rowZ + d.rd < 3
                        && node.colZ + d.cd > -1 && node.colZ + d.cd < 3)
                    {
                        tempNode = node;
                        tempNode.order = new Queue<Dir>(node.order.ToArray());
                        tempNode.rowZ += (short)d.rd;
                        tempNode.colZ += (short)d.cd;
                        tempNode.order.Enqueue(d.dir);
                        yield return tempNode;
                    }
                }
            }
        }

        Dir reverseDir(Dir d)
        {
            switch (d)
            {
                case Dir.down:
                    return Dir.up;
                case Dir.up:
                    return Dir.down;
                case Dir.left:
                    return Dir.right;
                case Dir.right:
                    return Dir.left;
            }
            return Dir.left;
        }
        #endregion
        #region GBFS solve
        List<int> visitedNode;

        IEnumerable<BFSNode> expandNodeGraph(BFSNode node, bool isRoot)
        {
            var direction = new[] { new { rd = -1, cd = 0,dir=Dir.up }, new { rd = 0, cd = 1 ,dir=Dir.right}
                                    , new { rd = 1, cd = 0 ,dir=Dir.down}, new { rd = 0, cd = -1,dir=Dir.left } };
            BFSNode tempNode;
            foreach (var d in direction)
            {
                if (isRoot || d.dir != reverseDir(node.order.Last()))
                {
                    if (node.rowZ + d.rd > -1 && node.rowZ + d.rd < 3
                        && node.colZ + d.cd > -1 && node.colZ + d.cd < 3)
                    {
                        tempNode = node;
                        tempNode.order = new Queue<Dir>(node.order.ToArray());
                        tempNode.rowZ += (short)d.rd;
                        tempNode.colZ += (short)d.cd;
                        tempNode.order.Enqueue(d.dir);
                        if (checkVisit(tempNode)) continue;
                        yield return tempNode;
                    }
                }
            }
        }

        bool checkVisit(BFSNode orderNode)
        {
            orderNode.order = new Queue<Dir>(orderNode.order.ToArray());
            int[,] p = Puzzle.Clone() as int[,];
            int rowz = rowZero, colz = colZero;
            var direction = new[] { new { rd = -1, cd = 0 }, new { rd = 0, cd = 1 }
                                    , new { rd = 1, cd = 0 }, new { rd = 0, cd = -1} };
            for (int i = orderNode.order.Count; i > 0; i--)
            {
                var dir = orderNode.order.Dequeue();
                int r = rowz + direction[(int)dir].rd, c = colz + direction[(int)dir].cd;
                p[rowz, colz] = p[r, c];
                p[r, c] = 0;
                rowz = r;
                colz = c;
            }
            int numOfNode = 0, e = 1;
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                {
                    numOfNode += p[i, j] * e;
                    e *= 10;
                }
            if (visitedNode.Contains(numOfNode)) return true;
            visitedNode.Add(numOfNode);
            return false;
        }

        void GBFS()
        {
            Queue<BFSNode> fringe = new Queue<BFSNode>();
            visitedNode = new List<int>();
            fringe.Enqueue(new BFSNode() { rowZ = (short)rowZero, colZ = (short)colZero, order = new Queue<Dir>() });
            if (CheckSolvePuzzle(Puzzle))
            {
                Dispatcher.Invoke(() =>
                {
                    GridPuzzle.IsEnabled = true;
                    stackButton.IsEnabled = true;
                });
                return;
            }
            while (true)
            {
                int numofq = fringe.Count;
                for (int i = 0; i < numofq; i++)
                    foreach (var item in expandNodeGraph(fringe.Dequeue(), numofq == 1))
                        fringe.Enqueue(item);
                foreach (var item in fringe)
                {
                    if (check(item))
                    {
                        timeStop = true;
                        SolveWithOrderPuzzle(item);
                        Dispatcher.Invoke(() =>
                        {
                            GridPuzzle.IsEnabled = true;
                            stackButton.IsEnabled = true;
                        });
                        return;
                    }
                }
            }
        }

        private void SolveGBFS_Click(object sender, RoutedEventArgs e)
        {
            timer();
            Task t = new Task(GBFS);
            GridPuzzle.IsEnabled = false;
            stackButton.IsEnabled = false;
            t.Start();
        }

        #endregion
        #region IDS solve

        private void SolveIDS_Click(object sender, RoutedEventArgs e)
        {
            timer();
            Task t = new Task(IDS);
            GridPuzzle.IsEnabled = false;
            stackButton.IsEnabled = false;
            t.Start();
        }

        void IDS()
        {
            int L = 0;
            Queue<Dir> order = new Queue<Dir>();
            while (true)
            {
                order.Clear();
                if (IDSSearch(Puzzle, rowZero, colZero, ref order, L, true)) break;
                L++;
            }
            timeStop = true;
            solveWithOrder(order);
            Dispatcher.Invoke(() =>
            {
                GridPuzzle.IsEnabled = true;
                stackButton.IsEnabled = true;
            });
        }

        void solveWithOrder(Queue<Dir> order)
        {
            clock.Interval = 500;
            var direction = new[] { new { rd = -1, cd = 0 }, new { rd = 0, cd = 1 }
                                  , new { rd = 1, cd = 0 }, new { rd = 0, cd = -1} };
            foreach (var item in order)
            {
                int r = rowZero + direction[(int)item].rd, c = colZero + direction[(int)item].cd, rz = rowZero, cz = colZero;
                Puzzle[rowZero, colZero] = Puzzle[r, c];
                Puzzle[r, c] = 0;
                rowZero = r;
                colZero = c;
                Monitor.Enter("lock");
                Monitor.Pulse("lock");
                Monitor.Wait("lock");
                Dispatcher.Invoke(() =>
                {
                    GridPuzzle.Children[9 + rz * 3 + cz].Visibility = Visibility.Visible;
                    (GridPuzzle.Children[9 + rz * 3 + cz] as Button).Content = Puzzle[rz, cz].ToString();
                    GridPuzzle.Children[9 + rowZero * 3 + colZero].Visibility = Visibility.Hidden;
                });
                Monitor.Exit("lock");
            }
            clock.Interval = 100;
        }

        bool IDSSearch(int[,] p, int rowZ, int colZ, ref Queue<Dir> order, int l, bool isRoot = false)
        {
            if (CheckSolvePuzzle(p)) return true;
            if (order.Count > l) return false;
            var direction = new[] { new { rd = -1, cd = 0,dir=Dir.up }, new { rd = 0, cd = 1 ,dir=Dir.right}
                                    , new { rd = 1, cd = 0 ,dir=Dir.down}, new { rd = 0, cd = -1,dir=Dir.left } };
            foreach (var d in direction)
            {
                if (isRoot || d.dir != reverseDir(order.Last()))
                {
                    if (rowZ + d.rd > -1 && rowZ + d.rd < 3
                        && colZ + d.cd > -1 && colZ + d.cd < 3)
                    {
                        int rz = rowZ + (short)d.rd, cz = colZ + (short)d.cd;
                        int[,] np = p.Clone() as int[,];
                        np[rowZ, colZ] = np[rz, cz];
                        np[rz, cz] = 0;
                        Queue<Dir> tempOrder = new Queue<Dir>(order);
                        tempOrder.Enqueue(d.dir);
                        if (IDSSearch(np, rz, cz, ref tempOrder, l))
                        {
                            order = tempOrder;
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        #endregion
    }
}
