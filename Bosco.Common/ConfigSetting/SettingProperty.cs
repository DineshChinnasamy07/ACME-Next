/*  Class Name      : SettingProperty.cs
 *  Purpose         : Reusable member for ACPP BuildingBlocks
 *  Author          : Dinesh C
 *  Created on      : 16-Oct-2026
 */
namespace Bosco.Common.ConfigSetting
{
    public class SettingProperty : CommonMember
    {
        private static SettingProperty current = new SettingProperty();

        public static SettingProperty Current
        {
            get { return current; }
            set { current = value ?? new SettingProperty(); }
        }

        public string Currency { get; set; }
        public string CurrencyName { get; set; }
        public int AllowMultiCurrency { get; set; }
        public bool IS_SDBANG { get; set; }
        public string DecimalSeparator { get; set; }
        public string HeadOfficeCode { get; set; }
        public string BranchOfficeCode { get; set; }
        public string YearFrom { get; set; }
        public string YearTo { get; set; }
        public static bool Is_Application_Logout { get; set; }

        public SettingProperty()
        {
            CurrencyName = "Rupees";
            DecimalSeparator = ".";
        }
    }
}
