using System.Net.Http.Headers;
using System.Text;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using TunisianEInvoice.Application.Interfaces;

namespace TunisianEInvoice.Infrastructure.ExternalServices
{
    /// <summary>
    /// Tunisie TradeNet (TTN) WebService client for El Fatoora invoice submission.
    /// Implements SOAP-based communication with TTN platform.
    /// </summary>
    public class TtnService : ITtnService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<TtnService> _logger;

        // TTN WebService endpoints
        private const string TestBaseUrl = "https://ws-test.elfatoora.tn/";
        private const string ProdBaseUrl = "https://ws.elfatoora.tn/";
        private const string SubmitEndpoint = "InvoiceSubmission";
        private const string StatusEndpoint = "InvoiceStatus";
        private const string RetrieveEndpoint = "InvoiceRetrieval";

        public TtnService(HttpClient httpClient, ILogger<TtnService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<TtnSubmissionResult> SubmitInvoiceAsync(string signedXml, TtnClientConfig clientConfig, CancellationToken cancellationToken = default)
        {
            var result = new TtnSubmissionResult
            {
                SubmissionTime = DateTime.UtcNow
            };

            try
            {
                var baseUrl = clientConfig.AccountMode == "PROD" ? ProdBaseUrl : TestBaseUrl;
                var requestUrl = $"{baseUrl}{SubmitEndpoint}";

                // Build SOAP envelope
                var soapEnvelope = BuildSubmissionSoapEnvelope(signedXml, clientConfig);

                var requestMessage = new HttpRequestMessage(HttpMethod.Post, requestUrl)
                {
                    Content = new StringContent(soapEnvelope, Encoding.UTF8, "text/xml")
                };

                // Add authentication header
                var authValue = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientConfig.Username}:{clientConfig.Password}"));
                requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Basic", authValue);
                requestMessage.Headers.Add("SOAPAction", "submitInvoice");

                _logger.LogInformation("Submitting invoice to TTN: {Url}", requestUrl);

                var response = await _httpClient.SendAsync(requestMessage, cancellationToken);
                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    var parsedResult = ParseSubmissionResponse(responseContent);
                    result.Success = parsedResult.Success;
                    result.TtnReference = parsedResult.TtnReference;
                    result.QrCodeData = parsedResult.QrCodeData;
                    result.ErrorCode = parsedResult.ErrorCode;
                    result.ErrorMessage = parsedResult.ErrorMessage;

                    _logger.LogInformation("Invoice submitted successfully. TTN Reference: {Reference}", result.TtnReference);
                }
                else
                {
                    result.Success = false;
                    result.ErrorCode = response.StatusCode.ToString();
                    result.ErrorMessage = $"HTTP Error: {response.StatusCode} - {responseContent}";
                    
                    _logger.LogError("TTN submission failed: {StatusCode} - {Content}", response.StatusCode, responseContent);
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorCode = "EXCEPTION";
                result.ErrorMessage = ex.Message;
                
                _logger.LogError(ex, "Exception during TTN invoice submission");
            }

            return result;
        }

