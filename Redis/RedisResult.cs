// Copyright 2012 Peter Townsend
// Licensed under the MIT License 

using System;
using System.Collections.Generic;

namespace Redis
{
	public abstract class RedisResult
	{
		public abstract void Apply(RedisResponse response);
	}

	public class RedisResult<TValue> : RedisResult, IRedisResult<TValue>
	{
		private readonly Func<RedisResponse, TValue> getValueAction;

		public RedisResult(Func<RedisResponse, TValue> getValueAction)
		{
			this.getValueAction = getValueAction;
		}

		public TValue Value { get; set; }

		public override void Apply(RedisResponse response)
		{
			this.Value = this.getValueAction(response);
		}
	}

	public class StringResult : RedisResult<string>
	{
		public StringResult() 
			: base(GetValue)
		{
		}

		public static string GetValue(RedisResponse response)
		{
			return response.Value;
		}
	}

	public class Int32Result : RedisResult<int>
	{
		public Int32Result()
			: base(GetValue)
		{
		}

		public static int GetValue(RedisResponse response)
		{
			return Int32.Parse(response.Value ?? "0");
		}
	}
	
	public class NullableInt32Result : RedisResult<int?>
	{
		public NullableInt32Result()
			: base(GetValue)
		{
		}

		public static int? GetValue(RedisResponse response)
		{
			int value;
			return Int32.TryParse(response.Value, out value) ? value : (int?) null;
		}
	}

	public class Int64Result : RedisResult<long>
	{
		public Int64Result() 
			: base(GetValue)
		{
		}

		public static long GetValue(RedisResponse response)
		{
			return Int64.Parse(response.Value ?? "0");
		}
	}

	public class NullableInt64Result : RedisResult<long?>
	{
		public NullableInt64Result()
			: base(GetValue)
		{
		}

		public static long? GetValue(RedisResponse response)
		{
			long value;
			return Int64.TryParse(response.Value, out value) ? value : (long?) null;
		}
	}

	public class ListResult : RedisResult<List<string>>
	{
		public ListResult() 
			: base(GetValue)
		{
		}

		public static List<string> GetValue(RedisResponse response)
		{
			if (response.Responses == null)
			{
				return null;
			}

			return response.Responses.ConvertAll(x => x.Value);
		}
	}

	public class DictionaryResult : RedisResult<Dictionary<string, string>>
	{
		public DictionaryResult() 
			: base(GetValue)
		{
		}

		public static Dictionary<string, string> GetValue(RedisResponse response)
		{
			var dictionary = new Dictionary<string, string>();

			if (response.Responses == null)
			{
				return null;
			}

			var keyValue = response.Responses.ConvertAll(x => x.Value);

			for (int i = 1; i < keyValue.Count; i += 2)
			{
				var key = keyValue[i - 1];
				var value = keyValue[i];

				dictionary[key] = value;
			}

			return dictionary;
		}
	}
}