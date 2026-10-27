/*  Class Name      : AcMELog.cs
 *  Purpose         : Reusable member for ACPP BuildingBlocks
 *  Author          : Dinesh C
 *  Created on      : 16-Oct-2026
 */
using System;
using System.IO;
using System.Reflection;

namespace Bosco.Common
{
    public static class AcMELog
    {
        public static string ACPERP_GENERAL_LOG = Path.Combine(
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? AppDomain.CurrentDomain.BaseDirectory,
            "AcppGeneralLog.txt");

        private static Int32 ACPERP_GENERAL_LOG_RESET_FILE_SIZE = 2;

        public static void WriteLog(string msg)
        {
            string logpath = ACPERP_GENERAL_LOG;
            try
            {
                using (StreamWriter sw = new StreamWriter(logpath, true))
                {
                    if (!string.IsNullOrEmpty(msg))
                    {
                        if (msg.Replace("-", "").Length > 0)
                        {
                            msg = (DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss tt")) + " || " + msg;
                        }
                        sw.WriteLine(msg);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error in writing log " + ex.Message);
            }
        }

        public static void ClearLog()
        {
            try
            {
                string logpath = ACPERP_GENERAL_LOG;
                if (File.Exists(logpath))
                {
                    using (var file = new FileStream(logpath, FileMode.Truncate))
                    {
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error in clearing log " + ex.Message);
            }
        }

        public static void ClearLogbySize()
        {
            try
            {
                if (File.Exists(ACPERP_GENERAL_LOG))
                {
                    double len = new FileInfo(ACPERP_GENERAL_LOG).Length;
                    double size = Math.Round((double)len / (1024 * 1024));
                    if (size >= ACPERP_GENERAL_LOG_RESET_FILE_SIZE)
                    {
                        ClearLog();
                    }
                }
            }
            catch (Exception err)
            {
                MessageRender.ShowMessage("Could not clear log file," + err.Message);
            }
        }
    }

    public class TDSLog
    {
        private static string TDSLOGFILE = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TDSGeneralLog.txt");

        public static void WriteLog(string msg)
        {
            try
            {
                using (StreamWriter sw = new StreamWriter(TDSLOGFILE, true))
                {
                    if (!string.IsNullOrEmpty(msg) && msg.Replace("-", "").Length > 0)
                    {
                        msg = (DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss tt")) + " || " + msg;
                    }
                    sw.WriteLine(msg);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error in writing log " + ex.Message + Environment.NewLine + ex.Source);
            }
        }

        public static void ClearLog()
        {
            try
            {
                if (File.Exists(TDSLOGFILE))
                {
                    using (var file = new FileStream(TDSLOGFILE, FileMode.Truncate))
                    {
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error in writing log" + ex.Message + Environment.NewLine + ex.Source);
            }
        }
    }
}
