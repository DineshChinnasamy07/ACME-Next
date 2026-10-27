/*  Class Name      : ExceptionHandler.cs
 *  Purpose         : Reusable member for ACPP BuildingBlocks
 *  Author          : Dinesh C
 *  Created on      : 16-Oct-2026
 */
using System;
using System.Text.RegularExpressions;

namespace Bosco.Common
{
    public class ExceptionHandler : Exception
    {
        private string message = "";
        private bool isError = false;
        private bool isDBError = false;
        private string errorSource = "";
        private Exception exception = new Exception();
        private static ExceptionHandler exceptionLog = null;

        public static ExceptionHandler ExceptionLog
        {
            set { exceptionLog = value; }
        }

        public virtual void WirteLog(ExceptionHandler exceptionHandler)
        {
        }

        public string TargetMethod
        {
            get { return this.GetExceptionItem("METHOD"); }
        }

        public string TargetSource
        {
            get { return this.GetExceptionItem("SOURCE"); }
        }

        public string TargetAssembly
        {
            get { return this.GetExceptionItem("ASSEMBLY"); }
        }

        public override string Message
        {
            get { return this.ErrorMessage; }
        }

        public string ErrorMessage
        {
            get { return this.message; }
            set { this.message = value; }
        }

        public bool IsError
        {
            get { return this.isError; }
            set { this.isError = value; }
        }

        public bool IsDBError
        {
            get { return this.isDBError; }
            set { this.isDBError = value; }
        }

        public string ErrorSource
        {
            get { return this.errorSource; }
            set { this.errorSource = value; }
        }

        public Exception Exception
        {
            get { return exception; }
            set { exception = value; this.isError = true; }
        }

        public ExceptionHandler()
        {
        }

        public ExceptionHandler(Exception e, bool showMessage)
            : this(e, "", true, showMessage) { }

        public ExceptionHandler(string message, bool showMessage)
            : this(null, message, false, showMessage) { }

        public ExceptionHandler(string message, bool isError, bool showMessage)
            : this(null, message, isError, showMessage) { }

        public ExceptionHandler(Exception e, string message, bool isError, bool showMessage)
        {
            HandleException(e, message, isError, showMessage);
        }

        public void Add(Exception e)
        {
            HandleException(e, "", true, false);
        }

        public void Add(Exception e, string message)
        {
            HandleException(e, message, false, true);
        }

        public void Add(Exception e, string message, bool isError)
        {
            HandleException(e, message, isError, false);
        }

        public void Add(string message)
        {
            HandleException(null, message, false, false);
        }

        public void Add(string message, bool isError)
        {
            HandleException(null, message, isError, false);
        }

        public void Add(string message, bool isError, bool showMessage)
        {
            HandleException(null, message, isError, showMessage);
        }

        private string GetExceptionItem(string type)
        {
            string value = "";

            if (exception != null && exception.TargetSite != null)
            {
                if (type == "METHOD")
                {
                    value = exception.TargetSite.ToString();
                }
                else if (type == "SOURCE")
                {
                    value = exception.StackTrace;
                }
                else if (type == "ASSEMBLY" && exception.TargetSite.Module != null)
                {
                    value = exception.TargetSite.Module.Name;
                }
            }

            return value;
        }

        private void HandleException(Exception e, string message, bool isError, bool showMessage)
        {
            if (e != null)
            {
                this.exception = e;
                this.message = e.Message;
                this.ErrorSource = e.Source + "\r\n\r\n" + e.StackTrace;
            }

            this.isError = isError;
            if (message != "") { this.message = message; }
            if (exceptionLog != null) { exceptionLog.WirteLog(this); }

            this.IsDBError = IsDBStructureException(this.message);

            if (showMessage)
            {
                MessageRender.ShowMessage(this.message, isError);
            }
        }

        private bool IsDBStructureException(string execptionmessage)
        {
            if (string.IsNullOrEmpty(execptionmessage))
            {
                return false;
            }

            Match matchtblexists = Regex.Match(execptionmessage, @"Table '[^']*' doesn't exist$", RegexOptions.IgnoreCase);
            if (!matchtblexists.Success)
            {
                matchtblexists = Regex.Match(execptionmessage, @"Unknown column '[^']*' in 'field list'$", RegexOptions.IgnoreCase);
            }

            return matchtblexists.Success;
        }
    }
}
