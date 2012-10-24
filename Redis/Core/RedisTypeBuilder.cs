// Copyright 2012 Peter Townsend
// Licensed under the MIT License 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Redis.Core
{
	public static class RedisTypeBuilder
	{
		public const string AssemblyName = "Redis.Dynamic";
		public const string AssemblyFileName = "Redis.Dynamic.dll";

		public static RedisTypes BuildRedisTypes()
		{
			var dymnamicAssemblyName = new AssemblyName(AssemblyName);
			var assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(dymnamicAssemblyName, AssemblyBuilderAccess.RunAndSave);

			var moduleBuilder = assemblyBuilder.DefineDynamicModule(AssemblyFileName, AssemblyFileName);

			// Create RedisPipeline dynamic type
			var redisPipelineType = BuildRedisPipelineType(moduleBuilder);

			// Create Redis dynamic type
			var redisType = BuildRedisType(moduleBuilder, redisPipelineType);

			var redisTypes = new RedisTypes(assemblyBuilder, AssemblyFileName, redisType, redisPipelineType);
			
			return redisTypes;
		}

		public static void SaveAssembly(AssemblyBuilder assemblyBuilder)
		{
			assemblyBuilder.Save(AssemblyFileName);
		}

		private static Type BuildRedisType(ModuleBuilder moduleBuilder, Type redisPipelineType)
		{
			const TypeAttributes typeAttributes = TypeAttributes.Public | TypeAttributes.AutoClass | TypeAttributes.AnsiClass | TypeAttributes.BeforeFieldInit;
			const MethodAttributes constructorAttributes = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName;

			var typeBuilder = moduleBuilder.DefineType("Redis.Dynamic.RedisDynamic", typeAttributes, typeof(RedisBase), Type.EmptyTypes);

            // Emit constructor:
            // public RedisDynamic(IRedisConnection redisConnection)

			var constructorBuilder = typeBuilder.DefineConstructor(constructorAttributes, CallingConventions.Standard, new[] { typeof(IRedisConnection) });
			var redisConstructor = typeof(RedisBase).GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { typeof(IRedisConnection) }, null);

			var generator = constructorBuilder.GetILGenerator();

			generator.Emit(OpCodes.Ldarg_0);
			generator.Emit(OpCodes.Ldarg_1);
			generator.Emit(OpCodes.Call, redisConstructor);
			generator.Emit(OpCodes.Ret);

			// Emit all abstract RedisBase methods

			var redisCommandMethods = typeof(RedisBase).GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

			foreach (var redisCommandMethod in redisCommandMethods.Where(x => x.Name != "CreatePipeline" && x.IsAbstract))
			{
				EmitRedisCommandMethod(typeBuilder, redisCommandMethod);
			}

			// Emit CreatePipeline method

			EmitRedisPipelineMethod(typeBuilder, redisPipelineType);

			return typeBuilder.CreateType();
		}

		private static Type BuildRedisPipelineType(ModuleBuilder moduleBuilder)
		{
			const TypeAttributes typeAttributes = TypeAttributes.Public | TypeAttributes.AutoClass | TypeAttributes.AnsiClass | TypeAttributes.BeforeFieldInit;
			const MethodAttributes constructorAttributes = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName;

			var typeBuilder = moduleBuilder.DefineType("Redis.Dynamic.RedisPipelineDynamic", typeAttributes, typeof(RedisPipelineBase), Type.EmptyTypes);
			var constructorBuilder = typeBuilder.DefineConstructor(constructorAttributes, CallingConventions.Standard, new[] { typeof(IRedisConnection), typeof(bool) });
			var baseConstructor = typeof(RedisPipelineBase).GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { typeof(IRedisConnection), typeof(bool) }, null);

			var generator = constructorBuilder.GetILGenerator();

			generator.Emit(OpCodes.Ldarg_0);
			generator.Emit(OpCodes.Ldarg_1);
			generator.Emit(OpCodes.Ldarg_2);
			generator.Emit(OpCodes.Call, baseConstructor);
			generator.Emit(OpCodes.Ret);

			// Define RedisPipeline methods

			var redisCommandMethods = typeof(RedisPipelineBase).GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

			foreach (var redisMethod in redisCommandMethods.Where(x => x.IsAbstract))
			{
				EmitRedisCommandPipelineMethod(typeBuilder, redisMethod);
			}

			return typeBuilder.CreateType();
		}

		private static void EmitRedisCommandMethod(TypeBuilder typeBuilder, MethodInfo redisCommandMethod)
		{
			var methodName = redisCommandMethod.Name;
			var methodParameters = redisCommandMethod.GetParameters();
			var methodReturnType = redisCommandMethod.ReturnType;

			var methodAttributes = redisCommandMethod.IsPublic ? MethodAttributes.Public : MethodAttributes.Family;
			methodAttributes |= MethodAttributes.HideBySig | MethodAttributes.Virtual;

			var methodBuilder = typeBuilder.DefineMethod(methodName, methodAttributes, redisCommandMethod.CallingConvention, methodReturnType, methodParameters.Select(x => x.ParameterType).ToArray());

			for (int i = 0; i < methodParameters.Length; i++)
			{
				methodBuilder.DefineParameter(i + 1, ParameterAttributes.None, methodParameters[i].Name);
			}

			var generator = methodBuilder.GetILGenerator();

			// Declare local variables
			var requestLocal = generator.DeclareLocal(typeof(string[])); // loc_0
			var responseLocal = generator.DeclareLocal(typeof(RedisResponse)); // loc_1
			var returnLocal = generator.DeclareLocal(methodReturnType); // loc_2

			// request = new string['x']
			InitializeRequestArray(generator, methodParameters);

			// request[0] = 'commandName'
			generator.Emit(OpCodes.Ldloc, requestLocal.LocalIndex);
			generator.Emit(OpCodes.Ldc_I4_0);
			generator.Emit(OpCodes.Ldstr, methodName.ToUpper());
			generator.Emit(OpCodes.Stelem_Ref);

			if (methodParameters.Any(x => x.ParameterType == typeof(string[])))
			{
				PopulateRequestArrayFromArrayParameters(generator, methodParameters);
			}
			else
			{
				PopulateRequestArrayFromSingleParameters(generator, methodParameters);
			}

			var connectionField = typeof(RedisBase).GetField("Connection", BindingFlags.NonPublic | BindingFlags.Instance);
			var connectionRunMethod = typeof(IRedisConnection).GetMethod("Run", new[] { typeof(string[]) });

			// response = this.RedisConnection.Run(arguments)
			generator.Emit(OpCodes.Ldarg_0);
			generator.Emit(OpCodes.Ldfld, connectionField);
			generator.Emit(OpCodes.Ldloc, requestLocal.LocalIndex);
			generator.Emit(OpCodes.Call, connectionRunMethod);
			generator.Emit(OpCodes.Stloc, responseLocal.LocalIndex);

			SetReturnFromResponse(generator, returnLocal, responseLocal);

			generator.Emit(OpCodes.Ldloc, returnLocal.LocalIndex);
			generator.Emit(OpCodes.Ret);
		}

		private static void EmitRedisCommandPipelineMethod(TypeBuilder typeBuilder, MethodInfo redisCommandMethod)
		{
			var methodName = redisCommandMethod.Name;
			var methodParameters = redisCommandMethod.GetParameters();
			var methodReturnType = redisCommandMethod.ReturnType;

			var methodAttributes = redisCommandMethod.IsPublic ? MethodAttributes.Public : MethodAttributes.Family;
			methodAttributes |= MethodAttributes.HideBySig | MethodAttributes.Virtual;

			var methodBuilder = typeBuilder.DefineMethod(methodName, methodAttributes, redisCommandMethod.CallingConvention, methodReturnType, methodParameters.Select(x => x.ParameterType).ToArray());

			for (int i = 0; i < methodParameters.Length; i++)
			{
				methodBuilder.DefineParameter(i + 1, ParameterAttributes.None, methodParameters[i].Name);
			}

			var generator = methodBuilder.GetILGenerator();

			// Declare local variables
			generator.DeclareLocal(typeof(string[])); // loc_0
			generator.DeclareLocal(methodReturnType); // loc_1

			// request = new string['x']
			InitializeRequestArray(generator, methodParameters);

			// request[0] = 'commandName'
			generator.Emit(OpCodes.Ldloc_0);
			generator.Emit(OpCodes.Ldc_I4_0);
			generator.Emit(OpCodes.Ldstr, methodName.ToUpper());
			generator.Emit(OpCodes.Stelem_Ref);

			if (methodParameters.Any(x => x.ParameterType == typeof(string[])))
			{
				PopulateRequestArrayFromArrayParameters(generator, methodParameters);
			}
			else
			{
				PopulateRequestArrayFromSingleParameters(generator, methodParameters);
			}

			var pipelineRequestsField = typeof(RedisPipelineBase).GetField("PipelineRequests", BindingFlags.NonPublic | BindingFlags.Instance);
			var stringArrayListAddMethod = typeof(List<string[]>).GetMethod("Add", BindingFlags.Public | BindingFlags.Instance);

			// this.QueuedRequests.Add(request)
			generator.Emit(OpCodes.Ldarg_0);
			generator.Emit(OpCodes.Ldfld, pipelineRequestsField);
			generator.Emit(OpCodes.Ldloc_0);
			generator.Emit(OpCodes.Call, stringArrayListAddMethod);

			// Return default 'result'
			generator.Emit(OpCodes.Ldloc_1);
			generator.Emit(OpCodes.Ret);
		}

		private static void EmitRedisPipelineMethod(TypeBuilder typeBuilder, Type redisPipelineType)
		{
			var methodAttributes = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Virtual;
			var callingConvention = CallingConventions.Standard | CallingConventions.HasThis;

			var methodBuilder = typeBuilder.DefineMethod("CreatePipeline", methodAttributes, callingConvention, typeof(IRedisPipeline), new [] { typeof(bool) });

			methodBuilder.DefineParameter(1, ParameterAttributes.None, "transactioned");

			var generator = methodBuilder.GetILGenerator();

			var connectionField = typeof(RedisBase).GetField("Connection", BindingFlags.NonPublic | BindingFlags.Instance);
			var redisPipelineConstructor = redisPipelineType.GetConstructor(new[] { typeof(IRedisConnection), typeof(bool) });

			// return new RedisPipeline(this.Connection, transactioned)
			generator.Emit(OpCodes.Ldarg_0);
			generator.Emit(OpCodes.Ldfld, connectionField);
			generator.Emit(OpCodes.Ldarg_1);
			generator.Emit(OpCodes.Newobj, redisPipelineConstructor);
			generator.Emit(OpCodes.Ret);
		}

		private static void InitializeRequestArray(ILGenerator generator, ParameterInfo[] methodParameters)
		{
			var singleArgumentParameters = methodParameters.Count(x => x.ParameterType != typeof(string[]));

			generator.Emit(OpCodes.Ldc_I4, singleArgumentParameters + 1);

			for (int i = 0; i < methodParameters.Length; i++)
			{
				if (methodParameters[i].ParameterType == typeof(string[]))
				{
					generator.Emit(OpCodes.Ldarg, i + 1);
					generator.Emit(OpCodes.Ldlen);
					generator.Emit(OpCodes.Conv_I4);
					generator.Emit(OpCodes.Add);
				}
			}

			generator.Emit(OpCodes.Newarr, typeof(string));
			generator.Emit(OpCodes.Stloc_0);
		}

		private static void PopulateRequestArrayFromSingleParameters(ILGenerator generator, ParameterInfo[] methodParameters)
		{
			for (int i = 0; i < methodParameters.Length; i++)
			{
				var parameter = methodParameters[i];
				var parameterType = parameter.ParameterType;
				var parameterIndex = i + 1;
				var requestArrayIndex = i + 1;

				if (parameterType == typeof(string))
				{
					// request[i + 1] = parameter
					generator.Emit(OpCodes.Ldloc_0);
					generator.Emit(OpCodes.Ldc_I4, requestArrayIndex);
					generator.Emit(OpCodes.Ldarg, parameterIndex);
					generator.Emit(OpCodes.Stelem_Ref);
				}
				else if (parameterType == typeof(long) || parameterType == typeof(int))
				{
					var parameterToStringMethod = parameterType.GetMethod("ToString", Type.EmptyTypes);

					// request[i + 1] = parameter.ToString()
					generator.Emit(OpCodes.Ldloc_0);
					generator.Emit(OpCodes.Ldc_I4, requestArrayIndex);
					generator.Emit(OpCodes.Ldarga, parameterIndex);
					generator.Emit(OpCodes.Call, parameterToStringMethod);
					generator.Emit(OpCodes.Stelem_Ref);
				}
				else
				{
					var log = string.Format("Unsupported Redis method parameter {0} '{1}'", parameterType.Name, parameter.Name);
					throw new RedisException(log);
				}
			}
		}

		private static void PopulateRequestArrayFromArrayParameters(ILGenerator generator, ParameterInfo[] methodParameters)
		{
			var requestArrayIndexLocal = generator.DeclareLocal(typeof(int));

			// requestArrayIndex = x
			generator.Emit(OpCodes.Ldc_I4_1);
			generator.Emit(OpCodes.Stloc, requestArrayIndexLocal.LocalIndex);

			for (int i = 0; i < methodParameters.Length; i++)
			{
				var parameter = methodParameters[i];
				var parameterType = parameter.ParameterType;
				var parameterIndex = i + 1;

				if (parameterType == typeof(string))
				{
					// request[requestArrayIndex] = parameter
					generator.Emit(OpCodes.Ldloc_0);
					generator.Emit(OpCodes.Ldloc, requestArrayIndexLocal.LocalIndex);
					generator.Emit(OpCodes.Ldarg, parameterIndex);
					generator.Emit(OpCodes.Stelem_Ref);

					// requestArrayIndex++
					generator.Emit(OpCodes.Ldloc, requestArrayIndexLocal.LocalIndex);
					generator.Emit(OpCodes.Ldc_I4_1);
					generator.Emit(OpCodes.Add);
					generator.Emit(OpCodes.Stloc, requestArrayIndexLocal.LocalIndex);
				}
				else if (parameterType == typeof(long) || parameterType == typeof(int))
				{
					var parameterToStringMethod = parameterType.GetMethod("ToString", Type.EmptyTypes);

					// request[requestArrayIndex] = parameter.ToString()
					generator.Emit(OpCodes.Ldloc_0);
					generator.Emit(OpCodes.Ldloc, requestArrayIndexLocal.LocalIndex);
					generator.Emit(OpCodes.Ldarga, parameterIndex);
					generator.Emit(OpCodes.Call, parameterToStringMethod);
					generator.Emit(OpCodes.Stelem_Ref);

					// requestArrayIndex++
					generator.Emit(OpCodes.Ldloc, requestArrayIndexLocal.LocalIndex);
					generator.Emit(OpCodes.Ldc_I4_1);
					generator.Emit(OpCodes.Add);
					generator.Emit(OpCodes.Stloc, requestArrayIndexLocal.LocalIndex);
				}
				else if (parameterType == typeof(string[]))
				{
					var indexLocal = generator.DeclareLocal(typeof(int));

					var loopStart = generator.DefineLabel();
					var loopEnd = generator.DefineLabel();

					// int i = 0
					generator.Emit(OpCodes.Ldc_I4_0);
					generator.Emit(OpCodes.Stloc, indexLocal.LocalIndex);
					generator.Emit(OpCodes.Br, loopEnd);

					generator.MarkLabel(loopStart);

					// request[requestWriteIndex] = parameter[i]
					generator.Emit(OpCodes.Ldloc_0);
					generator.Emit(OpCodes.Ldloc, requestArrayIndexLocal.LocalIndex);
					generator.Emit(OpCodes.Ldarg, parameterIndex);
					generator.Emit(OpCodes.Ldloc, indexLocal.LocalIndex);
					generator.Emit(OpCodes.Ldelem_Ref);
					generator.Emit(OpCodes.Stelem_Ref);

					// requestArrayIndex++
					generator.Emit(OpCodes.Ldloc, requestArrayIndexLocal.LocalIndex);
					generator.Emit(OpCodes.Ldc_I4_1);
					generator.Emit(OpCodes.Add);
					generator.Emit(OpCodes.Stloc, requestArrayIndexLocal.LocalIndex);

					// i++
					generator.Emit(OpCodes.Ldloc, indexLocal.LocalIndex);
					generator.Emit(OpCodes.Ldc_I4_1);
					generator.Emit(OpCodes.Add);
					generator.Emit(OpCodes.Stloc, indexLocal.LocalIndex);

					generator.MarkLabel(loopEnd);

					// i < parameter.Length
					generator.Emit(OpCodes.Ldloc, indexLocal.LocalIndex);
					generator.Emit(OpCodes.Ldarg, parameterIndex);
					generator.Emit(OpCodes.Ldlen);
					generator.Emit(OpCodes.Clt);
					generator.Emit(OpCodes.Brtrue, loopStart);
				}
			}
		}

		private static void SetReturnFromResponse(ILGenerator generator, LocalBuilder returnLocal, LocalBuilder responseLocal)
		{
			if (returnLocal.LocalType == typeof(string))
			{
				// result = response.Value
				generator.Emit(OpCodes.Ldloc, responseLocal.LocalIndex);
				generator.Emit(OpCodes.Call, typeof(RedisResponse).GetMethod("get_Value", Type.EmptyTypes));
				generator.Emit(OpCodes.Stloc, returnLocal.LocalIndex);
			}
			else if (returnLocal.LocalType == typeof(int))
			{
				// result = Int32Result.GetValue(response)
				generator.Emit(OpCodes.Ldloc, responseLocal.LocalIndex);
				generator.Emit(OpCodes.Call, typeof(Int32Result).GetMethod("GetValue", BindingFlags.Public | BindingFlags.Static));
				generator.Emit(OpCodes.Stloc, returnLocal.LocalIndex);
			}
			else if (returnLocal.LocalType == typeof(long))
			{
				// result = Int64Result.GetValue(response)
				generator.Emit(OpCodes.Ldloc, responseLocal.LocalIndex);
				generator.Emit(OpCodes.Call, typeof(Int64Result).GetMethod("GetValue", BindingFlags.Public | BindingFlags.Static));
				generator.Emit(OpCodes.Stloc, returnLocal.LocalIndex);
			}
			else if (returnLocal.LocalType == typeof(int?))
			{
				// result = NullableInt32Result.GetValue(response)
				generator.Emit(OpCodes.Ldloc, responseLocal.LocalIndex);
				generator.Emit(OpCodes.Call, typeof(NullableInt32Result).GetMethod("GetValue", BindingFlags.Public | BindingFlags.Static));
				generator.Emit(OpCodes.Stloc, returnLocal.LocalIndex);
			}
			else if (returnLocal.LocalType == typeof(long?))
			{
				// result = NullableInt64Result.GetValue(response)
				generator.Emit(OpCodes.Ldloc, responseLocal.LocalIndex);
				generator.Emit(OpCodes.Call, typeof(NullableInt64Result).GetMethod("GetValue", BindingFlags.Public | BindingFlags.Static));
				generator.Emit(OpCodes.Stloc, returnLocal.LocalIndex);
			}
			else if (returnLocal.LocalType == typeof(List<string>))
			{
				// result = ListResult.GetValue(response)
				generator.Emit(OpCodes.Ldloc, responseLocal.LocalIndex);
				generator.Emit(OpCodes.Call, typeof(ListResult).GetMethod("GetValue", BindingFlags.Public | BindingFlags.Static));
				generator.Emit(OpCodes.Stloc, returnLocal.LocalIndex);
			}
			else if (returnLocal.LocalType == typeof(Dictionary<string, string>))
			{
				// result = DictionaryResult.GetValue(response)
				generator.Emit(OpCodes.Ldloc, responseLocal.LocalIndex);
				generator.Emit(OpCodes.Call, typeof(DictionaryResult).GetMethod("GetValue", BindingFlags.Public | BindingFlags.Static));
				generator.Emit(OpCodes.Stloc, returnLocal.LocalIndex);
			}
			else
			{
				var log = string.Format("Unsupported Redis return type '{0}'", returnLocal.LocalType.Name);
				throw new RedisException(log);
			}
		}
	}
}