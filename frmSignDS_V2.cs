using CefSharp;
using CertificateGenerationSampleCode;
using Org.BouncyCastle.Pkcs;
using System;
//using System.Collections.Generic;
//using System.ComponentModel;
//using System.Configuration;
//using System.Data;
//using System.Drawing;
using System.IO;
//using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
//using System.Text;
//using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
//using CertificateGenerationConnector;
using CertificateGenerationConnector.CertificateGenerationWS;
using CefSharp.WinForms;
using SugarSystem.Client.wsPortal;
using System.Collections.Generic;
using System.Configuration;
using iTextSharp.text.pdf;
using iTextSharp.text;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Text;
using iTextSharp.tool.xml.xtra.xfa;
using iTextSharp.license;
using System.Collections.Specialized;
using iTextSharp.text.xml.xmp;

namespace SugarSystem.Client
{

    public partial class frmSignDS : Form
    {
        public X509Certificate2 selectedSigningCert = null;
        public X509Certificate2Collection signingCerts = new X509Certificate2Collection();
        public X509Certificate2 certificate = null;

        public string Reason = Properties.Settings.Default.Reason;
        public string Location = Properties.Settings.Default.Location;

        //public string FolderPDF = "OSCB_Documents";
        public string FTPUpload = Properties.Settings.Default.FTPUpload;
        public string FTPuser = Properties.Settings.Default.FTPuser;
        public string FTPpassword = Properties.Settings.Default.FTPpassword;

        public string pathWS = Properties.Settings.Default.pathWS; //ตำแหน่งการอัพเดทที่อยู่ไฟล์

        public string RefID;
        public string From;
        public string SIG_IMAGE; //พาทที่อยู่รูปภาพลายเซ็น
        public string SignSNToken;
        public string SendReqFrom;  //ส่งคำร้องจาก Web หรือ Portal

        public ChromiumWebBrowser _browser;

        public string _webURL = Properties.Settings.Default.URLwebsite;


        public static string RESOURCE_FOLDER = @"..\..\";  //System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + @"\";
        public static string KEYSTORE_PATH = @"D:\\Work\\OCSB\OCSB PDFSign and Browser\\Production\\OCSB Borwser Client V3.0.0_TEST\\resource\DanaiNTi.p12";
        public static string KEYSTORE_PASSWORD = "010553";

        public static string HASH_FUNCTION = Properties.Settings.Default.HASH_FUNCTION;
        public static string HASH_FUNCTION_OID = Properties.Settings.Default.HASH_FUNCTION_OID;

        public static string CERT_GEN_URL = Properties.Settings.Default.URLcergenserver;
        public static string CERT_GEN_USERNAME = Properties.Settings.Default.Userpdf;
        //public static string CERT_GEN_USERNAME = "nti_orgAdmin";
        public static string CERT_GEN_PASSWORD = Properties.Settings.Default.Passpdf;
        //public static string CERT_GEN_PASSWORD = "9yrJcHVR";

        public static string TIMESTAMP_URL = Properties.Settings.Default.URLts;
        public static string TIMESTAMP_USERNAME = Properties.Settings.Default.Userts;
        public static string TIMESTAMP_PASSWORD = Properties.Settings.Default.Passts;
        public static string XML_TEMP_PATH = ConfigurationManager.AppSettings.Get("XMLtmpFolder"); //Do not forget set in App.config for Physical   ..\tmp_xml
        public static string ICC_PROFILE_PATH = ConfigurationManager.AppSettings.Get("ICC_Profile_PATH"); // ..\res\sRGB Color Space Profile.icm

        public string LOCAL_VERSION = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();


        public string XMLDATA_PATH = "";
        public string CANONICALIZATION_XML_PATH = @"";
        public string formOID = @"";
        public string certRefNumber = Properties.Settings.Default.certRef;
        public string signatureField = @"";
        public string signatureField2 = @"";
        public string Validate_Url = Properties.Settings.Default.ValidationURL;
        public string GRCode_FormType;
        private static readonly HttpClient client = new HttpClient();
        public string FormSerial = "";
        public string GenFormDate = "";

