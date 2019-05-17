using System;
using System.Collections.Generic;
//using System.Linq;
//using System.Text;
using System.IO;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.security;
using iTextSharp.text;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Crypto;
using System.Security.Cryptography.X509Certificates;
//using System.Security.Cryptography;
using LTVDigitalSignature;
using TeDATime;
using Org.BouncyCastle.Security;
using SugarSystem.Client;
using System.Security.Cryptography;
using System.Text;
using iTextSharp.tool.xml.xtra.xfa;
using iTextSharp.license;
using System.Net;
using System.Collections.Specialized;
using Syncfusion.Pdf.Parsing;
using Syncfusion.Pdf.Graphics;
using OCSB_Client.Provider;
//using System.Configuration;

namespace CertificateGenerationSampleCode
{
   public  class PdfManipulator
    {
        public string TSAurl = SugarSystem.Client.Properties.Settings.Default.URLts;
        public string HASH_TSA = SugarSystem.Client.Properties.Settings.Default.HASH_FUNCTION;

        private string tsaUrl = "";
        private string tsaUsername = "";
        private string tsaPassword = "";
        private AsymmetricKeyParameter privateKey;
        private ICollection<Org.BouncyCastle.X509.X509Certificate> chain;
        private List<ICrlClient> crlList;

        private IExternalSignature signature;
        private bool tsa = false;

        public X509Certificate2 certificate;

        //ImportXML and Flatten to tmp folder
        public byte[] importXml(byte[] inputPdf, Stream xmlData, Boolean isSign)
        {
            Console.WriteLine("Read PDF");
            PdfReader reader = new PdfReader(inputPdf);
            using (var output = new MemoryStream())
            {
                var stamper = new PdfStamper(reader, output);
                stamper.Writer.CloseStream = false;
                AcroFields form = stamper.AcroFields;
             

                XfaForm xfa = form.Xfa;
                Console.WriteLine("Fill Data");
                xfa.FillXfaForm(xmlData);

                //stamper.FormFlattening = true;
                //form.GenerateAppearances = true;
                stamper.Close();

                //File.WriteAllBytes(AppDomain.CurrentDomain.BaseDirectory + "Fill Data.pdf", output.ToArray());



                using (var flattenOutput = new MemoryStream())
                {
                    Document document = new Document();
                    PdfWriter writer = PdfWriter.GetInstance(document, flattenOutput);
                    XFAFlattener xfaf = new XFAFlattener(document, writer);
                    output.Position = 0;
                    LicenseKey.LoadLicenseFile(AppDomain.CurrentDomain.BaseDirectory + "res\\itextkey.xml"); //license iTextSharp Trial Version
                    xfaf.Flatten(new PdfReader(output));


                    reader.Close();
                    document.Close();
                    //File.WriteAllBytes(AppDomain.CurrentDomain.BaseDirectory + "flatt.pdf", flattenOutput.ToArray());
                    return flattenOutput.ToArray();
                }
            }



        }

