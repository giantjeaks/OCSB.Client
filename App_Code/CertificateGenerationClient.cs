using iTextSharp.text.pdf;
using Org.BouncyCastle.Pkcs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace CertificateGenerationSampleCode
{
    class CertificateGenerationClient
    {
        public static string RESOURCE_FOLDER = @"C:\outputs\"; //System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + @"\";
        public static string XMLDATA_PATH = RESOURCE_FOLDER + @"resource\Aor2.xml";
        public static string CANONICALIZATION_XML_PATH = RESOURCE_FOLDER + @"out\canonicalize_fda_sample_data.xml";
        public static string BLANK_ECERTIFICATE_PATH = RESOURCE_FOLDER + @"out\blank_eCertificate.pdf";
        public static string FILLED_ECERTIFICATE_PATH = RESOURCE_FOLDER + @"out\filled_eCertificate.pdf";        

        public static string HASH_FUNCTION = "SHA256";
        public static string HASH_FUNCTION_OID = "2.16.840.1.101.3.4.2.1";

        public static string CERT_GEN_URL = "https://test.certificate.teda.th/service";
        public static string CERT_GEN_USERNAME = "nakorn_r";
        public static string CERT_GEN_PASSWORD = "3LHnacoT";

        public static string TIMESTAMP_URL = "http://test.time.teda.th";
        public static string TIMESTAMP_USERNAME = "nakorn";
        public static string TIMESTAMP_PASSWORD = "z!V7sJER";

        public static string KEYSTORE_PATH = RESOURCE_FOLDER + @"resource\TeDAGiGParticipant2016.p12";
        public static string KEYSTORE_PASSWORD = "P@ssw0rd";
        public static string SIG_IMAGE = RESOURCE_FOLDER + @"resource\Nakorn_Signature.png";

        public static void SignMeNow(string SIGNED_ECERTIFICATE_PATH, string token)
        {
            //PdfManipulator manipulator = new PdfManipulator();
            //byte[] xx = File.ReadAllBytes(RESOURCE_FOLDER + @"resource\A2Form_1p1.pdf");
            //byte[] filledPDF = manipulator.importXml(xx, new FileStream(RESOURCE_FOLDER + @"resource\Example_A2Document_1p1-Sign.xml", FileMode.Open), true);

            //File.WriteAllBytes(RESOURCE_FOLDER + @"out\demo2.pdf", filledPDF);

            CANONICALIZATION_XML_PATH = CANONICALIZATION_XML_PATH.Replace("canonicalize_fda_sample_data", token+"_CAN");
            BLANK_ECERTIFICATE_PATH = BLANK_ECERTIFICATE_PATH.Replace("blank_eCertificate", token+"_BLK");
            FILLED_ECERTIFICATE_PATH = FILLED_ECERTIFICATE_PATH.Replace("filled_eCertificate", token+"_FILLED");

            if (File.Exists(CANONICALIZATION_XML_PATH)) File.Delete(CANONICALIZATION_XML_PATH);
            if (File.Exists(BLANK_ECERTIFICATE_PATH)) File.Delete(BLANK_ECERTIFICATE_PATH);
            if (File.Exists(FILLED_ECERTIFICATE_PATH)) File.Delete(FILLED_ECERTIFICATE_PATH);
            if (File.Exists(SIGNED_ECERTIFICATE_PATH)) File.Delete(SIGNED_ECERTIFICATE_PATH);

            //======================================================
            // Prepare Data for create Certifiate Generation Reqeust
            //======================================================
            // Load xml data 
            byte[] xmldata = File.ReadAllBytes(XMLDATA_PATH);

            // Apply canonicalize function to loaded xml
            byte[] canonicalizeXmlData = new XMLCanonicalization().canonicalXML(xmldata);
            File.WriteAllBytes(CANONICALIZATION_XML_PATH, canonicalizeXmlData);

            HashAlgorithm hashFn = HashAlgorithm.Create(HASH_FUNCTION);
            byte[] rawHashValue = hashFn.ComputeHash(canonicalizeXmlData);

            string certRefNumber = "RFL 001";
            string hashValue = Convert.ToBase64String(rawHashValue);
            string formOID = "2.16.764.1.4.100.1.5.1";

            Console.WriteLine("Certificate Refernece Number = " + certRefNumber);
            Console.WriteLine("Form OID = " + formOID);
            Console.WriteLine("Hash Value = " + hashValue);
            Console.WriteLine("Hash Algorithm = " + HASH_FUNCTION_OID);
            Console.WriteLine("Request data preparation complete");

            //======================================================
            // Establish Certificate generation request
            //======================================================
            // Create certificate generation connection
            CertificateGenerationClientConnector connector = new CertificateGenerationClientConnector(CERT_GEN_URL, CERT_GEN_USERNAME, CERT_GEN_PASSWORD);
            CertificateGenerationWS.certificateGenerationResponse response = connector.submitRequest(certRefNumber, formOID, rawHashValue, HASH_FUNCTION_OID);

            Console.WriteLine("Certificate Generation response = " + response.statusMessage + ":Code(" + response.statusCode + ")");

            if (!response.statusCode.Equals("0")) // Check eCertificate retrieve is error or not
            {
                Console.ReadLine();
                return;
            }

            // continue if eCertificate request done successfully
            string rawECertificate = ((CertificateGenerationWS.certificateGenerationResponseObject)response.responseData).certificateForm;
            byte[] eCertificate = Convert.FromBase64String(rawECertificate);

            File.WriteAllBytes(BLANK_ECERTIFICATE_PATH, eCertificate);

            //================================================================
            // Import xml content to eCertificate from certificate generation
            //================================================================

            PdfManipulator manipulator = new PdfManipulator();
            byte[] filledPDF = manipulator.importXml(eCertificate, new FileStream(CANONICALIZATION_XML_PATH, FileMode.Open), true);

            File.WriteAllBytes(FILLED_ECERTIFICATE_PATH, filledPDF);

            // Digital Sign specify certificate 
            byte[] signedPDF = signWithWindowStore(filledPDF, SIG_IMAGE);
            File.WriteAllBytes(SIGNED_ECERTIFICATE_PATH, signedPDF);
            Console.WriteLine("Complete");
            Console.ReadLine();

            
        }

        private static byte[] signWithWindowStore(byte[] originalPDF, String imagePath)
        {
            PdfManipulator pdfM = new PdfManipulator();

            byte[] SigImg = File.ReadAllBytes(imagePath);
            string signatureField = "signature";
            signatureField = pdfM.findSignatureField(originalPDF, signatureField);
            pdfM.setTimestamp(TIMESTAMP_URL, TIMESTAMP_USERNAME, TIMESTAMP_PASSWORD);

            // ====================================== get key & sign ======================================
            X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser);

            store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly); //Openstore with mode
            X509Certificate2Collection collection = (X509Certificate2Collection)store.Certificates; // Get collection from storname.my
            X509Certificate2Collection fcollection = (X509Certificate2Collection)collection.Find(X509FindType.FindByTimeValid, DateTime.Now, false);//show only cert which still timevalid
            X509Certificate2Collection scollection = X509Certificate2UI.SelectFromCollection
                (fcollection
                , "กรุณาเลือก Certificate สำหรับเซ็นเอกสาร"
                , "เลือกใบรับรองจากรายการข้างล่างนี้"
                , X509SelectionFlag.SingleSelection);//Opendialog selection
            X509Certificate2 cert = scollection[0];// Select first certificate from root        
            pdfM.setKeyStore(cert, HASH_FUNCTION);

            return pdfM.signPdf(originalPDF, SigImg, signatureField, "", "","");
        }
    }
}