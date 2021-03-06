﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Open.Database.Extensions
{
    public static partial class Extensions
    {

        static QueryResult<Queue<object[]>> RetrieveInternal(this IDataReader reader, int[] ordinals, string[] columnNames = null, bool readStarted = false)
        {
            var fieldCount = ordinals.Length;
            if (columnNames == null) columnNames = ordinals.Select(n => reader.GetName(n)).ToArray();
            else if (columnNames.Length != fieldCount) throw new ArgumentException("Mismatched array lengths of ordinals and names.");

            return new QueryResult<Queue<object[]>>(
                ordinals,
                columnNames,
                new Queue<object[]>(AsEnumerableInternal(reader, ordinals, readStarted)));
        }

        /// <summary>
        /// Iterates all records within the first result set using an IDataReader and returns the results.
        /// DBNull values are left unchanged (retained).
        /// </summary>
        /// <param name="reader">The IDataReader to read results from.</param>
        /// <returns>The QueryResult that contains all the results and the column mappings.</returns>
        public static QueryResult<Queue<object[]>> Retrieve(this IDataReader reader)
        {
            var names = reader.GetNames();
            return new QueryResult<Queue<object[]>>(
                Enumerable.Range(0, names.Length).ToArray(),
                names,
                new Queue<object[]>(AsEnumerable(reader)));
        }

        /// <summary>
        /// Iterates all records within the current result set using an IDataReader and returns the desired results as a list of Dictionaries containing only the specified column values.
        /// DBNull values are left unchanged (retained).
        /// </summary>
        /// <param name="reader">The IDataReader to read results from.</param>
        /// <param name="ordinals">The ordinals to request from the reader for each record.</param>
        /// <returns>The QueryResult that contains all the results and the column mappings.</returns>
        public static QueryResult<Queue<object[]>> Retrieve(this IDataReader reader, IEnumerable<int> ordinals)
            => RetrieveInternal(reader, ordinals.ToArray());

        /// <summary>
        /// Iterates all records within the current result set using an IDataReader and returns the desired results.
        /// DBNull values are left unchanged (retained).
        /// </summary>
        /// <param name="reader">The IDataReader to read results from.</param>
        /// <param name="n">The first ordinal to include in the request to the reader for each record.</param>
        /// <param name="others">The remaining ordinals to request from the reader for each record.</param>
        /// <returns>The QueryResult that contains all the results and the column mappings.</returns>
        public static QueryResult<Queue<object[]>> Retrieve(this IDataReader reader, int n, params int[] others)
            => Retrieve(reader, new int[1] { n }.Concat(others));

        /// <summary>
        /// Iterates all records within the current result set using an IDataReader and returns the desired results.
        /// DBNull values are left unchanged (retained).
        /// </summary>
        /// <param name="reader">The IDataReader to read results from.</param>
        /// <param name="columnNames">The column names to select.</param>
        /// <returns>The QueryResult that contains all the results and the column mappings.</returns>
        public static QueryResult<Queue<object[]>> Retrieve(this IDataReader reader, IEnumerable<string> columnNames)
        {
            var columns = reader.GetOrdinalMapping(columnNames);
            var ordinalValues = columns.Select(c => c.Ordinal).ToArray();
            return RetrieveInternal(reader, ordinalValues, columns.Select(c => c.Name).ToArray());
        }

        /// <summary>
        /// Iterates all records within the current result set using an IDataReader and returns the desired results.
        /// DBNull values are left unchanged (retained).
        /// </summary>
        /// <param name="reader">The IDataReader to read results from.</param>
        /// <param name="c">The first column name to include in the request to the reader for each record.</param>
        /// <param name="others">The remaining column names to request from the reader for each record.</param>
        /// <returns>The QueryResult that contains all the results and the column mappings.</returns>
        public static QueryResult<Queue<object[]>> Retrieve(this IDataReader reader, string c, params string[] others)
            => Retrieve(reader, new string[1] { c }.Concat(others));

        /// <summary>
        /// Iterates all records within the first result set using an IDataReader and returns the results.
        /// DBNull values are left unchanged (retained).
        /// </summary>
        /// <param name="command">The IDbCommand to generate the reader from.</param>
        /// <returns>The QueryResult that contains all the results and the column mappings.</returns>
        public static QueryResult<Queue<object[]>> Retrieve(this IDbCommand command)
            => ExecuteReader(command, reader => reader.Retrieve());

        /// <summary>
        /// Iterates all records within the current result set using an IDataReader and returns the desired results.
        /// DBNull values are left unchanged (retained).
        /// </summary>
        /// <param name="command">The IDbCommand to generate the reader from.</param>
        /// <param name="ordinals">The ordinals to request from the reader for each record.</param>
        /// <returns>The QueryResult that contains all the results and the column mappings.</returns>
        public static QueryResult<Queue<object[]>> Retrieve(this IDbCommand command, IEnumerable<int> ordinals)
            => ExecuteReader(command, reader => reader.Retrieve(ordinals));

        /// <summary>
        /// Iterates all records within the current result set using an IDataReader and returns the desired results.
        /// DBNull values are left unchanged (retained).
        /// </summary>
        /// <param name="command">The IDbCommand to generate the reader from.</param>
        /// <param name="n">The first ordinal to include in the request to the reader for each record.</param>
        /// <param name="others">The remaining ordinals to request from the reader for each record.</param>
        /// <returns>The QueryResult that contains all the results and the column mappings.</returns>
        public static QueryResult<Queue<object[]>> Retrieve(this IDbCommand command, int n, params int[] others)
            => ExecuteReader(command, reader => Retrieve(reader, new int[1] { n }.Concat(others)));

        /// <summary>
        /// Iterates all records within the first result set using an IDataReader and returns the desired results as a list of Dictionaries containing only the specified column values.
        /// DBNull values are left unchanged (retained).
        /// </summary>
        /// <param name="command">The IDbCommand to generate the reader from.</param>
        /// <param name="columnNames">The column names to select.</param>
        /// <returns>The QueryResult that contains all the results and the column mappings.</returns>
        public static QueryResult<Queue<object[]>> Retrieve(this IDbCommand command, IEnumerable<string> columnNames)
            => ExecuteReader(command, reader => reader.Retrieve(columnNames));

        /// <summary>
        /// Iterates all records within the current result set using an IDataReader and returns the desired results.
        /// DBNull values are left unchanged (retained).
        /// </summary>
        /// <param name="command">The IDbCommand to generate the reader from.</param>
        /// <param name="c">The first column name to include in the request to the reader for each record.</param>
        /// <param name="others">The remaining column names to request from the reader for each record.</param>
        /// <returns>The QueryResult that contains all the results and the column mappings.</returns>
        public static QueryResult<Queue<object[]>> Retrieve(this IDbCommand command, string c, params string[] others)
            => ExecuteReader(command, reader => Retrieve(reader, new string[1] { c }.Concat(others)));

		/// <summary>
		/// Asynchronously enumerates all the remaining values of the current result set of a data reader.
		/// DBNull values are left unchanged (retained).
		/// </summary>
		/// <param name="reader">The reader to enumerate.</param>
		/// <param name="token">Optional cancellation token.</param>
		/// <returns>The QueryResult that contains a buffer block of the results and the column mappings.</returns>
		public static async Task<QueryResult<Queue<object[]>>> RetrieveAsync(this DbDataReader reader, CancellationToken? token = null)
        {
			var t = token ?? CancellationToken.None;
			var fieldCount = reader.FieldCount;
            var names = reader.GetNames(); // pull before first read.
            var buffer = new Queue<object[]>();

            while (await reader.ReadAsync(t))
            {
                var row = new object[fieldCount];
                reader.GetValues(row);
                buffer.Enqueue(row);
            }

            return new QueryResult<Queue<object[]>>(
                Enumerable.Range(0, names.Length).ToArray(),
                names,
                buffer);
        }

        static async Task<QueryResult<Queue<object[]>>> RetrieveAsyncInternal(DbDataReader reader, CancellationToken? token, int[] ordinals, string[] columnNames = null, bool readStarted = false)
        {
            var fieldCount = ordinals.Length;
            if (columnNames == null) columnNames = ordinals.Select(n => reader.GetName(n)).ToArray();
            else if (columnNames.Length != fieldCount) throw new ArgumentException("Mismatched array lengths of ordinals and names.");

            Func<IDataRecord, object[]> handler;
            if (fieldCount == 0) handler = record => Array.Empty<object>();
            else handler = record =>
            {
                var row = new object[fieldCount];
                for (var i = 0; i < fieldCount; i++)
                    row[i] = reader.GetValue(ordinals[i]);
                return row;
            };

			var t = token ?? CancellationToken.None;
			var buffer = new Queue<object[]>();
            if (readStarted || await reader.ReadAsync(t))
            {
                do
                {
                    buffer.Enqueue(handler(reader));
                }
                while (await reader.ReadAsync(t));
            }

            return new QueryResult<Queue<object[]>>(
                ordinals,
                columnNames,
                buffer);
        }


		/// <summary>
		/// Asynchronously enumerates all the remaining values of the current result set of a data reader.
		/// DBNull values are left unchanged (retained).
		/// </summary>
		/// <param name="reader">The reader to enumerate.</param>
		/// <param name="ordinals">The limited set of ordinals to include.  If none are specified, the returned objects will be empty.</param>
		/// <param name="token">Optional cancellation token.</param>
		/// <returns>The QueryResult that contains a buffer block of the results and the column mappings.</returns>
		public static Task<QueryResult<Queue<object[]>>> RetrieveAsync(this DbDataReader reader, IEnumerable<int> ordinals, CancellationToken? token = null)
            => RetrieveAsyncInternal(reader, token, ordinals as int[] ?? ordinals.ToArray());

        /// <summary>
        /// Asynchronously enumerates all the remaining values of the current result set of a data reader.
        /// DBNull values are left unchanged (retained).
        /// </summary>
        /// <param name="reader">The reader to enumerate.</param>
        /// <param name="n">The first ordinal to include in the request to the reader for each record.</param>
        /// <param name="others">The remaining ordinals to request from the reader for each record.</param>
        /// <returns>The QueryResult that contains a buffer block of the results and the column mappings.</returns>
        public static Task<QueryResult<Queue<object[]>>> RetrieveAsync(this DbDataReader reader, int n, params int[] others)
            => RetrieveAsync(reader, new int[1] { n }.Concat(others));

		/// <summary>
		/// Asynchronously enumerates all the remaining values of the current result set of a data reader.
		/// DBNull values are left unchanged (retained).
		/// </summary>
		/// <param name="reader">The reader to enumerate.</param>
		/// <param name="n">The first ordinal to include in the request to the reader for each record.</param>
		/// <param name="token">A cancellation token.</param>
		/// <param name="others">The remaining ordinals to request from the reader for each record.</param>
		/// <returns>The QueryResult that contains a buffer block of the results and the column mappings.</returns>
		public static Task<QueryResult<Queue<object[]>>> RetrieveAsync(this DbDataReader reader, CancellationToken token, int n, params int[] others)
			=> RetrieveAsync(reader, new int[1] { n }.Concat(others), token);

		/// <summary>
		/// Asynchronously enumerates all records within the current result set using an IDataReader and returns the desired results.
		/// DBNull values are left unchanged (retained).
		/// </summary>
		/// <param name="reader">The IDataReader to read results from.</param>
		/// <param name="columnNames">The column names to select.</param>
		/// <param name="normalizeColumnOrder">Orders the results arrays by ordinal.</param>
		/// <param name="token">Optional cancellation token.</param>
		/// <returns>The QueryResult that contains all the results and the column mappings.</returns>
		public static Task<QueryResult<Queue<object[]>>> RetrieveAsync(this DbDataReader reader, IEnumerable<string> columnNames, bool normalizeColumnOrder = false, CancellationToken? token = null)
        {
            // Validate columns first.
            var columns = reader.GetOrdinalMapping(columnNames, normalizeColumnOrder);

            return RetrieveAsyncInternal(reader, token,

				columns.Select(c => c.Ordinal).ToArray(),
                columns.Select(c => c.Name).ToArray());
        }

        /// <summary>
        /// Asynchronously enumerates all records within the current result set using an IDataReader and returns the desired results.
        /// DBNull values are left unchanged (retained).
        /// </summary>
        /// <param name="reader">The IDataReader to read results from.</param>
        /// <param name="c">The first column name to include in the request to the reader for each record.</param>
        /// <param name="others">The remaining column names to request from the reader for each record.</param>
        /// <returns>The QueryResult that contains all the results and the column mappings.</returns>
        public static Task<QueryResult<Queue<object[]>>> RetrieveAsync(this DbDataReader reader, string c, params string[] others)
            => RetrieveAsync(reader, new string[1] { c }.Concat(others));

		/// <summary>
		/// Asynchronously enumerates all records within the current result set using an IDataReader and returns the desired results.
		/// DBNull values are left unchanged (retained).
		/// </summary>
		/// <param name="reader">The IDataReader to read results from.</param>
		/// <param name="token">Optional cancellation token.</param>
		/// <param name="c">The first column name to include in the request to the reader for each record.</param>
		/// <param name="others">The remaining column names to request from the reader for each record.</param>
		/// <returns>The QueryResult that contains all the results and the column mappings.</returns>
		public static Task<QueryResult<Queue<object[]>>> RetrieveAsync(this DbDataReader reader, CancellationToken token, string c, params string[] others)
			=> RetrieveAsync(reader, new string[1] { c }.Concat(others), false, token);


		static Task<QueryResult<Queue<object[]>>> RetrieveAsyncInternal(DbCommand command, CancellationToken? token, int[] ordinals, string[] columnNames = null)
			=> command.ExecuteReaderAsync(reader => RetrieveAsyncInternal(reader, token, ordinals, columnNames), token: token);

		/// <summary>
		/// Asynchronously enumerates all the remaining values of the current result set of a data reader.
		/// DBNull values are left unchanged (retained).
		/// </summary>
		/// <param name="command">The command to generate a reader from.</param>
		/// <param name="ordinals">The limited set of ordinals to include.  If none are specified, the returned objects will be empty.</param>
		/// <param name="token">Optional cancellation token.</param>
		/// <returns>The QueryResult that contains a buffer block of the results and the column mappings.</returns>
		public static Task<QueryResult<Queue<object[]>>> RetrieveAsync(this DbCommand command, IEnumerable<int> ordinals, CancellationToken? token = null)
			=> RetrieveAsyncInternal(command, token, ordinals as int[] ?? ordinals.ToArray());

		/// <summary>
		/// Asynchronously enumerates all the remaining values of the current result set of a data reader.
		/// DBNull values are left unchanged (retained).
		/// </summary>>
		/// <param name="command">The command to generate a reader from.</param>
		/// <param name="n">The first ordinal to include in the request to the reader for each record.</param>
		/// <param name="others">The remaining ordinals to request from the reader for each record.</param>
		/// <returns>The QueryResult that contains a buffer block of the results and the column mappings.</returns>
		public static Task<QueryResult<Queue<object[]>>> RetrieveAsync(this DbCommand command, int n, params int[] others)
			=> RetrieveAsync(command, new int[1] { n }.Concat(others));

		/// <summary>
		/// Asynchronously enumerates all the remaining values of the current result set of a data reader.
		/// DBNull values are left unchanged (retained).
		/// </summary>>
		/// <param name="command">The command to generate a reader from.</param>
		/// <param name="n">The first ordinal to include in the request to the reader for each record.</param>
		/// <param name="token">A cancellation token.</param>
		/// <param name="others">The remaining ordinals to request from the reader for each record.</param>
		/// <returns>The QueryResult that contains a buffer block of the results and the column mappings.</returns>
		public static Task<QueryResult<Queue<object[]>>> RetrieveAsync(this DbCommand command, CancellationToken token, int n, params int[] others)
			=> RetrieveAsync(command, new int[1] { n }.Concat(others), token);

		/// <summary>
		/// Asynchronously enumerates all records within the current result set using an IDataReader and returns the desired results.
		/// DBNull values are left unchanged (retained).
		/// </summary>>
		/// <param name="command">The command to generate a reader from.</param>
		/// <param name="columnNames">The column names to select.</param>
		/// <param name="normalizeColumnOrder">Orders the results arrays by ordinal.</param>
		/// <param name="token">Optional cancellation token.</param>
		/// <returns>The QueryResult that contains all the results and the column mappings.</returns>
		public static Task<QueryResult<Queue<object[]>>> RetrieveAsync(this DbCommand command, IEnumerable<string> columnNames, bool normalizeColumnOrder = false, CancellationToken? token = null)
			=> command.ExecuteReaderAsync(reader => RetrieveAsync(reader, columnNames, normalizeColumnOrder, token), token: token);

		/// <summary>
		/// Asynchronously enumerates all records within the current result set using an IDataReader and returns the desired results.
		/// DBNull values are left unchanged (retained).
		/// </summary>
		/// <param name="command">The command to generate a reader from.</param>
		/// <param name="c">The first column name to include in the request to the reader for each record.</param>
		/// <param name="others">The remaining column names to request from the reader for each record.</param>
		/// <returns>The QueryResult that contains all the results and the column mappings.</returns>
		public static Task<QueryResult<Queue<object[]>>> RetrieveAsync(this DbCommand command, string c, params string[] others)
			=> RetrieveAsync(command, new string[1] { c }.Concat(others));

		/// <summary>
		/// Asynchronously enumerates all records within the current result set using an IDataReader and returns the desired results.
		/// DBNull values are left unchanged (retained).
		/// </summary>
		/// <param name="command">The command to generate a reader from.</param>
		/// <param name="token">Optional cancellation token.</param>
		/// <param name="c">The first column name to include in the request to the reader for each record.</param>
		/// <param name="others">The remaining column names to request from the reader for each record.</param>
		/// <returns>The QueryResult that contains all the results and the column mappings.</returns>
		public static Task<QueryResult<Queue<object[]>>> RetrieveAsync(this DbCommand command, CancellationToken token, string c, params string[] others)
			=> RetrieveAsync(command, new string[1] { c }.Concat(others), false, token);

	}
}
