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
        int[,] Puzzle;
        System.Timers.Timer clock;
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
            int num = rnd.Next(8, 21);
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
    }
}
