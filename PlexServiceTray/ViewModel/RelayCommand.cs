using System;
using System.Windows.Input;

namespace PlexServiceTray.ViewModel
{
    public class RelayCommand(Action<object?> execute, Predicate<object?>? canExecute = null)
        : ICommand
    {
        private readonly Action<object?>? _execute = execute ?? throw new ArgumentNullException(nameof(execute));

        public bool CanExecute(object? parameter)
        {
            return canExecute == null || canExecute(parameter);
        }

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public void Execute(object? parameter)
        {
            _execute?.Invoke(parameter);
        }
    }
}
