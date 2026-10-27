/*  Class Name      : clsGeneral.cs
 *  Purpose         : Reusable member for ACPP BuildingBlocks
 *  Author          : Dinesh C
 *  Created on      : 16-Oct-2026
 */
using System.Globalization;

namespace Bosco.Common.Common
{
    public class clsGeneral
    {
        public static IFormatProvider DATE_FORMAT = new CultureInfo("en-GB", true);

        public static int USER_ID = 0;
        public static int USER_LEVEL = 0;
        public const int SUPERVISOR = 0;
        public const int OPERATOR = 1;
    }
}