        public frmSignDS()
        {
            InitializeComponent();
            webBrowser1.Dock = DockStyle.Fill;
            WindowState = FormWindowState.Maximized;
        }

        public static string MsgStatus = "";
        private void btnSign_Click(object sender, EventArgs e)
        {
            try
            {

                if (inToSignPDFa(txtXML.Text, txtEmbedXML.Text, From, lblRefNo.Text,txtReqNo_CencelRenew.Text, SIG_IMAGE, SignSNToken))
                {
                    MessageBox.Show("ลงนามเรียบร้อย (pdfa)\n" + MsgStatus);
                    this.DialogResult = DialogResult.OK;
                }
                else
                {
                    if (MsgStatus != "")
                    {
                        MessageBox.Show(MsgStatus);
                    }
                    else
                    {
                        DialogResult result = MessageBox.Show("ยกเลิกโดยผู้ใช้", "Cancel", MessageBoxButtons.OK, MessageBoxIcon.Stop);

                        if (result == DialogResult.Yes)
                        {

                        }
                        //MessageBox.Show("เกิดข้อผิดพลาดในการ Sign");
                    }

                };
            }
            catch (Exception ex)
            {
                MessageBox.Show("เกิดข้อผิดพลาด: " + ex.Message);
            }

        }


   

        #region  ฟังก์ชั่น รับค่า XML จากหน้าเว็บ แล้วทำการแปลงเป็น PDFa3 แล้วทำการฝังไฟล์ลงไป 
        public Boolean inToSignPDFa(string strxml, string EmbedXML, string FormType, string Reference,string RequestNo, string SignaturePath, string SignSNToken)
        {

            X509Certificate2Collection strToken = CheckLCToken(SignSNToken);
            if (strToken != null)
            {
                string SignDate = ConvertFullDate(DateTime.Now);
                RefID = Reference;
                SIG_IMAGE = SignaturePath;
                //SendReqFrom = ReqFrom;
                Boolean SignStatus = false;
                string xmlfromweb = strxml;
                string EmbedXmlfromweb = EmbedXML;

                Properties.Settings.Default.signatureField = "";
                Properties.Settings.Default.signatureField2 = "";
                string SchemaID = "";
                if (xmlfromweb != "")
                {
                    switch (FormType)
                    {
                        case "A1":
                            certRefNumber = "แบบฟอร์มคำร้อง อ.1 เลขที่อ้างอิง " + Reference;
                            formOID = Properties.Settings.Default.OIDA1;
                            SchemaID = Properties.Settings.Default.OIDA1_Scm;
                            signatureField = Properties.Settings.Default.signatureField;
                            xmlfromweb = xmlfromweb.Replace("<SignName></SignName>", "<SignName>( " + lblName.Text + " )</SignName>");
                            xmlfromweb = xmlfromweb.Replace("<SignPosition></SignPosition>", "<SignPosition>" + lblPosition.Text + "</SignPosition>");
                            xmlfromweb = xmlfromweb.Replace("<SignDate></SignDate>", "<SignDate>" + SignDate + "</SignDate>");
                            break;
                        case "A2":
                            GRCode_FormType = "1";
                            certRefNumber = "แบบฟอร์มใบอนุญาต อ.2 เลขที่อ้างอิง " + Reference;
                            formOID = Properties.Settings.Default.OIDA2;
                            SchemaID = Properties.Settings.Default.OIDA2_Scm;
                            signatureField = Properties.Settings.Default.signatureField;
                            xmlfromweb = xmlfromweb.Replace("<cct:AuthorityName></cct:AuthorityName>", "<cct:AuthorityName>" + lblName.Text + "</cct:AuthorityName>");
                            EmbedXML = EmbedXML.Replace("<cct:AuthorityName></cct:AuthorityName>", "<cct:AuthorityName>" + lblName.Text + "</cct:AuthorityName>");
                            xmlfromweb = xmlfromweb.Replace("<cct:AuthorityPosition></cct:AuthorityPosition>", "<cct:AuthorityPosition>" + lblPosition.Text + "</cct:AuthorityPosition>");
                            EmbedXML = EmbedXML.Replace("<cct:AuthorityPosition></cct:AuthorityPosition>", "<cct:AuthorityPosition>" + lblPosition.Text + "</cct:AuthorityPosition>");
                            break;
                        case "KN9":
                            certRefNumber = "แบบฟอร์มคำร้อง กน.9 เลขที่อ้างอิง " + Reference;
                            formOID = Properties.Settings.Default.OIDKN9;
                            SchemaID = Properties.Settings.Default.OIDKN9_Scm;
                            Properties.Settings.Default.signatureField = "SignBy";
                            Properties.Settings.Default.signatureField2 = "SingBy2";
                            xmlfromweb = xmlfromweb.Replace("<SignName1></SignName1>", "<SignName1>( " + lblName.Text + " )</SignName1>");
                            xmlfromweb = xmlfromweb.Replace("<SignPosition1></SignPosition1>", "<SignPosition1>" + lblPosition.Text + "</SignPosition1>");
                            xmlfromweb = xmlfromweb.Replace("<SignDate1></SignDate1>", "<SignDate1>" + SignDate + "</SignDate1>");

                            xmlfromweb = xmlfromweb.Replace("<SignName2></SignName2>", "<SignName2>( " + lblName.Text + " )</SignName2>");
                            xmlfromweb = xmlfromweb.Replace("<SignPosition2></SignPosition2>", "<SignPosition2>" + lblPosition.Text + "</SignPosition2>");
                            xmlfromweb = xmlfromweb.Replace("<SignDate2></SignDate2>", "<SignDate2>" + SignDate + "</SignDate2>");
                            break;
                        case "KN10":
                            GRCode_FormType = "5";
                            certRefNumber = "แบบฟอร์มใบอนุญาต กน.10 เลขที่อ้างอิง " + Reference;
                            formOID = Properties.Settings.Default.OIDKN10;
                            SchemaID = Properties.Settings.Default.OIDKN10_Scm;
                            Properties.Settings.Default.signatureField = "SignBy";
                            //RefID = "";

                            xmlfromweb = xmlfromweb.Replace("<cct:AuthorityName></cct:AuthorityName>", "<cct:AuthorityName>( " + lblName.Text + " )</cct:AuthorityName>");
                            EmbedXML = EmbedXML.Replace("<cct:AuthorityName></cct:AuthorityName>", "<cct:AuthorityName>( " + lblName.Text + " )</cct:AuthorityName>");
                            xmlfromweb = xmlfromweb.Replace("<cct:AuthorityPosition></cct:AuthorityPosition>", "<cct:AuthorityPosition>( " + lblPosition.Text + " )</cct:AuthorityPosition>");
                            EmbedXML = EmbedXML.Replace("<cct:AuthorityPosition></cct:AuthorityPosition>", "<cct:AuthorityPosition>( " + lblPosition.Text + " )</cct:AuthorityPosition>");
                            break;
                        case "CRKN":
                            certRefNumber = "แบบฟอร์มคำร้องขอยกเลิก/ต่ออายุ กน.10 เลขที่อ้างอิง " + Reference;
                            formOID = Properties.Settings.Default.OidCRKN;
                            SchemaID = Properties.Settings.Default.OidCRKN_Scm;
                            Properties.Settings.Default.signatureField = "SignBy";
                            break;
                        case "RNKN":
                            certRefNumber = "แบบฟอร์มคำร้องขอยกเลิก/ต่ออายุ กน.10 เลขที่อ้างอิง " + Reference;
                            formOID = Properties.Settings.Default.OidCRKN;
                            SchemaID = Properties.Settings.Default.OidCRKN_Scm;
                            Properties.Settings.Default.signatureField = "SignBy";
                            break;


                        case "CNA2":
                            certRefNumber = "แบบฟอร์มคำร้องขอยกเลิก อ.2 เลขที่อ้างอิง " + Reference;
                            formOID = Properties.Settings.Default.OidCNA2;
                            SchemaID = Properties.Settings.Default.OidCNA2_Scm;
                            Properties.Settings.Default.signatureField = "SignBy";
                            break;
                        case "RNA2":
                            certRefNumber = "แบบฟอร์มคำร้องขอต่ออายุ อ.2 เลขที่อ้างอิง " + Reference;
                            formOID = Properties.Settings.Default.OidRNA2;
                            SchemaID = Properties.Settings.Default.OidRNA2_Scm;
                            Properties.Settings.Default.signatureField = "SignBy";
                            break;
                    }

                    xmlfromweb = xmlfromweb.Replace("<RefNo1></RefNo1>", "<RefNo1>" + RefID + "</RefNo1>");
                    xmlfromweb = xmlfromweb.Replace("<RefNo2></RefNo2>", "<RefNo2>" + RefID + "</RefNo2>");
                    xmlfromweb = xmlfromweb.Replace("<FormOID1></FormOID1>", "<FormOID1>" + formOID + "</FormOID1>");
                    xmlfromweb = xmlfromweb.Replace("<FormOID2></FormOID2>", "<FormOID2>" + formOID + "</FormOID2>");

                    XmlDocument xdoc = new XmlDocument();
                    xdoc.LoadXml(EmbedXmlfromweb);
                    byte[] bxembed = System.Text.Encoding.UTF8.GetBytes(xdoc.OuterXml);
                    string strAttXML = Encoding.UTF8.GetString(bxembed);


                    XmlDocument xmbed = new XmlDocument();
                    xmbed.LoadXml(xmlfromweb);
                    byte[] bxml = System.Text.Encoding.UTF8.GetBytes(xmbed.OuterXml);
                    string strBindingXML = Encoding.UTF8.GetString(bxml);
                    MsgStatus = "- Receive XML data from server: OK\n";

                    // Apply canonicalize function to loaded xml
                    byte[] canonicalizeXmlData = new XMLCanonicalization().canonicalXML(bxml);
                    string canonicalizeXmlStr = Encoding.UTF8.GetString(canonicalizeXmlData);

                    string HashVal = GetHash(strAttXML);


                    Uri uri = new Uri(ConfigurationManager.AppSettings.Get("URL_GetForm"));
                    string strRes = new PdfManipulator().getFormViaOid(uri, formOID.ToString());
                    JObject resJObj = JObject.Parse(strRes);

                    //Check response status
                    if (resJObj.GetValue("Status")["StatusCode"].ToString() != "200") // Check eCertificate retrieve is error or not
                    {
                        MsgStatus += "- Request CertGen Error " + resJObj.GetValue("Status")["StatusCode"] + ": " + resJObj.GetValue("Status")["StatusMessage"] + "\n";
                        return false;
                    }

                    if (resJObj.GetValue("Pdf")["FormSerialNumber"] != null)
                    {
                        FormSerial = resJObj.GetValue("Pdf")["FormSerialNumber"].ToString();
                    }
                    else { FormSerial = string.Empty; }

                    if (resJObj.GetValue("Pdf")["FormGennerateTime"] != null)
                    {
                        GenFormDate = resJObj.GetValue("Pdf")["FormGennerateTime"].ToString();
                    }
                    else { GenFormDate = string.Empty; }


                    //Get FormPDF and Convert to byte[]
                    string rawECertificate = resJObj.GetValue("Pdf")["FormPDF"].ToString();
                    byte[] eCertificate = Convert.FromBase64String(rawECertificate);
                    MsgStatus += "- Request PDF XFA from CertGen Server: OK\n";




                    //================================================================
                    // Import xml content to eCertificate from certificate generation
                    //================================================================


                    byte[] filledPDF = new PdfManipulator().binDataXFA(eCertificate, FormType, formOID, Reference, RequestNo, lblComNo.Text, "", "", lblName.Text, lblPosition.Text);


                    //File.WriteAllBytes("Afterflatten.pdf",filledPDF);

                    if (filledPDF != null && filledPDF.Length > 0)
                    {

                        byte[] PDFA3 = new PdfManipulator().ConvertToPDFa3(filledPDF, bxembed, strAttXML, formOID, SchemaID, HashVal, Reference, FormSerial, GenFormDate);
                        MsgStatus += "- Filled Data into PDF XFA Form: OK\n";

                        //File.WriteAllBytes(SIGNED_ECERTIFICATE_PATH, filledPDF);

                        //Gen URL สำหรับ QR-Code
                        string iQRcode = "";
                        if (FormType == "KN10" || FormType == "A2")
                        {
                            string prm = string.Format("{0}|{1}|{2}", GRCode_FormType, lblRefNo.Text, lblFormOID.Text);
                            iQRcode = CreateBarcode(string.Format(Validate_Url + "?prm={0}", Base64Encode(prm)));
                            //MsgStatus += "- Gen QRCode: OK\n";

                        }


                        // Digital Sign specify certificate 
                        //byte[] signedPDF = signWithKeyStore(filledPDF, KEYSTORE_PATH, KEYSTORE_PASSWORD, SIG_IMAGE, Reason, Location, iQRcode);
                        byte[] signedPDF = signWithWindowStore(PDFA3, SIG_IMAGE, Reason, Location, SignSNToken, iQRcode, FormType, strToken);




                        if (signedPDF != null && signedPDF.Length > 0)
                        {
                            //File.WriteAllBytes(SIGNED_ECERTIFICATE_PATH, signedPDF);
                            //File.WriteAllBytes(AppDomain.CurrentDomain.BaseDirectory + "tmp_xml\\FinishedPDFa.pdf", signedPDF);
                            MsgStatus += "- Signed PDF: OK\n";
                        }
                        else
                        {
                            MsgStatus += "- Signed PDF: Incomplete\n";
                        }

                        //===========Upload to FTP SERVER
                        //uploadFTP(RefID, signedPDF);  //ปิดการใช้งาน เปลี่ยนไปใช้เป็น upload ผ่าน web service
                        uploadbase64format(RefID, signedPDF);
                        SignStatus = true;

                    }
                    else
                    {
                        //MessageBox.Show("PDF is empty.");
                        MsgStatus += "- PDF is empty.\n";
                        return false;
                    }

                }
                else
                {
                    //MessageBox.Show("ไม่สามารถรับค่าได้");
                    MsgStatus += "- Receive XML data Error.\n";
                    return false;
                }


                return SignStatus;
            }
            return false;



        }
        #endregion

