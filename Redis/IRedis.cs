// Copyright 2012 Peter Townsend
// Licensed under the MIT License 

namespace Redis
{
	public interface IRedis : IRedisCommands
	{
		// Transactions

		string Unwatch();
		string Watch(string key, params string[] keys);
		
		IRedisPipeline CreatePipeline(bool transactioned = false);

		// Connection

		string Auth(string password);
		string Echo(string message);
		string Ping();
		string Quit();
		string Select(long index);

		// Server

		string BGRewriteAOF();
		string BGSave();
		long DBSize();
		string FlushAll();
		string FlushDB();
		string Info();
		long LastSave();
	}
}