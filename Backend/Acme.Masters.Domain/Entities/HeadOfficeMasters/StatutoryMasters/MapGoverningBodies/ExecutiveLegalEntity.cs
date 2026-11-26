/*  Class Name      : ExecutiveLegalEntity.cs
 *  Purpose         : HO tenant EXECUTIVE_LEGAL_ENTITY row (Governing Body member mapped to a Legal Entity)
 *  Author          : Dinesh C
 *  Created on      : 24-Nov-2026
 */
namespace Acme.Masters.Domain.Entities.HeadOfficeMasters.StatutoryMasters.MapGoverningBodies
{
    public class ExecutiveLegalEntity
    {
        public int ExeLegalId { get; set; }

        public int CustomerId { get; set; }

        public int ExecutiveId { get; set; }
    }
}
