using System;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using PerformanceTelemetry.Record;

namespace PerformanceTelemetry.Container.Saver.Item.SqlText
{
    public class SqlTextItemSaver : IItemSaver
    {
        //����� ���������� ������ ���������� ������ �������� ������� ������
        private const long BatchBetweenCleanups = 250000L;

        private readonly HashContainer _hashContainer;

        //���������� � ���� ������
        private readonly SqlTransaction _transaction;
        
        //����������� � ���� ������
        private readonly SqlConnection _connection;

        //����� ��� ������ �����. �����
        private readonly MD5 _md5;
        private readonly string _databaseName;

        //������ ����� ������ ��� ����������
        private readonly string _tableName;

        //������ ����� ��� �������
        private static long _cleanupIndex;

        //������� ���������������� �������
        private readonly SqlCommand _insertItemCommand;
        private readonly SqlCommand _insertStackCommand;

        //�������, ��� ����� ������������
        private bool _disposed = false;

        public SqlTextItemSaver(
            HashContainer hashContainer,
            SqlTransaction transaction,
            SqlConnection connection,
            MD5 md5,
            string databaseName,
            string tableName
            )
        {
            if (hashContainer == null)
            {
                throw new ArgumentNullException("hashContainer");
            }
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }
            if (md5 == null)
            {
                throw new ArgumentNullException("md5");
            }
            if (databaseName == null)
            {
                throw new ArgumentNullException("databaseName");
            }
            if (tableName == null)
            {
                throw new ArgumentNullException("tableName");
            }

            _hashContainer = hashContainer;
            _transaction = transaction;
            _connection = connection;
            _md5 = md5;
            _databaseName = databaseName;
            _tableName = tableName;

            var insertStackClause = InsertStackClause.Replace(
                "{_TableName_}",
                _tableName
                );

            _insertStackCommand = new SqlCommand(insertStackClause, _connection, _transaction);

            var insertItemClause = InsertItemClause.Replace(
                "{_TableName_}",
                _tableName
                );

            _insertItemCommand = new SqlCommand(insertItemClause, _connection, _transaction);

        }

        public void SaveItem(IPerformanceRecordData item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }

            //��������� �� ������� �� ���� ������� ������
            if (Interlocked.Increment(ref _cleanupIndex) == BatchBetweenCleanups)
            {
                //���� �������
                SqlTextItemSaverFactory.DoCleanup(
                    _connection,
                    _transaction,
                    _databaseName,
                    _tableName
                    );

                Interlocked.Exchange(ref _cleanupIndex, 0L);
            }


            this.SaveItem(
                null,
                item
                );
        }

        public void Commit()
        {
            if (!_disposed)
            {
                _insertItemCommand.Dispose();
                _insertStackCommand.Dispose();

                _transaction.Commit();
                _transaction.Dispose();

                _disposed = true;
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _insertItemCommand.Dispose();
                _insertStackCommand.Dispose();

                _transaction.Rollback();
                _transaction.Dispose();

                _disposed = true;
            }
        }

        #region private code

        private long SaveItem(
            long? parentId,
            IPerformanceRecordData item
            )
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }

            var result = 0L;

            //��������� � ��������� ����, ���� ����������

            var combined = string.Format("{0}.{1}", item.ClassName, item.MethodName);
            var combinedHash = _md5.ComputeHash(Encoding.UTF8.GetBytes(combined));
            var combinedGuid = new Guid(combinedHash);

            if (!_hashContainer.Contains(combinedGuid))
            {
                //������ ����� ���, ���������

                _insertStackCommand.Parameters.Clear();
                _insertStackCommand.Parameters.AddWithValue("id", combinedGuid);
                _insertStackCommand.Parameters.AddWithValue("class_name", CutOff(item.ClassName, SqlTextItemSaverFactory.ClassNameMaxLength));
                _insertStackCommand.Parameters.AddWithValue("method_name", CutOff(item.MethodName, SqlTextItemSaverFactory.MethodNameMaxLength));
                _insertStackCommand.Parameters.AddWithValue("creation_stack", item.CreationStack);

                _insertStackCommand.ExecuteNonQuery();

                _hashContainer.Add(combinedGuid);
            }


            //��������� ��������� ������
            var exceptionMessage = item.Exception != null ? (object) CutOff(item.Exception.Message, SqlTextItemSaverFactory.ExceptionMessageMaxLength) : null;
            var exceptionStack = item.Exception != null ? (object) item.Exception.StackTrace : null;
            var exceptionFullText = item.Exception != null ? (object) Exception2StringHelper.ToFullString(item.Exception) : null;

            _insertItemCommand.Parameters.Clear();
            _insertItemCommand.Parameters.AddWithValue("id_parent", (object) parentId ?? DBNull.Value);
            _insertItemCommand.Parameters.AddWithValue("start_time", item.StartTime);
            _insertItemCommand.Parameters.AddWithValue("exception_message", exceptionMessage ?? DBNull.Value);
            _insertItemCommand.Parameters.AddWithValue("exception_stack", exceptionStack ?? DBNull.Value);
            _insertItemCommand.Parameters.AddWithValue("time_interval", item.TimeInterval);
            _insertItemCommand.Parameters.AddWithValue("id_stack", combinedGuid);
            _insertItemCommand.Parameters.AddWithValue("exception_full_text", exceptionFullText ?? DBNull.Value);

            result = (long) (decimal) _insertItemCommand.ExecuteScalar();

            if (item.Children != null)
            {
                foreach (var child in item.Children)
                {
                    SaveItem(result, child);
                }
            }

            return
                result;
        }

        private string CutOff(string message, int maxLength)
        {
            //message allowed to be null
            if (maxLength <= 3)
            {
                throw new ArgumentException("maxLength");
            }

            if (message == null)
            {
                return
                    null;
            }

            if (message.Length < maxLength)
            {
                return
                    message;
            }

            return
                string.Format(
                    "{0}{1}",
                    message.Substring(0, maxLength - 3),
                    "..."
                    );
        }

        private const string InsertStackClause = @"
insert into [dbo].[{_TableName_}Stack]
    (  id,  class_name,  method_name,  creation_stack )
values
    ( @id, @class_name, @method_name, @creation_stack )
";

        private const string InsertItemClause = @"
insert into [dbo].[{_TableName_}]
    ( date_commit,  id_parent,  start_time,  exception_message,  exception_stack,  time_interval,  id_stack,  exception_full_text)
values
    (   GETDATE(), @id_parent, @start_time, @exception_message, @exception_stack, @time_interval, @id_stack, @exception_full_text);

select SCOPE_IDENTITY();
";

        #endregion

    }

}