using System;
using System.ComponentModel;

namespace Cloudpunk_Trainer.Models
{
    public sealed class Offset
    {
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
            get;
            set;
        }

        public Offset(string name, Type type, int value)
        {
            Name = name;
            Type = type;
            OffsetValue = value;
        }
    }
}
