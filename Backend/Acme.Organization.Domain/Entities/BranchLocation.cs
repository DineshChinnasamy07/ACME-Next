namespace Acme.Organization.Domain.Entities
{
    public class BranchLocation
    {
        public int LocationId { get; set; }

        public int BranchId { get; set; }

        public string LocationName { get; set; } = string.Empty;
    }
}
