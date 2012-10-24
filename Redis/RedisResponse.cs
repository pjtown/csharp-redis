// Copyright 2012 Peter Townsend
// Licensed under the MIT License 

using System.Collections.Generic;
using System.Text;

namespace Redis
{
	public class RedisResponse
	{
		public string Value { get; set; }

		public List<RedisResponse> Responses { get; set; }

		public override string ToString()
		{
			if (this.Value != null)
			{
				return this.Value;
			}

			if (this.Responses == null || this.Responses.Count == 0)
			{
				return "nil";
			}

			var sb = new StringBuilder("[");

			for (int i = 0; i < this.Responses.Count; i++)
			{
				if (i > 0)
				{
					sb.Append(",");
				}

				var childValue = this.Responses[i];

				var childValueText = childValue != null ? childValue.ToString() : "nil";

				if (childValueText == "nil")
				{
					sb.Append(childValueText);
				}
				else if (childValueText.StartsWith("["))
				{
					sb.Append(childValueText);
				}
				else
				{
					sb.AppendFormat("\"{0}\"", childValueText);
				}
			}

			sb.Append("]");

			return sb.ToString();
		}
	}
}