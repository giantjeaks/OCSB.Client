using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace CertificateGenerationSampleCode
{
    class XMLCanonicalization
    {
        public byte[] canonicalXML(byte[] xmlData)
        {
            //create c14n instance and load in xml file, false=ignore comment
            XmlDsigC14NTransform c14n = new XmlDsigC14NTransform(false);

            //Create XML object with preserve whitespace 
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.PreserveWhitespace = true;
            MemoryStream ms = new MemoryStream(xmlData);
            xmlDoc.Load(ms);

            //Load xml to canonicalizer
            c14n.LoadInput(xmlDoc);

            //Get canonicalized XML
            Stream s1 = (Stream)c14n.GetOutput(typeof(Stream));

            //Remove &#xD;
            StreamReader reader = new StreamReader(s1);
            string sXml = reader.ReadToEnd();
            sXml = sXml.Replace("&#xD;", "");
            s1 = convertStringToStream(sXml);

            return convertStreamToByteArray(s1);
        }

        /*
        * Transform data from stream to byte array
        */
        public static byte[] convertStreamToByteArray(Stream input)
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

        /*
        * Transform data from string to stream
        */
        public Stream convertStringToStream(string s)
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }
    }
}
