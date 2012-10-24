// Copyright 2012 Peter Townsend
// Licensed under the MIT License 

using System.Collections.Generic;

namespace Redis
{
	public interface IRedisCommands
	{
		// Keys

		long Del(string key);
		long Del(params string[] keys);
		long Exists(string key);
		long Expire(string key, long seconds);
		long ExpireAt(string key, long timestamp);
		List<string> Keys(string pattern);
		long Move(string key, long db);
		long Persist(string key);
		string RandomKey();
		string Rename(string key, string newkey);
		long RenameNx(string key, string newkey);
		List<string> Sort(string key, long offset = 0, long count = 0, bool alpha = false, SortOrder order = SortOrder.Asc, string destination = null);
		List<string> Sort(string key, string byPattern, long offset = 0, long count = 0, bool alpha = false, SortOrder order = SortOrder.Asc, string destination = null);
		List<string> Sort(string key, string[] getPatterns, long offset = 0, long count = 0, bool alpha = false, SortOrder order = SortOrder.Asc, string destination = null);
		List<string> Sort(string key, string byPattern, string[] getPatterns, long offset = 0, long count = 0, bool alpha = false, SortOrder order = SortOrder.Asc, string destination = null);
		long Ttl(string key);
		string Type(string key);

		// Strings

		long Append(string key, string value);
		long Decr(string key);
		long DecrBy(string key, long decrement);
		string Get(string key);
		long GetBit(string key, long offset);
		string GetRange(string key, long start, long end);
		string GetSet(string key, string value);
		long Incr(string key);
		long IncrBy(string key, long increment);
		List<string> MGet(string key);
		List<string> MGet(params string[] keys);
		string MSet(string key, string value);
		string MSet(params string[] keyValues);
		long MSetNx(string key, string value);
		long MSetNx(params string[] keyValues);
		string Set(string key, string value);
		long SetBit(string key, long offset, int value);
		string SetEx(string key, long seconds, string value);
		long SetNx(string key, string value);
		long SetRange(string key, long offset, string value);
		long Strlen(string key);

		// Hashes

		long HDel(string key, string field);
		long HDel(string key, params string[] fields);
		long HExists(string key, string field);
		string HGet(string key, string field);
		Dictionary<string, string> HGetAll(string key);
		long HIncrBy(string key, string field, long increment);
		List<string> HKeys(string key);
		long HLen(string key);
		List<string> HMGet(string key, string field);
		List<string> HMGet(string key, params string[] fields);
		string HMSet(string key, string field, string value);
		string HMSet(string key, params string[] fieldValues);
		long HSet(string key, string field, string value);
		long HSetNx(string key, string field, string value);
		List<string> HVals(string key);

		// Lists

		List<string> BLPop(string key, long timeout);
		List<string> BLPop(string[] keys, long timeout);
		List<string> BRPop(string key, long timeout);
		List<string> BRPop(string[] keys, long timeout);
		string BRPopLPush(string source, string destination, long timeout);
		string LIndex(string key, long index);
		long LInsert(string key, InsertOption option, string pivot, string value);
		long LLen(string key);
		string LPop(string key);
		long LPush(string key, string value);
		long LPush(string key, params string[] values);
		long LPushX(string key, string value);
		List<string> LRange(string key, long start, long stop);
		long LRem(string key, long count, string value);
		string LSet(string key, long index, string value);
		string LTrim(string key, long start, long stop);
		string RPop(string key);
		string RPopLPush(string source, string destination);
		long RPush(string key, string value);
		long RPush(string key, params string[] values);
		long RPushX(string key, string value);
		long SAdd(string key, string member);

		// Sets

		long SAdd(string key, params string[] members);
		long SCard(string key);
		List<string> SDiff(string key);
		List<string> SDiff(params string[] keys);
		long SDiffStore(string destination, string key);
		long SDiffStore(string destination, params string[] keys);
		List<string> SInter(string key);
		List<string> SInter(params string[] keys);
		long SInterStore(string destination, string key);
		long SInterStore(string destination, params string[] keys);
		long SIsMember(string key, string member);
		List<string> SMembers(string key);
		long SMove(string source, string destination, string member);
		string SPop(string key);
		string SRandMember(string key);
		long SRem(string key, string member);
		long SRem(string key, params string[] members);
		List<string> SUnion(string key);
		List<string> SUnion(params string[] keys);
		long SUnionStore(string destination, string key);
		long SUnionStore(string destination, params string[] keys);

		// Sorted sets

		long ZAdd(string key, string score, string member);
		long ZAdd(string key, params string[] scoreMembers);
		long ZCard(string key);
		long ZCount(string key, string min, string max);
		string ZIncrBy(string key, string increment, string member);
		long ZInterStore(string destination, string[] keys, AggregateOption option = AggregateOption.Sum);
		long ZInterStore(string destination, string[] keys, string[] weights, AggregateOption option = AggregateOption.Sum);
		List<string> ZRange(string key, long start, long stop);
		List<string> ZRangeWithScores(string key, long start, long stop);
		List<string> ZRangeByScore(string key, string min, string max);
		List<string> ZRangeByScoreWithScores(string key, string min, string max);
		List<string> ZRangeByScore(string key, string min, string max, long offset, long count);
		List<string> ZRangeByScoreWithScores(string key, string min, string max, long offset, long count);
		long? ZRank(string key, string member);
		long ZRem(string key, string member);
		long ZRem(string key, params string[] members);
		long ZRemRangeByRank(string key, long start, long stop);
		long ZRemRangeByScore(string key, string min, string max);
		List<string> ZRevRange(string key, long start, long stop);
		List<string> ZRevRangeWithScores(string key, long start, long stop);
		List<string> ZRevRangeByScore(string key, string min, string max);
		List<string> ZRevRangeByScoreWithScores(string key, string min, string max);
		List<string> ZRevRangeByScore(string key, string min, string max, long offset, long count);
		List<string> ZRevRangeByScoreWithScores(string key, string min, string max, long offset, long count);
		long? ZRevRank(string key, string member);
		string ZScore(string key, string member);
		long ZUnionStore(string destination, string[] keys);
		long ZUnionStore(string destination, string[] keys, AggregateOption option);
		long ZUnionStore(string destination, string[] keys, string[] weights);
		long ZUnionStore(string destination, string[] keys, string[] weights, AggregateOption option);
	}
}