using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Windows.Forms;
using Pro_Tweaker;
using Cloudpunk_Trainer.Cloudpunk;
using Cloudpunk_Trainer.Cloudpunk.Models;
using Cloudpunk_Trainer.Models;
using System.Linq;
using System.ComponentModel;

namespace Cloudpunk_Trainer
{
    public partial class MainForm : Form
    {
        private const string EXECUTABLE_NAME = "cloudpunk.exe";

        Memory memory;
        System.Threading.Timer searchTimer;
        bool gamePatched = false;

        // Hacks
        Global global;

        BindingList<Offset> globalOffsets = Offsets.global;
        BindingList<Offset> playerOffsets = Offsets.player;
        BindingList<Offset> playerCarOffsets = Offsets.playerCar;

        BindingList<HolocashAccount> holocashAccounts = new BindingList<HolocashAccount>();
        BindingList<Item> items = new BindingList<Item>();

        BindingSource globalBindingSource = new BindingSource();
        BindingSource playerBindingSource = new BindingSource();
        BindingSource playerCarBindingSource = new BindingSource();
        BindingSource holocashAccountsBindingSource = new BindingSource();
        BindingSource itemsBindingSource = new BindingSource();

        public MainForm()
        {
            InitializeComponent();

            CheckForIllegalCrossThreadCalls = false; // beurk :-(

            Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture("en-GB");

            statusLabel.Text = Text;
            statusStrip.Padding = new Padding(statusStrip.Padding.Left, statusStrip.Padding.Top, statusStrip.Padding.Left, statusStrip.Padding.Bottom);

            Utils.ChangeControlStyles(dataGridView1, ControlStyles.OptimizedDoubleBuffer, true);
            Utils.ChangeControlStyles(dataGridView2, ControlStyles.OptimizedDoubleBuffer, true);
            Utils.ChangeControlStyles(dataGridView3, ControlStyles.OptimizedDoubleBuffer, true);
            Utils.ChangeControlStyles(dataGridView4, ControlStyles.OptimizedDoubleBuffer, true);
            Utils.ChangeControlStyles(dataGridView5, ControlStyles.OptimizedDoubleBuffer, true);
        }
        
        private void MainForm_Load(object sender, EventArgs e)
        {
            memory = new Memory();

            global = new Global(memory);

            globalBindingSource.DataSource = globalOffsets;
            playerBindingSource.DataSource = playerOffsets;
            playerCarBindingSource.DataSource = playerCarOffsets;
            holocashAccountsBindingSource.DataSource = holocashAccounts;
            itemsBindingSource.DataSource = items;

            dataGridView1.DataSource = globalBindingSource;
            dataGridView2.DataSource = playerBindingSource;
            dataGridView3.DataSource = playerCarBindingSource;
            dataGridView4.DataSource = holocashAccountsBindingSource;
            dataGridView5.DataSource = itemsBindingSource;

            dataGridView1.Columns[0].Width = 50;
            dataGridView1.Columns[1].ReadOnly = true;

            searchTimer = new System.Threading.Timer(SearchProcess, null, 0, 250);
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (memory.Handle != IntPtr.Zero)
            {
                global.Disable();

                memory.CloseHandle();
            }
        }
        
        private void ReadGame()
        {
            if (global.Enabled)
            {
                long globalAddress = global.Value;

                UpdateEntriesValues(memory, dataGridView1, globalOffsets, new IntPtr(globalAddress), false);
                UpdateEntriesValues(memory, dataGridView2, playerOffsets, new IntPtr(global.Player), false);
                UpdateEntriesValues(memory, dataGridView3, playerCarOffsets, new IntPtr(global.PlayerCar), false);

                HolocashAccounts(globalAddress);
                InventoryEditor(globalAddress);

                // ReadArray<HolocashAccount>(new IntPtr(globalAddress + 0x5a8));
            }
        }

        private void SearchProcess(object state)
        {
            int processId = Memory.Utils.GetProcessIdFromName(EXECUTABLE_NAME);

            if (processId != -1 && memory.OpenHandle(processId))
            {
                statusLabel.Text = "Process opened: " + processId;

                Thread.Sleep(1000); // wait for the process to finish initializing itself

                if (!gamePatched)
                {
                    gamePatched = global.Enable();
                }
                else
                {
                    ReadGame();
                }
            }
            else if (memory.Process != null && !memory.ProcessAlive)
            {
                memory = new Memory();

                global = new Global(memory);

                statusLabel.Text = "Process has closed !";
            }
            else
            {
                statusLabel.Text = "Waiting for " + EXECUTABLE_NAME + " ...";
            }
        }

