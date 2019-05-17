//using CefSharp;
//using CefSharp.WinForms;
//using CertificateGenerationSampleCode;
//using Org.BouncyCastle.Pkcs;
using Newtonsoft.Json.Linq;
using NtiEncryptDataLibray;
using OCSB_Client.OCSB_Classes;
using OCSB_Client.Provider;
using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Globalization;
using System.Net;
//using CertificateGenerationConnector;
//using CertificateGenerationConnector.CertificateGenerationWS;
//using iTextSharp.text.pdf;
//using iTextSharp.text;
//using System.Configuration;
//using System.Xml;
using System.Security.Permissions;
using System.Text;
//using System.Collections.Generic;
//using System.Drawing;
//using System.IO;
//using System.Net;
//using System.Net.Http;
//using System.Net.Http.Headers;
//using System.Security.Cryptography;
//using System.Security.Cryptography.X509Certificates;
using System.Windows.Forms;
namespace SugarSystem.Client
{


    [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
    [System.Runtime.InteropServices.ComVisibleAttribute(true)]
    public partial class Main : Form
    {
        private string p;
        public string Websites = Properties.Settings.Default.URLwebsite;
        String StrConn = ConfigurationManager.ConnectionStrings["ORA10GConnection"].ConnectionString;
         public Main()
        {
            InitializeComponent();

            webBrowser1.Dock = DockStyle.Fill;
            WindowState = FormWindowState.Maximized;

            webBrowser1.AllowWebBrowserDrop = false;
            webBrowser1.IsWebBrowserContextMenuEnabled = false;
            webBrowser1.WebBrowserShortcutsEnabled = false;
            webBrowser1.ObjectForScripting = this;


            webBrowser1.Navigate(Websites);
            //webBrowser1.Navigate("http://10.3.20.57/");

        }

        public Main(string p)
        {
            // TODO: Complete member initialization
            this.p = p;
        }


        #region Get data A1,A2,KN9,KN10 from Web old code
        public void Sanddata(String xml, String pdfPath, String RefNo, String ComNo, String ComName, String SignName, String SignPosition, String SignSNToken, String SignExpire, String SignImg, String frmType, String ReqFrom)
        {

            frmSignDS FrmDs = new frmSignDS();

            FrmDs.RefID = RefNo;
            FrmDs.lblRefNo.Text = RefNo;
            FrmDs.lblComNo.Text = ComNo;
            FrmDs.lblComName.Text = ComName;
            FrmDs.lblName.Text = SignName;
            FrmDs.lblPosition.Text = SignPosition;
            FrmDs.lblSNToken_Reg.Text = snString(SignSNToken);
            FrmDs.webBrowser1.Navigate(pdfPath);

            Simple3Des wrapper = new Simple3Des("NT1P@$$w0rd");
            FrmDs.txtXML.Text = wrapper.DecryptData(xml);

            FrmDs.From = frmType;
            FrmDs.SIG_IMAGE = SignImg;
            FrmDs.picSign.ImageLocation = SignImg;
            FrmDs.lblExpireDate_Reg.Text = SignExpire;
            FrmDs.SignSNToken = snString(SignSNToken);
            FrmDs.SendReqFrom = ReqFrom;  //ส่งคำร้องจาก Web หรือ Portal

            switch (frmType)
            {
                case "A1":
                    FrmDs.lblFormOID.Text = Properties.Settings.Default.OIDA1;
                    break;
                case "A2":
                    FrmDs.lblFormOID.Text = Properties.Settings.Default.OIDA2;
                    break;
                case "KN9":
                    FrmDs.lblFormOID.Text = Properties.Settings.Default.OIDKN9;
                    break;
                case "KN10":
                    FrmDs.lblFormOID.Text = Properties.Settings.Default.OIDKN10;
                    break;
            }


            if (FrmDs.ShowDialog(this) == DialogResult.OK)
            {

                webBrowser1.Refresh();

            }
        }

        #endregion

        #region Get Data CRKN,CNA2,RNA2 from Web old code
        public void Sanddata2(String xml, String pdfPath, String RefNo, String ComNo, String ComName, String SignName, String SignPosition, String SignSNToken, String SignExpire, String SignImg, String frmType, String ReqFrom, String ReqNo)
        {


            frmSignDS FrmDs = new frmSignDS();

            FrmDs.RefID = RefNo;
            FrmDs.lblRefNo.Text = RefNo;
            FrmDs.lblComNo.Text = ComNo;
            FrmDs.lblComName.Text = ComName;
            FrmDs.lblName.Text = SignName;
            FrmDs.lblPosition.Text = SignPosition;
            FrmDs.lblSNToken_Reg.Text = SignSNToken;
            FrmDs.webBrowser1.Navigate(pdfPath);

            Simple3Des wrapper = new Simple3Des("NT1P@$$w0rd");
            FrmDs.txtXML.Text = wrapper.DecryptData(xml);

            FrmDs.From = frmType;
            FrmDs.SIG_IMAGE = SignImg;
            FrmDs.picSign.ImageLocation = SignImg;
            FrmDs.lblExpireDate_Reg.Text = SignExpire;
            FrmDs.SignSNToken = SignSNToken;
            FrmDs.SendReqFrom = ReqFrom;  //ส่งคำร้องจาก Web หรือ Portal

            FrmDs.txtReqNo_CencelRenew.Text = ReqNo;

            switch (frmType)
            {
                case "CRKN":
                    FrmDs.lblFormOID.Text = Properties.Settings.Default.OidCRKN;
                    Properties.Settings.Default.signatureField = "SignBy";
                    break;
                case "CNA2":
                    FrmDs.lblFormOID.Text = Properties.Settings.Default.OidCNA2;
                    Properties.Settings.Default.signatureField = "SignBy";
                    break;
                case "RNA2":
                    FrmDs.lblFormOID.Text = Properties.Settings.Default.OidRNA2;
                    Properties.Settings.Default.signatureField = "SignBy";
                    break;
            }


            if (FrmDs.ShowDialog(this) == DialogResult.OK)
            {

                webBrowser1.Refresh();

            }
        }
        #endregion

        #region Get data A1,A2,KN9,KN10 from Web new code ''' Action from Button Sign
        public void Sendata(String pdfPath, String RefNo, String ComNo, String User, String frmType)
        {
            //** Get user info before initial frmSignDS
            UserID us = new UserID_Provider().GetUserInfo(User, "", "", new Uri(ConfigurationManager.AppSettings.Get("URL_GetUserInfo")));

            if (us != null)
            {
                if (us.Status.StatusCode == "200") //If status OK
                {
                    frmSignDS FrmDs = new frmSignDS();

                    FrmDs.RefID = RefNo;  //Reference Number
                    FrmDs.lblRefNo.Text = RefNo; 
                    FrmDs.lblComNo.Text = ComNo; //Company number
                    FrmDs.lblComName.Text = us.Attorney.Organization; //Company Name
                    FrmDs.lblName.Text = us.Attorney.Name; //Attorney Name or Signer name
                    FrmDs.lblPosition.Text = us.Attorney.Position; //Position under sign name
                    FrmDs.lblSNToken_Reg.Text = snString(us.Attorney.CertSN); //Cert Serial Token

                    FrmDs.webBrowser1.Navigate(pdfPath); //Preview Form 

                    string imgPath = Properties.Settings.Default.URLwebsite + us.Attorney.SignImageUrl; //Sign image path
                    string SchemaID = "";

                    FrmDs.From = frmType;
                    FrmDs.SIG_IMAGE = imgPath;
                    FrmDs.picSign.ImageLocation = imgPath;
                    FrmDs.lblExpireDate_Reg.Text = us.Attorney.CertExpireDate.ToString("dd MMMM yyyy", new CultureInfo("th-TH")); //Cert Expire
                    FrmDs.SignSNToken = snString(us.Attorney.CertSN);

                    switch (frmType)
                    {
                        ///*** We used to binded data by strXml then we bind 1-to-1 textbox in pdf 
                        case "KN9":
                            try
                            {
                                FrmDs.lblFormOID.Text = Properties.Settings.Default.OIDKN9;
                                SchemaID = Properties.Settings.Default.OIDKN9_Scm;
                                string strConn = ConfigurationManager.AppSettings.Get("URL_KN9");
                                string attXML = OCSBForm_Provider.GetAttachXmlFormKN9(strConn, RefNo, FrmDs.lblFormOID.Text, SchemaID, "", "");


                                if (!string.IsNullOrWhiteSpace(attXML))

                                {
                                    FrmDs.txtXML.Text = attXML; 
                                    FrmDs.txtEmbedXML.Text = attXML;
                                }
                                else
                                { MessageBox.Show("Can not get XML {FormKN9} !"); }
                            }
                            catch (Exception ex)
                            {
                                throw new Exception(ex.Message);
                            }

                            break;
                        case "KN10":
                            try
                            {
                                FrmDs.lblFormOID.Text = Properties.Settings.Default.OIDKN10;
                                SchemaID = Properties.Settings.Default.OIDKN10_Scm;
                                string strCon = ConfigurationManager.AppSettings.Get("URL_KN10");
                                string strEmbbed = OCSBForm_Provider.GetAttachXmlFormKN10 (strCon, RefNo, FrmDs.lblFormOID.Text,SchemaID, "", "");
                                if (!string.IsNullOrWhiteSpace(strEmbbed))
                                {
                                    FrmDs.txtXML.Text = strEmbbed;
                                    FrmDs.txtEmbedXML.Text = strEmbbed;
                                }
                                else { MessageBox.Show("Can not get XML {FormKN10} !"); }


                            }
                            catch (Exception ex) { throw new Exception(ex.Message); }
                            break;
                        case "A1":
                            try
                            {
                                FrmDs.lblFormOID.Text = Properties.Settings.Default.OIDA1;
                                SchemaID = Properties.Settings.Default.OIDA1_Scm;
                                string strCon = ConfigurationManager.AppSettings.Get("URL_A1");
                                string EmbedXML = OCSBForm_Provider.GetAttatchXmlFormA1(strCon, RefNo, FrmDs.lblFormOID.Text, SchemaID, "", "");
                                if (!string.IsNullOrWhiteSpace(EmbedXML))
                                {
                                    FrmDs.txtXML.Text = EmbedXML;
                                    FrmDs.txtEmbedXML.Text = EmbedXML;
                                }
                                else { MessageBox.Show("Can not get XML {FormA1}!"); }


                            }
                            catch (Exception ex) { throw new Exception(ex.Message); }
                            break;
                        case "A2":
                            try
                            {
                                FrmDs.lblFormOID.Text = Properties.Settings.Default.OIDA2;
                                SchemaID = Properties.Settings.Default.OIDA2_Scm;
                                string strCon = ConfigurationManager.AppSettings.Get("URL_A2");
                                string EmbedXML = OCSBForm_Provider.GetAttachXmlFormA2(strCon, RefNo, FrmDs.lblFormOID.Text, SchemaID, "", "");
                                if (!string.IsNullOrWhiteSpace(EmbedXML))
                                {
                                    FrmDs.txtXML.Text = EmbedXML;
                                    FrmDs.txtEmbedXML.Text = EmbedXML;
                                }
                                else { MessageBox.Show("Can not get XML {FormA2} !"); }


                            }
                            catch (Exception ex) { throw new Exception(ex.Message); }
                            break;

                    }


                    if (FrmDs.ShowDialog(this) == DialogResult.OK)
                    {

                        webBrowser1.Refresh();

                    }
                }
                else
                {
                    string str = string.Format("- Receive User info Error : '{0}' - '{1}'", us.Status.StatusCode, us.Status.StatusMessage);
                    MessageBox.Show(str);
                }
            }
            else { MessageBox.Show("Receive User Info Error."); }
        }
        #endregion

        #region Get Data CRKN,RNKN,CNA2,RNA2 from web new code '''Action from button sign.
        public void Sendata2(String pdfPath, String RefNo, String ComNo, String User, String frmType, String ReqNo)
        {
            UserID us = new UserID_Provider().GetUserInfo(User, "", "", new Uri(ConfigurationManager.AppSettings.Get("URL_GetUserInfo")));
            if (us != null)
            {
                if (us.Status.StatusCode == "200")
                {
                    frmSignDS FrmDs = new frmSignDS();

                    FrmDs.RefID = RefNo; //Reference Number
                    FrmDs.lblRefNo.Text = RefNo; //Reference Number
                    FrmDs.txtReqNo_CencelRenew.Text = ReqNo;  
                    FrmDs.lblComNo.Text = ComNo; //Company Number
                    FrmDs.lblComName.Text = us.Attorney.Organization; //Company Name

                    FrmDs.lblName.Text = us.Attorney.Name; //Attorney Name or Anyone who can sign 
                    FrmDs.lblPosition.Text = us.Attorney.Position; //Position under sign name
                    FrmDs.lblSNToken_Reg.Text = us.Attorney.CertSN; //Cert Serial Token 
                    FrmDs.webBrowser1.Navigate(pdfPath); //Preview Form             
                    FrmDs.From = frmType;

                    string imgPath = Properties.Settings.Default.URLwebsite + us.Attorney.SignImageUrl; //Sign image Path

                    FrmDs.SIG_IMAGE = imgPath;
                    FrmDs.picSign.ImageLocation = imgPath;


                    string strExpCert = us.Attorney.CertExpireDate.ToString("yyyy-MM-dd");
                    FrmDs.lblExpireDate_Reg.Text = strExpCert; //Cert Expire
                    FrmDs.SignSNToken = us.Attorney.CertSN; //Serial Token
                    //FrmDs.SendReqFrom = ReqFrom;  //ส่งคำร้องจาก Web หรือ Portal

                    FrmDs.txtReqNo_CencelRenew.Text = ReqNo;
                    string SchemaID = "";

                    switch (frmType)
                    {

                        ///*** We used to binded data by strXml then we bind 1-to-1 textbox in pdf 
                        case "CRKN": //Cancel KN10
                            try
                            {
                                FrmDs.lblFormOID.Text = Properties.Settings.Default.OidCRKN;
                                SchemaID = Properties.Settings.Default.OidCRKN_Scm;
                                string strCon = ConfigurationManager.AppSettings.Get("URL_KN10Cancel");
                                string EmbedXml = OCSBForm_Provider.GetAttachXmlFormKN10Cancel(strCon, ReqNo, FrmDs.lblFormOID.Text, SchemaID, "", "");

                                if (!string.IsNullOrWhiteSpace(EmbedXml))
                                {
                                    FrmDs.txtXML.Text = EmbedXml;
                                    FrmDs.txtEmbedXML.Text = EmbedXml;

                                }
                                else { MessageBox.Show("Can not get XML {FormCRKN}"); }


                            }
                            catch (Exception ex) { MessageBox.Show("{FormCRKN error : " + ex.Message); }

                            break;
                        case "RNKN": //Renew KN10
                            try 
                            {
                                FrmDs.lblFormOID.Text = Properties.Settings.Default.OidCRKN;
                                SchemaID = Properties.Settings.Default.OidCRKN_Scm;
                                string strCon = ConfigurationManager.AppSettings.Get("URL_KN10Renew");
                                string EmbedXML = OCSBForm_Provider.GetAttachXmlFormKN10Renew(strCon, ReqNo, FrmDs.lblFormOID.Text, SchemaID, "", "");

                                if (!string.IsNullOrWhiteSpace(EmbedXML))
                                {
                                    FrmDs.txtXML.Text = EmbedXML;
                                    FrmDs.txtEmbedXML.Text = EmbedXML;

                                }
                                else { MessageBox.Show("Can not get XML {FormRNKN}"); }


                            }
                            catch (Exception ex) { MessageBox.Show("{FormRNKN error : }" + ex.Message); }

                            break;


                        case "CNA2": //Cancel A2
                            try
                            {
                                FrmDs.lblFormOID.Text = Properties.Settings.Default.OidCNA2;
                                SchemaID = Properties.Settings.Default.OidCNA2_Scm;
                                string strCon = ConfigurationManager.AppSettings.Get("URL_A2Cancel");
                                string EmbedXml = OCSBForm_Provider.GetAttachXmlFormA2Cancel(strCon, ReqNo, FrmDs.lblFormOID.Text, SchemaID, "", "");

                                if (!string.IsNullOrWhiteSpace(EmbedXml))
                                {
                                    FrmDs.txtXML.Text = EmbedXml;
                                    FrmDs.txtEmbedXML.Text = EmbedXml;
                                    

                                }
                                else { MessageBox.Show("Can not get XML {FormCNA2}"); }


                            }
                            catch (Exception ex) { MessageBox.Show("{FormCNA2 error : }" + ex.Message); }

                            break;
                        case "RNA2": //Renew A2

                            try
                            {
                                FrmDs.lblFormOID.Text = Properties.Settings.Default.OidRNA2;
                                SchemaID = Properties.Settings.Default.OidRNA2;
                                string strCon = ConfigurationManager.AppSettings.Get("URL_A2Renew");
                                string EmbedXml = OCSBForm_Provider.GetAttachXmlFormA2Renew(strCon, ReqNo, ComNo, FrmDs.lblFormOID.Text, SchemaID, "", "");

                                if (!string.IsNullOrWhiteSpace(EmbedXml))
                                {
                                    FrmDs.txtEmbedXML.Text = EmbedXml;
                                    FrmDs.txtXML.Text = EmbedXml;

                                }
                                else { MessageBox.Show("Can not get XML {FormRNA2}"); }


                            }
                            catch (Exception ex) { MessageBox.Show("{FormRNA2 error : }" + ex.Message); }

                            break;
                    }
                    if (FrmDs.ShowDialog(this) == DialogResult.OK)
                    {

                        webBrowser1.Refresh();

                    }
                }
                else
                {
                    string str = string.Format("- Receive User info Error : '{0}' - '{1}'", us.Status.StatusCode, us.Status.StatusMessage);
                    MessageBox.Show(str);
                }
            }
            else { MessageBox.Show("Receive User Info Error."); }
        }
        #endregion

        //#region Call Form by Register Form
        //public static JObject CallAPIForm(string url, string Oid, string usernane, string password)
        //{

        //    var client = new WebClient();
        //    var method = "POST"; // If your endpoint expects a GET then do it.
        //    var parameters = new NameValueCollection();
        //    parameters.Add("Oid", Oid);
        //    parameters.Add("Username", usernane);
        //    parameters.Add("Password", password);


        //    var response_data = client.UploadValues(url, method, parameters);
        //    var strRes = Encoding.UTF8.GetString(response_data);

        //    JObject resJObj = new JObject();
        //    resJObj = JObject.Parse(strRes);

        //    return resJObj;
        //}
        //#endregion




        public void Expportxml(String partfile)
        {
            frmExportXML Frmexp = new frmExportXML(partfile);

            if (Frmexp.ShowDialog(this) == DialogResult.OK)
            {

            }
        }

        public void Expportxml_A2Print(String partfile, String kn9no)
        {
            frmExportXML_A2Print Frmexp = new frmExportXML_A2Print(partfile, kn9no);

            if (Frmexp.ShowDialog(this) == DialogResult.OK)
            {

            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            webBrowser1.Document.InvokeScript("test",
                new String[] { "called from client code" });
        }



        private void Main_Load(object sender, EventArgs e)
        {
            webBrowser1.IsWebBrowserContextMenuEnabled = true;
            webBrowser1.WebBrowserShortcutsEnabled = true;
        }

        private void SettingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //this.Hide();
            frmSignReason fs = new frmSignReason();
            fs.ShowDialog();
            //fs.Show();
        }

        private static string snString(string sn)
        {
            string result = "";
            try
            {
                result = sn.Trim().Replace(" ", "").Replace("-", "").ToUpper(); ;
            }
            catch
            {
                result = "";
            }
            return result;
        }

    }
}
