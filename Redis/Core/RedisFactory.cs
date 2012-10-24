// Copyright 2012 Peter Townsend
// Licensed under the MIT License 

using System.Reflection.Emit;

namespace Redis.Core
{
	public class RedisFactory : IRedisFactory
	{
		private delegate IRedis CreateRedisDelegate(IRedisConnection redisConnection);
		private delegate IRedisPipeline CreateRedisPipelineDelegate(IRedisConnection redisConnection, bool transactioned);

		private readonly CreateRedisDelegate createRedis;
		private readonly CreateRedisPipelineDelegate createRedisPipeline;

		public RedisFactory(bool saveDynamicAssembly = false)
		{
			var redisTypes = RedisTypeBuilder.BuildRedisTypes();

			if (saveDynamicAssembly)
			{
				redisTypes.SaveAssembly();
			}

			// Build CreateRedis dynamic method

			var createRedisDynamicMethod = new DynamicMethod("CreateRedis", typeof(IRedis), new[] { typeof(IRedisConnection) });
			var redisConstructor = redisTypes.RedisType.GetConstructor(new[] { typeof(IRedisConnection) });
			
			var createRedisGenerator = createRedisDynamicMethod.GetILGenerator();
			createRedisGenerator.Emit(OpCodes.Ldarg_0);
			createRedisGenerator.Emit(OpCodes.Newobj, redisConstructor);
			createRedisGenerator.Emit(OpCodes.Ret);

			// Build CreateRedisExec dynamic method

			var createRedisPipelineDynamicMethod = new DynamicMethod("CreateRedisPipeline", typeof(IRedisPipeline), new[] { typeof(IRedisConnection), typeof(bool) });
			var redisPipelineConstructor = redisTypes.RedisPipelineType.GetConstructor(new[] { typeof(IRedisConnection), typeof(bool) });

			var createRedisPipelineGenerator = createRedisPipelineDynamicMethod.GetILGenerator();
			createRedisPipelineGenerator.Emit(OpCodes.Ldarg_0);
			createRedisPipelineGenerator.Emit(OpCodes.Ldarg_1);
			createRedisPipelineGenerator.Emit(OpCodes.Newobj, redisPipelineConstructor);
			createRedisPipelineGenerator.Emit(OpCodes.Ret);

			// Create delegates from dynamic methods

			this.createRedis = (CreateRedisDelegate) createRedisDynamicMethod.CreateDelegate(typeof (CreateRedisDelegate));
			this.createRedisPipeline = (CreateRedisPipelineDelegate)createRedisPipelineDynamicMethod.CreateDelegate(typeof(CreateRedisPipelineDelegate));
		}

		public IRedis CreateRedis(IRedisConnection connection)
		{
			return this.createRedis(connection);
		}

		public IRedisPipeline CreateRedisPipeline(IRedisConnection connection, bool transactioned)
		{
			return this.createRedisPipeline(connection, transactioned);
		}
	}
}