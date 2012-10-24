// Copyright 2012 Peter Townsend
// Licensed under the MIT License 

namespace Redis
{
	public interface IRedisFactory
	{
		IRedis CreateRedis(IRedisConnection connection);
		IRedisPipeline CreateRedisPipeline(IRedisConnection connection, bool transactioned);
	}
}