        public byte[] binDataXFA(byte[] inputPdf,string frmType,string Oid,string RefNo,string RequestNo, string CompNo,string Username,string password,string signerName,string signerPosition)
        {
            string strApiForm = SugarSystem.Client.Properties.Settings.Default.URL_API;
            switch (frmType)
            {
                case "KN9":
                    strApiForm += @"/LicenseKN9";

                    //Get data from API
                    OCSB_Client.OCSB_Classes.Document Dockn9 = OCSBForm_Provider.Deserializing(strApiForm, RefNo, Username, password, "");
                    using (var ms_PDf = new MemoryStream(inputPdf))
                    {
                        byte[] pdfBinded = OCSB_Binding.KN9_Bind(ms_PDf, Dockn9, signerName, signerPosition);
                        return pdfBinded;
                    }
                case "A1":
                    strApiForm += @"/LicenseA1";

                    //Get data from API
                    OCSB_Client.OCSB_Classes.Document DocA1 = OCSBForm_Provider.Deserializing(strApiForm, RefNo, Username, password, "");
                    using (var ms_PDf = new MemoryStream(inputPdf))
                    {
                        byte[] pdfBinded = OCSB_Binding.A1_Bind(ms_PDf, DocA1, signerName, signerPosition);
                        return pdfBinded;
                    }
                case "KN10":
                    strApiForm += @"/LicenseKN10";

                    //Get data from API
                    OCSB_Client.OCSB_Classes.Document DocKN10 = OCSBForm_Provider.Deserializing(strApiForm, RefNo, Username, password, "");
                    using (var ms_PDf = new MemoryStream(inputPdf))
                    {
                        byte[] pdfBinded = OCSB_Binding.KN10_Bind(ms_PDf, DocKN10, signerName, signerPosition);
                        return pdfBinded;
                    }
                case "A2":
                    strApiForm += @"/LicenseA2";
                    //Get data from API
                    OCSB_Client.OCSB_Classes.Document DocA2 = OCSBForm_Provider.Deserializing(strApiForm, RefNo, Username, password, "");
                    using (var ms_PDf = new MemoryStream(inputPdf))
                    {
                        byte[] pdfBinded = OCSB_Binding.A2_Bind(ms_PDf, DocA2, signerName, signerPosition);
                        return pdfBinded;
                    }
                    ///*** Same form Renew KN10 and Cancel KN10 so can use same bindig  method *-*-*- Classify with data in API -*-*-*-
                case "CRKN":
                    strApiForm += @"/CancelKN10";
                    //Get data from API
                    OCSB_Client.OCSB_Classes.CancelDocument DocCRKN = OCSBForm_Provider.CanCelDeserializing(strApiForm, RequestNo, Username, password);
                    using (var ms_PDf = new MemoryStream(inputPdf))
                    {
                        byte[] pdfBinded = OCSB_Binding.CRKN_Bind(ms_PDf, DocCRKN, signerName, signerPosition);
                        return pdfBinded;
                    }
                    
                case "RNKN":
                    strApiForm += @"/RenewKN10";
                    //Get data from API
                    OCSB_Client.OCSB_Classes.CancelDocument DocRNKN = OCSBForm_Provider.CanCelDeserializing(strApiForm, RequestNo, Username, password);
                    using (var ms_PDf = new MemoryStream(inputPdf))
                    {
                        byte[] pdfBinded = OCSB_Binding.CRKN_Bind(ms_PDf, DocRNKN, signerName, signerPosition);
                        return pdfBinded;
                    }
                    ///*******---------------------------- *-* -* -* -* -*- * -*-*
                case "CNA2":
                    strApiForm += @"/CancelA2";
                    //Get data from API
                    OCSB_Client.OCSB_Classes.A2CancelDocument DocCNA2 = OCSBForm_Provider.A2CanCelDeserializing(strApiForm, RequestNo, Username, password);
                    using (var ms_PDf = new MemoryStream(inputPdf))
                    {
                        byte[] pdfBinded = OCSB_Binding.CNA2_Bind(ms_PDf, DocCNA2, signerName, signerPosition);
                        return pdfBinded;
                    }
                case "RNA2":
                    strApiForm += @"/RenewA2";
                    //Get data from API
                    OCSB_Client.OCSB_Classes.A2RenewDocument DocRNA2 = OCSBForm_Provider.A2RenewDeserializing(strApiForm, RequestNo,CompNo, Username, password);
                    using (var ms_PDf = new MemoryStream(inputPdf))
                    {
                        byte[] pdfBinded = OCSB_Binding.RNA2_Bind(ms_PDf, DocRNA2, signerName, signerPosition);
                        return pdfBinded;
                    }
            }

            return null;

           

           






        }

