using TunisianEInvoice.Domain.Entities;

namespace TunisianEInvoice.Application.Interfaces
{
    /// <summary>
    /// Interface for Tunisie TradeNet (TTN) WebService integration.
    /// Handles invoice submission, validation, and status retrieval.
    /// </summary>
    public interface ITtnService
    {
        /// <summary>
        /// Submits a signed invoice XML to TTN for validation
        /// </summary>
        /// <param name="signedXml">The XAdES-BES signed invoice XML</param>
        /// <param name="clientConfig">TTN client configuration</param>
        /// <returns>TTN validation result with reference number</returns>
        Task<TtnSubmissionResult> SubmitInvoiceAsync(string signedXml, TtnClientConfig clientConfig, CancellationToken cancellationToken = default);

        /// <summary>
        /// Checks the status of a previously submitted invoice
        /// </summary>
        /// <param name="ttnReference">The TTN reference number</param>
        /// <param name="clientConfig">TTN client configuration</param>
        /// <returns>Invoice status from TTN</returns>
        Task<TtnStatusResult> GetInvoiceStatusAsync(string ttnReference, TtnClientConfig clientConfig, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves the validated invoice XML with TTN stamp
        /// </summary>
        /// <param name="ttnReference">The TTN reference number</param>
        /// <param name="clientConfig">TTN client configuration</param>
        /// <returns>Validated XML with TTN stamp</returns>
        Task<string?> GetValidatedInvoiceAsync(string ttnReference, TtnClientConfig clientConfig, CancellationToken cancellationToken = default);

        /// <summary>
        /// Tests connection to TTN WebService
        /// </summary>
        Task<bool> TestConnectionAsync(TtnClientConfig clientConfig, CancellationToken cancellationToken = default);
    }

    public class TtnClientConfig
    {
        public string AccountMode { get; set; } = "TEST"; // TEST or PROD
        public string AccountRank { get; set; } = "1";
        public string Profile { get; set; } = "STANDARD";
        public string ClientCode { get; set; } = "";
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
        public string BaseUrl { get; set; } = "https://ws.elfatoora.tn/";
    }

    public class TtnSubmissionResult
    {
        public bool Success { get; set; }
        public string? TtnReference { get; set; }
        public string? ErrorCode { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime SubmissionTime { get; set; }
        public string? QrCodeData { get; set; }
    }

    public class TtnStatusResult
    {
        public string TtnReference { get; set; } = "";
        public TtnInvoiceStatus Status { get; set; }
        public string? StatusMessage { get; set; }
        public DateTime? ValidationDate { get; set; }
        public string? ErrorCode { get; set; }
        public string? ErrorDetails { get; set; }
    }

    public enum TtnInvoiceStatus
    {
        Pending,
        Processing,
        Validated,
        Rejected,
        Cancelled,
        Unknown
    }
}
