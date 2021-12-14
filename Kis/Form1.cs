﻿
using System;
using SautinSoft.Document;
using System.Windows.Forms;
using System.Collections.Generic;
using SD = System.Data;
using MySql.Data.MySqlClient;
using Excel = Microsoft.Office.Interop.Excel;
namespace Kis
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            Excel.Application xlApp = new Excel.Application(); //Excel
            Excel.Workbook xlWB; //рабочая книга              
            Excel.Worksheet xlSht; //лист Excel   
            xlWB = xlApp.Workbooks.Open(@"D:\KIC\Kis\Kis\штатное для КИС 2021.xlsx"); //название файла Excel                                             
            xlSht = xlWB.Worksheets["штатний розклад"]; //название листа или 1-й лист в книге xlSht = xlWB.Worksheets[1];
            int iLastRow = xlSht.Cells[xlSht.Rows.Count, "A"].End[Excel.XlDirection.xlUp].Row;  //последняя заполненная строка в столбце А            
            var arrData = (object[,])xlSht.Range["A1:J" + iLastRow].Value; //берём данные с листа Excel
            //xlApp.Visible = true; //отображаем Excel     
            xlWB.Close(false); //закрываем книгу, изменения не сохраняем
            xlApp.Quit(); //закрываем Excel

            //настройка DataGridView
            this.dgv.Rows.Clear();
            int RowsCount = arrData.GetUpperBound(0);
            int ColumnsCount = arrData.GetUpperBound(1);
            dgv.RowCount = RowsCount; //кол-во строк в DGV
            dgv.ColumnCount = ColumnsCount; //кол-во столбцов в DGV

            //заполняем DataGridView данными из массива
            int i, j;
            for (i = 1; i <= RowsCount; i++)
            {
                for (j = 1; j <= ColumnsCount; j++)
                {
                    dgv.Rows[i - 1].Cells[j - 1].Value = arrData[i, j];
                }
            }
            /*
            queryText = "info";
            selection();
            for (int i = 0; i < dgv.Rows.Count - 1; i++)
            {
                if (dgv[0, i].Value != null && dgv[0, i].Value.ToString() != "")
                {
                    comboBox1.Items.Add(dgv[0, i].Value.ToString());
                    comboBox6.Items.Add(dgv[0, i].Value.ToString());
                }
                if (dgv[1, i].Value != null && dgv[1, i].Value.ToString() != "")
                {
                    comboBox5.Items.Add(dgv[1, i].Value.ToString());
                    comboBox2.Items.Add(dgv[1, i].Value.ToString());
                }
                if (dgv[2, i].Value != null && dgv[2, i].Value.ToString() != "")
                {
                    comboBox3.Items.Add(dgv[2, i].Value.ToString());
                    comboBox7.Items.Add(dgv[2, i].Value.ToString());
                }
                if (dgv[3, i].Value != null && dgv[3, i].Value.ToString() != "")
                {
                    comboBox4.Items.Add(dgv[3, i].Value.ToString());
                    comboBox8.Items.Add(dgv[3, i].Value.ToString());
                }
            }
            dgv1.Hide();
            */
        }
        public MySqlConnection mycon;
        public MySqlCommand mycom;
        public string connect = "Server=localhost;Database=kis;Uid=root;pwd=sk2v100nfyrjd;charset=utf8;";
        public SD.DataSet ds;
        string queryText;
        string selectedColumn;
        int selectedRow;
        string selectedContent;
        string selectedTable = "info";

        private void button2_Click(object sender, EventArgs e)
        {
            
        }


        public void selection()
        {
            try
            {
                string script;
                if (queryText != null)
                    script = "Select * from " + queryText + ";";
                else
                    script = "SHOW TABLES;";
                selectedTable = queryText;
                mycon = new MySqlConnection(connect);
                mycon.Open();
                MySqlDataAdapter ms_data = new MySqlDataAdapter(script, connect);
                SD.DataTable table = new SD.DataTable();
                ms_data.Fill(table);
                dgv.DataSource = table;
                mycon.Close();
            }
            catch
            {
                MessageBox.Show("Connection lost");
            }
        }

        private void button9_Click(object sender, EventArgs e)
        {
            try
            {
                string script;
                if (selectedContent != null)
                {
                    int i;
                    if (int.TryParse(selectedContent, out i))
                        script = "Delete from " + selectedTable + " where " + selectedColumn + "=" + selectedContent + ";";
                    else script = "Delete from " + selectedTable + " where " + selectedColumn + "='" + selectedContent + "';";
                }
                else
                {
                    script = "Delete from " + selectedTable + ";";
                }
                DialogResult dialogResult = MessageBox.Show("Ви впевнені що хочете видалити\n всі записи де" + selectedColumn + "=" + selectedContent + "?", "Видалення", MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.Yes)
                {
                    mycon = new MySqlConnection(connect);
                    mycon.Open();
                    MySqlDataAdapter ms_data = new MySqlDataAdapter(script, connect);
                    SD.DataTable table = new SD.DataTable();
                    ms_data.Fill(table);
                    script = "Select * from " + selectedTable + ";";
                    ms_data = new MySqlDataAdapter(script, connect);
                    table = new SD.DataTable();
                    ms_data.Fill(table);
                    dgv.DataSource = table;
                    mycon.Close();
                }
                else if (dialogResult == DialogResult.No)
                {

                }

            }
            catch
            {
                MessageBox.Show("Connection lost");
            }
        }
        

        private void button10_Click(object sender, EventArgs e)
        {
            try
            {
                string script;
                script = "SELECT DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '" + selectedTable + "';";
                mycon = new MySqlConnection(connect);
                mycon.Open();
                MySqlDataAdapter ms_data = new MySqlDataAdapter(script, connect);
                SD.DataTable table = new SD.DataTable();
                ms_data.Fill(table);
                dgv1.DataSource = table;
                InputBox.InputBox inputBox = new InputBox.InputBox();
                List<string> l = new List<string>();
                script = "Insert into " + selectedTable + " values (";
                for (int i = 0; i < dgv1.Rows.Count - 1; i++)
                {
                    l.Add(dgv1[0, i].Value.ToString());
                    if (i != dgv1.Rows.Count - 2)
                    {
                        if (l[i] == "int")
                        {
                            MessageBox.Show("Введіть ціле число");
                            script += inputBox.getString() + ", ";
                        }
                        if (l[i] == "char")
                        {
                            MessageBox.Show("Введіть рядок символів");
                            script += "'" + inputBox.getString() + "', ";
                        }
                        if (l[i] == "varchar")
                        {
                            MessageBox.Show("Введіть рядок символів");
                            script += selectedColumn + " = " + "'" + inputBox.getString() + "' ";
                        }
                        if (l[i] == "date")
                        {
                            MessageBox.Show("Введіть дату у форматі рррр-мм-дд");
                            script += "'" + inputBox.getString() + "', ";
                        }
                    }
                    else
                    {
                        if (l[i] == "int")
                        {
                            MessageBox.Show("Введіть ціле число");
                            script += inputBox.getString() + ");";
                        }
                        if (l[i] == "char")
                        {
                            MessageBox.Show("Введіть рядок символів");
                            script += "'" + inputBox.getString() + "');";
                        }
                        if (l[i] == "varchar")
                        {
                            MessageBox.Show("Введіть рядок символів");
                            script += "'" + inputBox.getString() + "');";
                        }
                        if (l[i] == "date")
                        {
                            MessageBox.Show("Введіть дату у форматі рррр-мм-дд");
                            script += "'" + inputBox.getString() + "');";
                        }
                    }
                }
                MessageBox.Show(script);
                ms_data = new MySqlDataAdapter(script, connect);
                table = new SD.DataTable();
                ms_data.Fill(table);
                dgv.DataSource = table;
                selection();
                mycon.Close();
                if (dgv[0, dgv.Rows.Count - 1].Value != null)
                {
                    comboBox1.Items.Add(dgv[0, dgv.Rows.Count - 1].Value.ToString());
                    comboBox6.Items.Add(dgv[0, dgv.Rows.Count - 1].Value.ToString());
                }
                if (dgv[1, dgv.Rows.Count - 1].Value != null)
                {
                    comboBox5.Items.Add(dgv[1, dgv.Rows.Count - 1].Value.ToString());
                    comboBox2.Items.Add(dgv[1, dgv.Rows.Count - 1].Value.ToString());
                }
                if (dgv[2, dgv.Rows.Count - 1].Value != null)
                {
                    comboBox3.Items.Add(dgv[2, dgv.Rows.Count - 1].Value.ToString());
                    comboBox7.Items.Add(dgv[2, dgv.Rows.Count - 1].Value.ToString());
                }
                if (dgv[3, dgv.Rows.Count - 1].Value != null)
                {
                    comboBox4.Items.Add(dgv[3, dgv.Rows.Count - 1].Value.ToString());
                    comboBox8.Items.Add(dgv[3, dgv.Rows.Count - 1].Value.ToString());
                }

            }
            catch { MessageBox.Show("Connection lost"); }
        }

        private void button11_Click(object sender, EventArgs e)
        {
            try
            {
                string script;
                script = "SELECT DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'info' AND COLUMN_NAME = '" + selectedColumn + "';";
                mycon = new MySqlConnection(connect);
                mycon.Open();
                MySqlDataAdapter ms_data = new MySqlDataAdapter(script, connect);
                SD.DataTable table = new SD.DataTable();
                ms_data.Fill(table);
                dgv1.DataSource = table;
                InputBox.InputBox inputBox = new InputBox.InputBox();
                List<string> l = new List<string>();
                script = "Update info set ";
                for (int i = 0; i < dgv1.Rows.Count - 1; i++)
                {
                    l.Add(dgv1[0, i].Value.ToString());
                    if (l[i] == "int")
                    {
                        MessageBox.Show("Введіть ціле число");
                        script += selectedColumn + " = " + inputBox.getString() + " ";
                    }
                    if (l[i] == "char")
                    {
                        MessageBox.Show("Введіть рядок символів");
                        script += selectedColumn + " = " + "'" + inputBox.getString() + "' ";
                    }
                    if (l[i] == "varchar")
                    {
                        MessageBox.Show("Введіть рядок символів");
                        script += selectedColumn + " = " + "'" + inputBox.getString() + "' ";
                    }
                    if (l[i] == "date")
                    {
                        MessageBox.Show("Введіть дату у форматі рррр-мм-дд");
                        script += selectedColumn + " = " + "'" + inputBox.getString() + "' ";
                    }
                }
                string scr = "SELECT C.COLUMN_NAME FROM information_schema.table_constraints AS pk INNER JOIN information_schema.KEY_COLUMN_USAGE AS C ON C.TABLE_NAME = pk.TABLE_NAME AND C.CONSTRAINT_NAME = pk.CONSTRAINT_NAME AND C.TABLE_SCHEMA = pk.TABLE_SCHEMA WHERE pk.TABLE_NAME = 'info' AND pk.CONSTRAINT_TYPE = 'PRIMARY KEY'; ";
                ms_data = new MySqlDataAdapter(scr, connect);
                table = new SD.DataTable();
                ms_data.Fill(table);
                dgv1.DataSource = table;
                
                l = new List<string>();

                for (int i = 0; i < dgv1.Rows.Count - 1; i++)
                    l.Add(dgv1[0, i].Value.ToString());
                
                int ind = 0;
                for (int i = 0; i < dgv1.Columns.Count; i++)
                    if (dgv.Columns[i].Name == l[0])
                    {
                        MessageBox.Show("all good1");
                        ind = i;
                        break;
                    }
                MessageBox.Show("all good2");
                script += "where " + l[0] + " = " + dgv[ind, selectedRow].Value.ToString() + ";";
                MessageBox.Show(script);
                ms_data = new MySqlDataAdapter(script, connect);
                table = new SD.DataTable();
                ms_data.Fill(table);
                dgv.DataSource = table;
                MessageBox.Show("all good3");
                selection();
                mycon.Close();
            }
            catch { MessageBox.Show("Connection lost"); }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            groupBox1.Show();
            groupBox2.Hide();
            groupBox4.Hide();
            button5.Enabled = false;
            button6.Enabled = true;
            button7.Enabled = true;
            
        }

        private void button6_Click(object sender, EventArgs e)
        {
            groupBox1.Hide();
            groupBox2.Show();
            groupBox4.Hide();
            button6.Enabled = false;
            button5.Enabled = true;
            button7.Enabled = true;
        }

        private void button7_Click(object sender, EventArgs e)
        {
            groupBox1.Hide();
            groupBox2.Hide();
            groupBox4.Show();
            button7.Enabled = false;
            button5.Enabled = true;
            button6.Enabled = true;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
        private void dgv_SelectionChanged(object sender, DataGridViewCellEventArgs e)
        {
            selectedColumn = dgv.Columns[e.ColumnIndex].Name;
            MessageBox.Show(selectedColumn);
            try
            {
                selectedContent = dgv[e.ColumnIndex, e.RowIndex].Value.ToString();
                MessageBox.Show(selectedContent);
            }
            catch
            {
                MessageBox.Show("Виберіть ланку для видалення або редагування");
            }
            selectedRow = dgv.Rows[e.RowIndex].Index;

        }



        private void dgv_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            selectedColumn = dgv.Columns[e.ColumnIndex].Name;
            try
            {
                selectedContent = dgv[e.ColumnIndex, e.RowIndex].Value.ToString();
            }
            catch
            {
                MessageBox.Show("Виберіть ланку для видалення або редагування");
            }
            selectedRow = dgv.Rows[e.RowIndex].Index;
        }
    }
}
