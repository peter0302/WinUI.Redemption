using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace WinUI.Redemption.Demo
{
    public class NamedCommand : ICommand
    {
        Action<object> _action;

        public event EventHandler CanExecuteChanged;

        public NamedCommand(Action<object> action)
        {
            _action = action;
        }

        public string Name { get; set; }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            _action?.Invoke(parameter);
        }
    }
}
