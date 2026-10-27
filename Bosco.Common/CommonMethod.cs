/*  Class Name      : CommonMethod.cs
 *  Purpose         : Reusable member for ACPP BuildingBlocks
 *  Author          : Dinesh C
 *  Created on      : 16-Oct-2026
 */
using System;
using System.Data;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Bosco.Common
{
    public class CommonMethod : IDisposable
    {
        public static DataTable dtApplyUserRights = new DataTable();
        public static DataTable dtUseRights = new DataTable();
        public static int Flag = 0;
        public static double SumofTaxAmount = 0;
        public static double NetTdsAmount = 0;
        public static string MultiDataBaseName = string.Empty;
        public static string HeadOfficeCode = string.Empty;
        public static string BranchOfficeCode = string.Empty;
        public static string RegistryPath = @"Software\BoscoSoft\AcMEERP";
        public static double CalCuTaxRate = 0;
        public static double CalCuTaxAmount = 0;
        public static double NetAmout = 0;
        public static double CalCuDiscountAmount = 0;
        public static double CalCuDiscountPer = 0;

        public DataTable GetCurrencySymbolList()
        {
            DataTable dtCurrencySymbol = new DataTable();
            dtCurrencySymbol.Columns.Add(new DataColumn("Currency Symbol", typeof(string)));
            dtCurrencySymbol.Columns.Add(new DataColumn("Currency Code", typeof(string)));
            dtCurrencySymbol.Columns.Add(new DataColumn("Country Code", typeof(string)));
            dtCurrencySymbol.Columns.Add(new DataColumn("Country", typeof(string)));
            dtCurrencySymbol.Columns.Add(new DataColumn("Currency Name", typeof(string)));
            try
            {
                CultureInfo[] cultures = CultureInfo.GetCultures(CultureTypes.SpecificCultures);
                foreach (CultureInfo culture in cultures)
                {
                    RegionInfo regionInfo = new RegionInfo(culture.Name);
                    dtCurrencySymbol.Rows.Add(regionInfo.CurrencySymbol, regionInfo.ISOCurrencySymbol, regionInfo.ThreeLetterISORegionName, regionInfo.DisplayName, regionInfo.CurrencyEnglishName);
                }
            }
            catch (Exception ex)
            {
                MessageRender.ShowMessage(ex.ToString(), true);
            }
            return dtCurrencySymbol;
        }

        public static string GetPredictedCode(string parCode, DataTable dtCode)
        {
            parCode = parCode != string.Empty ? parCode : "000";
            string finalCode = "";
            string tempstr = "";
            try
            {
                string prefixCode = Regex.Match(parCode, @"^[A-Z|a-z]+").Value;
                string digitCode = Regex.Match(parCode, @"\d+").Value;
                string suffixCode = Regex.Match(parCode, @"[A-Z|a-z]+$").Value;
                int tempCode = Convert.ToInt32(digitCode) + 1;
                if (prefixCode.Length != parCode.Length)
                {
                    if (ZeroCount(digitCode) != 0)
                        finalCode = prefixCode + tempCode.ToString(tempstr = AddZero(digitCode.Length)) + suffixCode;
                    else
                        finalCode = prefixCode + tempCode.ToString() + suffixCode;
                }
                if (dtCode != null)
                {
                    for (int i = 0; i < dtCode.Rows.Count; i++)
                    {
                        if (finalCode.Equals(dtCode.Rows[i][0].ToString()))
                        {
                            finalCode = GetPredictedCode(finalCode, dtCode);
                            i = 0;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string exception = ex.ToString();
            }

            return finalCode;
        }

        public static string EscapeLikeValue(string value)
        {
            string rtn = value;
            string pattern = @"([-\]\[<>\?\*\\\""/\|\~\(\)\#/=><+\%&\^\'])";
            Regex expression = new Regex(pattern);

            if (!string.IsNullOrEmpty(value) && expression.IsMatch(value))
            {
                StringBuilder sb = new StringBuilder(value.Length);
                for (int i = 0; i < value.Length; i++)
                {
                    char c = value[i];
                    switch (c)
                    {
                        case ']':
                        case '[':
                        case '%':
                        case '*':
                            sb.Append("[").Append(c).Append("]");
                            break;
                        case '\'':
                            sb.Append("''");
                            break;
                        default:
                            sb.Append(c);
                            break;
                    }
                }
                rtn = sb.ToString();
            }
            return rtn;
        }

        public DataTable AddHeaderColumn(DataTable dtSource, string ValueField, string DisplayField, string HeaderCaption = "<--All-->")
        {
            if (dtSource != null && dtSource.Columns.Contains(ValueField) && dtSource.Columns.Contains(DisplayField))
            {
                DataRow dr = dtSource.NewRow();
                dr[ValueField] = 0;
                dr[DisplayField] = HeaderCaption;
                dtSource.Rows.InsertAt(dr, 0);
            }
            return dtSource;
        }

        private static int ZeroCount(string digit)
        {
            Regex reg = new Regex(@"0+");
            Match mat = reg.Match(digit ?? string.Empty);
            return mat.Value.Length;
        }

        private static string AddZero(int length)
        {
            string tempstr = "";
            for (int i = 1; i <= length; i++)
                tempstr = tempstr + "0";
            return tempstr;
        }

        public void Dispose()
        {
        }
    }
}