        #region Get HashVal from XML
        private static string GetHash(string input)
        {
            
            byte[] bytes = Encoding.UTF8.GetBytes(input);
            SHA256Managed hashstring = new SHA256Managed();
            byte[] hash = hashstring.ComputeHash(bytes);
            string hashString = string.Empty;
            foreach (byte x in hash)
            {
                hashString += String.Format("{0:x2}", x);
            }
            return hashString;
        }

        #endregion

        public void uploadbase64format(string ID, byte[] _SignedPDF) //อัพโหลดไฟล์และอัพเดทสถานะ
        {
            try
            {
                string FromType = From;   //ประเภท ฟอร์ม อ1 อ2 กน9 กน10
                String SignedPdfBase64 = Convert.ToBase64String(_SignedPDF);  //แปลงไฟล์เป็น Base64

                if (FromType == "CRKN" || FromType == "CNA2" || FromType == "RNA2" || FromType == "RNKN")
                {

                    if (FromType == "RNKN")
                    {
                        FromType = "CRKN";
                    }
                    //UpdateSignaturePath.UploadResponeMsg Result = new UpdateSignaturePath.UploadResponeMsg();
                    Boolean Result;
                    UpdateSignaturePath.UpdateKN9Status uploadpdf = new UpdateSignaturePath.UpdateKN9Status();   //เรียก webservice
                    Result = uploadpdf.UploadSignedPDF_CancelRenew(SignedPdfBase64, ID, FromType, txtReqNo_CencelRenew.Text);   //ws ตอบกลับมาเป็น path เพื่อส่งไปอัพเดท

                    MsgStatus += "- WebService update signed status: " + Result + "\n";


                }
                else
                {

                    UpdateSignaturePath.UploadResponeMsg Result = new UpdateSignaturePath.UploadResponeMsg();
                    UpdateSignaturePath.UpdateKN9Status uploadpdf = new UpdateSignaturePath.UpdateKN9Status();   //เรียก webservice
                    Result = uploadpdf.UploadSignedPDFLicense(SignedPdfBase64, ID, FromType);   //ws ตอบกลับมาเป็น path เพื่อส่งไปอัพเดท

                    if (Result.ResponseID == 1)  //ส่ง ws 
                    {
                        string pdfpath = Result.ResponseResult;
                        MsgStatus += "- Upload PDF to OCSB Server: OK\n";

                        UpdateSignaturePath.UpdateKN9Status mySer = new UpdateSignaturePath.UpdateKN9Status();
                        Boolean _result;
                        //string pdfFolder = "~/" + pathWS + "/";

                        switch (FromType)
                        {
                            case "A1":

                                _result = mySer.SignatureA1(ID, pdfpath);
                                MsgStatus += "- WebService update signed status: " + _result + "\n";
                                break;

                            case "A2":
                                _result = mySer.SignatureA2(ID, pdfpath);
                                MsgStatus += "- WebService update signed status: " + _result + "\n";

                                //กรณีคำร้องที่ส่งจาก Portal เมื่อลงนาม อ.2 แล้วส่งไฟล์ PDF ให้ Portal ด้วย
                                //if (SendReqFrom.ToLower() == "portal")
                                //{
                                //    SentStatus_ToPortal(SignedPdfBase64, ID, FromType);
                                //}

                                break;
                            case "KN9":

                                _result = mySer.SignatureKN9(ID, pdfpath);
                                MsgStatus += "- WebService update signed status: " + _result + "\n";
                                break;

                            case "KN10":
                                _result = mySer.SignatureKN10(ID, pdfpath);
                                MsgStatus += "- WebService update signed status: " + _result + "\n";

                                //กรณีคำร้องที่ส่งจาก Portal เมื่อลงนาม กน.10 แล้วส่งไฟล์ PDF ให้ Portal ด้วย
                                //if (SendReqFrom.ToLower() == "portal")
                                //{
                                //    SentStatus_ToPortal(SignedPdfBase64, ID, FromType);
                                //}

                                break;
                        }

                    }
                    else
                    {
                        MsgStatus += "- Upload PDF to OCSB Server: NO (" + Result.ResponseResult + ")\n";
                    }

                }



            }
            catch (WebException ex)
            {
                //String status = ((FtpWebResponse)ex.Response).StatusDescription;
                MsgStatus += "- Upload file to Server: Error (" + ex.Message + ")\n";
            }

        }

