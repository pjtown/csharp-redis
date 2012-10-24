// Copyright 2012 Peter Townsend
// Licensed under the MIT License 

namespace Redis.Core
{
	public abstract class RedisBase : RedisCommandsBase, IRedis
	{
		protected readonly IRedisConnection Connection;

		protected RedisBase(IRedisConnection connection)
		{
			this.Connection = connection;
		}

		public abstract IRedisPipeline CreatePipeline(bool transactioned = false);

		// Transactions

		public abstract string Unwatch();
		public abstract string Watch(string key, params string[] keys);

		// Connection

		public abstract string Auth(string password);
		public abstract string Echo(string message);
		public abstract string Ping();
		public abstract string Quit();
		public abstract string Select(long index);

		// Server

		public abstract string BGRewriteAOF();
		public abstract string BGSave();
		public abstract long DBSize();
		public abstract string FlushAll();
		public abstract string FlushDB();
		public abstract string Info();
		public abstract long LastSave();
	}
}