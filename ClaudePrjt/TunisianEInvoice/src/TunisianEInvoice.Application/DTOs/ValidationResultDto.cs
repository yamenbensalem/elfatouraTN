using System.Collections.Generic;

namespace TunisianEInvoice.Application.DTOs
{
    public class ValidationResultDto
    {
        public bool IsValid { get; set; }
        public List<ValidationError> Errors { get; set; } = new();
    }
}
