using NUnit.Framework;
using Redis.Core;

namespace Redis.Tests
{
	[TestFixture]
	public class SortedSetTests
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
		public void ZAdd()
		{
			Assert.That(this.redis.ZAdd("setkey", "5", "dennis"), Is.EqualTo(1));
			
			var list = this.redis.ZRange("setkey", 0, -1);

			Assert.That(list, Has.Count.EqualTo(1));
			Assert.That(list[0], Is.EqualTo("dennis"));
		}

		[Test]
		public void ZCard()
		{
			this.redis.ZAdd("setkey", "5", "dennis");

			Assert.That(this.redis.ZCard("setkey"), Is.EqualTo(1));
			Assert.That(this.redis.ZCard("nokey"), Is.EqualTo(0));
		}
		
		[Test]
		public void ZIncrBy()
		{
			this.redis.ZAdd("setkey", "5", "dennis");

			Assert.That(this.redis.ZIncrBy("setkey", "2", "dennis"), Is.EqualTo("7"));
			Assert.That(this.redis.ZIncrBy("setkey", "2", "martha"), Is.EqualTo("2"));
		}

		[Test]
		[Ignore("TODO")]
		public void ZInterStore()
		{
		}

		[Test]
		public void ZRange()
		{
			this.redis.ZAdd("setkey", "5", "dennis");
			this.redis.ZAdd("setkey", "4", "martha");

			var list = this.redis.ZRange("setkey", 0, -1);

			Assert.That(list, Has.Count.EqualTo(2));
			Assert.That(list[0], Is.EqualTo("martha"));
			Assert.That(list[1], Is.EqualTo("dennis"));
		}
		
		[Test]
		public void ZRange_with_scores()
		{
			this.redis.ZAdd("setkey", "5", "dennis");
			this.redis.ZAdd("setkey", "4", "martha");

			var list = this.redis.ZRangeWithScores("setkey", 0, -1);

			Assert.That(list, Has.Count.EqualTo(4));
			Assert.That(list[0], Is.EqualTo("martha"));
			Assert.That(list[1], Is.EqualTo("4"));
			Assert.That(list[2], Is.EqualTo("dennis"));
			Assert.That(list[3], Is.EqualTo("5"));
		}

		[Test]
		public void ZRangeByScore()
		{
			this.redis.ZAdd("setkey", "5", "dennis");
			this.redis.ZAdd("setkey", "4", "martha");
			this.redis.ZAdd("setkey", "9", "tonka");

			var list = this.redis.ZRangeByScore("setkey", "5", "10", 0, 10);

			Assert.That(list, Has.Count.EqualTo(2));
			Assert.That(list[0], Is.EqualTo("dennis"));
			Assert.That(list[1], Is.EqualTo("tonka"));
		}

		[Test]
		public void ZRank()
		{
			this.redis.ZAdd("setkey", "5", "dennis");
			this.redis.ZAdd("setkey", "4", "martha");
			this.redis.ZAdd("setkey", "9", "tonka");

			Assert.That(this.redis.ZRank("setkey", "tonka"), Is.EqualTo(2));
			Assert.That(this.redis.ZRank("setkey", "nomember"), Is.Null);
			Assert.That(this.redis.ZRank("nokey", "nomember"), Is.Null);
		}

		[Test]
		public void ZRem()
		{
			this.redis.ZAdd("setkey", "5", "dennis");
			this.redis.ZAdd("setkey", "4", "martha");
			this.redis.ZAdd("setkey", "9", "tonka");

			Assert.That(this.redis.ZRem("setkey", "dennis"), Is.EqualTo(1));
			Assert.That(this.redis.ZRem("setkey", "michael"), Is.EqualTo(0));
			Assert.That(this.redis.ZCard("setkey"), Is.EqualTo(2));
		}
		
		[Test]
		public void ZRemRangeByRank()
		{
			this.redis.ZAdd("setkey", "5", "dennis");
			this.redis.ZAdd("setkey", "4", "martha");
			this.redis.ZAdd("setkey", "9", "tonka");

			Assert.That(this.redis.ZRemRangeByRank("setkey", 0, 1), Is.EqualTo(2));
			
			var list = this.redis.ZRange("setkey", 0, -1);

			Assert.That(list, Has.Count.EqualTo(1));
			Assert.That(list[0], Is.EqualTo("tonka"));
		}

		[Test]
		public void ZRemRangeByScore()
		{
			this.redis.ZAdd("setkey", "5", "dennis");
			this.redis.ZAdd("setkey", "4", "martha");
			this.redis.ZAdd("setkey", "9", "tonka");

			Assert.That(this.redis.ZRemRangeByScore("setkey", "2", "4"), Is.EqualTo(1));
			
			var list = this.redis.ZRange("setkey", 0, -1);

			Assert.That(list, Has.Count.EqualTo(2));
			Assert.That(list[0], Is.EqualTo("dennis"));
			Assert.That(list[1], Is.EqualTo("tonka"));
		}

		[Test]
		public void ZRevRange()
		{
			this.redis.ZAdd("setkey", "5", "dennis");
			this.redis.ZAdd("setkey", "4", "martha");

			var list = this.redis.ZRevRange("setkey", 0, -1);

			Assert.That(list, Has.Count.EqualTo(2));
			Assert.That(list[0], Is.EqualTo("dennis"));
			Assert.That(list[1], Is.EqualTo("martha"));
		}

		[Test]
		public void ZRevRange_with_scores()
		{
			this.redis.ZAdd("setkey", "5", "dennis");
			this.redis.ZAdd("setkey", "4", "martha");

			var list = this.redis.ZRevRangeWithScores("setkey", 0, -1);

			Assert.That(list, Has.Count.EqualTo(4));
			Assert.That(list[0], Is.EqualTo("dennis"));
			Assert.That(list[1], Is.EqualTo("5"));
			Assert.That(list[2], Is.EqualTo("martha"));
			Assert.That(list[3], Is.EqualTo("4"));
		}

		[Test]
		[Ignore]
		public void ZRevRangeByScore()
		{
			this.redis.ZAdd("setkey", "5", "dennis");
			this.redis.ZAdd("setkey", "4", "martha");
			this.redis.ZAdd("setkey", "9", "tonka");

			var list = this.redis.ZRevRangeByScore("setkey", "5", "10", 0, 10);

			Assert.That(list, Has.Count.EqualTo(2));
			Assert.That(list[0], Is.EqualTo("tonka"));
			Assert.That(list[1], Is.EqualTo("dennis"));
		}

		[Test]
		[Ignore]
		public void ZUnionStore()
		{
		}
	}
}