        public void SentStatus_ToPortal(string fileBase64, string spRefNo, string From)
        {
            try
            {
                OCSB ws = new OCSB();
                string Result = null;
                string userws = Properties.Settings.Default.UserwsPortal;
                string passws = Properties.Settings.Default.PasswsPortal;

                List<DeptFile> file = new List<DeptFile>();
                DeptFile df = new DeptFile();
                df.Base64Value = fileBase64;
                df.DocDetailText = "ใบอนุญาตส่งออกน้ำตาลทราย";

                if (From == "A2")
                {
                    df.DocTypeCode = "A2";
                    file.Add(df);
                    Result = ws.putUpdateData(userws, passws, spRefNo, "4", "", file.ToArray(), 0);
                }
                if (From == "KN10")
                {
                    df.DocTypeCode = "KN10";
                    file.Add(df);
                    Result = ws.putUpdateData(userws, passws, spRefNo, "13", "", file.ToArray(), 0);
                }

                MsgStatus += "- Send PDF to portal: " + Result + "\n";
            }
            catch (Exception ex)
            {
                MsgStatus += "- Send PDF to portal Error: " + ex.Message + "\n";
            }

        }

        public string ConvertFileToBase64(string fileName)
        {
            //string x = Convert.ToBase64String(System.IO.File.ReadAllBytes(fileName));
            string x = fileName;
            return x;
        }

