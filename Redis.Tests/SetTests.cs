using NUnit.Framework;
using Redis.Core;

namespace Redis.Tests
{
	[TestFixture]
	public class SetTests
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
		public void SAdd()
		{
			Assert.That(this.redis.SAdd("setkey", "dennis"), Is.EqualTo(1));
			Assert.That(this.redis.SIsMember("setkey", "dennis"), Is.EqualTo(1));
		}
		
		[Test]
		public void SAdd_duplicates()
		{
			Assert.That(this.redis.SAdd("setkey", "dennis"), Is.EqualTo(1));
			Assert.That(this.redis.SAdd("setkey", "dennis"), Is.EqualTo(0));
			Assert.That(this.redis.SCard("setkey"), Is.EqualTo(1));
		}

		[Test]
		public void SCard()
		{
			this.redis.SAdd("setkey", "pete");
			Assert.That(this.redis.SCard("setkey"), Is.EqualTo(1));
			Assert.That(this.redis.SCard("nokey"), Is.EqualTo(0));
		}
		
		[Test]
		public void SDiff()
		{
			this.redis.SAdd("setkey1", "sarah");
			this.redis.SAdd("setkey1", "dennis");
			this.redis.SAdd("setkey1", "michael");
			this.redis.SAdd("setkey2", "sarah");
			this.redis.SAdd("setkey2", "jonathan");

			var list = this.redis.SDiff("setkey1", "setkey2");

			Assert.That(list, Has.Count.EqualTo(2));
			Assert.That(list, Has.Member("dennis"));
			Assert.That(list, Has.Member("michael"));
		}

		[Test]
		public void SDiff_one_set()
		{
			this.redis.SAdd("setkey1", "sarah");

			var list = this.redis.SDiff("setkey1");

			Assert.That(list, Has.Count.EqualTo(1));
			Assert.That(list, Has.Member("sarah"));
		}

		[Test]
		public void SDiff_sets_equal()
		{
			this.redis.SAdd("setkey1", "sarah");
			this.redis.SAdd("setkey2", "sarah");

			var list = this.redis.SDiff("setkey1", "setkey2");

			Assert.That(list, Has.Count.EqualTo(0));
		}
		
		[Test]
		public void SDiff_sets_empty()
		{
			var list = this.redis.SDiff("setkey1", "setkey2");
			Assert.That(list, Has.Count.EqualTo(0));
		}

		[Test]
		public void SDiffStore()
		{
			this.redis.SAdd("setkey1", "sarah");
			this.redis.SAdd("setkey1", "dennis");
			this.redis.SAdd("setkey1", "michael");
			this.redis.SAdd("setkey2", "sarah");
			this.redis.SAdd("setkey2", "jonathan");

			Assert.That(this.redis.SDiffStore("destKey", "setkey1", "setkey2"), Is.EqualTo(2));

			var list = this.redis.SMembers("destKey");

			Assert.That(list, Has.Count.EqualTo(2));
			Assert.That(list, Has.Member("dennis"));
			Assert.That(list, Has.Member("michael"));
		}

		[Test]
		public void SInter()
		{
			this.redis.SAdd("setkey1", "sarah");
			this.redis.SAdd("setkey1", "dennis");
			this.redis.SAdd("setkey1", "michael");
			this.redis.SAdd("setkey2", "sarah");
			this.redis.SAdd("setkey2", "jonathan");

			var list = this.redis.SInter("setkey1", "setkey2");

			Assert.That(list, Has.Count.EqualTo(1));
			Assert.That(list, Has.Member("sarah"));
		}

		[Test]
		public void SInter_one_set()
		{
			this.redis.SAdd("setkey1", "sarah");

			var list = this.redis.SInter("setkey1");

			Assert.That(list, Has.Count.EqualTo(1));
			Assert.That(list, Has.Member("sarah"));
		}

		[Test]
		public void SInter_sets_different()
		{
			this.redis.SAdd("setkey1", "sarah");
			this.redis.SAdd("setkey2", "dennis");

			var list = this.redis.SInter("setkey1", "setkey2");

			Assert.That(list, Has.Count.EqualTo(0));
		}

		[Test]
		public void SInter_sets_empty()
		{
			var list = this.redis.SInter("setkey1", "setkey2");
			Assert.That(list, Has.Count.EqualTo(0));
		}

		[Test]
		public void SInterStore()
		{
			this.redis.SAdd("setkey1", "sarah");
			this.redis.SAdd("setkey1", "dennis");
			this.redis.SAdd("setkey1", "michael");
			this.redis.SAdd("setkey2", "sarah");
			this.redis.SAdd("setkey2", "jonathan");

			Assert.That(this.redis.SInterStore("destKey", "setkey1", "setkey2"), Is.EqualTo(1));

			var list = this.redis.SMembers("destKey");

			Assert.That(list, Has.Count.EqualTo(1));
			Assert.That(list, Has.Member("sarah"));
		}

		[Test]
		public void SIsMember()
		{
			this.redis.SAdd("setkey1", "sarah");

			Assert.That(this.redis.SIsMember("setkey1", "sarah"), Is.EqualTo(1));
			Assert.That(this.redis.SIsMember("setkey1", "michael"), Is.EqualTo(0));
		}

		[Test]
		public void SMembers()
		{
			this.redis.SAdd("setkey1", "sarah");
			this.redis.SAdd("setkey1", "dennis");

			var list = this.redis.SMembers("setkey1");

			Assert.That(list, Has.Count.EqualTo(2));
			Assert.That(list, Has.Member("sarah"));
			Assert.That(list, Has.Member("dennis"));
		}

