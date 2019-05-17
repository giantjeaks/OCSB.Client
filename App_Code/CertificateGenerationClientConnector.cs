using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CertificateGenerationSampleCode.CertificateGenerationWS;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Net;

namespace CertificateGenerationSampleCode
{
    class CertificateGenerationClientConnector
    {
        CertificateGenerationServiceClient wsClient = null;

        public CertificateGenerationClientConnector(String serviceEndpoint, String username, String password)
        {
            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            wsClient = new CertificateGenerationServiceClient("CertificateGenerationServicePort", serviceEndpoint);
            wsClient.ClientCredentials.UserName.UserName = username;
            wsClient.ClientCredentials.UserName.Password = password;
        }
        public certificateGenerationResponse submitRequest(String certRefNo, String formOID, byte[] hashValue, String hashAlgorithm)
        {
            certificateGenerationRequest requestMsg = new certificateGenerationRequest();
            requestMsg.certificateReferenceNumber = certRefNo;
            requestMsg.formOID = formOID;
            requestMsg.hashAlgorithm = hashAlgorithm;
            requestMsg.hashValue = Convert.ToBase64String(hashValue);

            using (new OperationContextScope(wsClient.InnerChannel))
            {
                // Add a HTTP Header to an outgoing request
                string auth = "Basic " + Convert.ToBase64String(System.Text.Encoding.Default.GetBytes(wsClient.ClientCredentials.UserName.UserName + ":" + wsClient.ClientCredentials.UserName.Password));
                Console.WriteLine(auth);
                HttpRequestMessageProperty requestMessage = new HttpRequestMessageProperty();
                requestMessage.Headers["Authorization"] = auth;
                OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = requestMessage;

                // Bypass Certificate (Temporary Coding)
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

                certificateGenerationResponse resp = wsClient.generationReqeust(requestMsg);

                return resp;
            }
        }
    }
}