        private void UpdateEntriesValues(Memory memory, DataGridView dataGridView, BindingList<Offset> bindingList, IntPtr baseAddress, bool checkIfIsRunning)
        {
            foreach (Offset offset in bindingList.ToList())
            {
                string value = "??";

                if (offset.Type == typeof(int))
                {
                    if (offset.Frozen)
                    {
                        memory.Writer.WriteInt(baseAddress + offset.OffsetValue, int.Parse(offset.FrozenValue));
                    }

                    value = memory.Reader.ReadInt(baseAddress + offset.OffsetValue).ToString();
                }
                else if (offset.Type == typeof(float))
                {
                    if (offset.Frozen)
                    {
                        memory.Writer.WriteFloat(baseAddress + offset.OffsetValue, float.Parse(offset.FrozenValue));
                    }

                    value = memory.Reader.ReadFloat(baseAddress + offset.OffsetValue).ToString();
                }

                offset.Value = value;
            }
        }

        private void CheckFreeze(object sender, DataGridViewCellEventArgs e, DataGridView dataGridView)
        {
            int rowIndex = e.RowIndex;
            int columnIndex = e.ColumnIndex;

            if (rowIndex != -1 && columnIndex == 0)
            {
                Offset offset = dataGridView.Rows[rowIndex].DataBoundItem as Offset;
                bool isChecked = (bool)dataGridView[columnIndex, rowIndex].EditedFormattedValue;

                offset.FrozenValue = offset.Value;
                offset.Frozen = isChecked;
            }
        }

