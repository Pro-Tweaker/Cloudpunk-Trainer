using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Cloudpunk_Trainer.Models
{
    public sealed class Offset : INotifyPropertyChanged
    {
        #region Private Backing Fields
        private string _value;
        #endregion

        public bool Frozen
        {
            get;
            set;
        }

        [Browsable(false)]
        public string FrozenValue
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }

        [Browsable(false)]
        public Type Type
        {
            get;
            set;
        }

        [Browsable(false)]
        public int OffsetValue
        {
            get;
            set;
        }

        public string Value
        {
            get
            {
                return _value;
            }
            set
            {
                if (value != _value)
                {
                    _value = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public Offset(string name, Type type, int value)
        {
            Name = name;
            Type = type;
            OffsetValue = value;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
