// Copyright 2012 Peter Townsend
// Licensed under the MIT License 

using System;
using System.Collections.Generic;
using System.Linq;

namespace Redis.Core
{
	public abstract class RedisPipelineBase : RedisCommandsBase, IRedisPipeline
	{
		protected readonly IRedisConnection Connection;
		protected readonly List<string[]> PipelineRequests = new List<string[]>();

		private readonly List<RedisResult> pipelineResults = new List<RedisResult>();
		private readonly bool transactioned;

		protected RedisPipelineBase(IRedisConnection connection, bool transactioned)
		{
			this.Connection = connection;
			this.transactioned = transactioned;
		}

		IRedisResult<TValue> IRedisPipeline.Add<TValue>(Func<IRedisCommands, TValue> command)
		{
			RedisResult result;

			if (typeof(TValue) == typeof(string))
			{
				result = new StringResult();
			}
			else if (typeof(TValue) == typeof(int))
			{
				result = new Int32Result();
			}
			else if (typeof(TValue) == typeof(long))
			{
				result = new Int64Result();
			}
			else if (typeof(TValue) == typeof(int?))
			{
				result = new NullableInt32Result();
			}
			else if (typeof(TValue) == typeof(long?))
			{
				result = new NullableInt64Result();
			}
			else if (typeof(TValue) == typeof(List<string>))
			{
				result = new ListResult();
			}
			else if (typeof(TValue) == typeof(Dictionary<string, string>))
			{
				result = new DictionaryResult();
			}
			else
			{
				var log = string.Format("Unsupported redis return type {0}", typeof(TValue).Name);
				throw new Exception(log);
			}

			command(this);

			this.pipelineResults.Add(result);

			return (IRedisResult<TValue>) result;
		}

		bool IRedisPipeline.Commit()
		{
			List<RedisResponse> pipelineResponses;

			if (this.PipelineRequests.Count == 0)
			{
				return true;
			}

			if (this.transactioned)
			{
				var transactionRequests = new List<string[]> { new[] { "MULTI" } };
				transactionRequests.AddRange(this.PipelineRequests);
				transactionRequests.Add(new[] { "EXEC" });

				var responses = this.Connection.Run(transactionRequests.ToArray());

				var execResponse = responses.Last();

				// The pipeline responses are nested on the EXEC response
				pipelineResponses = execResponse.Responses;
			}
			else
			{
				pipelineResponses = this.Connection.Run(this.PipelineRequests.ToArray());
			}

			if (pipelineResponses == null)
			{
				this.PipelineRequests.Clear();
				this.pipelineResults.Clear();

				return false;
			}

			for (int i = 0; i < pipelineResponses.Count; i++)
			{
				var result = this.pipelineResults[i];
				var response = pipelineResponses[i];

				result.Apply(response);
			}

			this.PipelineRequests.Clear();
			this.pipelineResults.Clear();

			return true;
		}

		void IRedisPipeline.Discard()
		{
			this.PipelineRequests.Clear();
			this.pipelineResults.Clear();
		}

		public void Dispose()
		{
			this.PipelineRequests.Clear();
			this.pipelineResults.Clear();
		}
	}
}