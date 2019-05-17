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
    public partial class frmpass : Form
    {
        public frmpass()
        {
            InitializeComponent();
        }

        public string PasswordEdit = "P@ssw0rd";
        
        private void btnok_Click(object sender, EventArgs e)
        {
            frmSignReason fp1 = new frmSignReason();
            if (txtpass.Text == PasswordEdit)
            {
                this.Hide();
            }
            else
            {
                MessageBox.Show("รหัสผ่านไม่ถูกต้อง");
            }

            //Properties.Settings.Default.passwordedit = txtpass.Text;
            
        }

        private void txtpass_TextChanged(object sender, EventArgs e)
        {

                this.AcceptButton = btnok;
                //btnok_Click(this, new EventArgs());
            
        }


    }
}
