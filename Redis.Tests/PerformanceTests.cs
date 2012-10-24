using System;
using System.Collections.Generic;
using System.Diagnostics;
using NUnit.Framework;
using Redis.Core;

namespace Redis.Tests
{
	[TestFixture]
	public class PerformanceTests
	{
		private IRedisFactory redisFactory;
		private IRedis redis;

		[TestFixtureSetUp]
		public void TestFixtureSetUp()
		{
			this.redisFactory = new RedisFactory();
		}

		[SetUp]
		public void SetUp()
		{
			var connection = new RedisConnection("localhost", dbIndex: 10);
			this.redis = this.redisFactory.CreateRedis(connection);

			this.redis.FlushDB();
		}

		[Test]
		public void Looped_set_get_vs_pipeline_set_get()
		{
			const int commandCount = 2000;
			var stopwatch = new Stopwatch();

			// Looped SET ------------------------------------

			stopwatch.Start();

			for (int i = 0; i < commandCount; i++)
			{
				var key = "key" + i;
				var value = "value" + i;
				this.redis.Set(key, value);
			}

			stopwatch.Stop();

			Console.WriteLine("Looped SET   : {0} commands {1}ms", commandCount, stopwatch.ElapsedMilliseconds);

			// Looped GET ------------------------------------

			stopwatch.Restart();

			var loopedGetResults = new List<string>();

			for (int i = 0; i < commandCount; i++)
			{
				var key = "key" + i;
				loopedGetResults.Add(this.redis.Get(key));
			}

			stopwatch.Stop();

			for (int i = 0; i < commandCount; i++)
			{
				Assert.That(loopedGetResults[i] == "value" + i);
			}

			Console.WriteLine("Looped GET   : {0} commands {1}ms", commandCount, stopwatch.ElapsedMilliseconds);

			// Pipeline SET ----------------------------------

			this.redis.FlushDB();
			
			stopwatch.Restart();

			using (var pipe = this.redis.CreatePipeline(true))
			{
				for (int i = 0; i < commandCount; i++)
				{
					var key = "key" + i;
					var value = "value" + i;
					pipe.Add(x => x.Set(key, value));
				}

				pipe.Commit();
			}

			stopwatch.Stop();

			Console.WriteLine("Pipeline SET : {0} commands {1}ms", commandCount, stopwatch.ElapsedMilliseconds);

			// Pipeline GET ----------------------------------

			stopwatch.Restart();
			
			var pipelineGetResults = new List<IRedisResult<string>>();

			using (var pipe = this.redis.CreatePipeline(true))
			{
				for (int i = 0; i < commandCount; i++)
				{
					var key = "key" + i;
					pipelineGetResults.Add(pipe.Add(x => x.Get(key)));
				}

				pipe.Commit();
			}

			stopwatch.Stop();

			for (int i = 0; i < commandCount; i++)
			{
				Assert.That(pipelineGetResults[i].Value == "value" + i);
			}

			Console.WriteLine("Pipeline GET : {0} commands {1}ms", commandCount, stopwatch.ElapsedMilliseconds);
		}
	}
}