        public string findSignatureField(byte[] inputPdf, string fieldName)
        {
            string resultFieldName = "";
            Console.WriteLine("Read PDF");
            PdfReader reader = new PdfReader(inputPdf);
            AcroFields form = reader.AcroFields;
            foreach (string key in form.Fields.Keys)
            {
                if (key.IndexOf(fieldName) > 0)
                {
                    if (AcroFields.FIELD_TYPE_SIGNATURE == form.GetFieldType(key))
                    {
                        Console.WriteLine("Signature Field Found");
                        resultFieldName = key;
                        break;
                    }
                }
            }
            reader.Close();
            return resultFieldName;
        }

        public void setTimestamp(String tsaUrl, String tsaUsername, String tsaPassword)
        {
            this.tsaUrl = tsaUrl;
            this.tsaUsername = tsaUsername;
            this.tsaPassword = tsaPassword;
            this.tsa = true;
        }

        public void setKeyStore(Pkcs12Store keystore, string hashAlgorithm) //Getkey
        {
            //get name
            String alias = "";
            foreach (string al in keystore.Aliases)
            {
                if (keystore.IsKeyEntry(al) && keystore.GetKey(al).Key.IsPrivate) // ****  what this if do ?
                {
                    alias = al;
                    break;
                }
            }

            //get privatekey
            this.privateKey = keystore.GetKey(alias).Key;

            //create instance of Cretificate list for Long Time 
            this.chain = new List<Org.BouncyCastle.X509.X509Certificate>();

            X509CertificateEntry[] a = keystore.GetCertificateChain(alias);

            //foreach (X509CertificateEntry entry in a)
            //{
            //    this.chain.Add(entry.Certificate);
            //}

            for (int i = 0; i < a.Length; i++)
            {
                this.chain.Add(a[i].Certificate);
            }

            //for (int i = a.Length; i > 0; i--)
            //{
            //    this.chain.Add(a[i - 1].Certificate);
            //}            

            this.signature = new PrivateKeySignature(privateKey, hashAlgorithm);
        }

        public void setKeyStore(X509Certificate2 cert, string hashAlgorithm) //Getkey
        {
            /*GET Certificate chain from Cert and translate info x509 Bouncycastle List*/
            Org.BouncyCastle.X509.X509Certificate bcCert = Org.BouncyCastle.Security.DotNetUtilities.FromX509Certificate(cert); // ไม่ได้เก็ต ของผู้ออก Certificate Chain มาด้วย 
            chain = new List<Org.BouncyCastle.X509.X509Certificate> { bcCert };

            // Initial .netx509 certchain and build chain
            X509Chain cert_chain = new X509Chain();
            cert_chain.Build(cert);

            int i = 0;
            //Add chain into bouncyCastle.chain
            foreach (X509ChainElement entry in cert_chain.ChainElements)
            {
                if (i != 0)//Skip first certchain due to cert_chain.Build provided first chain(entry.chain.[0]) 
                    this.chain.Add(Org.BouncyCastle.Security.DotNetUtilities.FromX509Certificate(entry.Certificate));
                i++;

                //this.chain.Add(Org.BouncyCastle.Security.DotNetUtilities.FromX509Certificate(entry.Certificate));
            }

            this.signature = new RSAProviderPrivateKey(cert, hashAlgorithm);
        }


