using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BuscadorPDF
{
    public partial class FrmBuscadorPdf : Form
    {
        public List<PDFCarregado> listaArquivos { get; set; }
        private bool _listagemArquivosVisivel;
        private string _fonteArquivos;
        public FrmBuscadorPdf()
        {
            InitializeComponent();
            dataGridView1.AutoGenerateColumns = false;
            _listagemArquivosVisivel = true;
            dataGridView1.Visible = false;
            _fonteArquivos = @"C:\Zuera\";
            btnFonteArquivo.Text = "Fonte arquivos: " + _fonteArquivos;
        }

        public async Task CarregaDadosArquivos()
        {
            pictureBox1.Visible = true;
            dataGridView1.Visible = false;

            listaArquivos = new List<PDFCarregado>();
            var dir = new DirectoryInfo(_fonteArquivos);

            string msgErro = "";

            FileInfo[] files = await Task.Factory.StartNew(() =>
            {
                FileInfo[] arquivos = null;
                try
                {
                    arquivos = dir.GetFiles("*.pdf", SearchOption.AllDirectories);
                }
                catch (Exception ex)
                {
                    msgErro = ex.Message;                    
                }                
                return arquivos;
            });
            if (!string.IsNullOrEmpty(msgErro))
            {
                pictureBox1.Visible = false;
                throw new Exception(msgErro);
            }
            for (int i = 0; i <= files.Length - 1; i++)
            {
                listaArquivos.Add(await ObterObjetoPDf(files[i]));
            }
            CarregaGrid(textBox1.Text);
            pictureBox1.Visible = false;
            dataGridView1.Visible = true;
            dataGridView1.Dock = DockStyle.Fill;

        }

        private async Task<PDFCarregado> ObterObjetoPDf(FileInfo fileInfo)
        {
            string txtPdf = await ObterTextoPdf(fileInfo.FullName);            
            return new PDFCarregado { Arquivo = fileInfo.Name, Caminho = fileInfo.FullName, TxtPDF = txtPdf };
        }

        private async Task<string> ObterTextoPdf(string fullName)
        {
            using (PdfReader reader = new PdfReader(fullName))
            {
                StringBuilder text = new StringBuilder();
                for (int j = 1; j <= reader.NumberOfPages; j++)
                {
                    text.Append(await Task.Factory.StartNew(() =>
                    {
                        return PdfTextExtractor.GetTextFromPage(reader, j);
                    }));
                }
                return text.ToString().ToUpper();
            }
        }

        private async void CarregaGrid(string filtro)
        {
            List<PDFCarregado> sourceConsulta = await ObterFonteDados();
            if (!string.IsNullOrEmpty(filtro))            
            {
                var palavras = filtro.Split(' ');
                foreach (var item in palavras)
                {                   
                    sourceConsulta = await FiltraArquivosPorTermo(sourceConsulta, item);
                }
            }
            dataGridView1.DataSource = sourceConsulta;
        }

        private Task<List<PDFCarregado>> FiltraArquivosPorTermo(List<PDFCarregado> sourceConsulta, string termo)
        {
            return Task.Run(() =>
            {
                return sourceConsulta.Where(d => d.TxtPDF.Contains(termo.Trim().ToUpper())).ToList();
            });
        }

        private Task<List<PDFCarregado>> ObterFonteDados()
        {
            return Task.Run(() =>
            {
                return (from a in listaArquivos
                        select a).ToList();
            });
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

        private async void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                await CarregaDadosArquivos();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnMostraEscondeArquivos_Click(object sender, EventArgs e)
        {
            if (_listagemArquivosVisivel)
            {
                splitContainer1.Panel1Collapsed = true;
                splitContainer1.Panel1.Hide();
                _listagemArquivosVisivel = false;
                btnMostraEscondeArquivos.Text = "Mostra arquivos";
            }
            else
            {
                splitContainer1.Panel1Collapsed = false;
                splitContainer1.Panel1.Show();
                _listagemArquivosVisivel = true;
                btnMostraEscondeArquivos.Text = "Esconde arquivos";
            }
        }

        private async void btnFonteArquivo_Click(object sender, EventArgs e)
        {
            if (seletorPasta.ShowDialog() == DialogResult.OK)
            {
                _fonteArquivos = seletorPasta.SelectedPath;
                btnFonteArquivo.Text = "Fonte arquivos: " + _fonteArquivos;
                await CarregaDadosArquivos();
            }
        }
    }
}
