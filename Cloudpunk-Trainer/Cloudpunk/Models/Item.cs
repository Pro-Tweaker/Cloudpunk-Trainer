using Cloudpunk_Trainer.Cloudpunk.Enums;
using Pro_Tweaker;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Cloudpunk_Trainer.Cloudpunk.Models
{
    public sealed class Item : INotifyPropertyChanged
    {
        #region Private Backing Fields
        private int amount;
        private int totalAmountAvailable;
        #endregion

        [Browsable(false)]
        public IntPtr Address { get; set; }

        [DisplayName("Name")]
        public string LocalizedName { get; set; }

        [Browsable(false)]
        [DisplayName("Description")]
        public string localizedDescription { get; set; }

        public ItemCategory Category { get; set; }

        public int Amount
        {
            get
            {
                return amount;
            }
            set
            {
                if (value != amount)
                {
                    amount = value;
                    NotifyPropertyChanged();
                }
            }
        }

        [DisplayName("Consumable")]
        public bool IsConsumable { get; set; }

        [DisplayName("Sellable")]
        public bool CanBeSold { get; set; }

        [DisplayName("Usable")]
        public bool CanBeUsed { get; set; }

        [DisplayName("Base Price")]
        public int BasePrice { get; set; }

        [DisplayName("Force Price")]
        public bool ForceBasePrice { get; set; }

        [DisplayName("Total Available")]
        public int TotalAmountAvailable
        {
            get
            {
                return totalAmountAvailable;
            }
            set
            {
                if (value != totalAmountAvailable)
                {
                    totalAmountAvailable = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public float Cooldown { get; set; }

        public Item(IntPtr address)
        {
            Address = address;
        }
        
        public bool Read(Memory memory)
        {
            if(memory != null && memory.Process != null && memory.ProcessAlive != false && Address != IntPtr.Zero)
            {
                LocalizedName = Utils.ReadUnityString(memory, IntPtr.Add(Address, 0x20));
                Category = (ItemCategory)memory.Reader.ReadInt(IntPtr.Add(Address, 0x38));
                Amount = memory.Reader.ReadInt(IntPtr.Add(Address, 0x3C));
                IsConsumable = memory.Reader.ReadBool(IntPtr.Add(Address, 0x40));
                CanBeSold = memory.Reader.ReadBool(IntPtr.Add(Address, 0x41));
                CanBeUsed = memory.Reader.ReadBool(IntPtr.Add(Address, 0x42));
                BasePrice = memory.Reader.ReadInt(IntPtr.Add(Address, 0x44));
                ForceBasePrice = memory.Reader.ReadBool(IntPtr.Add(Address, 0x48));
                TotalAmountAvailable = memory.Reader.ReadInt(IntPtr.Add(Address, 0x4C));
                Cooldown = memory.Reader.ReadFloat(IntPtr.Add(Address, 0x70));
            }           

            return false;
        }

        public bool Write(Memory memory)
        {
            if (memory != null && memory.Process != null && memory.ProcessAlive != false && Address != IntPtr.Zero)
            {
                bool result;

                result = memory.Writer.WriteInt(IntPtr.Add(Address, 0x3C), Amount);
                result = memory.Writer.WriteBool(IntPtr.Add(Address, 0x40), IsConsumable);
                result = memory.Writer.WriteBool(IntPtr.Add(Address, 0x41), CanBeSold);
                result = memory.Writer.WriteBool(IntPtr.Add(Address, 0x42), CanBeUsed);
                result = memory.Writer.WriteInt(IntPtr.Add(Address, 0x44), BasePrice);
                result = memory.Writer.WriteBool(IntPtr.Add(Address, 0x48), ForceBasePrice);
                result = memory.Writer.WriteInt(IntPtr.Add(Address, 0x4C), TotalAmountAvailable);
                result = memory.Writer.WriteFloat(IntPtr.Add(Address, 0x70), Cooldown);

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