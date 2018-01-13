﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Open.Database.Extensions
{
	public abstract class ExpressiveAsyncCommandBase<TConnection, TCommand, TDbType, TThis>
        : ExpressiveCommandBase<TConnection, TCommand, TDbType, TThis>
        where TConnection : class, IDbConnection
        where TCommand : class, IDbCommand
        where TDbType : struct
        where TThis : ExpressiveAsyncCommandBase<TConnection, TCommand, TDbType, TThis>
    {

        protected ExpressiveAsyncCommandBase(
            IDbConnectionFactory<TConnection> connFactory,
            CommandType type,
            string name,
            List<Param> @params = null)
            : base(connFactory, type, name, @params)
        {
        }

        protected ExpressiveAsyncCommandBase(
            IDbConnectionFactory<TConnection> connFactory,
            CommandType type,
            string name,
            params Param[] @params)
            : this(connFactory, type, name, @params.ToList())
        {

        }


        public abstract Task ExecuteAsync(Func<TCommand, Task> handler);

        public abstract Task<T> ExecuteAsync<T>(Func<TCommand, Task<T>> handler);

        public abstract Task<int> ExecuteNonQueryAsync();

        public abstract Task<object> ExecuteScalarAsync();

        public async Task<T> ExecuteScalarAsync<T>()
        {
            return (T)(await ExecuteScalarAsync());
        }

        /// <summary>
        /// Iterates asynchronously and will stop iterating if canceled.
        /// </summary>
        /// <param name="handler">The active IDataRecord is passed to this handler.</param>
        /// <param name="token">An optional cancellation token.</param>
        /// <returns></returns>
        public abstract Task IterateReaderAsync(Action<IDataRecord> handler, CancellationToken? token = null);

        /// <summary>
        /// Iterates asynchronously until the handler returns false.  Then cancels.
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public abstract Task IterateReaderAsyncWhile(Func<IDataRecord, bool> predicate);

        public async Task ToTargetBlockAsync<T>(Func<IDataRecord, T> transform, ITargetBlock<T> target)
        {
            await IterateReaderAsyncWhile(r => target.Post(transform(r)));
            target.Complete();
        }

        public ISourceBlock<T> AsSourceBlockAsync<T>(Func<IDataRecord, T> transform)
        {
            var source = new BufferBlock<T>();
            ToTargetBlockAsync(transform, source).ConfigureAwait(false);
            return source;
        }

        public abstract Task<List<T>> ToListAsync<T>(Func<IDataRecord, T> transform);

        public async Task<List<Dictionary<string, object>>> ToListAsync(HashSet<string> columnNames)
        {
            var list = new List<Dictionary<string, object>>();
            await IterateReaderAsync(r => list.Add(r.ToDictionary(columnNames)));
            return list;
        }

        public Task<List<Dictionary<string, object>>> ToListAsync(IEnumerable<string> columnNames)
            => ToListAsync(new HashSet<string>(columnNames));

        public async Task<List<Dictionary<string, object>>> ToListAsync(params string[] columnNames)
        {
            // Probably an unnecessary check, but need to be sure.
            if (columnNames.Length != 0)
                return await ToListAsync(new HashSet<string>(columnNames));

            var list = new List<Dictionary<string, object>>();
            await IterateReaderAsync(r => list.Add(r.ToDictionary()));
            return list;
        }

        public async Task<IEnumerable<T>> ResultsAsync<T>()
            where T : new()
        {
            var x = new Transformer<T>();
            // ToListAsync pulls extracts all the data first.  Then we use the .Select to transform into the desired model T;
            return (await ToListAsync(x.PropertyNames))
                .Select(entry => x.Transform(entry));
        }

    }
}
