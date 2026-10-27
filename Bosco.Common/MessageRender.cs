/*  Class Name      : MessageRender.cs
 *  Purpose         : Reusable member for ACPP BuildingBlocks
 *  Author          : Dinesh C
 *  Created on      : 16-Oct-2026
 */
using System;

namespace Bosco.Common
{
    public class MessageEventArgs : EventArgs
    {
        public string Message { get; set; }
        public bool IsError { get; set; }
        public ResultArgs Result { get; set; }
    }

    public class MessageRender
    {
        private static ResultArgs returnResult;

        public static event EventHandler<MessageEventArgs> MessageRaised;

        public static ResultArgs Result
        {
            get { return returnResult; }
            set { returnResult = value; }
        }

        public static string GetMessage(ResultArgs resultArgs)
        {
            string msg = "";

            if (resultArgs != null && !resultArgs.Success)
            {
                ExceptionHandler e = resultArgs.Exception as ExceptionHandler;
                if (e != null)
                {
                    msg = e.ErrorMessage;
                    if (e.IsError)
                    {
                        msg += "\r\n\r\nSource::" + e.ErrorSource;
                    }
                }
                else if (!string.IsNullOrEmpty(resultArgs.Message))
                {
                    msg = resultArgs.Message;
                }
            }

            return msg;
        }

        public static string GetMessage(string keyCode)
        {
            return keyCode;
        }

        public static string GetReportMessage(string keyCode)
        {
            return keyCode;
        }

        public static void ShowMessage(ResultArgs resultArgs)
        {
            ShowMessage(resultArgs, false);
        }

        public static void ShowMessage(ResultArgs resultArgs, bool isError)
        {
            returnResult = resultArgs;
            ExceptionHandler handler = returnResult.Exception as ExceptionHandler;
            if (handler != null)
            {
                handler.IsError = isError;
            }
            RaiseMessage(GetMessage(resultArgs), isError, resultArgs);
        }

        public static void ShowMessage(Exception e)
        {
            returnResult = new ResultArgs();
            returnResult.Exception = e;
            RaiseMessage(e != null ? e.Message : "", true, returnResult);
        }

        public static void ShowMessage(string message)
        {
            ShowMessage(message, false);
        }

        public static void ShowMessage(string message, bool isError)
        {
            returnResult = new ResultArgs();
            returnResult.Message = message;
            ExceptionHandler handler = returnResult.Exception as ExceptionHandler;
            if (handler != null)
            {
                handler.IsError = isError;
            }
            RaiseMessage(message, isError, returnResult);
        }

        public static bool ShowConfirmationMessage(string message)
        {
            MessageEventArgs args = new MessageEventArgs
            {
                Message = message,
                IsError = false
            };
            if (MessageRaised != null)
            {
                MessageRaised(null, args);
            }
            return true;
        }

        private static void RaiseMessage(string message, bool isError, ResultArgs resultArgs)
        {
            if (string.IsNullOrEmpty(message))
            {
                return;
            }

            AcMELog.WriteLog((isError ? "ERROR: " : "INFO: ") + message);

            if (MessageRaised != null)
            {
                MessageRaised(null, new MessageEventArgs
                {
                    Message = message,
                    IsError = isError,
                    Result = resultArgs
                });
            }
        }
    }
}
