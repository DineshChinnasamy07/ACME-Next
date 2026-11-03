/*  Class Name      : RightsActivity.cs
 *  Purpose         : rights_activities catalog row
 *  Author          : Dinesh C
 *  Created on      : 27-Oct-2026
 */
namespace Bosco.Auth.Domain.Entities
{
    public class RightsActivity
    {
        public int ActivityId { get; set; }

        public string ModuleCode { get; set; } = string.Empty;

        public string Module { get; set; } = string.Empty;

        public string ActivityCode { get; set; } = string.Empty;

        public string Activity { get; set; } = string.Empty;

        public int ModuleOrder { get; set; }

        public int ActivityOrder { get; set; }

        public int Type { get; set; }

        public int Accessibility { get; set; }
    }
}
