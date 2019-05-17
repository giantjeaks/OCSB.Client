using System;
//using System.Collections.Generic;
//using System.ComponentModel;
//using System.Data;
//using System.Drawing;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
using System.Windows.Forms;

namespace SugarSystem.Client
{
    public partial class frmSignReason : Form
    {
        public frmSignReason()
        {
            InitializeComponent();
        }

        private void btnsave_Click(object sender, EventArgs e)
        {
             Properties.Settings.Default.Reason = txtReason.Text;
             Properties.Settings.Default.Location = txtLocation.Text;
             Properties.Settings.Default.OIDKN9 = txtKN9.Text;
             Properties.Settings.Default.OIDKN10 = txtKN10.Text;
             Properties.Settings.Default.OIDA1 = txtA1.Text;
             Properties.Settings.Default.OIDA2 = txtA2.Text;
             Properties.Settings.Default.URLwebsite = txturlwebsite.Text;

             Properties.Settings.Default.Client_UpdateSignaturePath_UpdateKN9Status = txtpathupdate.Text;
             Properties.Settings.Default.URLts = txttimestamp.Text; 
             Properties.Settings.Default.FTPUpload = txtftpurl.Text;
             Properties.Settings.Default.Userts = txtftpuser.Text; 
             Properties.Settings.Default.Passts = txtftppass.Text;
            Properties.Settings.Default.pathWS = txtFTPFolder.Text;

            Properties.Settings.Default.URLcergenserver = txtURLcergenserver.Text; 
             Properties.Settings.Default.Userpdf = txtfcergenUser.Text;
             Properties.Settings.Default.Passpdf = txtfcergenPass.Text; 
             Properties.Settings.Default.Client_wsPortal_OCSB = txtURLPortal.Text;
             Properties.Settings.Default.UserwsPortal = txtfPortalUser.Text; 
             Properties.Settings.Default.PasswsPortal = txtfPortalPass.Text; 
             Properties.Settings.Default.HASH_FUNCTION = txthash.Text; 
             Properties.Settings.Default.HASH_FUNCTION_OID = txthashoid.Text; 

             Properties.Settings.Default.Save();
             this.Hide();
            //Main fs = new Main();
            //fs.Show();

        }

        private void frmSignReason_Load(object sender, EventArgs e)
        {
            txtReason.Text = Properties.Settings.Default.Reason; //เหตุผลการเซ็น
            txtLocation.Text = Properties.Settings.Default.Location; //สถานที่เว็น
            txtKN9.Text = Properties.Settings.Default.OIDKN9; //Oid กน9
            txtKN10.Text = Properties.Settings.Default.OIDKN10; //Oid กน10
            txtA1.Text = Properties.Settings.Default.OIDA1; //Oid อ1
            txtA2.Text = Properties.Settings.Default.OIDA2; //Oid อ2
            txturlwebsite.Text = Properties.Settings.Default.URLwebsite; // URl เว็บไซต์

            txtpathupdate.Text = Properties.Settings.Default.Client_UpdateSignaturePath_UpdateKN9Status; //ตำแหน่งไฟล์ที่ทำการอัพโหลด
            txttimestamp.Text = Properties.Settings.Default.URLts; // URL Timestamp
            txtftpurl.Text = Properties.Settings.Default.FTPUpload; // URLFTP
            txtftpuser.Text = Properties.Settings.Default.FTPuser; // FTPUser
            txtftppass.Text = Properties.Settings.Default.FTPpassword; // FTPPassword
            txtFTPFolder.Text= Properties.Settings.Default.pathWS;
            txtURLcergenserver.Text = Properties.Settings.Default.URLcergenserver; // Url Cergen Server
            txtfcergenUser.Text = Properties.Settings.Default.Userpdf; //Cergen User
            txtfcergenPass.Text = Properties.Settings.Default.Passpdf; //Cergen Password
            txtURLPortal.Text = Properties.Settings.Default.Client_wsPortal_OCSB; //Url portal server
            txtfPortalUser.Text = Properties.Settings.Default.UserwsPortal; //portal user
            txtfPortalPass.Text = Properties.Settings.Default.PasswsPortal; //portal password
            txthash.Text = Properties.Settings.Default.HASH_FUNCTION; //hash function เข้ารหัสไฟล์
            txthashoid.Text = Properties.Settings.Default.HASH_FUNCTION_OID; //hash Oid

            g2.Enabled = false;
            g3.Enabled = false;
            g4.Enabled = false;
            g5.Enabled = false;

        }


        private void btnedit_Click(object sender, EventArgs e)
        {

            frmpass frm = new frmpass();
            if (frm.ShowDialog() != DialogResult.OK)
            {
                if (frm.txtpass.Text == frm.PasswordEdit)
                {
                    g2.Enabled = true;
                    g3.Enabled = true;
                    g4.Enabled = true;
                    g5.Enabled = true;
                }
                else
                {
                    g2.Enabled = false;
                    g3.Enabled = false;
                    g4.Enabled = false;
                    g5.Enabled = false;
                }
            }

        }
        
    }
}