		[Test]
		public void SMembers_empty_set()
		{
			var list = this.redis.SMembers("setkey1");
			Assert.That(list, Has.Count.EqualTo(0));
		}

		[Test]
		public void SMove()
		{
			this.redis.SAdd("setkey1", "sarah");
			this.redis.SAdd("setkey1", "dennis");

			Assert.That(this.redis.SMove("setkey1", "setkey2", "sarah"), Is.EqualTo(1));

			var list1 = this.redis.SMembers("setkey1");

			Assert.That(list1, Has.Count.EqualTo(1));
			Assert.That(list1, Has.Member("dennis"));
			
			var list2 = this.redis.SMembers("setkey2");

			Assert.That(list2, Has.Count.EqualTo(1));
			Assert.That(list2, Has.Member("sarah"));
		}

		[Test]
		public void SMove_member_not_in_source()
		{
			this.redis.SAdd("setkey1", "dennis");

			Assert.That(this.redis.SMove("setkey1", "setkey2", "sarah"), Is.EqualTo(0));

			Assert.That(this.redis.SCard("setkey1"), Is.EqualTo(1));
			Assert.That(this.redis.SCard("setkey2"), Is.EqualTo(0));
		}
		
		[Test]
		public void SMove_member_already_in_dest()
		{
			this.redis.SAdd("setkey1", "sarah");
			this.redis.SAdd("setkey1", "dennis");
			this.redis.SAdd("setkey2", "sarah");

			Assert.That(this.redis.SMove("setkey1", "setkey2", "sarah"), Is.EqualTo(1));

			var list1 = this.redis.SMembers("setkey1");

			Assert.That(list1, Has.Count.EqualTo(1));
			Assert.That(list1, Has.Member("dennis"));

			var list2 = this.redis.SMembers("setkey2");

			Assert.That(list2, Has.Count.EqualTo(1));
			Assert.That(list2, Has.Member("sarah"));
		}

		[Test]
		public void SPop()
		{
			this.redis.SAdd("setkey", "sarah");
			this.redis.SAdd("setkey", "dennis");

			var result = this.redis.SPop("setkey");

			Assert.That(result, Is.EqualTo("sarah").Or.EqualTo("dennis"));
			Assert.That(this.redis.SIsMember("setkey", result), Is.EqualTo(0));
		}
		
		[Test]
		public void SPop_empty_set()
		{
			Assert.That(this.redis.SPop("setkey"), Is.Null);
		}
		
		[Test]
		public void SRandMember()
		{
			this.redis.SAdd("setkey", "sarah");
			this.redis.SAdd("setkey", "dennis");

			var result = this.redis.SRandMember("setkey");

			Assert.That(result, Is.EqualTo("sarah").Or.EqualTo("dennis"));
			Assert.That(this.redis.SCard("setkey"), Is.EqualTo(2));
			Assert.That(this.redis.SIsMember("setkey", result), Is.EqualTo(1));
		}
		
		[Test]
		public void SRandMember_empty_set()
		{
			Assert.That(this.redis.SRandMember("setkey"), Is.Null);
		}

		[Test]
		public void SRem()
		{
			this.redis.SAdd("setkey", "sarah");
			this.redis.SAdd("setkey", "dennis");

			Assert.That(this.redis.SRem("setkey", "sarah"), Is.EqualTo(1));
			Assert.That(this.redis.SRem("setkey", "michael"), Is.EqualTo(0));
			Assert.That(this.redis.SCard("setkey"), Is.EqualTo(1));
		}

		[Test]
		public void SUnion()
		{
			this.redis.SAdd("setkey1", "sarah");
			this.redis.SAdd("setkey1", "michael");
			this.redis.SAdd("setkey2", "sarah");
			this.redis.SAdd("setkey2", "jonathan");

			var list = this.redis.SUnion("setkey1", "setkey2");

			Assert.That(list, Has.Count.EqualTo(3));
			Assert.That(list, Has.Member("sarah"));
			Assert.That(list, Has.Member("michael"));
			Assert.That(list, Has.Member("jonathan"));
		}

		[Test]
		public void SUnion_one_set()
		{
			this.redis.SAdd("setkey1", "sarah");

			var list = this.redis.SUnion("setkey1");

			Assert.That(list, Has.Count.EqualTo(1));
			Assert.That(list, Has.Member("sarah"));
		}

		[Test]
		public void SUnion_sets_different()
		{
			this.redis.SAdd("setkey1", "sarah");
			this.redis.SAdd("setkey2", "dennis");

			var list = this.redis.SUnion("setkey1", "setkey2");

			Assert.That(list, Has.Count.EqualTo(2));
			Assert.That(list, Has.Member("sarah"));
			Assert.That(list, Has.Member("dennis"));
		}

		[Test]
		public void SUnion_sets_empty()
		{
			var list = this.redis.SUnion("setkey1", "setkey2");
			Assert.That(list, Has.Count.EqualTo(0));
		}

		[Test]
		public void SUnionStore()
		{
			this.redis.SAdd("setkey1", "sarah");
			this.redis.SAdd("setkey1", "michael");
			this.redis.SAdd("setkey2", "sarah");
			this.redis.SAdd("setkey2", "jonathan");

			Assert.That(this.redis.SUnionStore("destkey", "setkey1", "setkey2"), Is.EqualTo(3));

			var list = this.redis.SMembers("destkey");

			Assert.That(list, Has.Count.EqualTo(3));
			Assert.That(list, Has.Member("sarah"));
			Assert.That(list, Has.Member("michael"));
			Assert.That(list, Has.Member("jonathan"));
		}
	}
}