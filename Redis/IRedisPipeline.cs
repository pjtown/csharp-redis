// Copyright 2012 Peter Townsend
// Licensed under the MIT License 

using System;

namespace Redis
{
	public interface IRedisPipeline : IDisposable
	{
		IRedisResult<TValue> Add<TValue>(Func<IRedisCommands, TValue> command);

		bool Commit();

		void Discard();
	}
}