        public byte[] signPdf(byte[] inputPdf, byte[] sigImg, string signatureField, string Reason, string Location, string streturn, byte[] urlqrcode, String FrmType)
        {
            if (streturn == "true")
            {

                this.getCRLList();

                //Console.WriteLine("Read PDF");
                PdfReader reader = new PdfReader(inputPdf);
                MemoryStream output = new MemoryStream();

                PdfStamper stamper = PdfStamper.CreateSignature(reader, output, '\0', null, true);
                PdfSignatureAppearance sap = stamper.SignatureAppearance;

                sap.Reason = Reason;
                sap.Location = Location;

                // Set Signature Image
                if (sigImg != null)
                {
                    sap.SignatureGraphic = Image.GetInstance(sigImg);
                    sap.ImageScale = -1;
                    sap.SignatureRenderingMode = PdfSignatureAppearance.RenderingMode.GRAPHIC;
                }

                //Set QR Code
                AcroFields form = stamper.AcroFields;

                try
                {
                    //AcroFields.FieldPosition fq = form.GetFieldPositions("QRCode")[0];  **Cannot use old code cause Flatten hide "QRCode" Field so just add image to PDFA
                    //frmSignDS.MsgStatus += "- form.GetFieldPositions(QRCode): OK\n";

                    if ((urlqrcode != null) || (urlqrcode.Equals("") != true))
                    {
                        //Rectangle location2 = new Rectangle(f.position.Left, f.position.Bottom, f.position.Right, f.position.Top);
                        //var pdfContentByte = stamper.GetOverContent(fq.page);
                        //Image image = Image.GetInstance(urlqrcode);
                        //image.SetAbsolutePosition(fq.position.Left, fq.position.Bottom);
                        //image.ScaleAbsolute(fq.position.Width, fq.position.Height);
                        //pdfContentByte.AddImage(image);
                        //frmSignDS.MsgStatus += "- Set QRCode Image: OK\n";
                        var qrPdfContent = stamper.GetOverContent(1);
                        switch (FrmType)
                        {
                            case "A2":

                                Image image = Image.GetInstance(urlqrcode);
                                image.SetAbsolutePosition(35, 20);
                                image.ScaleAbsolute(75, 75);
                                qrPdfContent.AddImage(image);
                                stamper.Close();
                                break;
                            case "KN10":
                                Image imageKN10qr = Image.GetInstance(urlqrcode);
                                imageKN10qr.SetAbsolutePosition(25, 20);
                                imageKN10qr.ScaleAbsolute(75, 75);
                                qrPdfContent.AddImage(imageKN10qr);
                                stamper.Close();
                                break;
                        }


                    }

                }
                catch (Exception e)
                {
                    //frmSignDS.MsgStatus += "- Set QRCode Image: Error (" + e.Message + ")\n";
                }


                // Set Signature Field
                if (signatureField.Equals("") || signatureField == null)
                {


                    switch (FrmType)
                    {
                        case "KN9":
                            Rectangle location = new Rectangle(400, 75, 490, 115);
                            sap.SetVisibleSignature(location, 1, "signatureField");
                            break;
                        case "A1":
                            Rectangle location_A1 = new Rectangle(400, 95, 490, 135);
                            sap.SetVisibleSignature(location_A1, 1, "signatureField");
                            break;
                        case "KN10":
                            Rectangle location_KN10 = new Rectangle(400, 55, 490, 95);
                            sap.SetVisibleSignature(location_KN10, 1, "signatureField");
                            break;
                        case "A2":
                            Rectangle location_A2 = new Rectangle(410, 95, 490, 125);
                            sap.SetVisibleSignature(location_A2, 1, "signatureField");
                            break;
                        case "CRKN":
                            Rectangle location_CRKN = new Rectangle(360, 205, 450, 245);
                            sap.SetVisibleSignature(location_CRKN, 1, "signatureField");
                            break;
                        case "RNKN":
                            Rectangle location_RNKN = new Rectangle(360, 200, 450, 240);
                            sap.SetVisibleSignature(location_RNKN, 1, "signatureField");
                            break;

                        case "CNA2":
                            Rectangle location_CRA2 = new Rectangle(360, 325, 450, 365);
                            sap.SetVisibleSignature(location_CRA2, 1, "signatureField");
                            break;
                        case "RNA2":
                            Rectangle location_RNA2 = new Rectangle(370, 75, 460, 115);
                            sap.SetVisibleSignature(location_RNA2, 1, "signatureField");
                            break;

                    }
                    //Set signature Img to all pages
                    int countPage = reader.NumberOfPages;
                    for (int page = 2; page <= countPage; page++)
                    {
                        var pdfContentByte = stamper.GetOverContent(page);
                        Image repSignImg = Image.GetInstance(sigImg);
                        repSignImg.SetAbsolutePosition(430, 70);
                        repSignImg.ScaleAbsolute(70, 40);
                        pdfContentByte.AddImage(repSignImg);
                    }
                }
                else
                {
                    if (SugarSystem.Client.Properties.Settings.Default.signatureField2 != "")
                    {
                        //AcroFields form = stamper.AcroFields;
                        AcroFields.FieldPosition f = form.GetFieldPositions(SugarSystem.Client.Properties.Settings.Default.signatureField2)[0];

                        Rectangle location2 = new Rectangle(f.position.Left, f.position.Bottom, f.position.Right, f.position.Top);
                        //sap.SetVisibleSignature(location2, f.page, "signatureField");

                        var pdfContentByte = stamper.GetOverContent(f.page);
                        Image image = Image.GetInstance(sigImg);
                        image.SetAbsolutePosition(f.position.Left + 15, f.position.Bottom - 20);
                        image.ScaleAbsolute(f.position.Width - 35, f.position.Height + 15);
                        pdfContentByte.AddImage(image);
                    }

                    sap.SetVisibleSignature(signatureField);

                }

                sap.CertificationLevel = PdfSignatureAppearance.NOT_CERTIFIED;


                //Create TSA server
                ITSAClient tsaClient = null;
                Boolean isTsaConnected = false;
                if (tsa)
                {
                    //tsaClient = new TSAClientBouncyCastle(tsaUrl, tsaUsername, tsaPassword);

                    //this.certificate = SelectUserCertificate(@"C:\Users\DevNew\Desktop\OCSB Borwser Client\resource\TeDAGiGParticipant2016.p12");
                    //this.certificate = new X509Certificate2(@"C:\outputs\resource\tedagig2016.p12", "P@ssw0rd");
                    this.certificate = SelectUserCertificate();
                    
                    tsaClient = new TeDATimeSSLClient(TSAurl, certificate);

                    for (int retry = 0; retry < 5; retry++)
                    {
                        try
                        {
                            //int hash = tsaClient.GetHashCode();
                            string testString = "test";
                            byte[] digest;
                            using (SHA256Managed sha256 = new SHA256Managed())
                            {
                                digest = sha256.ComputeHash(Encoding.UTF8.GetBytes(testString));
                            }
                            tsaClient.GetTimeStampToken(digest);
                            isTsaConnected = true;
                            break;
                        }
                        catch (Exception e)
                        {
                            //Console.WriteLine(e.StackTrace);
                        }
                        //Console.WriteLine("retry " + (retry + 1));
                    }
                }


                //Do Signing Check not null timestamp and crl
                if (tsaClient != null && crlList != null && isTsaConnected)
                {
                    try
                    {
                        ////MakeSignature.SignDetached(sap, this.signature, chain, this.crlList, null, tsaClient, 0, CryptoStandard.CADES);

                        MakeSignature.SignDetached(sap, new RSAProviderPrivateKey(this.certificate, HASH_TSA), getCertificateChain(this.certificate), null, null, tsaClient, 0, CryptoStandard.CMS);
                        //MakeSignature.SignDetached(sap, signature, chain, null, null, null, 0, CryptoStandard.CMS);
                        frmSignDS.MsgStatus += "- Signed Status: OK\n";
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.StackTrace);
                    }
                }
                else
                {
                    MakeSignature.SignDetached(sap, signature, chain, null, null, null, 0, CryptoStandard.CMS);
                    frmSignDS.MsgStatus += "- Signed Status: OK\n";
                    //Console.WriteLine("Cannot sign the PDF file.");
                    //return null;
                }
                reader.Close();
                stamper.Close();
                signature = null;

                return output.ToArray();

                #region [Old Code] Get time stamp
                //try
                //{
                //    if (status == "ok")
                //    {

                //        //Get Time Stamp Server
                //        //MakeSignature.SignDetached(sap, new RSAProviderPrivateKey(this.certificate, HASH_TSA), getCertificateChain(this.certificate), null, null, tsaClient, 0, CryptoStandard.CMS);
                //        MakeSignature.SignDetached(sap, signature, chain, null, null, null, 0, CryptoStandard.CMS);
                //        frmSignDS.MsgStatus += "- Signed Status: OK\n";
                //    }
                //}
                //catch (Exception e)
                //{
                //    try
                //    {
                //        reader.Close();
                //        stamper.Close();
                //        signature = null;
                //        throw new Exception("Cannot sign the PDF file.", e);
                //    }
                //    catch (Exception ex)
                //    {
                //        Console.WriteLine(ex.Message);
                //        return null;
                //    }
                //}

                //return output.ToArray();
                #endregion
            }