        public bool DirectoryExists(string directory)
        {
            bool _Result = false;

            dynamic request = (FtpWebRequest)WebRequest.Create(directory);
            request.Method = WebRequestMethods.Ftp.ListDirectory;

            //Enter FTP Server credentials.
            request.Credentials = new NetworkCredential(FTPuser, FTPpassword);
            request.UsePassive = true;
            request.UseBinary = true;
            request.EnableSsl = false;

            try
            {
                using (request.GetResponse())
                {
                    _Result = true;
                }
            }
            catch (WebException ex)
            {
                _Result = false;
            }

            return _Result;
        }

        private bool FtpFolderCreate(string folder_name)
        {
            System.Net.FtpWebRequest request = (FtpWebRequest)FtpWebRequest.Create(folder_name);
            request.Method = WebRequestMethods.Ftp.MakeDirectory;

            //Enter FTP Server credentials.
            request.Credentials = new NetworkCredential(FTPuser, FTPpassword);
            request.UsePassive = true;
            request.UseBinary = true;
            request.EnableSsl = false;

            try
            {
                using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                {
                    // Folder created
                }
            }
            catch (WebException ex)
            {
                FtpWebResponse response = (FtpWebResponse)ex.Response;
                // an error occurred
                if (response.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable)
                {
                    return false;
                }
            }
            return true;
        }


