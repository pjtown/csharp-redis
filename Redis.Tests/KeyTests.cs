using NUnit.Framework;
using Redis.Core;

namespace Redis.Tests
{
	[TestFixture]
	public class KeyTests
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
		public void Del()
		{
			this.redis.Set("key", "human");
			Assert.That(this.redis.Del("key"), Is.EqualTo(1));
			Assert.That(this.redis.Del("key"), Is.EqualTo(0));
		}
		
		[Test]
		public void Exists()
		{
			this.redis.Set("key", "human");
			Assert.That(this.redis.Exists("key"), Is.EqualTo(1));
			Assert.That(this.redis.Exists("nokey"), Is.EqualTo(0));
		}
		
		[Test]
		public void Expire()
		{
			this.redis.Set("key", "human");
			Assert.That(this.redis.Expire("key", 10), Is.EqualTo(1));
			Assert.That(this.redis.Ttl("key"), Is.EqualTo(10));
			Assert.That(this.redis.Expire("nokey", 10), Is.EqualTo(0));
		}

		[Test]
		public void ExpireAt()
		{
			this.redis.Set("key", "human");
			Assert.That(this.redis.ExpireAt("key", 1234567890), Is.EqualTo(1));
			Assert.That(this.redis.ExpireAt("nokey", 1234567890), Is.EqualTo(0));
		}
		
		[Test]
		public void Keys()
		{
			this.redis.MSet("key11", "human", "key12", "civilization", "key22", "apes");
			
			var list = this.redis.Keys("*");
			Assert.That(list, Has.Count.EqualTo(3));

			list = this.redis.Keys("key");
			Assert.That(list, Has.Count.EqualTo(0));

			list = this.redis.Keys("key1*");
			Assert.That(list, Has.Count.EqualTo(2));

			list = this.redis.Keys("key*2");
			Assert.That(list, Has.Count.EqualTo(2));
		}

		[Test]
		[Ignore]
		public void Persist()
		{
			this.redis.Set("key", "human");

			Assert.That(this.redis.Persist("key"), Is.EqualTo(0));

			this.redis.Expire("key", 10);

			Assert.That(this.redis.Persist("key"), Is.EqualTo(1));
			Assert.That(this.redis.Persist("key"), Is.EqualTo(0));
			Assert.That(this.redis.Ttl("key"), Is.EqualTo(-1));
		}

		[Test]
		public void RandomKey()
		{
			Assert.That(this.redis.RandomKey(), Is.Null);

			this.redis.MSet("key1", "human", "key2", "civilization");

			Assert.That(this.redis.RandomKey(), Is.EqualTo("key1").Or.EqualTo("key2"));
		}

		[Test]
		public void Rename()
		{
			this.redis.Set("key", "human");
			Assert.That(this.redis.Rename("key", "newkey"), Is.EqualTo("OK"));
			Assert.That(this.redis.Get("key"), Is.Null);
			Assert.That(this.redis.Get("newkey"), Is.EqualTo("human"));
		}

		[Test]
		[ExpectedException(typeof(RedisException), ExpectedMessage = "ERR no such key")]
		public void Rename_missing_key()
		{
			this.redis.Rename("key", "newkey");
		}

		[Test]
		[ExpectedException(typeof(RedisException), ExpectedMessage = "ERR source and destination objects are the same")]
		public void Rename_to_same_key_name()
		{
			this.redis.Set("key", "human");
			this.redis.Rename("key", "key");
		}

		[Test]
		public void RenameNx()
		{
			this.redis.MSet("key1", "human", "key2", "civilization");
			Assert.That(this.redis.RenameNx("key1", "key3"), Is.EqualTo(1));
			Assert.That(this.redis.RenameNx("key2", "key3"), Is.EqualTo(0));
			Assert.That(this.redis.Get("key"), Is.Null);
			Assert.That(this.redis.Get("key2"), Is.EqualTo("civilization"));
			Assert.That(this.redis.Get("key3"), Is.EqualTo("human"));
		}

		[Test]
		[ExpectedException(typeof(RedisException), ExpectedMessage = "ERR no such key")]
		public void RenameNx_missing_key()
		{
			this.redis.RenameNx("key", "newkey");
		}

		[Test]
		[ExpectedException(typeof(RedisException), ExpectedMessage = "ERR source and destination objects are the same")]
		public void RenameNx_to_same_key_name()
		{
			this.redis.Set("key", "human");
			this.redis.RenameNx("key", "key");
		}

		[Test]
		[Ignore("TODO")]
		public void Sort()
		{
			this.redis.Set("key", "human");
		}

		[Test]
		public void Ttl()
		{
			this.redis.SetEx("key", 10, "human");
			Assert.That(this.redis.Ttl("key"), Is.EqualTo(10));
			Assert.That(this.redis.Ttl("nokey"), Is.EqualTo(-1));
		}

		[Test]
		public void Type()
		{
			this.redis.Set("stringkey", "human");
			this.redis.LPush("listkey", "civilization");
			this.redis.HSet("hashkey", "field", "runners");
			this.redis.SAdd("setkey", "monkey");
			this.redis.ZAdd("zsetkey", "1", "michael");

			Assert.That(this.redis.Type("stringkey"), Is.EqualTo("string"));
			Assert.That(this.redis.Type("listkey"), Is.EqualTo("list"));
			Assert.That(this.redis.Type("hashkey"), Is.EqualTo("hash"));
			Assert.That(this.redis.Type("setkey"), Is.EqualTo("set"));
			Assert.That(this.redis.Type("zsetkey"), Is.EqualTo("zset"));
		}
	}

}