            return null;
        }


        public string status;

        public void getCRLList()
        {
            try
            {
                //Create CRLs list
                this.crlList = new List<ICrlClient>();
                ICrlClient crlOnline = new CrlClientOnline(this.chain);
                this.crlList.Add(crlOnline);
                status = "ok";
            }
            catch (Exception ex)
            {
                status = "cancel";
                Console.WriteLine(ex.Message);

                return;
            }
        }

        static ICollection<Org.BouncyCastle.X509.X509Certificate> getCertificateChain(X509Certificate2 certs)
        {
            Org.BouncyCastle.X509.X509Certificate bcCert = DotNetUtilities.FromX509Certificate(certs); ICollection<Org.BouncyCastle.X509.X509Certificate> chain = new List<Org.BouncyCastle.X509.X509Certificate> { bcCert }; X509Chain cert_chain = new X509Chain(); cert_chain.Build(certs);
            int i = 0; foreach (X509ChainElement entry in cert_chain.ChainElements) { if (i != 0) chain.Add(DotNetUtilities.FromX509Certificate(entry.Certificate)); i++; }
            return chain;
        }

        static X509Certificate2 SelectUserCertificate()
        {
            X509Certificate2 selectedCert = null;
            X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            // Create Windows Cert store point to storename = my and storelocation = currentuser
            store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);
            X509Certificate2Collection collection = new X509Certificate2Collection();
            X509Certificate2Collection fcollection = new X509Certificate2Collection();
            X509Certificate2Collection scollection = new X509Certificate2Collection();
            collection = (X509Certificate2Collection)store.Certificates;
            fcollection =
            (X509Certificate2Collection)collection.Find(X509FindType.FindByTimeValid,
            DateTime.Now, false);
            scollection = X509Certificate2UI.SelectFromCollection(fcollection,
            "เลือกใบรับรองที่ใช้งานสำหรับ TimeStamp",
            "กรุณาเลือกใบรับรองของคุณสำหรับลงเวลา",
            X509SelectionFlag.SingleSelection);//Opendialog selection
            if (scollection.Count > 0)
            {
                X509Certificate2Enumerator en = scollection.GetEnumerator();
                en.MoveNext();
                selectedCert = en.Current;
            }

