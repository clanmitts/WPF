using System.ComponentModel;
using System.Printing;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace Snake.MainWindow.ViewModel
{
    public class Cell : INotifyPropertyChanged
    {
        public int Row { get; set; }
        public int Column { get; set; }

        private bool _isFilled;

        private bool _isApple;

        public bool IsFilled
        {
            get { return _isFilled; }
            set
            {
                if (_isFilled == value) return;
                _isFilled = value;
                OnPropertyChanged();
            }
        }

        public bool IsApple
        {
            get { return _isApple; }
            set
            {
                if (_isApple == value) return;
                _isApple = value;
                OnPropertyChanged();
            }
        }

        public Cell(int row, int column, bool isFilled, bool isApple = false)
        {
            Row = row; 
            Column = column;
            IsFilled= isFilled;
            IsApple = isApple;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
