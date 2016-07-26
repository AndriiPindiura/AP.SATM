using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Management;
using System.Security;
using System.Security.Principal;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.IO;

namespace satmDebugger
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            ls = new List<satmCore.Events>();
            owners = new List<satmCore.Cores>();
            entries = new List<satmCore.Entries>();
        }

        private bool logged;
        private List<satmCore.Cores> owners;
        private List<satmCore.Events> ls;
        private List<satmCore.Entries> entries;
        private void btns()
        {
            if (logged) 
                button1.Enabled = false;   
            else 
                button1.Enabled = true;
            button2.Enabled = logged;
            button3.Enabled = logged;
            button4.Enabled = logged;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            /*StringBuilder token = new StringBuilder();
            DateTime startdate = DateTime.Now;
            try
            {
                logged = satmClient.SignIn();
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null) richTextBox1.AppendText(ex.InnerException.Message + Environment.NewLine);
                else richTextBox1.AppendText(ex.Message + Environment.NewLine);
                return;
            }*/
            btns();
        }

        private satmCore.satmICoreClient satmClient;

        private void button1_Click(object sender, EventArgs e)
        {
            StringBuilder token = new StringBuilder();
            DateTime startdate = DateTime.Now;
            satmClient = new satmCore.satmICoreClient();
            try
            {
                logged = satmClient.SignIn();
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null) richTextBox1.AppendText(ex.InnerException.Message + Environment.NewLine);
                else richTextBox1.AppendText(ex.Message + Environment.NewLine);
                return;
            }
            button4.PerformClick();
            btns();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                logged = satmClient.SignOut();
            }
            catch (Exception ex)
            {
                richTextBox1.AppendText(ex.Message + Environment.NewLine);
            }
            btns();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            DateTime startdate = DateTime.Now;
            List<string> cores = new List<string>();
            List<satmCore.Entries> selectedEntries = new List<satmCore.Entries>();
            if (listBox1.SelectedItems.Count == 0 && listBox2.SelectedItems.Count == 0)
            {
                MessageBox.Show("Необхідно вибрати принаймні один об’єкт!");
                return;
            }

            if (listBox1.SelectedItems.Count > 0)
            {
                String entry;
                foreach (Object item in listBox1.SelectedItems)
                {
                    entry = item as String;
                    string[] tmp = entry.Split('\\');
                    selectedEntries.Add(new satmCore.Entries { core = tmp[0], entryDescription = tmp[1] });
                }
            }
            else if (listBox2.SelectedItems.Count > 0)
            {
                //MessageBox.Show(listBox2.SelectedItems.Count.ToString());
                String s;
                foreach (Object item in listBox2.SelectedItems)
                {
                    s = item as String;
                    string core = owners.Find(y => y.Description.Contains(s)).Name;
                    //MessageBox.Show(core);

                    foreach (satmCore.Entries st in entries.FindAll(x => x.core.Contains(core)))
                    {
                        //MessageBox.Show(st.core);
                        selectedEntries.Add(new satmCore.Entries { core = owners.Find(x => x.Description.Contains(s)).Name, entryDescription = st.entryDescription });
                        //cores.Add(owners.Find(x => x.Description.Contains(s)).Name);
                    }
                }
            }
            /*foreach (satmCore.Entries tmpentry in selectedEntries)
            {
                MessageBox.Show(tmpentry.core + "\\" + tmpentry.entryDescription);
            }*/
            try
            {
                ls = satmClient.GetEvents(selectedEntries, dateTimePicker1.Value, dateTimePicker2.Value, false);
                dataGridView1.DataSource = ls;
                
                //File.WriteAllLines("test.txt", ls);
                richTextBox1.AppendText("Get " + dataGridView1.Rows.Count + " rows at: " + (DateTime.Now - startdate).ToString() + "\r\n");
            }
            catch (Exception ex)
            {
                richTextBox1.AppendText(ex.Message + "\r\n");
                logged = false;
                btns();
            }
            /*string text = "";

            foreach (System.Data.DataRowView item in listBox2.SelectedItems)
            {
                text = item.Row.Field<String>(0) + ", ";
                MessageBox.Show(text);

            }*/


            //MessageBox.Show(listBox2.SelectedValue.ToString());

            /*if (listBox2.SelectedItems.Count == 0 && listBox1.SelectedItems.Count == 0)
            {
                MessageBox.Show("Необхідно визначити принаймні один об’єкт!");
                return;
            }*/

            //MessageBox.Show(listBox1.SelectedItems.Count.ToString());
            //if (listBox1.SelectedItems.Count == 0)

            //if (listBox1.SelectedItems.Count > 0)
            //List<string> cores = new
            try
            {
                richTextBox1.AppendText("Welcome " + satmClient.ARM() + " delay: " + (DateTime.Now - startdate).ToString() + Environment.NewLine);
            }
            catch (Exception ex)
            {
                richTextBox1.AppendText(ex.Message + Environment.NewLine);
            }
            startdate = DateTime.Now;
            
            //satmClient.g
            //ls = satmClient.GetEvents("VS-AGB", DateTime.Parse("2014-08-04 00:00:00"), DateTime.Parse("2014-08-04 23:59:59"), false);
            //dataGridView1.DataSource = ls;//satmClient.GetEvents("VS-AGB", DateTime.Parse("2014-08-04 00:00:00"), DateTime.Parse("2014-08-04 23:59:59"), false);
            btns();

        }

        private void dataGridView1_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            foreach (DataGridViewColumn column in dataGridView1.Columns)
                column.SortMode = DataGridViewColumnSortMode.Programmatic;
            foreach (DataGridViewRow row in dataGridView1.Rows)
                if ((string)row.Cells["user"].Value == String.Empty)
                    if ((bool)row.Cells["legal"].Value)
                        row.DefaultCellStyle.BackColor = Color.DarkOliveGreen;
                    else
                        row.DefaultCellStyle.BackColor = Color.IndianRed;
                else
                    row.DefaultCellStyle.BackColor = Color.MidnightBlue;

        }

        private void button4_Click(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
            listBox2.Items.Clear();
            //MessageBox.Show("click");
            try
            {
                owners = satmClient.GetCores();
                foreach (satmCore.Cores c in owners)
                    listBox2.Items.Add(c.Description);
            }
            catch (CommunicationObjectFaultedException ex)
            {
                richTextBox1.AppendText(ex.Message + "\r\n");
                logged = false;
                btns();
            }
            /*listBox2.DataSource = owners;
            listBox2.DisplayMember = "Description";
            listBox2.ValueMember = "Name";*/
            entries = new List<satmCore.Entries>();
            entries = satmClient.GetEntries();
            foreach (satmCore.Entries item in entries)
                listBox1.Items.Add(item.core + @"\" + item.entryDescription);
        }

        private void Sort(string column, SortOrder sortOrder)
        {
            switch (column)
            {
                case "owner": 
                    {
                        if (sortOrder == SortOrder.Ascending)
                        {
                            dataGridView1.DataSource = ls.OrderBy(x => x.owner).ToList();
                        }
                        else
                        {
                            dataGridView1.DataSource = ls.OrderByDescending(x => x.owner).ToList();
                        }
                        break;
                    }
                case "entry":
                    {
                        if (sortOrder == SortOrder.Ascending)
                        {
                            dataGridView1.DataSource = ls.OrderBy(x => x.entry).ToList();
                        }
                        else
                        {
                            dataGridView1.DataSource = ls.OrderByDescending(x => x.entry).ToList();
                        }
                        break;
                    }
                case "startDate":
                    {
                        if (sortOrder == SortOrder.Ascending)
                        {
                            dataGridView1.DataSource = ls.OrderBy(x => x.startDate).ToList();
                        }
                        else
                        {
                            dataGridView1.DataSource = ls.OrderByDescending(x => x.startDate).ToList();
                        }
                        break;
                    }
            }
            /*foreach (DataGridViewRow row in dataGridView1.Rows)
                if ((bool)row.Cells["legal"].Value)
                    row.DefaultCellStyle.BackColor = Color.DarkOliveGreen;
                else
                    row.DefaultCellStyle.BackColor = Color.IndianRed;*/


        }

        private void dataGridView1_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            //dataGridView1.Sort(dataGridView1.Columns[e.ColumnIndex], ListSortDirection.Ascending);
            //dataGridView1.Columns[e.ColumnIndex].HeaderCell.SortGlyphDirection = SortOrder.Ascending;

            
            DataGridView grid = (DataGridView)sender;
            SortOrder so = SortOrder.None;
            if (grid.Columns[e.ColumnIndex].HeaderCell.SortGlyphDirection == SortOrder.None ||
                grid.Columns[e.ColumnIndex].HeaderCell.SortGlyphDirection == SortOrder.Ascending)
            {
                so = SortOrder.Descending;
            }
            else
            {
                so = SortOrder.Ascending;
            }
            //set SortGlyphDirection after databinding otherwise will always be none 
            Sort(grid.Columns[e.ColumnIndex].Name, so);
            grid.Columns[e.ColumnIndex].HeaderCell.SortGlyphDirection = so;
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            listBox2.SelectedIndex = -1;
        }

        private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            listBox1.SelectedIndex = -1;
        }


        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            //MessageBox.Show(e.ColumnIndex.ToString() + ":" + e.RowIndex.ToString());
            if (e.RowIndex > -1)
                richTextBox1.AppendText(dataGridView1["owner", e.RowIndex].Value.ToString() + "\\" + dataGridView1["uid", e.RowIndex].Value.ToString() + "\r\n");
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            richTextBox1.SelectionStart = richTextBox1.TextLength;
            richTextBox1.ScrollToCaret();
        }

    }






   



}
