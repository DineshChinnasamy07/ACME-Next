namespace Acme.Organization.Application.Dtos
{
    public class BranchLocationListItemDto
    {
        public int LocationId { get; set; }
        public int BranchId { get; set; }
        public string Location { get; set; } = string.Empty;
        public string Branch { get; set; } = string.Empty;
        public bool IsPrimary { get; set; }
    }

    public class BranchLocationSaveRequest
    {
        public int LocationId { get; set; }
        public int BranchId { get; set; }
        public string? LocationName { get; set; }
    }

    public class BranchLocationActionResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int LocationId { get; set; }
    }

    public class BranchOptionDto
    {
        public int BranchOfficeId { get; set; }
        public string BranchOfficeCode { get; set; } = string.Empty;
        public string BranchOfficeName { get; set; } = string.Empty;
    }
}