using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace WinUI.Redemption.Demo
{
    public class ViewModel : INotifyPropertyChanged
    {

        #region bool Option1 property
        private bool _Option1;
        public bool Option1
        {
            get
            {
                return _Option1;
            }
            set
            {
                if (_Option1 == value)
                    return;
                _Option1 = value;
                OnPropertyChanged();
            }
        }
        #endregion


        #region bool Option2 property
        private bool _Option2;
        public bool Option2
        {
            get
            {
                return _Option2;
            }
            set
            {
                if (_Option2 == value)
                    return;
                _Option2 = value;
                OnPropertyChanged();
            }
        }
        #endregion


        #region bool Option3 property
        private bool _Option3;
        public bool Option3
        {
            get
            {
                return _Option3;
            }
            set
            {
                if (_Option3 == value)
                    return;
                _Option3 = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region ICommand First Command

        private NamedCommand _FirstCommand;
        public ICommand FirstCommand
        {
            get
            {
                return _FirstCommand ?? (_FirstCommand = new NamedCommand(
                    (obj) =>
                    {
                    })
                {
                    Name = "First",
                });
            }
        }

        #endregion

        #region ICommand Second Command

        private NamedCommand _SecondCommand;
        public ICommand SecondCommand
        {
            get
            {
                return _SecondCommand ?? (_SecondCommand = new NamedCommand(
                    (obj) =>
                    {
                    })
                {
                    Name = "Second",
                });
            }
        }

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(
                this,
                new PropertyChangedEventArgs(propertyName));
        }
    }
}
