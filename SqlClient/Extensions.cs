﻿
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Open.Database.Extensions.SqlClient
{
	public static class Extensions
	{
		public static IDbDataParameter AddParameterType(this IDbCommand target, string name, SqlDbType value)
		{
			var c = (SqlParameter)target.CreateParameter();
			c.ParameterName = name;
			c.SqlDbType = value;
			target.Parameters.Add(c);
			return c;
		}

		public static SqlCommand CreateCommand(this SqlConnection conn,
			CommandType type, string commandText, int secondsTimeout = 30)
		{
			var command = conn.CreateCommand();
			command.CommandType = type;
			command.CommandText = commandText;
			command.CommandTimeout = secondsTimeout;

			return command;
		}

		public static ISourceBlock<T> IterateReaderAsync<T>(this SqlDataReader reader,
			Func<IDataRecord, T> transform)
		{
			var block = new BufferBlock<T>();

			Task.Run(async () =>
			{
				while (await reader.ReadAsync())
					block.Post(transform(reader));

				block.Complete();
			});

			return block;
		}

		public static async Task IterateReaderAsync<T>(this SqlDataReader reader,
			Func<IDataRecord, T> transform,
			Action<T> whileWaitingForNext)
		{
			var block = new ActionBlock<T>(whileWaitingForNext);

			while (await reader.ReadAsync())
				block.Post(transform(reader));

			block.Complete();
			await block.Completion;
		}

		public static async Task<List<T>> ToListAsync<T>(this SqlDataReader reader,
			Func<IDataRecord, T> transform)
		{
			var list = new List<T>();
			await IterateReaderAsync(reader, transform, v => list.Add(v));
			return list;
		}

		public static ExpressiveSqlCommand Command(
			this IDbConnectionFactory<SqlConnection> target,
			string command, CommandType type = CommandType.Text)
		{
			return new ExpressiveSqlCommand(target, type, command);
		}

		public static ExpressiveSqlCommand StoredProcedure(
			this IDbConnectionFactory<SqlConnection> target,
			string command)
		{
			return new ExpressiveSqlCommand(target, CommandType.StoredProcedure, command);
		}
	}
}
