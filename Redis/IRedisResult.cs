// Copyright 2012 Peter Townsend
// Licensed under the MIT License 

namespace Redis
{
	public interface IRedisResult<TValue>
	{
		TValue Value { get; set; }
	}
}