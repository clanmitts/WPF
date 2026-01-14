using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using YourApp.Utils;

namespace Testing.ViewModels
{
    public sealed class MainWindowViewModel : INotifyPropertyChanged
    {
        private string _name = string.Empty;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string Name
        {
            get => _name;
            set
            {
                if (_name == value) return;
                _name = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(StatusMessage));
                ((RelayCommand)AddCommand).RaiseCanExecuteChanged();
            }
        }

        public ObservableCollection<string> Names { get; } = new();

        public string StatusMessage =>
            string.IsNullOrWhiteSpace(Name)
                ? "Type a name, then press Add."
                : $"Ready to add: \"{Name}\"";

        public ICommand AddCommand { get; }
        public ICommand ClearCommand { get; }

        public MainWindowViewModel()
        {
            AddCommand = new RelayCommand(
                execute: _ =>
                {
                    Names.Add(Name.Trim());
                    Name = string.Empty;
                },
                canExecute: _ => !string.IsNullOrWhiteSpace(Name)
            );

            ClearCommand = new RelayCommand(
                execute: _ => Names.Clear()
            );
        }

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
