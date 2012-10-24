// Copyright 2012 Peter Townsend
// Licensed under the MIT License 

using System.Collections.Generic;

namespace Redis
{
	public interface IRedisConnection
	{
		RedisResponse Run(params string[] request);
		List<RedisResponse> Run(params string[][] requests);
	}
}
