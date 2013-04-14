using System;
using System.Windows.Input;

namespace WpfGists.ViewModel
{
  
  public class DelegateCommand : ICommand
  {
    private Action<object> _execute;
    private Predicate<object> _canExecute;

    
    public DelegateCommand(Action<object> execute, Predicate<object> canExecute)
    {
      this._execute = execute;
      this._canExecute = canExecute;
    }

    
    public DelegateCommand(Action<object> execute)
      : this(execute, null)
    { }

    
    public bool CanExecute(object parameter)
    {
      if (_canExecute == null)
      {
        return true;
      }
      else
      {
        return _canExecute(parameter);
      }
    }

    
    public event EventHandler CanExecuteChanged
    {
      add { CommandManager.RequerySuggested += value; }
      remove { CommandManager.RequerySuggested -= value; }
    }

   
    public void Execute(object parameter)
    {
      if (_execute != null)
        _execute(parameter);
    }
  }
}
