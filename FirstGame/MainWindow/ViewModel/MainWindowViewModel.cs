using Snake.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.DirectoryServices;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;

namespace Snake.MainWindow.ViewModel
{
    public class MainWindowViewModel : IMainWindowViewModel, INotifyPropertyChanged
    {
        private int _rows = 20;
        private int _columns = 20;
        private Snake _snake;
        private int _score;
        private bool _isGameOver;

        private readonly Random _rng = new();
        private int Index(int r, int c) => r * Columns + c;

        private CancellationTokenSource? _loopCts;

        public event PropertyChangedEventHandler? PropertyChanged;

        public int Rows
        {
            get { return _rows; }
            set
            {
                if (_rows == value) return;
                _rows = value;
                OnPropertyChanged();
            }
        }

        public int Columns
        {
            get { return _columns; }
            set
            {
                if (_columns == value) return;
                _columns = value;
                OnPropertyChanged();
            }
        }

        public bool IsGameOver
        {
            get => _isGameOver;
            private set
            {
                if (_isGameOver == value) return;
                _isGameOver = value;
                OnPropertyChanged();
            }
        }

        public int Score
        {
            get => _score;
            private set
            {
                if (_score == value) return;
                _score = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<Cell> Cells { get; } = new();

        public ICommand ChangeDirectionCommand { get; }
        public ICommand RestartCommand { get; }

        public MainWindowViewModel()
        {
            Rows = 20;
            Columns = 20;
            Score = 0;

            for (int r = 0; r < Rows; r++)
            {
                for (int c = 0; c < Columns; c++)
                {
                    bool filled = false;
                    Cells.Add(new Cell(r, c, filled));
                }
            }
            SetCellFilled(0, 0, true);
            _snake = new Snake(new Cell(0, 0, true), 2);
            SpawnApple();

            ChangeDirectionCommand = new RelayCommand<int>(SetDirection);
            RestartCommand = new RelayCommand(_ => RestartGame());

            StartGameLoop();
        }

        private void SetDirection(int newDir)
        {
            int cur = _snake.Direction;
            if ((newDir + 2) % 4 == cur) return;

            _snake.Direction = newDir;
        }

        public void StartGameLoop()
        {
            IsGameOver = false;
            _loopCts?.Cancel();
            _loopCts = new CancellationTokenSource();

            _ = GameLoopAsync(_loopCts.Token);
        }

        public void StopGameLoop()
        {
            _loopCts?.Cancel();
        }

        private async Task GameLoopAsync(CancellationToken ct)
        {
            using var timer = new PeriodicTimer(TimeSpan.FromSeconds(0.15));

            try
            {
                while (await timer.WaitForNextTickAsync(ct))
                {
                    await Application.Current.Dispatcher.InvokeAsync(UpdateBoard, DispatcherPriority.Render);
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }
        }

        private void UpdateBoard()
        {
            if (IsGameOver) return;

            Cell head = _snake.Body.First!.Value;

            int dr = 0, dc = 0;
            switch (_snake.Direction)
            {
                case 0: dc = -1; break; // left
                case 1: dr = -1; break; // up
                case 2: dc = 1; break; // right
                case 3: dr = 1; break; // down
            }

            int newRow = head.Row + dr;
            int newCol = head.Column + dc;

            if (newRow < 0 || newRow >= Rows || newCol < 0 || newCol >= Columns)
            {
                TriggerGameOver();
                return;
            }

            int newIdx = Index(newRow, newCol);

            // Is there an apple where we’re moving?
            bool ateApple = Cells[newIdx].IsApple;

            if (Cells[newIdx].IsFilled)
            {
                Cell tail = _snake.Body.Last!.Value;
                bool movingIntoTail = !ateApple && tail.Row == newRow && tail.Column == newCol;
                if (!movingIntoTail)
                {
                    TriggerGameOver();
                    return;
                }
            }

            // Add new head
            Cell newHead = new Cell(newRow, newCol, true);
            _snake.Body.AddFirst(newHead);

            // Update the board cell
            Cells[newIdx].IsApple = false;     // if it was an apple, we "eat" it
            Cells[newIdx].IsFilled = true;     // snake now occupies it

            if (!ateApple)
            {
                // normal move: remove tail
                Cell oldTail = _snake.Body.Last!.Value;
                Cells[Index(oldTail.Row, oldTail.Column)].IsFilled = false;
                _snake.Body.RemoveLast();
            }
            else
            {
                // grew: do NOT remove tail, just spawn a new apple somewhere empty
                Score += 1;
                SpawnApple();
            }
        }

        private void SpawnApple()
        {
            // Collect all cells that are empty (not snake) AND not already an apple
            List<Cell> candidates = new();
            foreach (var cell in Cells)
            {
                if (!cell.IsFilled && !cell.IsApple)
                    candidates.Add(cell);
            }

            // No space left (snake filled the board) – you can treat this as "win"
            if (candidates.Count == 0)
            {
                TriggerGameOver();
                return;
            }

            var chosen = candidates[_rng.Next(candidates.Count)];
            chosen.IsApple = true;
        }

        private void TriggerGameOver()
        {
            IsGameOver = true;
            StopGameLoop(); // cancels token -> exits GameLoopAsync
        }

        private void RestartGame()
        {
            StopGameLoop();

            foreach (var cell in Cells)
            {
                cell.IsFilled = false;
                cell.IsApple = false;
            }

            Score = 0;
            IsGameOver = false;

            SetCellFilled(0, 0, true);
            _snake = new Snake(new Cell(0, 0, true), 2);
            SpawnApple();

            StartGameLoop();
        }

        public void SetCellFilled(int r, int c, bool filled)
        {
            int index = r * Columns + c;
            Cells[index].IsFilled = filled;
        }

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
