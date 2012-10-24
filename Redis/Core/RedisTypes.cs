// Copyright 2012 Peter Townsend
// Licensed under the MIT License 

using System;
using System.Reflection.Emit;

namespace Redis.Core
{
	public class RedisTypes
	{
		public RedisTypes(AssemblyBuilder assemblyBuilder, string assemblyFileName, Type redisType, Type redisPipelneType)
		{
			this.Assembly = assemblyBuilder;
			this.AssemblyFileName = assemblyFileName;
			this.RedisType = redisType;
			this.RedisPipelineType = redisPipelneType;
		}

		public AssemblyBuilder Assembly { get; private set; }

		public string AssemblyName
		{
			get { return this.Assembly.GetName().Name; }
		}

		public string AssemblyFileName { get; private set; }

		public Type RedisType { get; private set; }

		public Type RedisPipelineType { get; private set; }

		public void SaveAssembly()
		{
			this.Assembly.Save(this.AssemblyFileName);
		}
	}
}