        public void openDevTool()
        {
            _browser.ShowDevTools();
        }

        private void Browser_LoadingStateChanged(object sender, LoadingStateChangedEventArgs e)
        {
            if (e.IsLoading)
            {
                this.Cursor = Cursors.WaitCursor;
            }
            else
            {
                this.Cursor = Cursors.Default;
            }
        }

        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            ExitApp();
        }

        private static byte[] signWithKeyStore(byte[] originalPDF, string keyStorePath, string keystorePassword, string imagePath, String sReason, String sLocation, String urlqrcode, String FrmType)
        {
            PdfManipulator pdfM = new PdfManipulator();

            var webClient = new WebClient();
            byte[] qrimg = Convert.FromBase64String(urlqrcode);
            byte[] SigImg = webClient.DownloadData(imagePath);
            //byte[] SigImg = File.ReadAllBytes(imagePath);
            string signatureField = "SignBy";
            signatureField = pdfM.findSignatureField(originalPDF, signatureField);
            pdfM.setTimestamp(TIMESTAMP_URL, TIMESTAMP_USERNAME, TIMESTAMP_PASSWORD);

            // ====================================== get key & sign ======================================
            Pkcs12Store keystore = new Pkcs12Store(); // Create Key store
            keystore.Load(new FileStream(keyStorePath, FileMode.Open), keystorePassword.ToCharArray()); // Load key using filestream via path and password
            pdfM.setKeyStore(keystore, HASH_FUNCTION);
            string streturn = "true";
            return pdfM.signPdf(originalPDF, SigImg, signatureField, sReason, sLocation, streturn, qrimg, FrmType);
        }


