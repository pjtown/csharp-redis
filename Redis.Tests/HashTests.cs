using NUnit.Framework;
using Redis.Core;

namespace Redis.Tests
{
	[TestFixture]
	public class HashTests
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
		public void HDel()
		{
			this.redis.HSet("hashkey", "field", "human");
			Assert.That(this.redis.HDel("hashkey", "field"), Is.EqualTo(1));
			Assert.That(this.redis.HDel("hashkey", "field"), Is.EqualTo(0));
			Assert.That(this.redis.HDel("hashkey", "nofield"), Is.EqualTo(0));
		}

		[Test]
		public void HExists()
		{
			this.redis.HSet("hashkey", "field", "human");
			Assert.That(this.redis.HExists("hashkey", "field"), Is.EqualTo(1));
			Assert.That(this.redis.HExists("hashkey", "nofield"), Is.EqualTo(0));
		}

		[Test]
		public void HGet()
		{
			this.redis.HSet("hashkey", "field", "human");
			Assert.That(this.redis.HGet("hashkey", "field"), Is.EqualTo("human"));
			Assert.That(this.redis.HGet("hashkey", "nofield"), Is.Null);
		}

		[Test]
		public void HGetAll()
		{
			this.redis.HMSet("hashkey", "name", "superman", "address", "north pole");

			var dict = this.redis.HGetAll("hashkey");

			Assert.That(dict, Has.Count.EqualTo(2));
			Assert.That(dict["name"], Is.EqualTo("superman"));
			Assert.That(dict["address"], Is.EqualTo("north pole"));
		}

		[Test]
		public void HGetAll_empty_hash()
		{
			var dict = this.redis.HGetAll("hashkey");
			Assert.That(dict, Has.Count.EqualTo(0));
		}

		[Test]
		public void HIncrBy()
		{
			Assert.That(this.redis.HIncrBy("hashkey", "field", 10), Is.EqualTo(10));
			Assert.That(this.redis.HIncrBy("hashkey", "field", -4), Is.EqualTo(6));
		}

		[Test]
		[ExpectedException(typeof(RedisException), ExpectedMessage = "ERR hash value is not an integer")]
		public void HIncrBy_invalid_hash_value()
		{
			this.redis.HSet("hashkey", "field", "human");
			this.redis.HIncrBy("hashkey", "field", 10);
		}

		[Test]
		public void HKeys()
		{
			this.redis.HMSet("hashkey", "name", "superman", "address", "north pole");

			var list = this.redis.HKeys("hashkey");

			Assert.That(list, Has.Count.EqualTo(2));
			Assert.That(list[0], Is.EqualTo("name"));
			Assert.That(list[1], Is.EqualTo("address"));
		}

		[Test]
		public void HKeys_empty_hash()
		{
			var list = this.redis.HKeys("hashkey");
			Assert.That(list, Has.Count.EqualTo(0));
		}

		[Test]
		public void HLen()
		{
			this.redis.HMSet("hashkey", "name", "superman", "address", "north pole");
			Assert.That(this.redis.HLen("hashkey"), Is.EqualTo(2));
			Assert.That(this.redis.HLen("nokey"), Is.EqualTo(0));
		}

		[Test]
		public void HMGet()
		{
			this.redis.HMSet("hashkey", "name", "superman", "address", "north pole");

			var list = this.redis.HMGet("hashkey", "name", "age");

			Assert.That(list, Has.Count.EqualTo(2));
			Assert.That(list[0], Is.EqualTo("superman"));
			Assert.That(list[1], Is.Null);
		}
		
		[Test]
		public void HMSet()
		{
			Assert.That(this.redis.HMSet("hashkey", "name", "superman", "address", "north pole"), Is.EqualTo("OK"));
			Assert.That(this.redis.HGet("hashkey", "name"), Is.EqualTo("superman"));
			Assert.That(this.redis.HGet("hashkey", "address"), Is.EqualTo("north pole"));
		}

		[Test]
		public void HSet()
		{
			Assert.That(this.redis.HSet("hashkey", "name", "superman"), Is.EqualTo(1));
			Assert.That(this.redis.HGet("hashkey", "name"), Is.EqualTo("superman"));
			Assert.That(this.redis.HSet("hashkey", "name", "batman"), Is.EqualTo(0));
			Assert.That(this.redis.HGet("hashkey", "name"), Is.EqualTo("batman"));
		}
		
		[Test]
		public void HSetNx()
		{
			Assert.That(this.redis.HSetNx("hashkey", "name", "superman"), Is.EqualTo(1));
			Assert.That(this.redis.HGet("hashkey", "name"), Is.EqualTo("superman"));
			Assert.That(this.redis.HSetNx("hashkey", "name", "batman"), Is.EqualTo(0));
			Assert.That(this.redis.HGet("hashkey", "name"), Is.EqualTo("superman"));
		}

		[Test]
		public void HVals()
		{
			this.redis.HMSet("hashkey", "name", "superman", "address", "north pole");

			var list = this.redis.HVals("hashkey");

			Assert.That(list, Has.Count.EqualTo(2));
			Assert.That(list[0], Is.EqualTo("superman"));
			Assert.That(list[1], Is.EqualTo("north pole"));
		}
	}
}