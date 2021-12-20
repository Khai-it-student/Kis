
using System;
using SautinSoft.Document;
using System.Windows.Forms;
using System.Collections.Generic;
using SD = System.Data;
using MySql.Data.MySqlClient;
using Excel = Microsoft.Office.Interop.Excel;
using System.Collections;
using System.Linq;

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
            int iLastRow = xlSht.Cells[xlSht.Rows.Count, "B"].End[Excel.XlDirection.xlUp].Row;  //последняя заполненная строка в столбце А            
            var arrTeachers = (object[,])xlSht.Range["B15:B" + iLastRow].Value; //берём данные с листа Excel
            var arrPositions = (object[,])xlSht.Range["F15:F" + iLastRow].Value; //берём данные с листа Excel
            int RowsCount = arrTeachers.GetUpperBound(0);

            teachersTable();

            for (int i = 1; i <= RowsCount; i++)
            {
                if (arrTeachers[i, 1] == null || arrPositions[i, 1] == null)
                    break;
                for (int j = 0; j < dgv.Rows.Count - 1; j++)
                {
                    if (arrTeachers[i, 1].ToString() == dgv[1, j].Value.ToString())
                        arrTeachers[i, 1] = "-1";
                }
            }

            try
            {
                string script;
                mycon = new MySqlConnection(connect);
                mycon.Open();
                MySqlDataAdapter ms_data;
                SD.DataTable table;
                script = "insert into teachers values";
                for (int i = 1; i <= RowsCount; i++)
                {                    
                    if (arrTeachers[i, 1] == null || arrPositions[i, 1] == null)
                        break;
                    if (arrTeachers[i, 1].ToString() == "-1")
                        continue;
                    if (script == "insert into teachers values") { 
                        script += "(\"" + arrTeachers[i, 1].ToString() + "\",\"" + arrPositions[i, 1].ToString() + "\")";
                        continue;
                    }
                    if (arrTeachers[i+1, 1] == null || arrPositions[i+1, 1] == null)
                        script += "(\"" + arrTeachers[i, 1].ToString() + "\",\"" + arrPositions[i, 1].ToString() + "\")";
                    else
                        script += ",(\"" + arrTeachers[i, 1].ToString() + "\",\"" + arrPositions[i, 1].ToString() + "\")";
                }
                script += ";";
                if (script!= "insert into teachers values;") {
                ms_data = new MySqlDataAdapter(script, connect);
                table = new SD.DataTable();
                ms_data.Fill(table);
                dgv.DataSource = table;
                }
                mycon.Close();
            }
            catch
            {
                MessageBox.Show("Connection lost");
            }
            //xlApp.Visible = true; //отображаем Excel     
            xlWB.Close(false); //закрываем книгу, изменения не сохраняем
            xlApp.Quit(); //закрываем Excel
          
            selectedTable = "info";
            selection();
            for (int i = 0; i < dgv.Rows.Count - 1; i++)
            {
                if (dgv[0, i].Value != null && dgv[0, i].Value.ToString() != "")
                {
                    kafedra.Items.Add(dgv[0, i].Value.ToString());
                    kafedra1.Items.Add(dgv[0, i].Value.ToString());
                }
                if (dgv[1, i].Value != null && dgv[1, i].Value.ToString() != "")
                {
                    year1.Items.Add(dgv[1, i].Value.ToString());
                    year.Items.Add(dgv[1, i].Value.ToString());
                }
            }
            dgv1.Hide();
            selectedColumn = dgv.Columns[0].Name;
        }
        public MySqlConnection mycon;
        public MySqlCommand mycom;
        public string connect = "Server=localhost;Database=kis;Uid=root;pwd=sk2v100nfyrjd;charset=utf8;";
        public SD.DataSet ds;
        string queryText;
        string selectedColumn;
        int selectedRow = 0;
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
                script = "Select * from " + selectedTable + ";";
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
                    if (selectedTable == "info")
                    {
                        year.Items.Clear();
                        year1.Items.Clear();
                        kafedra.Items.Clear();
                        kafedra1.Items.Clear();
                        for (int i = 0; i < dgv.Rows.Count - 1; i++)
                        {
                            if (dgv[0, i].Value != null && dgv[0, i].Value.ToString() != "")
                            {
                                kafedra.Items.Add(dgv[0, i].Value.ToString());
                                kafedra1.Items.Add(dgv[0, i].Value.ToString());
                            }
                            if (dgv[1, i].Value != null && dgv[1, i].Value.ToString() != "")
                            {
                                year1.Items.Add(dgv[1, i].Value.ToString());
                                year.Items.Add(dgv[1, i].Value.ToString());
                            }
                        }
                    }
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
                if (dgv[0, dgv.Rows.Count - 1].Value != null && selectedTable == "info")
                {
                    kafedra.Items.Add(dgv[0, dgv.Rows.Count - 1].Value.ToString());
                    kafedra1.Items.Add(dgv[0, dgv.Rows.Count - 1].Value.ToString());
                }
                if (dgv[1, dgv.Rows.Count - 1].Value != null && selectedTable == "info")
                {
                    year1.Items.Add(dgv[1, dgv.Rows.Count - 1].Value.ToString());
                    year.Items.Add(dgv[1, dgv.Rows.Count - 1].Value.ToString());
                }

            }
            catch { MessageBox.Show("Connection lost"); }
        }

        private void button11_Click(object sender, EventArgs e)
        {
            try
            { 
                string script;
                script = "SELECT DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '" + selectedTable + "' AND COLUMN_NAME = '" + selectedColumn + "';";
                mycon = new MySqlConnection(connect);
                mycon.Open();
                MySqlDataAdapter ms_data = new MySqlDataAdapter(script, connect);
                SD.DataTable table = new SD.DataTable();
                ms_data.Fill(table);
                dgv1.DataSource = table;
                InputBox.InputBox inputBox = new InputBox.InputBox();
                List<string> l = new List<string>();
                script = "Update " + selectedTable + " set ";              
                for (int i = 0; i < dgv1.Rows.Count - 1; i++)
                {
                    l.Add(dgv1[0, i].Value.ToString());
                    if (l[i] == "int")
                    {
                        //MessageBox.Show("Введіть ціле число");
                        script += "`"+selectedColumn + "` = " + inputBox.getString() + " ";
                    }
                    if (l[i] == "char")
                    {
                        MessageBox.Show("Введіть рядок символів");
                        script += "`"+selectedColumn + "` = " + "'" + inputBox.getString() + "' ";
                    }
                    if (l[i] == "varchar")
                    {
                        MessageBox.Show("Введіть рядок символів");
                        script += "`"+selectedColumn + "` = " + "'" + inputBox.getString() + "' ";
                    }
                    if (l[i] == "date")
                    {
                        MessageBox.Show("Введіть дату у форматі рррр-мм-дд");
                        script += "`"+selectedColumn + "` = " + "'" + inputBox.getString() + "' ";
                    }
                }
                string scr = "SELECT C.COLUMN_NAME FROM information_schema.table_constraints AS pk INNER JOIN information_schema.KEY_COLUMN_USAGE AS C ON C.TABLE_NAME = pk.TABLE_NAME AND C.CONSTRAINT_NAME = pk.CONSTRAINT_NAME AND C.TABLE_SCHEMA = pk.TABLE_SCHEMA WHERE pk.TABLE_NAME = '" + selectedTable + "' AND pk.CONSTRAINT_TYPE = 'PRIMARY KEY'; ";
                ms_data = new MySqlDataAdapter(scr, connect);
                table = new SD.DataTable();
                ms_data.Fill(table);
                dgv1.DataSource = table;
                dgv1.Show();
                l = new List<string>();

                for (int i = 0; i < dgv1.Rows.Count - 1; i++)
                    l.Add(dgv1[0, i].Value.ToString());
                
                int ind = 0;
                for (int i = 0; i < dgv.Columns.Count; i++)
                    if (dgv.Columns[i].Name == l[0])
                    {                        
                        ind = i;
                        break;
                    }
                script += "where " + l[0] + " = \"" + dgv[ind, selectedRow].Value.ToString() + "\";";
               MessageBox.Show(script);
                ms_data = new MySqlDataAdapter(script, connect);
                table = new SD.DataTable();                
                ms_data.Fill(table);
                dgv.DataSource = table;
                selection();
                mycon.Close();
                if (selectedTable == "info")
                {
                    year.Items.Clear();
                    year1.Items.Clear();
                    kafedra.Items.Clear();
                    kafedra1.Items.Clear();
                    for (int i = 0; i < dgv.Rows.Count - 1; i++)
                    {
                        if (dgv[0, i].Value != null && dgv[0, i].Value.ToString() != "")
                        {
                            kafedra.Items.Add(dgv[0, i].Value.ToString());
                            kafedra1.Items.Add(dgv[0, i].Value.ToString());
                        }
                        if (dgv[1, i].Value != null && dgv[1, i].Value.ToString() != "")
                        {
                            year1.Items.Add(dgv[1, i].Value.ToString());
                            year.Items.Add(dgv[1, i].Value.ToString());
                        }
                    }
                }
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

        private void button8_Click(object sender, EventArgs e)
        {
            try
            {
                string script;
                script = "Select * from info;";
                mycon = new MySqlConnection(connect);
                mycon.Open();
                MySqlDataAdapter ms_data = new MySqlDataAdapter(script, connect);
                SD.DataTable table = new SD.DataTable();
                ms_data.Fill(table);
                dgv.DataSource = table;
                mycon.Close();
                selectedTable = "info";
                selectedColumn = dgv.Columns[0].Name;
                selectedRow = 0;
            }
            catch
            {
                MessageBox.Show("Connection lost");
            }
        }

        private void button12_Click(object sender, EventArgs e)
        {
            teachersTable();
        }
        private void teachersTable()
        {
            try
            {
                string script;
                script = "Select * from teachers;";
                mycon = new MySqlConnection(connect);
                mycon.Open();
                MySqlDataAdapter ms_data = new MySqlDataAdapter(script, connect);
                SD.DataTable table = new SD.DataTable();
                ms_data.Fill(table);
                dgv.DataSource = table;
                mycon.Close();
                selectedTable = "teachers";
                selectedColumn = dgv.Columns[0].Name;
                selectedRow = 0;
            }
            catch
            {
                MessageBox.Show("Connection lost");
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            int[] plan = new int[5];
            int[] fact = new int[5];
            selectedTable = "teachers";
            selection();
            for (int i = 0; i < dgv.Rows.Count - 1; i++)
            {
                if (dgv[0, i].Value != null && dgv[0, i].Value.ToString() != "" && dgv[0, i].Value != null && dgv[0, i].Value.ToString() != "")
                {
                    switch (dgv[0, i].Value.ToString())
                    {
                        case "1": plan[0] += Convert.ToInt32(dgv[3, i].Value); fact[0] += Convert.ToInt32(dgv[4, i].Value); break;
                        case "2": plan[1] += Convert.ToInt32(dgv[3, i].Value); fact[1] += Convert.ToInt32(dgv[4, i].Value); break;
                        case "3": plan[2] += Convert.ToInt32(dgv[3, i].Value); fact[2] += Convert.ToInt32(dgv[4, i].Value); break;
                        case "4": plan[3] += Convert.ToInt32(dgv[3, i].Value); fact[3] += Convert.ToInt32(dgv[4, i].Value); break;
                        case "5": plan[4] += Convert.ToInt32(dgv[3, i].Value); fact[4] += Convert.ToInt32(dgv[4, i].Value); break;
                    }
                }
            }
            for (int i = 0; i < 5; i++)
            {
                MessageBox.Show(plan[i].ToString());
                MessageBox.Show(fact[i].ToString());
            }

        }

        private void button1_Click(object sender, EventArgs e)
        {
            //Dictionary<string, string> idPosition = new Dictionary<string, string>();
            int[] plan = new int[5];
            int[] fact = new int[5];
            selectedTable = "teachers";
            selection();
            for (int i = 0; i < dgv.Rows.Count - 1; i++)
            {
                if (dgv[0, i].Value != null && dgv[0, i].Value.ToString() != "" && dgv[0, i].Value != null && dgv[0, i].Value.ToString() != "")
                {                             
                    switch (dgv[0, i].Value.ToString()) {
                        case "1": plan[0] += Convert.ToInt32(dgv[3, i].Value); fact[0] += Convert.ToInt32(dgv[4, i].Value); break;
                        case "2": plan[1] += Convert.ToInt32(dgv[3, i].Value); fact[1] += Convert.ToInt32(dgv[4, i].Value); break;
                        case "3": plan[2] += Convert.ToInt32(dgv[3, i].Value); fact[2] += Convert.ToInt32(dgv[4, i].Value); break;
                        case "4": plan[3] += Convert.ToInt32(dgv[3, i].Value); fact[3] += Convert.ToInt32(dgv[4, i].Value); break;
                        case "5": plan[4] += Convert.ToInt32(dgv[3, i].Value); fact[4] += Convert.ToInt32(dgv[4, i].Value); break;
                    }
                }                
            }
            for (int i = 0; i<5; i++)
            {
                MessageBox.Show(plan[i].ToString());
                MessageBox.Show(fact[i].ToString());
            }
        }
    }
}
