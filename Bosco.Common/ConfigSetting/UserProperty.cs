/*  Class Name      : UserProperty.cs
 *  Purpose         : Reusable member for ACPP BuildingBlocks
 *  Author          : Dinesh C
 *  Created on      : 16-Oct-2026
 */
using System.Data;

namespace Bosco.Common.ConfigSetting
{
    public class UserProperty : SettingProperty
    {
        private static DataView dvUser = null;

        public DataView UserInfo
        {
            get { return dvUser; }
            set { dvUser = value; }
        }

        public string LoginUserId
        {
            get { return GetUserInfo("USER_ID"); }
        }

        public string LoginUserName
        {
            get { return GetUserInfo("USER_NAME"); }
        }

        public string LoginUserFullName
        {
            get { return GetUserInfo("NAME"); }
        }

        public string LoginUserRole
        {
            get { return GetUserInfo("USERROLE"); }
        }

        public string LoginUserEmailId
        {
            get { return GetUserInfo("EMAIL_ID"); }
        }

        public string LoginUserMobileNo
        {
            get { return GetUserInfo("MOBILE_NO"); }
        }

        private string GetUserInfo(string name)
        {
            string val = "";
            if (dvUser != null && dvUser.Count > 0 && dvUser.Table.Columns.Contains(name))
            {
                val = dvUser[0][name].ToString();
            }
            return val;
        }
    }
}
