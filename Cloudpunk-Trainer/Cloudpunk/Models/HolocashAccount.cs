using Pro_Tweaker;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Cloudpunk_Trainer.Cloudpunk.Models
{
    public sealed class HolocashAccount : INotifyPropertyChanged
    {
        #region Private Backing Fields
        private int funds;
        #endregion

        [Browsable(false)]
        public IntPtr Address { get; set; }

        public string KeyCode { get; set; }

        [DisplayName("Default Amount")]
        public int DefaultAmount { get; set; }

        public int Funds
        {
            get
            {
                return funds;
            }
            set
            {
                if (value != funds)
                {
                    funds = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public HolocashAccount(IntPtr address)
        {
            Address = address;
        }

        public HolocashAccount(string keyCode, int defaultAmount, int funds)
        {
            KeyCode = keyCode;
            DefaultAmount = defaultAmount;
            Funds = funds;
        }

        public bool Read(Memory memory)
        {    
            if (memory != null && memory.Process != null && memory.ProcessAlive != false && Address != IntPtr.Zero)
            {
                KeyCode = Utils.ReadUnityString(memory, IntPtr.Add(Address, 0x10));
                DefaultAmount = memory.Reader.ReadInt(IntPtr.Add(Address, 0x28));
                Funds = memory.Reader.ReadInt(IntPtr.Add(Address, 0x2C));
            }

            return false;
        }

        public bool Write(Memory memory)
        {
            if (memory != null && memory.Process != null && memory.ProcessAlive != false && Address != IntPtr.Zero)
            {
                bool result;

                result = memory.Writer.WriteInt(IntPtr.Add(Address, 0x2C), Funds);

                return result;
            }

            return false;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}