            return selectedCert;
        }

        static X509Certificate2 SelectUserCertificate(string path)
        {
            X509Certificate2 selectedCert = null;

            selectedCert = new X509Certificate2(path, "P@ssw0rd");

            return selectedCert;
        }

        public byte[] ConvertToPDFa3(byte[] OriginPDF, byte[] FileStore, string AttachXML, string FormOID,string SchemaID, string hashVal, string RefNo, string SerialNumber, string GenTime)
        {
            //***************************************
            // Convert PDF to PDFa3 and Attach XML .
            //***************************************
            //Create Reader for Reading PDF in  byte[]

            using (var reader = new PdfReader(OriginPDF))
            {
                // Open Stream for Converting
                using (var Convertstream = new MemoryStream())
                {
                    // New Doc for PDF/A-3
                    Document pdfAdocument = new Document();

                    //Init instance for pdfAdocument
                    PdfAWriter writer = PdfAWriter.GetInstance(pdfAdocument, Convertstream, PdfAConformanceLevel.PDF_A_3U);



                    pdfAdocument.AddHeader("Hash Value", hashVal);
                    pdfAdocument.AddHeader("Hash Algorithm", "SHA256");
                    pdfAdocument.AddHeader("Form OID", FormOID);
                    pdfAdocument.AddHeader("Schema OID", SchemaID);
                    pdfAdocument.AddHeader("Serial Number", SerialNumber);
                    pdfAdocument.AddHeader("Generate time", GenTime);

                                                    
                    writer.CreateXmpMetadata();

                    if (!pdfAdocument.IsOpen())
                        pdfAdocument.Open();

                    PdfContentByte cb = writer.DirectContent; // Holds the PDF data	

                    //Count original PDF pages and uses for pdfa-3
                    PdfImportedPage page;
                    int pageCount = reader.NumberOfPages;
                    for (int i = 0; i < pageCount; i++)
                    {
                        pdfAdocument.NewPage();
                        page = writer.GetImportedPage(reader, i + 1);
                        cb.AddTemplate(page, 0, 0);
                    }

                    //Create Output Intents 
                    ICC_Profile icc = ICC_Profile.GetInstance(AppDomain.CurrentDomain.BaseDirectory + "res\\sRGB Color Space Profile.icm");
                    writer.SetOutputIntents("sRGB IEC61966-2.1", "", "http://www.color.org", "sRGB IEC61966-2.1", icc);




                    //---------Code ตอนพยายามจะเพิ่มความเกี่ยวข้องของ pdf และไฟล์ xml ที่แนบลงไป
                    //PdfDictionary parameters = new PdfDictionary();
                    //parameters.Put(PdfName.MODDATE,pdfAdocument.GetAccessibleAttribute(PdfName.MODDATE));
                    //PdfFileSpecification fileSpec = PdfFileSpecification.FileEmbedded(writer, AttachXML, "XML for " + RefNo + " Certificate", FileStore, false, "text/xml", parameters);
                    //fileSpec.Put(new PdfName("AFRelationship"), new PdfName("Alternative"));
                    //writer.AddFileAttachment(string.Format("Attached for {0}", RefNo), fileSpec);

                    //---------Code ตอนพยายามจะเพิ่มความเกี่ยวข้องของ pdf และไฟล์ xml ที่แนบลงไป V2
                    //PdfFileSpecification fileSpecxlsx = PdfFileSpecification.FileEmbedded(writer, AttachXML, "0016200015.xml", FileStore);
                    //fileSpecxlsx.Put(new PdfName("AFRelationship"), new PdfName("Alternative"));
                    //writer.AddFileAttachment("XML for " + RefNo + " Certificate", fileSpecxlsx);


                    //Embedded File to PDFa3 --------- Code ปัจจุบันที่ทำได้จริง
                    writer.AddFileAttachment("XML for " + RefNo + " Certificate", FileStore, AttachXML, "ContentInformation.xml", new PdfName("Alternative"));





                    //Close all file to finish 
                    pdfAdocument.Close();
                    reader.Close();

                    //Test writes output in Phys path
                    //File.WriteAllBytes(@"E:\\Workspace\\OCSB\\App\\OCSB Borwser Client V3.0.0_TEST\\bin\\x64\\Debug\\tmp_xml\\OutPDFa_withNoSign.pdf", Convertstream.ToArray());
                    byte[] filledPDfa = Convertstream.ToArray();
                    return filledPDfa;

                }


            }



            // End Convert and Attached
        }


        public Dictionary<string, string> GetPdfProperties(byte[] PDF)
        {
            Dictionary<string, string> propertyInfo = null;

            using (PdfReader reader = new PdfReader(PDF))
            {
                propertyInfo = reader.Info;
                reader.Close();
            }

            return propertyInfo;
        }

        public string getFormViaOid(Uri uri, string Oid)
        {
            using (var client = new WebClient())
            {
                var method = "POST"; // If your endpoint expects a GET then do it.
                var parameters = new NameValueCollection();
                parameters.Add("Oid", Oid);
                /* Always returns a byte[] array data as a response. */
                var response_data = client.UploadValues(uri, method, parameters);
                var strRes = Encoding.UTF8.GetString(response_data);
                return strRes;
            }

        }

        

    }
}
