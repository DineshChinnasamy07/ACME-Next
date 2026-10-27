/*  Class Name      : ExceptionLogger.cs
 *  Purpose         : Reusable member for ACPP BuildingBlocks
 *  Author          : Dinesh C
 *  Created on      : 16-Oct-2026
 */
namespace Bosco.Common
{
    public class ExceptionLogger : ExceptionHandler
    {
        public ExceptionLogger()
        {
            ExceptionHandler.ExceptionLog = this;
        }

        public override void WirteLog(ExceptionHandler exceptionHandler)
        {
            if (exceptionHandler != null && !string.IsNullOrEmpty(exceptionHandler.ErrorMessage))
            {
                AcMELog.WriteLog(exceptionHandler.ErrorMessage);
            }
        }
    }
}
