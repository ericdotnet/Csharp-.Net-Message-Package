﻿using MessagePack.Formatters;
using MessagePack.Resolvers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace MessagePack.Tests
{
    public class PrimitivelikeResolver : IFormatterResolver
    {
        public IMessagePackFormatter<T> GetFormatter<T>()
        {
            if (typeof(T) == typeof(string)) return (IMessagePackFormatter<T>)(object)new DummyStringFormatter();
            if (typeof(T) == typeof(DateTime)) return (IMessagePackFormatter<T>)(object)new DummyDateTimeFormatter();
            if (typeof(T) == typeof(byte[])) return (IMessagePackFormatter<T>)(object)new DummyBinaryFormatter();
            return StandardResolver.Instance.GetFormatter<T>();
        }
    }

    public class DummyStringFormatter : IMessagePackFormatter<string>
    {
        public string Deserialize(byte[] bytes, int offset, IFormatterResolver formatterResolver, out int readSize)
        {
            throw new NotImplementedException();
        }

        public int Serialize(ref byte[] bytes, int offset, string value, IFormatterResolver formatterResolver)
        {
            throw new NotImplementedException();
        }
    }

    public class DummyDateTimeFormatter : IMessagePackFormatter<DateTime>
    {
        public DateTime Deserialize(byte[] bytes, int offset, IFormatterResolver formatterResolver, out int readSize)
        {
            throw new NotImplementedException();
        }

        public int Serialize(ref byte[] bytes, int offset, DateTime value, IFormatterResolver formatterResolver)
        {
            throw new NotImplementedException();
        }
    }

    public class DummyBinaryFormatter : IMessagePackFormatter<byte[]>
    {
        public byte[] Deserialize(byte[] bytes, int offset, IFormatterResolver formatterResolver, out int readSize)
        {
            throw new NotImplementedException();
        }

        public int Serialize(ref byte[] bytes, int offset, byte[] value, IFormatterResolver formatterResolver)
        {
            throw new NotImplementedException();
        }
    }

    [MessagePackObject]
    public class MyDateTimeResolverTest1
    {
        [Key(0)]
        public DateTime MyProperty1 { get; set; }
    }

    [MessagePackObject]
    public class MyDateTimeResolverTest2
    {
        [Key(0)]
        public string MyProperty1 { get; set; }
    }

    [MessagePackObject]
    public class MyDateTimeResolverTest3
    {
        [Key(0)]
        public byte[] MyProperty1 { get; set; }
    }

    public class PrimitivelikeFormatterTest
    {
        [Fact]
        public void CanResolve()
        {
            var resolver = new PrimitivelikeResolver();

            Assert.Throws<NotImplementedException>(() =>
            {
                MessagePackSerializer.Serialize(new MyDateTimeResolverTest1() { MyProperty1 = DateTime.Now }, resolver);
            });

            Assert.Throws<NotImplementedException>(() =>
            {
                MessagePackSerializer.Serialize(new MyDateTimeResolverTest2() { MyProperty1 = "aaa" }, resolver);
            });

            Assert.Throws<NotImplementedException>(() =>
            {
                MessagePackSerializer.Serialize(new MyDateTimeResolverTest3() { MyProperty1 = new byte[] { 0, 1, 2 } }, resolver);
            });
        }

        [Fact]
        public void NativeDateTime()
        {
            var referenceContext = new MsgPack.Serialization.SerializationContext(MsgPack.PackerCompatibilityOptions.Classic)
            {
                DefaultDateTimeConversionMethod = MsgPack.Serialization.DateTimeConversionMethod.Native,
            };

            var now = DateTime.Now;

            var serializer = referenceContext.GetSerializer<DateTime>();

            var a = MessagePackSerializer.Serialize(now, NativeDateTimeResolver.Instance);
            var b = serializer.PackSingleObject(now);

            a.Is(b);

            var dt1 = MessagePackSerializer.Deserialize<DateTime>(a, NativeDateTimeResolver.Instance);
            var dt2 = serializer.UnpackSingleObject(b);

            dt1.Is(dt2);
        }

        [Fact]
        public void OldSpecString()
        {
            var referenceContext = new MsgPack.Serialization.SerializationContext(MsgPack.PackerCompatibilityOptions.Classic)
            {
                DefaultDateTimeConversionMethod = MsgPack.Serialization.DateTimeConversionMethod.Native,
            };

            var data = "あいうえおabcdefgかきくけこあいうえおabcdefgかきくけこあいうえおabcdefgかきくけこあいうえおabcdefgかきくけこ"; // Japanese

            var serializer = referenceContext.GetSerializer<string>();

            var a = MessagePackSerializer.Serialize(data, OldSpecResolver.Instance);
            var b = serializer.PackSingleObject(data);

            a.Is(b);

            var r1 = MessagePackSerializer.Deserialize<string>(a, OldSpecResolver.Instance);
            var r2 = serializer.UnpackSingleObject(b);

            r1.Is(r2);
        }


        [Fact]
        public void OldSpecBinary()
        {
            var referenceContext = new MsgPack.Serialization.SerializationContext(MsgPack.PackerCompatibilityOptions.Classic)
            {
                DefaultDateTimeConversionMethod = MsgPack.Serialization.DateTimeConversionMethod.Native,
            };

            var data = Enumerable.Range(0, 10000).Select(x => (byte)1).ToArray();

            var serializer = referenceContext.GetSerializer<byte[]>();

            var a = MessagePackSerializer.Serialize(data, OldSpecResolver.Instance);
            var b = serializer.PackSingleObject(data);

            a.Is(b);

            var r1 = MessagePackSerializer.Deserialize<byte[]>(a, OldSpecResolver.Instance);
            var r2 = serializer.UnpackSingleObject(b);

            r1.Is(r2);
        }
    }
}
