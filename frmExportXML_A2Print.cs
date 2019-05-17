using iTextSharp.text.pdf;
using System;
//using System.Collections.Generic;
//using System.ComponentModel;
//using System.Data;
//using System.Drawing;
using System.IO;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
using System.Windows.Forms;

namespace SugarSystem.Client
{
    public partial class frmExportXML_A2Print : Form
    {
        string filename;

        public frmExportXML_A2Print(String partfile, String kn9no)
        {
            InitializeComponent();
            WindowState = FormWindowState.Maximized;

            UpdateSignaturePath.UpdateKN9Status mySer = new UpdateSignaturePath.UpdateKN9Status();
            Boolean A2PrintStatus= mySer.UpdateA2PrintStatus(kn9no);
            webBrowser1.Navigate(partfile);
        }

        private void exportXMLToolStripMenuItem_Click(object sender, EventArgs e)
        {

            string pdfTemplate = webBrowser1.Url.AbsoluteUri;
            PdfReader pdfReader = new PdfReader(pdfTemplate);
            XfaForm xfa = pdfReader.AcroFields.Xfa;

            string ss = xfa.DomDocument.GetElementsByTagName("xfa:datasets").Item(0).OuterXml;

            saveFileDialog1.Filter = "xml document|*.xml";
            saveFileDialog1.Title = "Save an Image File";
            string name = saveFileDialog1.FileName = "Export.xml";
            saveFileDialog1.ShowDialog();
            File.WriteAllText(saveFileDialog1.FileName, ss);

        }

    }
}
