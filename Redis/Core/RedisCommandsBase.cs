// Copyright 2012 Peter Townsend
// Licensed under the MIT License 

using System.Collections.Generic;

namespace Redis.Core
{
	public abstract class RedisCommandsBase : IRedisCommands
	{
		// Keys

		public abstract long Del(string key);
		public abstract long Del(params string[] keys);
		public abstract long Exists(string key);
		public abstract long Expire(string key, long seconds);
		public abstract long ExpireAt(string key, long timestamp);
		public abstract List<string> Keys(string pattern);
		public abstract long Move(string key, long db);
		public abstract long Persist(string key);
		public abstract string RandomKey();
		public abstract string Rename(string key, string newkey);
		public abstract long RenameNx(string key, string newkey);

		public List<string> Sort(string key, long offset = 0, long count = 0, bool alpha = false, SortOrder order = SortOrder.Asc, string destination = null)
		{
			var arguments = new List<string>();
			AppendSortArguments(arguments, offset, count, alpha, order, destination);
			return this.Sort(key, arguments.ToArray());
		}

		public List<string> Sort(string key, string byPattern, long offset = 0, long count = 0, bool alpha = false, SortOrder order = SortOrder.Asc, string destination = null)
		{
			var arguments = new List<string>();
			AppendSortArguments(arguments, offset, count, alpha, order, destination);
			return this.Sort(key, byPattern, arguments.ToArray());
		}

		public List<string> Sort(string key, string[] getPatterns, long offset = 0, long count = 0, bool alpha = false, SortOrder order = SortOrder.Asc, string destination = null)
		{
			var arguments = new List<string>(getPatterns);
			AppendSortArguments(arguments, offset, count, alpha, order, destination);
			return this.Sort(key, arguments.ToArray());
		}

		public List<string> Sort(string key, string byPattern, string[] getPatterns, long offset = 0, long count = 0, bool alpha = false, SortOrder order = SortOrder.Asc, string destination = null)
		{
			var arguments = new List<string>(getPatterns);
			AppendSortArguments(arguments, offset, count, alpha, order, destination);
			return this.Sort(key, byPattern, arguments.ToArray());
		}

		protected abstract List<string> Sort(string key, string[] arguments);
		protected abstract List<string> Sort(string key, string byPattern, string[] arguments);

		private void AppendSortArguments(List<string> arguments, long offset, long count, bool alpha, SortOrder order, string destination)
		{
			if (count > 0)
			{
				arguments.Add("LIMIT");
				arguments.Add(offset.ToString());
				arguments.Add(count.ToString());
			}

			if (alpha)
			{
				arguments.Add("ALPHA");
			}

			arguments.Add(order.ToString());

			if (!string.IsNullOrEmpty(destination))
			{
				arguments.Add("STORE");
				arguments.Add(destination);
			}
		}

		public abstract long Ttl(string key);
		public abstract string Type(string key);

		// Strings

		public abstract long Append(string key, string value);
		public abstract long Decr(string key);
		public abstract long DecrBy(string key, long decrement);
		public abstract string Get(string key);
		public abstract long GetBit(string key, long offset);
		public abstract string GetRange(string key, long start, long end);
		public abstract string GetSet(string key, string value);
		public abstract long Incr(string key);
		public abstract long IncrBy(string key, long increment);
		public abstract List<string> MGet(string key);
		public abstract List<string> MGet(params string[] keys);
		public abstract string MSet(string key, string value);
		public abstract string MSet(params string[] keyValues);
		public abstract long MSetNx(string key, string value);
		public abstract long MSetNx(params string[] keyValues);
		public abstract string Set(string key, string value);
		public abstract long SetBit(string key, long offset, int value);
		public abstract string SetEx(string key, long seconds, string value);
		public abstract long SetNx(string key, string value);
		public abstract long SetRange(string key, long offset, string value);
		public abstract long Strlen(string key);

		// Hashes

