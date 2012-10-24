using NUnit.Framework;
using Redis.Core;

namespace Redis.Tests
{
	[TestFixture]
	public class StringTests
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
		public void Append()
		{
			Assert.That(this.redis.Append("key", "human"), Is.EqualTo(5));
			Assert.That(this.redis.Append("key", "kind"), Is.EqualTo(9));
			Assert.That(this.redis.Get("key"), Is.EqualTo("humankind"));
		}

		[Test]
		public void Decr()
		{
			this.redis.Set("key", "10");
			Assert.That(this.redis.Decr("key"), Is.EqualTo(9));
		}

		[Test]
		public void Decr_missing_key()
		{
			Assert.That(this.redis.Decr("key"), Is.EqualTo(-1));
		}

		[Test]
		[ExpectedException(typeof(RedisException), ExpectedMessage = "ERR value is not an integer")]
		public void Decr_invalid_value()
		{
			this.redis.Set("key", "human");
			this.redis.Decr("key");
		}

		[Test]
		public void DecrBy()
		{
			Assert.That(this.redis.DecrBy("key", -10), Is.EqualTo(10));
			Assert.That(this.redis.DecrBy("key", 3), Is.EqualTo(7));
			Assert.That(this.redis.DecrBy("key",20), Is.EqualTo(-13));
		}

		[Test]
		[ExpectedException(typeof(RedisException), ExpectedMessage = "ERR value is not an integer")]
		public void DecrBy_invalid_value()
		{
			this.redis.Set("key", "human");
			this.redis.DecrBy("key", 3);
		}

		[Test]
		public void Get()
		{
			this.redis.Set("key", "human");
			Assert.That(this.redis.Get("key"), Is.EqualTo("human"));
		}

		[Test]
		public void Get_missing_key()
		{
			Assert.That(this.redis.Get("key"), Is.Null);
		}

		[Test]
		[Ignore]
		public void GetBit()
		{
			this.redis.SetBit("key", 5, 1);
			Assert.That(this.redis.GetBit("key", 0), Is.EqualTo(0));
			Assert.That(this.redis.GetBit("key", 5), Is.EqualTo(1));
			Assert.That(this.redis.GetBit("key", 6), Is.EqualTo(0));
		}

		[Test]
		[Ignore]
		public void GetRange()
		{
			this.redis.Set("key", "breakfast");
			Assert.That(this.redis.GetRange("key", 2, 5), Is.EqualTo("eak"));
			Assert.That(this.redis.GetRange("key", 3, 100), Is.EqualTo("akfast"));
		}

		[Test]
		public void GetSet()
		{
			Assert.That(this.redis.GetSet("key", "human"), Is.Null);
			Assert.That(this.redis.GetSet("key", "monkey"), Is.EqualTo("human"));
			Assert.That(this.redis.GetSet("key", "human"), Is.EqualTo("monkey"));
		}

		[Test]
		public void Incr()
		{
			this.redis.Set("key", "10");
			Assert.That(this.redis.Incr("key"), Is.EqualTo(11));
		}

		[Test]
		public void Incr_missing_key()
		{
			Assert.That(this.redis.Incr("key"), Is.EqualTo(1));
		}

		[Test]
		[ExpectedException(typeof(RedisException), ExpectedMessage = "ERR value is not an integer")]
		public void Incr_invalid_value()
		{
			this.redis.Set("key", "human");
			this.redis.Incr("key");
		}

		[Test]
		public void IncrBy()
		{
			Assert.That(this.redis.IncrBy("key", -10), Is.EqualTo(-10));
			Assert.That(this.redis.IncrBy("key", 3), Is.EqualTo(-7));
			Assert.That(this.redis.IncrBy("key", 20), Is.EqualTo(13));
		}

		[Test]
		[ExpectedException(typeof(RedisException), ExpectedMessage = "ERR value is not an integer")]
		public void IncrBy_invalid_value()
		{
			this.redis.Set("key", "human");
			this.redis.IncrBy("key", 3);
		}

