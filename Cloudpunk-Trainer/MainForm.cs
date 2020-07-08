using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Windows.Forms;
using Cloudpunk_Trainer.Cloudpunk;
using Cloudpunk_Trainer.Models;
using Process_Memory;

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
        Player player;
        PlayerCar playerCar;

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
        }

        List<Offset> globalOffsets;
        List<Offset> playerOffsets;
        List<Offset> playerCarOffsets;

        BindingSource globalBindingSource;
        BindingSource playerBindingSource;
        BindingSource playerCarBindingSource;

        private void MainForm_Load(object sender, EventArgs e)
        {
            memory = new Memory();

            global = new Global(memory);
            player = new Player(memory);
            playerCar = new PlayerCar(memory);          

            globalOffsets = new List<Offset>();
            playerOffsets = new List<Offset>();
            playerCarOffsets = new List<Offset>();

            globalBindingSource = new BindingSource { DataSource = globalOffsets };
            playerBindingSource = new BindingSource { DataSource = playerOffsets };
            playerCarBindingSource = new BindingSource { DataSource = playerCarOffsets };

            dataGridView1.DataSource = globalBindingSource;
            dataGridView2.DataSource = playerBindingSource;
            dataGridView3.DataSource = playerCarBindingSource;
            
            AddEntries(global, globalBindingSource);
            AddEntries(player, playerBindingSource);
            AddEntries(playerCar, playerCarBindingSource);
            
            dataGridView1.Columns[0].Width = 50;
            dataGridView1.Columns[1].ReadOnly = true;

            searchTimer = new System.Threading.Timer(SearchProcess, null, 0, 250);          
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if(memory.Handle != IntPtr.Zero)
            {
                global.Disable();
                player.Disable();
                playerCar.Disable();

                memory.CloseHandle();
            }
        }

        private void PatchGame()
        {
            global.Enable();
            player.Enable();
            playerCar.Enable();
        }

        private void ReadGame()
        {
            if(global.Enabled)
            {
                long globalAddress = global.Value;

                UpdateEntriesValues(memory, dataGridView1, globalBindingSource, global, new IntPtr(globalAddress), false);
            }

            if (player.Enabled)
            {
                long playerAddress = player.Value;

                UpdateEntriesValues(memory, dataGridView2, playerBindingSource, player, new IntPtr(playerAddress), false);
            }

            if (playerCar.Enabled)
            {
                long playerCarAddress = playerCar.Value;

                UpdateEntriesValues(memory, dataGridView3, playerCarBindingSource, playerCar, new IntPtr(playerCarAddress), false);
            }
        }

        private void SearchProcess(object state)
        {
            int processId = Process_Memory.Utils.GetProcessIdFromName(EXECUTABLE_NAME);
            
            if(processId != -1 && memory.OpenHandle(processId))
            {
                statusLabel.Text = "Process opened: " + processId;

                Thread.Sleep(1000); // wait for the process to finish initializing itself

                if (!gamePatched)
                {
                    gamePatched = true;
                    PatchGame();                    
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
                player = new Player(memory);
                playerCar = new PlayerCar(memory);

                statusLabel.Text = "Process has closed !";                              
            }
            else
            {
                statusLabel.Text = "Waiting for " + EXECUTABLE_NAME + " ...";
            }            
        }

        private void AddEntries(Hack hack, BindingSource bindingSource)
        {
            for (int i = 0; i <= hack.Offsets.Count - 1; i++)
            {
                bindingSource.Add(hack.Offsets[i]);
            }
        }

        private void UpdateEntriesValues(Memory memory, DataGridView dataGridView, BindingSource bindingSource, Hack hack, IntPtr baseAddress, bool checkIfIsRunning)
        {
            foreach (Offset offset in bindingSource)
            {
                string value = "??";

                if(offset.Type == typeof(int))
                {
                    if (offset.Frozen)
                    {
                        memory.Writer.WriteInt(new IntPtr(hack.Value + offset.OffsetValue), int.Parse(offset.FrozenValue));
                    }

                    value = memory.Reader.ReadInt(new IntPtr(hack.Value + offset.OffsetValue)).ToString();
                }
                else if (offset.Type == typeof(float))
                {
                    if (offset.Frozen)
                    {
                        memory.Writer.WriteFloat(new IntPtr(hack.Value + offset.OffsetValue), float.Parse(offset.FrozenValue));
                    }

                    value = memory.Reader.ReadFloat(baseAddress + offset.OffsetValue).ToString();
                }

                offset.Value = value;
            }
            
            dataGridView.Refresh();
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

                    if(!result)
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
            EditValue(sender, e, dataGridView2, new IntPtr(player.Value));
        }

        private void dataGridView3_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            EditValue(sender, e, dataGridView3, new IntPtr(playerCar.Value));
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

        private void dataGridView2_Enter(object sender, EventArgs e)
        {
            dataGridView2.Columns[0].Width = 50;
            dataGridView2.Columns[1].ReadOnly = true;
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
        }
    }
}
