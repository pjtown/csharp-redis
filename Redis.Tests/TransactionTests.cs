using NUnit.Framework;
using Redis.Core;

namespace Redis.Tests
{
	[TestFixture]
	public class TransactionTests
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
		public void Pipeline_set_get()
		{
			using (var pipe = this.redis.CreatePipeline())
			{
				var set1 = pipe.Add(x => x.Set("key1", "human"));
				var set2 = pipe.Add(x => x.Set("key2", "civilization"));
				var get1 = pipe.Add(x => x.Get("key1"));
				var get2 = pipe.Add(x => x.Get("nokey"));

				Assert.That(set1.Value, Is.Null);
				Assert.That(set2.Value, Is.Null);
				Assert.That(get1.Value, Is.Null);
				Assert.That(get2.Value, Is.Null);

				pipe.Commit();

				Assert.That(set1.Value, Is.EqualTo("OK"));
				Assert.That(set2.Value, Is.EqualTo("OK"));
				Assert.That(get1.Value, Is.EqualTo("human"));
				Assert.That(get2.Value, Is.Null);
			}
		}
		
		[Test]
		public void Transaction_set_get()
		{
			using (var pipe = this.redis.CreatePipeline(true))
			{
				var set1 = pipe.Add(x => x.Set("key1", "human"));
				var set2 = pipe.Add(x => x.Set("key2", "civilization"));
				var get1 = pipe.Add(x => x.Get("key1"));
				var get2 = pipe.Add(x => x.Get("nokey"));

				Assert.That(set1.Value, Is.Null);
				Assert.That(set2.Value, Is.Null);
				Assert.That(get1.Value, Is.Null);
				Assert.That(get2.Value, Is.Null);

				pipe.Commit();

				Assert.That(set1.Value, Is.EqualTo("OK"));
				Assert.That(set2.Value, Is.EqualTo("OK"));
				Assert.That(get1.Value, Is.EqualTo("human"));
				Assert.That(get2.Value, Is.Null);
			}
		}
		
		[Test]
		public void Pipeline_test()
		{
			using (var pipe = this.redis.CreatePipeline(true))
			{
				var set1 = pipe.Add(x => x.Set("key1", "human"));
				var set2 = pipe.Add(x => x.Set("key2", "civilization"));
				var get1 = pipe.Add(x => x.Get("key1"));
				var get2 = pipe.Add(x => x.Get("nokey"));

				Assert.That(set1.Value, Is.Null);
				Assert.That(set2.Value, Is.Null);
				Assert.That(get1.Value, Is.Null);
				Assert.That(get2.Value, Is.Null);

				pipe.Commit();

				Assert.That(set1.Value, Is.EqualTo("OK"));
				Assert.That(set2.Value, Is.EqualTo("OK"));
				Assert.That(get1.Value, Is.EqualTo("human"));
				Assert.That(get2.Value, Is.Null);
			}
		}
	}
}