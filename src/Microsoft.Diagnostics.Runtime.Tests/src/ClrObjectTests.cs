﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Diagnostics.Runtime.Tests.Fixtures;
using Microsoft.Diagnostics.Runtime.Utilities;

using Xunit;

namespace Microsoft.Diagnostics.Runtime.Tests
{
    public class ClrObjectTests : IClassFixture<ClrObjectConnection>
    {
        private readonly ClrObjectConnection _connection;

        private ClrObject _primitiveCarrier => _connection.TestDataClrObject;

        public ClrObjectTests(ClrObjectConnection connection)
            => _connection = connection;

        [Fact]
        public void GetField_WhenBool_ReturnsExpected()
        {
            // Arrange
            var prototype = _connection.Prototype;

            // Act
            bool actual = _primitiveCarrier.ReadField<bool>(nameof(prototype.TrueBool));

            // Assert
            Assert.True(actual);
        }

        [Fact]
        public void GetField_WhenLong_ReturnsExpected()
        {
            // Arrange
            var prototype = _connection.Prototype;

            // Act
            long actual = _primitiveCarrier.ReadField<long>(nameof(prototype.OneLargerMaxInt));

            // Assert
            Assert.Equal(prototype.OneLargerMaxInt, actual);
        }

        [Fact]
        public void GetField_WhenEnum_ReturnsExpected()
        {
            // Arrange
            var prototype = _connection.Prototype;

            // Act
            ClrObjectConnection.EnumType enumValue = _primitiveCarrier.ReadField<ClrObjectConnection.EnumType>(nameof(prototype.SomeEnum));

            // Assert
            Assert.Equal(prototype.SomeEnum, enumValue);
        }

        [Fact]
        public void GetStringField_WhenStringField_ReturnsPointerToObject()
        {
            // Arrange
            var prototype = _connection.Prototype;

            // Act
            string text = _primitiveCarrier.ReadStringField(nameof(prototype.HelloWorldString));

            // Assert
            Assert.Equal(prototype.HelloWorldString, text);
        }

        [Fact]
        public void GetStringField_WhenTypeMismatch_ThrowsInvalidOperation()
        {
            // Arrange
            var prototype = _connection.Prototype;

            // Act
            void readDifferentFieldTypeAsString() => _primitiveCarrier.ReadStringField(nameof(prototype.SomeEnum));

            // Assert
            Assert.Throws<InvalidOperationException>(readDifferentFieldTypeAsString);
        }

        [Fact]
        public void GetObjectField_WhenStringField_ReturnsPointerToObject()
        {
            // Arrange
            var prototype = _connection.Prototype;

            // Act
            ClrObject textPointer = _primitiveCarrier.ReadObjectField(nameof(prototype.HelloWorldString));

            // Assert
            Assert.Equal(prototype.HelloWorldString, (string)textPointer);
        }

        [Fact]
        public void GetObjectField_WhenReferenceField_ReturnsPointerToObject()
        {
            // Arrange
            var prototype = _connection.Prototype;

            // Act
            ClrObject referenceFieldValue = _primitiveCarrier.ReadObjectField(nameof(prototype.SamplePointer));

            // Assert
            Assert.Equal("SamplePointerType", referenceFieldValue.Type.Name);
        }

        [Fact]
        public void GetObjectField_WhenNonExistingField_ThrowsArgumentException()
        {
            // Arrange
            var prototype = _connection.Prototype;

            // Act
            void readNonExistingField() => _primitiveCarrier.ReadObjectField("nonExistingField");

            // Assert
            Assert.Throws<ArgumentException>(readNonExistingField);
        }

        [Fact]
        public void GetValueTypeField_WhenDateTime_ThrowsException()
        {
            // Arrange
            var prototype = _connection.Prototype;

            // Act
            ClrValueType birthday = _primitiveCarrier.ReadValueTypeField(nameof(prototype.Birthday));

            // Assert
            Assert.Equal(typeof(DateTime).FullName, birthday.Type.Name);
        }

        [Fact]
        public void GetValueTypeField_WhenGuid_ThrowsException()
        {
            // Arrange
            var prototype = _connection.Prototype;

            // Act
            ClrValueType sampleGuid = _primitiveCarrier.ReadValueTypeField(nameof(prototype.SampleGuid));

            // Assert
            Assert.Equal(typeof(Guid).FullName, sampleGuid.Type.Name);
        }

        /// <summary>
        /// Expected values obtained by:
        /// dumpheap -type System.String
        /// dumpheap -strings -min 80 -max 83
        /// dumpheap -strings -min 88 -max 90
        /// </summary>
        [Fact]
        public void GetStringValue()
        {
            using DataTarget dt = TestTargets.Types.LoadFullDump();
            using ClrRuntime runtime = dt.ClrVersions.Single().CreateRuntime();
            ClrHeap heap = runtime.Heap;
            var strings = heap.EnumerateObjects()
                .Where(o => o.Type.ElementType == ClrElementType.String)
                .ToDictionary(o => o.Address, o => o.GetStringValue());

            Match(0x0000000003D7AB10, "Windows.Foundation.Diagnostics.AsyncCausalityTracer", strings);
            Match(0x0000000003D7C278, "ERROR: Exception during construction of EventSource ", strings);
            Match(0x0000000003D7B298, "Switch.System.Globalization.EnforceJapaneseEraYearRanges", strings);
            Match(0x0000000003D7B7F0, "Switch.System.Diagnostics.IgnorePortablePDBsInStackTraces", strings);

            static void Match(ulong address, string expectedString,
                Dictionary<ulong, string> computedStrings)
            {
                var computedString = computedStrings[address];
                Assert.Equal(expectedString, computedString);
            }
        }


        /// <summary>
        /// dumpheap -stat
        /// dumpheap -mt 00007ff9fd298538
        /// 00007ff9fd298538       32         1520 System.Int32[]
        /// dumpheap -mt 00007ff9fd298538
        /// dumparray 0000000003d726e0 (72 elements)
        /// objsize 0000000003d726e0
        /// sizeof(0000000003D726E0) = 312 (0x138) bytes (System.Int32[])
        /// </summary>
        [Fact]
        public void GetGraphSize()
        {
            using DataTarget dt = TestTargets.Types.LoadFullDump();
            using ClrRuntime runtime = dt.ClrVersions.Single().CreateRuntime();
            ClrHeap heap = runtime.Heap;
            var intArrays = heap.EnumerateObjects()
                .Where(o => o.Type.Name == "System.Int32[]")
                .ToList();

            var examinedIntArray = intArrays.Single(a => a.Address == 0x0000000003d726e0);
            var graphSizeIntArray = examinedIntArray.GetGraphSize();
            Assert.Equal(312, (int)graphSizeIntArray);
        }
    }
}
