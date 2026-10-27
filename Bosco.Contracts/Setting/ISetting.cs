/*  Class Name      : ISetting.cs
 *  Purpose         : Reusable member for ACPP BuildingBlocks
 *  Author          : Dinesh C
 *  Created on      : 16-Oct-2026
 */
using System.Data;
using Bosco.Common;

namespace Bosco.Contracts.Setting
{
    public interface ISetting
    {
        void ApplySetting();
        ResultArgs SaveSetting(DataTable dtSetting);
        ResultArgs FetchSettingDetails(int userId);
    }
}
