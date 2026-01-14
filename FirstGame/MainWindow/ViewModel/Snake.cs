using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace Snake.MainWindow.ViewModel
{
    public class Snake
    {
        /// <summary>
        /// 0 for left
        /// 1 for up
        /// 2 for right
        /// 3 for down
        /// </summary>
        private int _direction = 0;
        public LinkedList<Cell> Body { get; } = new();
        public int Direction
        {
            get { return _direction; }
            set
            {
                if (_direction == value) return;
                _direction = value;
                OnPropertyChanged();
            }
        }

        public Snake(Cell head, int direction)
        {
            Direction = direction;
            Body = new LinkedList<Cell>();
            Body.AddFirst(head);
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
