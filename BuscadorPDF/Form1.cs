using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace BuscadorPDF
{
    public partial class Form1 : Form
    {
        public List<PDFCarregado> listaArquivos { get; set; }
        public Form1()
        {
            InitializeComponent();
            dataGridView1.AutoGenerateColumns = false;
            listaArquivos = new List<PDFCarregado>();
            var dir = new DirectoryInfo(@"C:\pdfs\");
            FileInfo[] files = dir.GetFiles("*.pdf", SearchOption.AllDirectories);
            for (int i = 0; i <= files.Length - 1; i++)
            {
                string txtPdf = "";
                using (PdfReader reader = new PdfReader(files[i].FullName))
                {
                    StringBuilder text = new StringBuilder();

                    for (int j = 1; j <= reader.NumberOfPages; j++)
                    {
                        text.Append(PdfTextExtractor.GetTextFromPage(reader, j));
                    }

                    txtPdf = text.ToString().ToUpper();
                }
                listaArquivos.Add(new PDFCarregado { Arquivo = files[i].Name, Caminho = files[i].FullName, TxtPDF = txtPdf });
            }
            CarregaGrid("");
        }

        private void CarregaGrid(string filtro)
        {
            List<PDFCarregado> sourceConsulta = null;
            if (string.IsNullOrEmpty(filtro))
                sourceConsulta = listaArquivos.Where(d => d.TxtPDF.Contains(filtro.ToUpper())).ToList();
            else
            {
                var palavras = filtro.Split(' ');
                foreach (var item in palavras)
                {
                    if(sourceConsulta == null)
                        sourceConsulta = listaArquivos.Where(d => d.TxtPDF.Contains(item.Trim().ToUpper())).ToList();
                    else
                        sourceConsulta = sourceConsulta.Where(d => d.TxtPDF.Contains(item.Trim().ToUpper())).ToList();
                }
            }
            dataGridView1.DataSource = sourceConsulta;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            CarregaGrid(textBox1.Text);
        }

        public class PDFCarregado
        {
            public string Arquivo { get; set; }
            public string Caminho { get; set; }
            public string TxtPDF { get; set; }
        }

        private void dataGridView1_RowEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (dataGridView1.Rows[e.RowIndex] != null)
            {
                var caminho = dataGridView1.Rows[e.RowIndex].Cells["Caminho"].Value.ToString();
                var acro = (AcroPDFLib.IAcroAXDocShim)axAcroPDF1.GetOcx();
                acro.LoadFile(caminho);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (dataGridView1.CurrentRow != null)
            {
                var caminho = dataGridView1.CurrentRow.Cells["Caminho"].Value.ToString();
                Process.Start(caminho);
            }
        }
    }
}