        public static X509Certificate2 cert = null;
        public static byte[] signWithWindowStore(byte[] originalPDF, String imagePath, String sReason, String sLocation, String SNToken, String urlqrcode, String FrmType, X509Certificate2Collection scollection)
        {
            PdfManipulator pdfM = new PdfManipulator();

            var webClient = new WebClient();
            byte[] qrimg = Convert.FromBase64String(urlqrcode);
            byte[] SigImg = webClient.DownloadData(imagePath);
            //byte[] SigImg = File.ReadAllBytes(imagePath);
            string signatureField = "SignBy";
            signatureField = pdfM.findSignatureField(originalPDF, signatureField);

            cert = scollection[0];
            pdfM.setKeyStore(cert, HASH_FUNCTION);
            string streturn = "true";

            return pdfM.signPdf(originalPDF, SigImg, signatureField, sReason, sLocation, streturn, qrimg, FrmType);
        }

        public static byte[] ReadFully(Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }

        private void loadCertificateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _browser.Load(_webURL);
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExitApp();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void ExitApp()
        {
            this.Hide();
            Cef.Shutdown();
            Application.Exit();
        }

        private void testToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //webBrowser1.Navigate("file:\\D:\\@Workspace\\OCSB Website\\Website\\OSCB_Documents\\KN9\\2016\\11\\KN9_0015801830.pdf");
        }


        private X509Certificate2Collection GetCertificatesFromStore(bool validonly)
        {
            X509Store st = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            try
            {
                st.Open((OpenFlags.OpenExistingOnly | OpenFlags.ReadOnly));
                X509Certificate2Collection col = st.Certificates.Find(X509FindType.FindByIssuerName, "", validonly);
                return col;
            }
            catch (Exception ex)
            {
                return null;
            }
            finally
            {
                st.Close();
            }

        }


        public static string ConvertFullDate(object objData)
        {
            try
            {
                if ((!object.ReferenceEquals(objData, DBNull.Value)))
                {
                    System.Globalization.CultureInfo CI = default(System.Globalization.CultureInfo);
                    CI = System.Globalization.CultureInfo.GetCultureInfo("th-TH");
                    System.DateTime dt = Convert.ToDateTime(objData);
                    string strdate = null;
                    strdate = dt.ToString("D", CI);
                    string[] chDatTH = strdate.Split(' ');
                    string strTH = " พ.ศ " + chDatTH[chDatTH.Length - 1];
                    string sumTh = chDatTH[0] + " " + chDatTH[1] + " " + strTH;
                    return sumTh;
                }
                else
                {
                    return "";
                }
            }
            catch (Exception ex)
            {
                return "";
            }

        }


        static string base64String = null;
        private string CreateBarcode(string addressurl)
        {
            string code = addressurl;
            string width = "200";
            string height = "200";

            var url = string.Format("http://chart.apis.google.com/chart?cht=qr&chs={1}x{2}&chl={0}", code, width, height);
            WebResponse response = default(WebResponse);
            Stream remoteStream = default(Stream);
            StreamReader readStream = default(StreamReader);
            WebRequest request = WebRequest.Create(url);
            response = request.GetResponse();
            remoteStream = response.GetResponseStream();
            readStream = new StreamReader(remoteStream);
            System.Drawing.Image img = System.Drawing.Image.FromStream(remoteStream);

            //img.Save("D:/" + code + ".png");
            using (MemoryStream m = new MemoryStream())
            {
                img.Save(m, img.RawFormat);
                byte[] imageBytes = m.ToArray();
                base64String = Convert.ToBase64String(imageBytes);

            }
            response.Close();
            remoteStream.Close();
            readStream.Close();
            code = string.Empty;
            width = string.Empty;
            height = string.Empty;
            return base64String;
        }


        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        public static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
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


        public static X509Certificate2Collection CheckLCToken(string SNToken)
        {
            PdfManipulator pdfM = new PdfManipulator();
            // ====================================== get key & sign ======================================
            X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser);

            store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly); //Openstore with mode
            X509Certificate2Collection collection = (X509Certificate2Collection)store.Certificates; // Get collection from storname.my
            X509Certificate2Collection fcollection = (X509Certificate2Collection)collection.Find(X509FindType.FindByTimeValid, DateTime.Now, false);//show only cert which still timevalid
            X509Certificate2Collection scollection = X509Certificate2UI.SelectFromCollection
                (fcollection
                , "เลือกใบรับรอง "
                , "เลือกใบรับรองสำหรับการลงชื่อแบบดิจิตอลลงในเอกสาร"
                , X509SelectionFlag.SingleSelection);    //Opendialog selection
            try
            {
                //X509Certificate2 cert = scollection[0];    // Select first certificate from root   
                cert = scollection[0];
                pdfM.setKeyStore(cert, HASH_FUNCTION);

                //frmSignDS FrmDs = new frmSignDS();  //765f5b89035a67c1
                string snCertRegister = snString(SNToken);
                string snCertToken = snString(cert.SerialNumber.ToString());
                if (snCertRegister != snCertToken)
                {
                    DialogResult result = MessageBox.Show("S/N Certificate ไม่ตรงกับที่ลงทะเบียนไว้ในระบบ \nลงทะเบียนในระบบ: " + snCertRegister + "\nCertificate ที่ใช้: " + snCertToken, "Cancel", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                    if (result == DialogResult.OK)
                    {
                        return null;
                        
                    }
                }

            }catch(Exception ex)
            {
                MessageBox.Show("พบปัญหาในการเรียกข้อมูล Token : "+ ex.Message);

            }
            return scollection;
            }









      



    }
}
