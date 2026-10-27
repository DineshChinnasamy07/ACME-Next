/*  Class Name      : PostgreSqlDatabase.cs
 *  Purpose         : PostgreSQL implementation of IDatabase for all ACPP services
 *  Author          : Dinesh C
 *  Created on      : 16-Oct-2026
 */
using System.Data;
using Bosco.Common;
using Bosco.Contracts;
using Bosco.Contracts.Data;
using Npgsql;

namespace Bosco.Data
{
    public class PostgreSqlDatabase : IDatabase
    {
        private readonly string connectionString;
        private readonly IDatabaseQuery databaseQuery;
        private NpgsqlConnection connection;
        private NpgsqlTransaction transaction;
        private string previousTransactionSource = "";
        private ExecutionMode transExecutionMode = ExecutionMode.None;
        private bool disposed;

        public PostgreSqlDatabase(string connectionString)
            : this(connectionString, null)
        {
        }

        public PostgreSqlDatabase(string connectionString, IDatabaseQuery databaseQuery)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentException("PostgreSQL connection string is required.", "connectionString");
            }

            this.connectionString = connectionString;
            this.databaseQuery = databaseQuery;
        }

        public string PreviousTransactionSource
        {
            get { return previousTransactionSource; }
        }

        public ExecutionMode TransExecutionMode
        {
            set { transExecutionMode = value; }
        }

        public ResultArgs HasConnectionEstablished()
        {
            ResultArgs resultArgs = new ResultArgs(false);

            try
            {
                EnsureOpen();
                using (NpgsqlCommand command = CreateCommand("SELECT 1", SQLType.SQLStatic))
                {
                    object pingValue = command.ExecuteScalar();
                    resultArgs.Success = (pingValue != null && pingValue.ToString() == "1");
                    resultArgs.ReturnValue = pingValue;
                }
            }
            catch (Exception ex)
            {
                resultArgs.Exception = ex;
            }

            return resultArgs;
        }

        public ResultArgs Execute(DataCommandArguments command, SQLType sqlType, bool showMessage = false)
        {
            SQLType resolvedType = sqlType;
            string sqlStatement = ResolveSql(command, ref resolvedType);
            return Execute(command, sqlStatement, resolvedType, showMessage);
        }

        public ResultArgs Execute(DataCommandArguments command, string sqlStatement, SQLType sqlType, bool showMessage = false)
        {
            ResultArgs resultArgs = new ResultArgs(showMessage);

            if (string.IsNullOrWhiteSpace(sqlStatement))
            {
                resultArgs.Message = "SQL statement is empty.";
                return resultArgs;
            }

            try
            {
                EnsureOpen();
                previousTransactionSource = sqlStatement;

                using (NpgsqlCommand dbCommand = CreateCommand(sqlStatement, sqlType))
                {
                    resultArgs.RowsAffected = dbCommand.ExecuteNonQuery();
                    resultArgs.Success = true;
                }
            }
            catch (Exception ex)
            {
                resultArgs.Exception = ex;
            }

            return resultArgs;
        }

        public ResultArgs Fetch(DataCommandArguments command, DataSource dataSourceType, string sqlStatement, SQLType sqlType, ref object dataSource)
        {
            ResultArgs resultArgs = new ResultArgs(false);

            if (string.IsNullOrWhiteSpace(sqlStatement))
            {
                resultArgs.Message = "SQL statement is empty.";
                return resultArgs;
            }

            try
            {
                EnsureOpen();
                previousTransactionSource = sqlStatement;

                using (NpgsqlCommand dbCommand = CreateCommand(sqlStatement, sqlType))
                {
                    switch (dataSourceType)
                    {
                        case DataSource.DataSet:
                            DataSet dataSet = new DataSet();
                            using (NpgsqlDataAdapter adapter = new NpgsqlDataAdapter(dbCommand))
                            {
                                adapter.Fill(dataSet);
                            }
                            dataSource = dataSet;
                            resultArgs.DataSource.Data = dataSet;
                            break;

                        case DataSource.DataTable:
                            DataTable dataTable = new DataTable();
                            using (NpgsqlDataAdapter tableAdapter = new NpgsqlDataAdapter(dbCommand))
                            {
                                tableAdapter.Fill(dataTable);
                            }
                            dataSource = dataTable;
                            resultArgs.DataSource.Data = dataTable;
                            break;

                        case DataSource.DataView:
                            DataTable viewTable = new DataTable();
                            using (NpgsqlDataAdapter viewAdapter = new NpgsqlDataAdapter(dbCommand))
                            {
                                viewAdapter.Fill(viewTable);
                            }
                            dataSource = viewTable.DefaultView;
                            resultArgs.DataSource.Data = viewTable.DefaultView;
                            break;

                        case DataSource.DataReader:
                            dataSource = dbCommand.ExecuteReader(CommandBehavior.Default);
                            resultArgs.DataSource.Data = dataSource;
                            break;

                        case DataSource.Scalar:
                            dataSource = dbCommand.ExecuteScalar();
                            resultArgs.DataSource.Data = dataSource;
                            resultArgs.ReturnValue = dataSource;
                            break;

                        default:
                            resultArgs.Message = "Unsupported data source type.";
                            return resultArgs;
                    }

                    resultArgs.Success = true;
                }
            }
            catch (Exception ex)
            {
                resultArgs.Exception = ex;
            }

            return resultArgs;
        }

        public void BeginTransaction()
        {
            EnsureOpen();
            if (transaction == null)
            {
                transaction = connection.BeginTransaction();
            }
        }

        public void EndTransaction()
        {
            if (transaction == null)
            {
                return;
            }

            if (transExecutionMode == ExecutionMode.Fail)
            {
                transaction.Rollback();
            }
            else
            {
                transaction.Commit();
            }

            transaction.Dispose();
            transaction = null;
            transExecutionMode = ExecutionMode.None;
        }

        public void RollBackTransaction()
        {
            if (transaction == null)
            {
                return;
            }

            transaction.Rollback();
            transaction.Dispose();
            transaction = null;
            transExecutionMode = ExecutionMode.None;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }

            if (disposing)
            {
                if (transaction != null)
                {
                    transaction.Dispose();
                    transaction = null;
                }

                if (connection != null)
                {
                    connection.Dispose();
                    connection = null;
                }
            }

            disposed = true;
        }

        private void EnsureOpen()
        {
            if (connection == null)
            {
                connection = new NpgsqlConnection(connectionString);
            }

            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
            }
        }

        private NpgsqlCommand CreateCommand(string sqlStatement, SQLType sqlType)
        {
            NpgsqlCommand dbCommand = connection.CreateCommand();
            dbCommand.CommandText = sqlStatement;
            dbCommand.CommandType = (sqlType == SQLType.SQLStoredProcedure)
                ? CommandType.StoredProcedure
                : CommandType.Text;
            dbCommand.Transaction = transaction;
            return dbCommand;
        }

        private string ResolveSql(DataCommandArguments command, ref SQLType sqlType)
        {
            if (databaseQuery == null || command == null)
            {
                return string.Empty;
            }

            return databaseQuery.GetQuery(command, ref sqlType);
        }
    }
}
