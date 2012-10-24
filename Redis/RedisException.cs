// Copyright 2012 Peter Townsend
// Licensed under the MIT License 

using System;
using System.Runtime.Serialization;

namespace Redis
{
	public class RedisException : Exception
	{
		public RedisException()
		{
		}

		public RedisException(string message) : base(message)
		{
		}

		public RedisException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected RedisException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}