/*  Class Name      : ResultArgs.cs
 *  Purpose         : Reusable member for ACPP BuildingBlocks
 *  Author          : Dinesh C
 *  Created on      : 16-Oct-2026
 */
using System;
using System.Collections.Generic;
using System.Data;

namespace Bosco.Common
{
    public class ResultArgs : IDisposable
    {
        private ExceptionHandler exception = new ExceptionHandler();
        private bool success = false;
        private int rowsAffected = 0;
        private bool deadLock = false;
        private bool isShowExceptionMessage = true;
        private object rowUniqueId = "";
        private object returnValue = null;
        private Dictionary<string, object> rowUniqueIdCollection = null;
        private ResultSource resultSource = new ResultSource();

        public ResultArgs()
        {
        }

        public ResultArgs(bool isShowExceptionMessage)
        {
            this.isShowExceptionMessage = isShowExceptionMessage;
        }

        public bool IsDBException
        {
            get { return this.exception.IsDBError; }
            set { this.exception.IsDBError = value; }
        }

        public bool Success
        {
            get { return success; }
            set { success = value; }
        }

        public bool IsShowExceptionMessage
        {
            get { return isShowExceptionMessage; }
            set { isShowExceptionMessage = value; }
        }

        public int RowsAffected
        {
            get { return rowsAffected; }
            set
            {
                rowsAffected = value;
                success = (rowsAffected >= 0);
            }
        }

        public bool IsDeadLock
        {
            get { return deadLock; }
            set { deadLock = value; }
        }

        public object RowUniqueId
        {
            get { return rowUniqueId; }
            set
            {
                this.success = true;
                rowUniqueId = value;
            }
        }

        public Dictionary<string, object> RowUniqueIdCollection
        {
            get
            {
                if (rowUniqueIdCollection == null)
                {
                    rowUniqueIdCollection = new Dictionary<string, object>();
                }

                return rowUniqueIdCollection;
            }
            set
            {
                this.success = true;
                rowUniqueIdCollection = value;
            }
        }

        public object ReturnValue
        {
            get { return returnValue; }
            set { returnValue = value; }
        }

        public ResultSource DataSource
        {
            get { return resultSource; }
            set
            {
                this.success = true;
                resultSource.Data = value;
            }
        }

        public Exception Exception
        {
            get { return this.exception; }
            set
            {
                this.success = false;
                this.exception.Add(value);

                if (isShowExceptionMessage && value != null)
                {
                    if (value.Message.Contains("Unable to connect to any of the specified MySQL hosts"))
                    {
                        MessageRender.ShowMessage("Could not connect to Database Server. Contact Acme.erp Support Team", true);
                    }
                    else
                    {
                        MessageRender.ShowMessage(value.Message, true);
                    }
                }
            }
        }

        public string Message
        {
            get { return this.exception.Message; }
            set
            {
                this.success = false;
                this.exception.Add(value);
            }
        }

        public void ShowMessage()
        {
            MessageRender.ShowMessage(this);
        }

        public void ShowMessage(string message)
        {
            MessageRender.ShowMessage(message);
        }

        public void ShowMessage(string message, bool isError)
        {
            MessageRender.ShowMessage(message, isError);
        }

        public void ShowMessage(Exception e, bool isError)
        {
            this.Exception = e;
            MessageRender.ShowMessage(this, isError);
        }

        public class ResultSource : IDisposable
        {
            private object dataSource = null;
            private ScalarType scalarType = null;

            public ResultSource()
            {
                scalarType = new ScalarType();
            }

            public object Data
            {
                get { return dataSource; }
                set
                {
                    dataSource = value;
                    scalarType.SclarSource = dataSource;
                }
            }

            public DataSet TableSet
            {
                get { return dataSource as DataSet; }
            }

            public DataTable Table
            {
                get { return dataSource as DataTable; }
            }

            public DataView TableView
            {
                get { return dataSource as DataView; }
            }

            public ScalarType Sclar
            {
                get { return scalarType; }
            }

            public class ScalarType
            {
                private object dataSource = null;

                public object SclarSource
                {
                    set { dataSource = value; }
                }

                public new string ToString
                {
                    get
                    {
                        string sclarVal = "";
                        if (dataSource != null)
                        {
                            sclarVal = dataSource.ToString();
                        }
                        return sclarVal;
                    }
                }

                public int ToInteger
                {
                    get
                    {
                        int sclarVal = 0;
                        int.TryParse(this.ToString, out sclarVal);
                        return sclarVal;
                    }
                }
            }

            public void Dispose()
            {
                if (scalarType != null) { scalarType = null; }
                if (dataSource != null) { dataSource = null; }
            }
        }

        public void Dispose()
        {
            if (exception != null) { exception = null; }

            if (resultSource != null)
            {
                resultSource.Dispose();
                resultSource = null;
            }

            if (rowUniqueIdCollection != null)
            {
                rowUniqueIdCollection.Clear();
                rowUniqueIdCollection = null;
            }
        }
    }
}