        public async Task<TtnStatusResult> GetInvoiceStatusAsync(string ttnReference, TtnClientConfig clientConfig, CancellationToken cancellationToken = default)
        {
            var result = new TtnStatusResult
            {
                TtnReference = ttnReference,
                Status = TtnInvoiceStatus.Unknown
            };

            try
            {
                var baseUrl = clientConfig.AccountMode == "PROD" ? ProdBaseUrl : TestBaseUrl;
                var requestUrl = $"{baseUrl}{StatusEndpoint}";

                var soapEnvelope = BuildStatusSoapEnvelope(ttnReference, clientConfig);

                var requestMessage = new HttpRequestMessage(HttpMethod.Post, requestUrl)
                {
                    Content = new StringContent(soapEnvelope, Encoding.UTF8, "text/xml")
                };

                var authValue = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientConfig.Username}:{clientConfig.Password}"));
                requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Basic", authValue);
                requestMessage.Headers.Add("SOAPAction", "getInvoiceStatus");

                var response = await _httpClient.SendAsync(requestMessage, cancellationToken);
                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    var parsedResult = ParseStatusResponse(responseContent);
                    result.Status = parsedResult.Status;
                    result.StatusMessage = parsedResult.StatusMessage;
                    result.ValidationDate = parsedResult.ValidationDate;
                    result.ErrorCode = parsedResult.ErrorCode;
                    result.ErrorDetails = parsedResult.ErrorDetails;
                }
                else
                {
                    result.ErrorCode = response.StatusCode.ToString();
                    result.ErrorDetails = responseContent;
                }
            }
            catch (Exception ex)
            {
                result.ErrorCode = "EXCEPTION";
                result.ErrorDetails = ex.Message;
                _logger.LogError(ex, "Exception during TTN status check for reference: {Reference}", ttnReference);
            }

            return result;
        }

        public async Task<string?> GetValidatedInvoiceAsync(string ttnReference, TtnClientConfig clientConfig, CancellationToken cancellationToken = default)
        {
            try
            {
                var baseUrl = clientConfig.AccountMode == "PROD" ? ProdBaseUrl : TestBaseUrl;
                var requestUrl = $"{baseUrl}{RetrieveEndpoint}";

                var soapEnvelope = BuildRetrievalSoapEnvelope(ttnReference, clientConfig);

                var requestMessage = new HttpRequestMessage(HttpMethod.Post, requestUrl)
                {
                    Content = new StringContent(soapEnvelope, Encoding.UTF8, "text/xml")
                };

                var authValue = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientConfig.Username}:{clientConfig.Password}"));
                requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Basic", authValue);
                requestMessage.Headers.Add("SOAPAction", "retrieveInvoice");

                var response = await _httpClient.SendAsync(requestMessage, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
                    return ExtractInvoiceFromResponse(responseContent);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception during TTN invoice retrieval for reference: {Reference}", ttnReference);
            }

            return null;
        }

        public async Task<bool> TestConnectionAsync(TtnClientConfig clientConfig, CancellationToken cancellationToken = default)
        {
            try
            {
                var baseUrl = clientConfig.AccountMode == "PROD" ? ProdBaseUrl : TestBaseUrl;
                
                var response = await _httpClient.GetAsync(baseUrl, cancellationToken);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "TTN connection test failed");
                return false;
            }
        }

        #region SOAP Envelope Builders

        private string BuildSubmissionSoapEnvelope(string signedXml, TtnClientConfig config)
        {
            // Encode XML content for SOAP transport
            var encodedXml = System.Security.SecurityElement.Escape(signedXml);
            
            return $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"" 
               xmlns:ttn=""http://ws.elfatoora.tn/"">
    <soap:Header>
        <ttn:AuthHeader>
            <ttn:ClientCode>{config.ClientCode}</ttn:ClientCode>
            <ttn:AccountMode>{config.AccountMode}</ttn:AccountMode>
            <ttn:AccountRank>{config.AccountRank}</ttn:AccountRank>
            <ttn:Profile>{config.Profile}</ttn:Profile>
        </ttn:AuthHeader>
    </soap:Header>
    <soap:Body>
        <ttn:submitInvoice>
            <ttn:invoiceXml><![CDATA[{signedXml}]]></ttn:invoiceXml>
        </ttn:submitInvoice>
    </soap:Body>
</soap:Envelope>";
        }

        private string BuildStatusSoapEnvelope(string ttnReference, TtnClientConfig config)
        {
            return $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"" 
               xmlns:ttn=""http://ws.elfatoora.tn/"">
    <soap:Header>
        <ttn:AuthHeader>
            <ttn:ClientCode>{config.ClientCode}</ttn:ClientCode>
            <ttn:AccountMode>{config.AccountMode}</ttn:AccountMode>
        </ttn:AuthHeader>
    </soap:Header>
    <soap:Body>
        <ttn:getInvoiceStatus>
            <ttn:ttnReference>{ttnReference}</ttn:ttnReference>
        </ttn:getInvoiceStatus>
    </soap:Body>
</soap:Envelope>";
        }

        private string BuildRetrievalSoapEnvelope(string ttnReference, TtnClientConfig config)
        {
            return $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"" 
               xmlns:ttn=""http://ws.elfatoora.tn/"">
    <soap:Header>
        <ttn:AuthHeader>
            <ttn:ClientCode>{config.ClientCode}</ttn:ClientCode>
            <ttn:AccountMode>{config.AccountMode}</ttn:AccountMode>
        </ttn:AuthHeader>
    </soap:Header>
    <soap:Body>
        <ttn:retrieveInvoice>
            <ttn:ttnReference>{ttnReference}</ttn:ttnReference>
        </ttn:retrieveInvoice>
    </soap:Body>
</soap:Envelope>";
        }

        #endregion

        #region Response Parsers

        private TtnSubmissionResult ParseSubmissionResponse(string responseXml)
        {
            var result = new TtnSubmissionResult();

            try
            {
                var doc = XDocument.Parse(responseXml);
                XNamespace soapNs = "http://schemas.xmlsoap.org/soap/envelope/";
                XNamespace ttnNs = "http://ws.elfatoora.tn/";

                var body = doc.Descendants(soapNs + "Body").FirstOrDefault();
                var responseElement = body?.Descendants().FirstOrDefault();

                if (responseElement != null)
                {
                    var statusCode = responseElement.Element(ttnNs + "statusCode")?.Value;
                    result.Success = statusCode == "0" || statusCode == "SUCCESS";
                    result.TtnReference = responseElement.Element(ttnNs + "ttnReference")?.Value;
                    result.QrCodeData = responseElement.Element(ttnNs + "qrCode")?.Value;
                    result.ErrorCode = statusCode;
                    result.ErrorMessage = responseElement.Element(ttnNs + "statusMessage")?.Value;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing TTN submission response");
                result.Success = false;
                result.ErrorMessage = "Failed to parse response";
            }

            return result;
        }

        private TtnStatusResult ParseStatusResponse(string responseXml)
        {
            var result = new TtnStatusResult();

            try
            {
                var doc = XDocument.Parse(responseXml);
                XNamespace soapNs = "http://schemas.xmlsoap.org/soap/envelope/";
                XNamespace ttnNs = "http://ws.elfatoora.tn/";

                var body = doc.Descendants(soapNs + "Body").FirstOrDefault();
                var responseElement = body?.Descendants().FirstOrDefault();

                if (responseElement != null)
                {
                    var statusValue = responseElement.Element(ttnNs + "invoiceStatus")?.Value;
                    result.Status = ParseInvoiceStatus(statusValue);
                    result.StatusMessage = responseElement.Element(ttnNs + "statusMessage")?.Value;
                    
                    var validationDateStr = responseElement.Element(ttnNs + "validationDate")?.Value;
                    if (DateTime.TryParse(validationDateStr, out var validationDate))
                    {
                        result.ValidationDate = validationDate;
                    }

                    result.ErrorCode = responseElement.Element(ttnNs + "errorCode")?.Value;
                    result.ErrorDetails = responseElement.Element(ttnNs + "errorDetails")?.Value;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing TTN status response");
            }

            return result;
        }

        private TtnInvoiceStatus ParseInvoiceStatus(string? statusValue)
        {
            return statusValue?.ToUpperInvariant() switch
            {
                "PENDING" => TtnInvoiceStatus.Pending,
                "PROCESSING" => TtnInvoiceStatus.Processing,
                "VALIDATED" => TtnInvoiceStatus.Validated,
                "REJECTED" => TtnInvoiceStatus.Rejected,
                "CANCELLED" => TtnInvoiceStatus.Cancelled,
                _ => TtnInvoiceStatus.Unknown
            };
        }

        private string? ExtractInvoiceFromResponse(string responseXml)
        {
            try
            {
                var doc = XDocument.Parse(responseXml);
                XNamespace soapNs = "http://schemas.xmlsoap.org/soap/envelope/";
                XNamespace ttnNs = "http://ws.elfatoora.tn/";

                var invoiceElement = doc.Descendants(ttnNs + "invoiceXml").FirstOrDefault();
                return invoiceElement?.Value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting invoice from TTN response");
                return null;
            }
        }

        #endregion
    }
}
