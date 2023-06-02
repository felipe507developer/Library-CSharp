using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using Proceso;

namespace Library
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private async void SelectFileBbutton_Click(object sender, EventArgs e)
        {
            lblStatus.Text = "In process...";

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Text Files|*.txt";
            openFileDialog.Title = "Select ISBN Numbers File";

            if (openFileDialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            string filePath = openFileDialog.FileName;
            string[] isbns = File.ReadAllLines(filePath);

            Process process = new Process();
            (List<string> invalidIsbns, string message) = await process.Requests(filePath);

            if (!string.IsNullOrEmpty(message))
            {
                MessageBox.Show(message);

                string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                string fileName = "output.csv";
                string[] files = Directory.GetFiles(folderPath, fileName, SearchOption.TopDirectoryOnly);

                if (files.Length > 0)
                {
                   
                    string outputFilePath = files[0];

                    DataTable dataTable = process.LoadCsvToDataTable(outputFilePath);

                    if (invalidIsbns.Count > 0)
                    {
                        string invalidCodes = string.Join("\n", invalidIsbns);
                        MessageBox.Show($"These codes were not processed because they do not meet the ISBN-10 or ISBN-13 standards:\n{invalidCodes}");
                    }

                    dataGridView1.DataSource = dataTable;
                    dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                    dataGridView1.ReadOnly = true;

                    lblStatus.Text = "OK";
                    lblStatus.ForeColor = Color.Green;
                }
                    
            }
        }
    }
}