		[Test]
		public void MGet()
		{
			this.redis.MSet("key1", "hello", "key3", "world");

			var list = this.redis.MGet("key1", "key2", "key3", "key3");

			Assert.That(list, Has.Count.EqualTo(4));
			Assert.That(list[0], Is.EqualTo("hello"));
			Assert.That(list[1], Is.Null);
			Assert.That(list[2], Is.EqualTo("world"));
			Assert.That(list[3], Is.EqualTo("world"));
		}

		[Test]
		public void MSet()
		{
			Assert.That(this.redis.MSet("key1", "hello", "key3", "world"), Is.EqualTo("OK"));

			Assert.That(this.redis.Get("key1"), Is.EqualTo("hello"));
			Assert.That(this.redis.Get("key3"), Is.EqualTo("world"));
		}

		[Test]
		public void MSetNx()
		{
			Assert.That(this.redis.MSetNx("key1", "hello", "key3", "world"), Is.EqualTo(1));
			Assert.That(this.redis.MSetNx("key1", "goodbye", "key2", "sweet"), Is.EqualTo(0));

			Assert.That(this.redis.Get("key1"), Is.EqualTo("hello"));
			Assert.That(this.redis.Get("key2"), Is.Null);
			Assert.That(this.redis.Get("key3"), Is.EqualTo("world"));
		}

		[Test]
		public void Set()
		{
			Assert.That(this.redis.Set("key", "human"), Is.EqualTo("OK"));
			Assert.That(this.redis.Get("key"), Is.EqualTo("human"));
		}

		[Test]
		public void Set_empty_value()
		{
			Assert.That(this.redis.Set("key", ""), Is.EqualTo("OK"));
			Assert.That(this.redis.Get("key"), Is.EqualTo(""));
		}

		[Test]
		[ExpectedException(typeof(RedisException), ExpectedMessage = "Invalid request. NULL request parameter")]
		public void Set_null_value()
		{
			Assert.That(this.redis.Set("key", null), Is.EqualTo("OK"));
			Assert.That(this.redis.Get("key"), Is.Null);
		}

		[Test]
		[Ignore]
		public void SetBit()
		{
			Assert.That(this.redis.SetBit("key", 7, 1), Is.EqualTo(0));
			Assert.That(this.redis.GetBit("key", 7), Is.EqualTo(1));
			Assert.That(this.redis.GetBit("key", 8), Is.EqualTo(0));
			Assert.That(this.redis.SetBit("key", 7, 0), Is.EqualTo(1));
			Assert.That(this.redis.GetBit("key", 7), Is.EqualTo(0));
		}

		[Test]
		public void SetEx()
		{
			Assert.That(this.redis.SetEx("key", 10, "human"), Is.EqualTo("OK"));
			Assert.That(this.redis.Ttl("key"), Is.EqualTo(10));
			Assert.That(this.redis.Get("key"), Is.EqualTo("human"));
		}

		[Test]
		[ExpectedException(typeof(RedisException), ExpectedMessage = "ERR invalid expire time in SETEX")]
		public void SetEx_invalid_seconds()
		{
			this.redis.SetEx("key", -1, "human");
		}

		[Test]
		public void SetNx()
		{
			Assert.That(this.redis.SetNx("key", "human"), Is.EqualTo(1));
			Assert.That(this.redis.SetNx("key", "monkey"), Is.EqualTo(0));
			Assert.That(this.redis.Get("key"), Is.EqualTo("human"));
		}

		[Test]
		[Ignore]
		public void SetRange()
		{
			this.redis.Set("key", "human");
			Assert.That(this.redis.SetRange("key", 3, "or is the best medicine"), Is.EqualTo(26));
			Assert.That(this.redis.Get("key"), Is.EqualTo("humor is the best medicine"));
		}

		[Test]
		[Ignore]
		public void Strlen()
		{
			this.redis.Set("key", "human");
			Assert.That(this.redis.Strlen("key"), Is.EqualTo(5));
			Assert.That(this.redis.Strlen("nokey"), Is.EqualTo(0));
		}
	}
}