		public abstract long HDel(string key, string field);
		public abstract long HDel(string key, params string[] fields);
		public abstract long HExists(string key, string field);
		public abstract string HGet(string key, string field);
		public abstract Dictionary<string, string> HGetAll(string key);
		public abstract long HIncrBy(string key, string field, long increment);
		public abstract List<string> HKeys(string key);
		public abstract long HLen(string key);
		public abstract List<string> HMGet(string key, string field);
		public abstract List<string> HMGet(string key, params string[] fields);
		public abstract string HMSet(string key, string field, string value);
		public abstract string HMSet(string key, params string[] fieldValues);
		public abstract long HSet(string key, string field, string value);
		public abstract long HSetNx(string key, string field, string value);
		public abstract List<string> HVals(string key);

		// Lists

		public abstract List<string> BLPop(string key, long timeout);
		public abstract List<string> BLPop(string[] keys, long timeout);
		public abstract List<string> BRPop(string key, long timeout);
		public abstract List<string> BRPop(string[] keys, long timeout);
		public abstract string BRPopLPush(string source, string destination, long timeout);
		public abstract string LIndex(string key, long index);
		
		public long LInsert(string key, InsertOption option, string pivot, string value)
		{
			return this.LInsert(key, option.ToString(), pivot, value);
		}

		protected abstract long LInsert(string key, string option, string pivot, string value);

		public abstract long LLen(string key);
		public abstract string LPop(string key);
		public abstract long LPush(string key, string value);
		public abstract long LPush(string key, params string[] values);
		public abstract long LPushX(string key, string value);
		public abstract List<string> LRange(string key, long start, long stop);
		public abstract long LRem(string key, long count, string value);
		public abstract string LSet(string key, long index, string value);
		public abstract string LTrim(string key, long start, long stop);
		public abstract string RPop(string key);
		public abstract string RPopLPush(string source, string destination);
		public abstract long RPush(string key, string value);
		public abstract long RPush(string key, params string[] values);
		public abstract long RPushX(string key, string value);

		// Sets

		public abstract long SAdd(string key, string member);
		public abstract long SAdd(string key, params string[] members);
		public abstract long SCard(string key);
		public abstract List<string> SDiff(string key);
		public abstract List<string> SDiff(params string[] keys);
		public abstract long SDiffStore(string destination, string key);
		public abstract long SDiffStore(string destination, params string[] keys);
		public abstract List<string> SInter(string key);
		public abstract List<string> SInter(params string[] keys);
		public abstract long SInterStore(string destination, string key);
		public abstract long SInterStore(string destination, params string[] keys);
		public abstract long SIsMember(string key, string member);
		public abstract List<string> SMembers(string key);
		public abstract long SMove(string source, string destination, string member);
		public abstract string SPop(string key);
		public abstract string SRandMember(string key);
		public abstract long SRem(string key, string member);
		public abstract long SRem(string key, params string[] members);
		public abstract List<string> SUnion(string key);
		public abstract List<string> SUnion(params string[] keys);
		public abstract long SUnionStore(string destination, string key);
		public abstract long SUnionStore(string destination, params string[] keys);

		// Sorted sets

		public abstract long ZAdd(string key, string score, string member);
		public abstract long ZAdd(string key, params string[] scoreMembers);
		public abstract long ZCard(string key);
		public abstract long ZCount(string key, string min, string max);
		public abstract string ZIncrBy(string key, string increment, string member);

		public long ZInterStore(string destination, string[] keys, AggregateOption option = AggregateOption.Sum)
		{
			return this.ZInterScore(destination, keys.Length, keys, "AGGREGATE", option.ToString().ToUpper());
		}

		public long ZInterStore(string destination, string[] keys, string[] weights, AggregateOption option = AggregateOption.Sum)
		{
			return this.ZInterScore(destination, keys.Length, keys, "WEIGHT", weights, "AGGREGATE", option.ToString().ToUpper());
		}

		protected abstract long ZInterScore(string destination, int numkeys, string[] keys, string aggregate, string option);
		protected abstract long ZInterScore(string destination, int numkeys, string[] keys, string weight, string[] weights, string aggregate, string option);

		public abstract List<string> ZRange(string key, long start, long stop);

