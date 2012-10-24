using NUnit.Framework;
using Redis.Core;

namespace Redis.Tests
{
	[TestFixture]
	public class ListTests
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
		public void BLPop()
		{
			this.redis.LPush("listkey", "tyrannosaurus rex");
			this.redis.LPush("listkey", "triceratops");

			var list = this.redis.BLPop(new[] { "nokey", "listkey" }, 1);

			Assert.That(list, Has.Count.EqualTo(2));
			Assert.That(list[0], Is.EqualTo("listkey"));
			Assert.That(list[1], Is.EqualTo("triceratops"));
		}

		[Test]
		public void BLPop_empty_list()
		{
			Assert.That(this.redis.BLPop("listkey", 1), Is.Null);
		}

		[Test]
		public void BRPop()
		{
			this.redis.LPush("listkey", "tyrannosaurus rex");
			this.redis.LPush("listkey", "triceratops");

			var list = this.redis.BRPop(new[] { "nokey", "listkey" }, 1);

			Assert.That(list, Has.Count.EqualTo(2));
			Assert.That(list[0], Is.EqualTo("listkey"));
			Assert.That(list[1], Is.EqualTo("tyrannosaurus rex"));
		}

		[Test]
		public void BRPop_empty_list()
		{
			Assert.That(this.redis.BRPop("listkey", 1), Is.Null);
		}

		[Test]
		[Ignore]
		public void BRPopLPush()
		{
			this.redis.LPush("listkey1", "stegosaurus");

			Assert.That(this.redis.BRPopLPush("listkey1", "listkey2", 1), Is.EqualTo("stegosaurus"));

			Assert.That(this.redis.LLen("listkey1"), Is.EqualTo(0));
			Assert.That(this.redis.LLen("listkey2"), Is.EqualTo(1));
			Assert.That(this.redis.LIndex("listkey2", 0), Is.EqualTo("stegosaurus"));
		}

		[Test]
		[Ignore]
		public void BRPopLPush_empty_list()
		{
			Assert.That(this.redis.BRPopLPush("listkey1", "listkey2", 1), Is.EqualTo(null));
		}

		[Test]
		public void LIndex()
		{
			this.redis.LPush("listkey", "brontosaurus");
			this.redis.LPush("listkey", "velociraptor");
			this.redis.LPush("listkey", "pterodactyl");

			Assert.That(this.redis.LIndex("listkey", 0), Is.EqualTo("pterodactyl"));
			Assert.That(this.redis.LIndex("listkey", 1), Is.EqualTo("velociraptor"));
			Assert.That(this.redis.LIndex("listkey", 2), Is.EqualTo("brontosaurus"));
			Assert.That(this.redis.LIndex("nokey", 2), Is.Null);
		}

		[Test]
		[Ignore]
		public void LInsert()
		{
			this.redis.LPush("listkey", "planet");
			this.redis.LPush("listkey", "the");
			this.redis.LPush("listkey", "save");

			Assert.That(this.redis.LInsert("listkey", InsertOption.After, "save", "superman"), Is.EqualTo(1));
			Assert.That(this.redis.LInsert("listkey", InsertOption.Before, "planet", "please"), Is.EqualTo(1));
			Assert.That(this.redis.LInsert("listkey", InsertOption.After, "novalue", "dance"), Is.EqualTo(0));

			Assert.That(this.redis.LLen("listkey"), Is.EqualTo(5));
			Assert.That(this.redis.LIndex("listkey", 0), Is.EqualTo("please"));
			Assert.That(this.redis.LIndex("listkey", 4), Is.EqualTo("superman"));
		}

		[Test]
		public void LLen()
		{
			this.redis.LPush("listkey", "two");
			this.redis.LPush("listkey", "one");

			Assert.That(this.redis.LLen("listkey"), Is.EqualTo(2));
			Assert.That(this.redis.LLen("nokey"), Is.EqualTo(0));
		}

		[Test]
		public void LPop()
		{
			this.redis.LPush("listkey", "two");
			this.redis.LPush("listkey", "one");

			Assert.That(this.redis.LPop("listkey"), Is.EqualTo("one"));
			Assert.That(this.redis.LPop("nokey"), Is.EqualTo(null));
			Assert.That(this.redis.LLen("listkey"), Is.EqualTo(1));
		}

		[Test]
		public void LPush()
		{
			Assert.That(this.redis.LPush("listkey", "stop"), Is.EqualTo(1));
			Assert.That(this.redis.LPush("listkey", "dont"), Is.EqualTo(2));

			Assert.That(this.redis.LLen("listkey"), Is.EqualTo(2));
			Assert.That(this.redis.LIndex("listkey", 0), Is.EqualTo("dont"));
			Assert.That(this.redis.LIndex("listkey", 1), Is.EqualTo("stop"));
		}

		[Test]
		[Ignore]
		public void LPushX()
		{
			Assert.That(this.redis.LPushX("listkey", "two"), Is.EqualTo(0));

			this.redis.LPush("listkey", "two");

			Assert.That(this.redis.LPushX("listkey", "one"), Is.EqualTo(1));

			Assert.That(this.redis.LLen("listkey"), Is.EqualTo(2));
		}

		[Test]
		public void LRange()
		{
			this.redis.LPush("listkey", "three");
			this.redis.LPush("listkey", "two");
			this.redis.LPush("listkey", "one");

			var list = this.redis.LRange("listkey", 0, -1);

			Assert.That(list, Has.Count.EqualTo(3));
			Assert.That(list[0], Is.EqualTo("one"));
			Assert.That(list[1], Is.EqualTo("two"));
			Assert.That(list[2], Is.EqualTo("three"));
		}

