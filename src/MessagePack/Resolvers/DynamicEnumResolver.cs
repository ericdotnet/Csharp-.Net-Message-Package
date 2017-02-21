﻿using System;
using MessagePack.Formatters;
using MessagePack.Internal;
using System.Reflection;
using System.Reflection.Emit;

namespace MessagePack.Resolvers
{
    /// <summary>
    /// EnumResolver by dynamic code generation, serialized underlying type.
    /// </summary>
    public class DynamicEnumResolver : IFormatterResolver
    {
        public static IFormatterResolver Instance = new DynamicEnumResolver();

        const string ModuleName = "MessagePack.Resolvers.DynamicEnumResolver";

        static readonly DynamicAssembly assembly;

        DynamicEnumResolver()
        {

        }

        static DynamicEnumResolver()
        {
            assembly = new DynamicAssembly(ModuleName);
        }

        IMessagePackFormatter<T> IFormatterResolver.GetFormatter<T>()
        {
            return FormatterCache<T>.formatter;
        }

        static class FormatterCache<T>
        {
            public static readonly IMessagePackFormatter<T> formatter;

            static FormatterCache()
            {
                var ti = typeof(T).GetTypeInfo();
                if (ti.IsNullable())
                {
                    // build underlying type and use wrapped formatter.
                    ti = ti.GenericTypeArguments[0].GetTypeInfo();
                    if (!ti.IsEnum)
                    {
                        return;
                    }

                    var innerFormatter = DynamicEnumResolver.Instance.GetFormatterDynamic(ti.AsType());
                    if (innerFormatter == null)
                    {
                        return;
                    }
                    formatter = (IMessagePackFormatter<T>)Activator.CreateInstance(typeof(StaticNullableFormatter<>).MakeGenericType(ti.AsType()), new object[] { innerFormatter });
                    return;
                }
                else if (!ti.IsEnum)
                {
                    return;
                }

                var formatterTypeInfo = BuildType(typeof(T));
                formatter = (IMessagePackFormatter<T>)Activator.CreateInstance(formatterTypeInfo.AsType());
            }
        }

        static TypeInfo BuildType(Type enumType)
        {
            var ti = enumType.GetTypeInfo();

            var underlyingType = Enum.GetUnderlyingType(enumType);
            var formatterType = typeof(IMessagePackFormatter<>).MakeGenericType(enumType);

            var typeBuilder = assembly.ModuleBuilder.DefineType("MessagePack.Formatters." + enumType.FullName.Replace(".", "_") + "Formatter", TypeAttributes.Public, null, new[] { formatterType });

            // int Serialize(ref byte[] bytes, int offset, T value, IFormatterResolver formatterResolver);
            {
                var method = typeBuilder.DefineMethod("Serialize", MethodAttributes.Public | MethodAttributes.Final | MethodAttributes.Virtual,
                    typeof(int),
                    new Type[] { typeof(byte[]).MakeByRefType(), typeof(int), enumType, typeof(IFormatterResolver) });

                var il = method.GetILGenerator();
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Ldarg_2);
                il.Emit(OpCodes.Ldarg_3);
                il.Emit(OpCodes.Call, typeof(MessagePackBinary).GetTypeInfo().GetDeclaredMethod("Write" + underlyingType.Name));
                il.Emit(OpCodes.Ret);
            }

            // T Deserialize(byte[] bytes, int offset, IFormatterResolver formatterResolver, out int readSize);
            {
                var method = typeBuilder.DefineMethod("Deserialize", MethodAttributes.Public | MethodAttributes.Final | MethodAttributes.Virtual,
                    enumType,
                    new Type[] { typeof(byte[]), typeof(int), typeof(IFormatterResolver), typeof(int).MakeByRefType() });

                var il = method.GetILGenerator();
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Ldarg_2);
                il.Emit(OpCodes.Ldarg_S, (byte)4);
                il.Emit(OpCodes.Call, typeof(MessagePackBinary).GetTypeInfo().GetDeclaredMethod("Read" + underlyingType.Name));
                il.Emit(OpCodes.Ret);
            }

            return typeBuilder.CreateTypeInfo();
        }
    }
}