		public List<string> ZRangeWithScores(string key, long start, long stop)
		{
			return this.ZRange(key, start, stop, "WITHSCORES");
		}

		protected abstract List<string> ZRange(string key, long start, long stop, string withScores);

		public abstract List<string> ZRangeByScore(string key, string min, string max);
		
		public List<string> ZRangeByScoreWithScores(string key, string min, string max)
		{
			return this.ZRangeByScore(key, min, max, "WITHSCORES");
		}

		public List<string> ZRangeByScore(string key, string min, string max, long offset, long count)
		{
			return this.ZRangeByScore(key, min, max, "LIMIT", offset, count);
		}

		public List<string> ZRangeByScoreWithScores(string key, string min, string max, long offset, long count)
		{
			return this.ZRangeByScore(key, min, max, "WITHSCORES", "LIMIT", offset, count);
		}

		protected abstract List<string> ZRangeByScore(string key, string min, string max, string withScores);
		protected abstract List<string> ZRangeByScore(string key, string min, string max, string limit, long offset, long count);
		protected abstract List<string> ZRangeByScore(string key, string min, string max, string withScores, string limit, long offset, long count);

		public abstract long? ZRank(string key, string member);
		public abstract long ZRem(string key, string member);
		public abstract long ZRem(string key, params string[] members);
		public abstract long ZRemRangeByRank(string key, long start, long stop);
		public abstract long ZRemRangeByScore(string key, string min, string max);

		public abstract List<string> ZRevRange(string key, long start, long stop);

		protected abstract List<string> ZRevRange(string key, long start, long stop, string withScores);

		public List<string> ZRevRangeWithScores(string key, long start, long stop)
		{
			return this.ZRevRange(key, start, stop, "WITHSCORES");
		}

		public abstract List<string> ZRevRangeByScore(string key, string min, string max);

		public List<string> ZRevRangeByScoreWithScores(string key, string min, string max)
		{
			return this.ZRevRangeByScore(key, min, max, "WITHSCORES");
		}

		public List<string> ZRevRangeByScore(string key, string min, string max, long offset, long count)
		{
			return this.ZRevRangeByScore(key, min, max, "LIMIT", offset, count);
		}

		public List<string> ZRevRangeByScoreWithScores(string key, string min, string max, long offset, long count)
		{
			return this.ZRevRangeByScore(key, min, max, "WITHSCORES", "LIMIT", offset, count);
		}

		protected abstract List<string> ZRevRangeByScore(string key, string min, string max, string withScores);
		protected abstract List<string> ZRevRangeByScore(string key, string min, string max, string limit, long offset, long count);
		protected abstract List<string> ZRevRangeByScore(string key, string min, string max, string withScores, string limit, long offset, long count);

		public abstract long? ZRevRank(string key, string member);
		public abstract string ZScore(string key, string member);

		public long ZUnionStore(string destination, string[] keys)
		{
			return this.ZUnionStore(destination, keys.Length, keys);
		}

		public long ZUnionStore(string destination, string[] keys, AggregateOption option)
		{
			return this.ZUnionStore(destination, keys.Length, keys, "AGGREGATE", option.ToString().ToUpper());
		}

		public long ZUnionStore(string destination, string[] keys, string[] weights)
		{
			return this.ZUnionStore(destination, keys.Length, keys, "WEIGHT", weights);
		}

		public long ZUnionStore(string destination, string[] keys, string[] weights, AggregateOption option)
		{
			return this.ZUnionStore(destination, keys.Length, keys, "WEIGHT", weights, "AGGREGATE", option.ToString().ToUpper());
		}

		protected abstract long ZUnionStore(string destination, int numkeys, string[] keys);
		protected abstract long ZUnionStore(string destination, int numkeys, string[] keys, string aggregate, string option);
		protected abstract long ZUnionStore(string destination, int numkeys, string[] keys, string weight, string[] weights);
		protected abstract long ZUnionStore(string destination, int numkeys, string[] keys, string weight, string[] weights, string aggregate, string option);
	}
}