		[Test]
		public void LRange_trim_start()
		{
			this.redis.LPush("listkey", "three");
			this.redis.LPush("listkey", "two");
			this.redis.LPush("listkey", "one");

			var list = this.redis.LRange("listkey", 1, -1);

			Assert.That(list, Has.Count.EqualTo(2));
			Assert.That(list[0], Is.EqualTo("two"));
			Assert.That(list[1], Is.EqualTo("three"));
		}

		[Test]
		public void LRange_trim_end()
		{
			this.redis.LPush("listkey", "three");
			this.redis.LPush("listkey", "two");
			this.redis.LPush("listkey", "one");

			var list = this.redis.LRange("listkey", 0, 1);

			Assert.That(list, Has.Count.EqualTo(2));
			Assert.That(list[0], Is.EqualTo("one"));
			Assert.That(list[1], Is.EqualTo("two"));
		}

		[Test]
		public void LRange_trim_start_and_end()
		{
			this.redis.LPush("listkey", "three");
			this.redis.LPush("listkey", "two");
			this.redis.LPush("listkey", "one");

			var list = this.redis.LRange("listkey", 1, 1);

			Assert.That(list, Has.Count.EqualTo(1));
			Assert.That(list[0], Is.EqualTo("two"));
		}

		[Test]
		public void LRange_empty_range()
		{
			this.redis.LPush("listkey", "three");
			this.redis.LPush("listkey", "two");
			this.redis.LPush("listkey", "one");

			var list = this.redis.LRange("listkey", 5, 100);

			Assert.That(list, Has.Count.EqualTo(0));
		}

		[Test]
		public void LRem()
		{
			this.redis.LPush("listkey", "dog");
			this.redis.LPush("listkey", "dog");
			this.redis.LPush("listkey", "dog");
			this.redis.LPush("listkey", "cat");
			this.redis.LPush("listkey", "mouse");
			this.redis.LPush("listkey", "cat");
			this.redis.LPush("listkey", "horse");
			this.redis.LPush("listkey", "dog");

			Assert.That(this.redis.LRem("listkey", 0, "cat"), Is.EqualTo(2));
			Assert.That(this.redis.LRem("listkey", -2, "dog"), Is.EqualTo(2));
			Assert.That(this.redis.LRem("listkey", 2, "horse"), Is.EqualTo(1));

			var list = this.redis.LRange("listkey", 0, -1);

			Assert.That(list, Has.Count.EqualTo(3));
			Assert.That(list[0], Is.EqualTo("dog"));
			Assert.That(list[1], Is.EqualTo("mouse"));
			Assert.That(list[2], Is.EqualTo("dog"));
		}

		[Test]
		public void LSet()
		{
			this.redis.LPush("listkey", "horse");
			this.redis.LPush("listkey", "cat");
			this.redis.LPush("listkey", "dog");
			this.redis.LPush("listkey", "mouse");

			Assert.That(this.redis.LSet("listkey", 0, "dragon"), Is.EqualTo("OK"));
			Assert.That(this.redis.LSet("listkey", -2, "sphinx"), Is.EqualTo("OK"));

			var list = this.redis.LRange("listkey", 0, -1);

			Assert.That(list, Has.Count.EqualTo(4));
			Assert.That(list[0], Is.EqualTo("dragon"));
			Assert.That(list[1], Is.EqualTo("dog"));
			Assert.That(list[2], Is.EqualTo("sphinx"));
			Assert.That(list[3], Is.EqualTo("horse"));
		}

		[Test]
		[ExpectedException(typeof(RedisException), ExpectedMessage = "ERR index out of range")]
		public void LSet_out_of_range_index()
		{
			this.redis.LPush("listkey", "dog");
			this.redis.LSet("listkey", 2, "dragon");
		}

		[Test]
		public void LTrim()
		{
			this.redis.LPush("listkey", "three");
			this.redis.LPush("listkey", "two");
			this.redis.LPush("listkey", "one");

			Assert.That(this.redis.LTrim("listkey", 0, 1), Is.EqualTo("OK"));

			var list = this.redis.LRange("listkey", 0, -1);

			Assert.That(list, Has.Count.EqualTo(2));
			Assert.That(list[0], Is.EqualTo("one"));
			Assert.That(list[1], Is.EqualTo("two"));
		}

		[Test]
		public void RPop()
		{
			this.redis.LPush("listkey", "two");
			this.redis.LPush("listkey", "one");

			Assert.That(this.redis.RPop("listkey"), Is.EqualTo("two"));
			Assert.That(this.redis.RPop("nokey"), Is.EqualTo(null));
			Assert.That(this.redis.LLen("listkey"), Is.EqualTo(1));
		}

		[Test]
		public void RPush()
		{
			Assert.That(this.redis.RPush("listkey", "dont"), Is.EqualTo(1));
			Assert.That(this.redis.RPush("listkey", "stop"), Is.EqualTo(2));

			Assert.That(this.redis.LLen("listkey"), Is.EqualTo(2));
			Assert.That(this.redis.LIndex("listkey", 0), Is.EqualTo("dont"));
			Assert.That(this.redis.LIndex("listkey", 1), Is.EqualTo("stop"));
		}

		[Test]
		[Ignore]
		public void RPushX()
		{
			Assert.That(this.redis.RPushX("listkey", "one"), Is.EqualTo(0));

			this.redis.LPush("listkey", "one");

			Assert.That(this.redis.RPushX("listkey", "two"), Is.EqualTo(1));

			Assert.That(this.redis.LLen("listkey"), Is.EqualTo(2));
		}
	}
}