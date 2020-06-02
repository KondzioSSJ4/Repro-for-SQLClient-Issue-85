using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SQLClient_Issue85
{

    public sealed class SimpleBulkCopy<T> : IDisposable
    {
        public const int InitialSaveBatchSize = 4000;

        private readonly string _connectionString;
        private readonly BulkCopyTableDefinition<T> _definition;
        private readonly int _bulkCopyTimeout;
        private SqlConnection sqlConnection;
        private SqlBulkCopy bulkCopy;
        private DataTable table;

        public SimpleBulkCopy(
            string connectionString,
            BulkCopyTableDefinition<T> definition,
            int bulkCopyTimeout = 3600)
        {
            _connectionString = connectionString;
            _definition = definition;
            _bulkCopyTimeout = bulkCopyTimeout;
        }

        public void Dispose()
        {
            (bulkCopy as IDisposable)?.Dispose();
            table?.Dispose();
            if (sqlConnection != null)
            {
                sqlConnection.Close();
                sqlConnection.Dispose();
            }
        }

        public Task BegginSave()
        {
            sqlConnection = new SqlConnection(_connectionString);
            if (sqlConnection.State == ConnectionState.Closed)
            {
                sqlConnection.Open();
            }

            bulkCopy = ConfigureBulkCopy(sqlConnection, _definition, _bulkCopyTimeout);
            table = ConfigureDataTable(_definition);

            table.Columns.Cast<DataColumn>().ToList().ForEach(x =>
                bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping(x.ColumnName, x.ColumnName)));

            return Task.CompletedTask;
        }

        private static DataTable ConfigureDataTable(BulkCopyTableDefinition<T> definition)
        {
            var table = new DataTable(definition.TableName);
            foreach (var column in definition.Schema)
            {
                table.Columns.Add(column.FieldName, column.PropertyType);
            }

            return table;
        }

        private static SqlBulkCopy ConfigureBulkCopy(SqlConnection connection, BulkCopyTableDefinition<T> definition, int bulkCopyTimeout)
        {
            var bulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.CheckConstraints | SqlBulkCopyOptions.FireTriggers, null);
            bulkCopy.BulkCopyTimeout = bulkCopyTimeout;
            bulkCopy.DestinationTableName = $"[dbo].[{definition.TableName}]";
            bulkCopy.BatchSize = InitialSaveBatchSize;
            return bulkCopy;
        }

        public async Task Save(IReadOnlyCollection<T> model)
        {
            if (model.Count == 0)
            {
                return;
            }

            table.Clear();

            foreach (var d in model.Select(s => _definition.Schema.Select(def => def.GetValueFunc(s)).ToArray()))
            {
                table.LoadDataRow(d.ToArray(), true);
            }

            bulkCopy.BatchSize = table.Rows.Count;

            var timeout = TimeSpan.FromSeconds(_bulkCopyTimeout + 1);
            var cancellationTokenSource = new CancellationTokenSource(timeout);
            //cancellationTokenSource.CancelAfter(timeout);

            await bulkCopy.WriteToServerAsync(table, cancellationTokenSource.Token);
        }

        public Task FinalizeSaving(bool withException)
        {
            return Task.CompletedTask;
        }
    }
}