        private void EditValue(object sender, DataGridViewCellEventArgs e, DataGridView dataGridView, IntPtr baseAddress)
        {
            int rowIndex = e.RowIndex;
            int columnIndex = e.ColumnIndex;

            if (rowIndex != -1 && columnIndex == 2)
            {
                try
                {
                    Offset offset = dataGridView.Rows[rowIndex].DataBoundItem as Offset;

                    string value = dataGridView.Rows[rowIndex].Cells[columnIndex].Value.ToString();

                    bool result = false;

                    if (offset.Type == typeof(float))
                    {
                        float newvalue = float.Parse(value);

                        result = memory.Writer.WriteFloat(baseAddress + offset.OffsetValue, newvalue);
                    }
                    else if (offset.Type == typeof(int))
                    {
                        int newvalue = int.Parse(value);

                        result = memory.Writer.WriteInt(baseAddress + offset.OffsetValue, newvalue);
                    }

                    if (!result)
                    {
                        throw new Exception();
                    }
                }
                catch (Exception)
                {
                    MessageBox.Show("Error writing new value !", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void HolocashAccounts(long globalAddress)
        {
            IntPtr holocashAccountsPtr = new IntPtr(globalAddress + 0x5a8);
            IntPtr holocashAccountsArrayAddr = new IntPtr(memory.Reader.ReadInt64(holocashAccountsPtr));

            int holocashAccountsLength = memory.Reader.ReadInt(holocashAccountsArrayAddr + 0x18);

            int index = 0x20;
            for (int i = 0; i < holocashAccountsLength; i++)
            {
                IntPtr holocashAccountPtr = IntPtr.Add(holocashAccountsArrayAddr, index);
                IntPtr holocashAccountAddr = new IntPtr(memory.Reader.ReadInt64(holocashAccountPtr));

                HolocashAccount newHolocashAccount = new HolocashAccount(holocashAccountAddr);

                newHolocashAccount.Read(memory);

                HolocashAccount oldHolocashAccount = holocashAccounts.SingleOrDefault(p => p.KeyCode == newHolocashAccount.KeyCode);

                if (oldHolocashAccount == null)
                {
                    holocashAccountsBindingSource.Add(newHolocashAccount);
                }
                else
                {
                    oldHolocashAccount.Read(memory); //TODO
                }

                index += IntPtr.Size;
            }
        }
              
        private void InventoryEditor(long globalAddress)
        {
            IntPtr inventoryPtr = new IntPtr(globalAddress + 0x2A8);
            IntPtr inventoryArrayAddr = new IntPtr(memory.Reader.ReadInt64(inventoryPtr));

            int inventoryLength = memory.Reader.ReadInt(inventoryArrayAddr + 0x18);

            int index = 0x20;
            for (int i = 0; i < inventoryLength; i++)
            {
                IntPtr itemPtr = IntPtr.Add(inventoryArrayAddr, index);
                IntPtr itemAddr = new IntPtr(memory.Reader.ReadInt64(itemPtr));

                Item newItem = new Item(itemAddr);

                newItem.Read(memory);

                Item oldItem = items.SingleOrDefault(p => p.LocalizedName == newItem.LocalizedName);

                if (oldItem == null)
                {
                    itemsBindingSource.Add(newItem);
                }
                else
                {
                    oldItem.Read(memory); //TODO
                }

                index += IntPtr.Size;
            }
        }

        private List<T> ReadArray<T>(IntPtr address)
        {
            Type type = typeof(T);

            int length = memory.Reader.ReadInt(IntPtr.Add(address, 0x18));

            int index = 0x20;
            for (int i = 0; i < length; i++)
            {
                IntPtr itemAddress = new IntPtr(memory.Reader.ReadInt64(IntPtr.Add(address, index)));
                                               
                index += IntPtr.Size;
            }
            
            return new List<T>();
        }

        private void websiteLabel_Click(object sender, EventArgs e)
        {
            DialogResult dialogResult = MessageBox.Show("Would you like to visit the website ?", Application.ProductName, MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (dialogResult == DialogResult.Yes)
            {
                Process.Start("https://" + websiteLabel.Text);
            }
        }

        private void dataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            EditValue(sender, e, dataGridView1, new IntPtr(global.Value));
        }

        private void dataGridView2_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            EditValue(sender, e, dataGridView2, new IntPtr(global.Player));
        }

        private void dataGridView3_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
           EditValue(sender, e, dataGridView3, new IntPtr(global.PlayerCar));
        }

        private void dataGridView4_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            int rowIndex = e.RowIndex;

            if (rowIndex != -1)
            {
                try
                {
                    HolocashAccount holocashAccount = dataGridView4.Rows[rowIndex].DataBoundItem as HolocashAccount;

                    bool result = holocashAccount.Write(memory);

                    if (!result)
                    {
                        throw new Exception();
                    }
                }
                catch (Exception)
                {
                    MessageBox.Show("Error writing new values !", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void dataGridView5_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            int rowIndex = e.RowIndex;

            if (rowIndex != -1)
            {
                try
                {
                    Item item = dataGridView5.Rows[rowIndex].DataBoundItem as Item;

                    bool result = item.Write(memory);

                    if (!result)
                    {
                        throw new Exception();
                    }
                }
                catch (Exception)
                {
                    MessageBox.Show("Error writing new values !", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void dataGridView5_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            if (dataGridView5.IsCurrentCellDirty)
            {
                if (dataGridView5.Columns[dataGridView5.CurrentCell.ColumnIndex] is DataGridViewCheckBoxColumn)
                {
                    dataGridView5.CommitEdit(DataGridViewDataErrorContexts.Commit);
                }
            }
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            CheckFreeze(sender, e, dataGridView1);
        }

        private void dataGridView2_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            CheckFreeze(sender, e, dataGridView2);
        }

        private void dataGridView3_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            CheckFreeze(sender, e, dataGridView3);
        }

        private void tabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (((TabControl)sender).SelectedIndex == 0)
            {
                dataGridView1.Columns[0].Width = 50;
                dataGridView1.Columns[1].ReadOnly = true;
            }
            else if (((TabControl)sender).SelectedIndex == 1)
            {
                dataGridView2.Columns[0].Width = 50;
                dataGridView2.Columns[1].ReadOnly = true;
            }
            else if (((TabControl)sender).SelectedIndex == 2)
            {
                dataGridView3.Columns[0].Width = 50;
                dataGridView3.Columns[1].ReadOnly = true;
            }
            else if (((TabControl)sender).SelectedIndex == 3) // Holocash Accounts Editor
            {
                dataGridView4.Columns[0].ReadOnly = true;
                dataGridView4.Columns[1].ReadOnly = true;
            }
            else if (((TabControl)sender).SelectedIndex == 4) // Inventory Editor
            {
                dataGridView5.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.False;

                dataGridView5.Columns[0].ReadOnly = true;
                dataGridView5.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCellsExceptHeader;

                dataGridView5.Columns[1].ReadOnly = true;
                dataGridView5.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;

                dataGridView5.Columns[2].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                dataGridView5.Columns[3].AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
                dataGridView5.Columns[4].AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
                dataGridView5.Columns[5].AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
                dataGridView5.Columns[6].AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
                dataGridView5.Columns[7].AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
                dataGridView5.Columns[8].AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
                dataGridView5.Columns[9].AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
            }
        }
        
        private void tabControl_Selecting(object sender, TabControlCancelEventArgs e)
        {
            TabPage current = (sender as TabControl).SelectedTab;

            if(current.Text == "Inventory")
            {
                Width += 400;
                tabControl.Width += 400;
                dataGridView5.Width += 400;
            }
            else
            {
                Width = 410;
                tabControl.Width = 396;
                dataGridView5.Width = 382;
            }          
        }        
    }
}
