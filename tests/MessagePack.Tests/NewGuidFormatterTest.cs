﻿using MessagePack.Formatters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace MessagePack.Tests
{
    public class NewGuidFormatterTest
    {
        // GuidBits is internal...

        //[Fact]
        //public void GuidBitsTest()
        //{
        //    var original = Guid.NewGuid();

        //    var patternA = Encoding.UTF8.GetBytes(original.ToString().ToUpper());
        //    var patternB = Encoding.UTF8.GetBytes(original.ToString().ToLower());
        //    var patternC = Encoding.UTF8.GetBytes(original.ToString().ToUpper().Replace("-", ""));
        //    var patternD = Encoding.UTF8.GetBytes(original.ToString().ToLower().Replace("-", ""));

        //    new MessagePack.Internal.GuidBits(new ArraySegment<byte>(patternA, 0, patternA.Length)).Value.Is(original);
        //    new MessagePack.Internal.GuidBits(new ArraySegment<byte>(patternB, 0, patternB.Length)).Value.Is(original);
        //    new MessagePack.Internal.GuidBits(new ArraySegment<byte>(patternC, 0, patternC.Length)).Value.Is(original);
        //    new MessagePack.Internal.GuidBits(new ArraySegment<byte>(patternD, 0, patternD.Length)).Value.Is(original);
        //}

        [MessagePackObject(true)]
        public class InClass
        {
            public int MyProperty { get; set; }
            public Guid Guid { get; set; }
        }

        [Fact]
        public void FastGuid()
        {
            {
                var original = Guid.NewGuid();
                byte[] bytes = null;
                GuidFormatter.Instance.Serialize(ref bytes, 0, original, null).Is(38);

                int readSize;
                GuidFormatter.Instance.Deserialize(bytes, 0, null, out readSize).Is(original);
                readSize.Is(38);
            }
            {
                var c = new InClass() { MyProperty = 3414141, Guid = Guid.NewGuid() };
                var c2 = MessagePackSerializer.Deserialize<InClass>(MessagePackSerializer.Serialize(c));
                c.MyProperty.Is(c2.MyProperty);
                c.Guid.Is(c2.Guid);
            }
        }
    }
}
