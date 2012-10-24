// Copyright 2012 Peter Townsend
// Licensed under the MIT License 

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace Redis.Core
{
	public class RedisConnection : IRedisConnection
	{
		public const int DefaultPort = 6379;
		public const int DefaultDbIndex = 0;

		private const string CommandMark = "*";
		private const string ArgumentMark = "$";
		private const string EndOfLineMark = "\r\n";

		private static readonly UTF8Encoding BomlessUtf8Encoding = new UTF8Encoding(false);

		private readonly byte[] byteBuffer = new byte[1];
		private readonly char[] charBuffer = new char[1];
		private readonly Decoder decoder = Encoding.UTF8.GetDecoder();
		private readonly StringBuilder stringBuilder = new StringBuilder();

		private readonly string hostname;
		private readonly int port;
		private readonly int dbIndex;

		private Socket socket;
		private NetworkStream stream;
		private TextWriter writer;
		private bool reusable;

		public RedisConnection(string hostname, int port = DefaultPort, int dbIndex = DefaultDbIndex)
		{
			this.hostname = hostname;
			this.port = port;
			this.dbIndex = dbIndex;

			this.OpenConnection();
		}

		public bool IsReusable()
		{
			return this.reusable;
		}

		public RedisResponse Run(params string[] request)
		{
			if (request.Length == 0)
			{
				return null;
			}

			this.AttemptWriteRequests(request);

			return this.ReadResponse();
		}

		public List<RedisResponse> Run(params string[][] requests)
		{
			if (requests.Length == 0)
			{
				return new List<RedisResponse>();
			}

			this.AttemptWriteRequests(requests);

			var responses = requests.Select(x => this.ReadResponse()).ToList();

			return responses;
		}

		public void Dispose()
		{
			this.CloseConnection();
		}

		private void OpenConnection()
		{
			try
			{
				this.socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

				this.socket.Connect(this.hostname, this.port);

				this.reusable = this.socket.Connected;

				this.stream = new NetworkStream(this.socket);

				this.writer = new StreamWriter(this.stream, BomlessUtf8Encoding);

				this.Run("SELECT", this.dbIndex.ToString());
			}
			catch
			{
				this.reusable = false;

				throw;
			}
		}

		private void CloseConnection()
		{
			try
			{
				this.reusable = false;

				this.writer.Dispose();
				this.socket.Close();
			}
			catch
			{
			}
		}

		private RedisResponse ReadResponse()
		{
			var firstByte = this.stream.ReadByte();

			switch (firstByte)
			{
				case '+':
				{
					var value = this.ReadLine();
					return new RedisResponse { Value = value };
				}
				case '-':
				{
					var value = this.ReadLine();
					throw new RedisException(value);
				}
				case ':':
				{
					var value = this.ReadInteger();
					return new RedisResponse { Value = value.ToString() };
				}
				case '$':
				{
					var length = this.ReadInteger();
					var value = length >= 0 ? this.ReadLine(length) : null;
					return new RedisResponse { Value = value };
				}
				case '*':
				{
					var responses = this.ReadMultiBulkResponses();
					return new RedisResponse { Responses = responses };
				}
				default:
				{
					var log = string.Format("Unexpected first char in reply {0}", firstByte);
					throw new RedisException(log);
				}
			}
		}

		private List<RedisResponse> ReadMultiBulkResponses()
		{
			var count = this.ReadInteger();

			if (count == -1)
			{
				return null;
			}

			var responses = new List<RedisResponse>();

			for (int i = 0; i < count; i++)
			{
				responses.Add(this.ReadResponse());
			}

			return responses;
		}

		private void AttemptWriteRequests(params string[][] requests)
		{
			try
			{
				this.WriteRequests(requests);
			}
			catch (Exception)
			{
				this.CloseConnection();
				this.OpenConnection();

				this.WriteRequests(requests);

				throw;
			}
		}
		private void WriteRequests(params string[][] requests)
		{
			foreach (var request in requests)
			{
				if (request == null || request.Length == 0)
				{
					throw new RedisException("Invalid request. Empty command and arguments");
				}

				this.writer.Write(CommandMark);
				this.writer.Write(request.Length.ToString());
				this.writer.Write(EndOfLineMark);

				foreach (var argument in request)
				{
					if (argument == null)
					{
						throw new RedisException("Invalid request. NULL request parameter");
					}

					var argumentByteLength = Encoding.UTF8.GetByteCount(argument);

					this.writer.Write(ArgumentMark);
					this.writer.Write(argumentByteLength.ToString());
					this.writer.Write(EndOfLineMark);

					this.writer.Write(argument);
					this.writer.Write(EndOfLineMark);
				}
			}

			this.writer.Flush();
		}

		private void ReadChar(char value)
		{
			var x = this.stream.ReadByte();

			if (x != value)
			{
				var log = string.Format("Recieved unexpected char {0} instead of {1}", (char) x, value);
				throw new RedisException(log);
			}
		}

		private long ReadInteger()
		{
			var line = this.ReadLine();

			long value;

			if (!Int64.TryParse(line, out value))
			{
				var log = string.Format("Redis received unexpected integer value {0}", line);
				throw new RedisException(log);
			}

			return value;
		}

		private string ReadLine()
		{
			this.stringBuilder.Length = 0;

			this.decoder.Reset();

			while (true)
			{
				var x = this.stream.ReadByte();

				if (x == -1)
				{
					throw new RedisException("Unexpected end of stream");
				}

				if (x == '\r')
				{
					this.ReadChar('\n');

					break;
				}

				this.byteBuffer[0] = (byte)x;

				if (this.decoder.GetChars(this.byteBuffer, 0, 1, this.charBuffer, 0, false) > 0)
				{
					this.stringBuilder.Append(this.charBuffer[0]);
				}
			}

			return this.stringBuilder.ToString();
		}

		private string ReadLine(long length)
		{
			var i = 0;

			this.stringBuilder.Length = 0;
			this.decoder.Reset();

			while (i < length)
			{
				var x = this.stream.ReadByte();

				if (x == -1)
				{
					throw new RedisException("Unexpected end of stream");
				}

				this.byteBuffer[0] = (byte)x;

				if (this.decoder.GetChars(this.byteBuffer, 0, 1, this.charBuffer, 0, false) > 0)
				{
					this.stringBuilder.Append(this.charBuffer[0]);
				}

				i++;
			}

			this.ReadChar('\r');
			this.ReadChar('\n');

			return this.stringBuilder.ToString();
		}
	}
}