﻿using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Lab1IT
{
    public partial class Form1 : Form
    {
        dbManager dbm = new dbManager();
        string cellOldValue = "";
        string cellNewValue = "";
        public Form1()
        {
            InitializeComponent();
            cbTypes.SelectedIndex = 0;
        }

        private void butCreate_Click(object sender, EventArgs e)
        {
            dbm.CreateDB(tbCreateDBName.Text);
            tabControl.TabPages.Clear();
            dataGridView.Rows.Clear();
            dataGridView.Columns.Clear();
        }

        private void butAddTable_Click(object sender, EventArgs e)
        {
            if (dbm.AddTable(tbAddTableName.Text))
            {
                tabControl.TabPages.Add(tbAddTableName.Text);
            }
        }

        private void tabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            //int ind = tabControl.SelectedIndex;
            //if (ind != -1) VisualTable(dbm.GetTable(ind));
            int ind = tabControl.SelectedIndex;
            if (ind >= 0 && ind < dbm.GetTableNameList().Count)  // Перевірка меж індексу
            {
                VisualTable(dbm.GetTable(ind));
            }
        }

        void VisualTable(Table t)
        {
            try
            {
                dataGridView.Rows.Clear();
                dataGridView.Columns.Clear();

                foreach (Column c in t.tColumnsList)
                {
                    DataGridViewTextBoxColumn column = new DataGridViewTextBoxColumn();
                    column.Name = c.cName;
                    column.HeaderText = c.cName;
                    dataGridView.Columns.Add(column);
                }

                foreach (Row r in t.tRowsList)
                {
                    DataGridViewRow row = new DataGridViewRow();
                    foreach (string s in r.rValuesList)
                    {
                        DataGridViewCell cell = new DataGridViewTextBoxCell();
                        cell.Value = s;
                        row.Cells.Add(cell);
                    }
                    try
                    {
                        dataGridView.Rows.Add(row);
                    }
                    catch { }
                }
            }
            catch { }
        }

        private void butAddColumn_Click(object sender, EventArgs e)
        {
            if (dbm.AddColumn(tabControl.SelectedIndex, tbAddColumnName.Text, cbTypes.Text))
            {

                int ind = tabControl.SelectedIndex;
                if (ind != -1) VisualTable(dbm.GetTable(ind));
            }
        }

        private void butAddRow_Click(object sender, EventArgs e)
        {
            if (dbm.AddRow(tabControl.SelectedIndex))
            {

                int ind = tabControl.SelectedIndex;
                if (ind != -1) VisualTable(dbm.GetTable(ind));
            }
        }

        private void dataGridView_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            cellOldValue = dataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString();
        }

        private void dataGridView_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            cellNewValue = dataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString();
            if (!dbm.ChangeValue(cellNewValue, tabControl.SelectedIndex, e.ColumnIndex, e.RowIndex))
            {
                dataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = cellOldValue;
            }

            int ind = tabControl.SelectedIndex;
            if (ind != -1) VisualTable(dbm.GetTable(ind));
        }

        private void butDeleteRow_Click(object sender, EventArgs e)
        {
            if (dataGridView.Rows.Count == 0) return;
            try
            {
                dbm.DeleteRow(tabControl.SelectedIndex, dataGridView.CurrentCell.RowIndex);
            }
            catch { }

            int ind = tabControl.SelectedIndex;
            if (ind != -1) VisualTable(dbm.GetTable(ind));
        }

        private void butDeleteColumn_Click(object sender, EventArgs e)
        {
            if (dataGridView.Columns.Count == 0) return;
            try
            {
                dbm.DeleteColumn(tabControl.SelectedIndex, dataGridView.CurrentCell.ColumnIndex);
            }
            catch { }

            int ind = tabControl.SelectedIndex;
            if (ind != -1) VisualTable(dbm.GetTable(ind));
        }

        private void butDeleteTable_Click(object sender, EventArgs e)
        {
            if (tabControl.TabCount == 0) return;
            try
            {
                dbm.DeleteTable(tabControl.SelectedIndex);
                tabControl.TabPages.RemoveAt(tabControl.SelectedIndex);
            }
            catch { }
            if (tabControl.TabCount == 0) return;

            int ind = tabControl.SelectedIndex;
            if (ind != -1) VisualTable(dbm.GetTable(ind));
        }

        private void butSaveDB_Click(object sender, EventArgs e)
        {
            Stream myStream;

            sfdSaveDB.Filter = "tdb files (*.tdb)|*.tdb";
            sfdSaveDB.FilterIndex = 1;
            sfdSaveDB.RestoreDirectory = true;

            if (sfdSaveDB.ShowDialog() == DialogResult.OK)
            {
                if ((myStream = sfdSaveDB.OpenFile()) != null)
                {
                    // Code to write the stream goes here.
                    myStream.Close();

                    dbm.SaveDB(sfdSaveDB.FileName);
                }
            }
        }
        private void butOpen_Click(object sender, EventArgs e)
        {
            ofdOpenDB.Filter = "tdb files (*.tdb)|*.tdb";
            ofdOpenDB.FilterIndex = 1;
            ofdOpenDB.RestoreDirectory = true;

            if (ofdChooseFilePath.ShowDialog() == DialogResult.OK)
            {
                dbm.OpenDB(ofdChooseFilePath.FileName);
            }

            tabControl.TabPages.Clear();
            List<string> buf = dbm.GetTableNameList();
            foreach (string s in buf)
                tabControl.TabPages.Add(s);

            int ind = tabControl.SelectedIndex;
            if (ind != -1) VisualTable(dbm.GetTable(ind));
        }

        private void butJoinTables_Click(object sender, EventArgs e)
        {
            try
            {
                string tableName1 = tbTable1.Text;
                string tableName2 = tbTable2.Text;
                string commonField = tbCommonField.Text;

                if (string.IsNullOrEmpty(tableName1) || string.IsNullOrEmpty(tableName2) || string.IsNullOrEmpty(commonField))
                {
                    MessageBox.Show("Please enter both table names and the common field.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var joinedTable = dbm.JoinTables(tableName1, tableName2, commonField);
                TabPage newTab = new TabPage(joinedTable.tName);
                tabControl.TabPages.Add(newTab);

                dataGridView.Rows.Clear();
                dataGridView.Columns.Clear();

                foreach (var column in joinedTable.tColumnsList)
                {
                    dataGridView.Columns.Add(column.cName, column.cName);
                }

                foreach (var row in joinedTable.tRowsList)
                {
                    dataGridView.Rows.Add(row.rValuesList.ToArray());
                }
                
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
    }
}
