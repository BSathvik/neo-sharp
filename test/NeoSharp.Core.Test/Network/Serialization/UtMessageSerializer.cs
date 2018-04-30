﻿using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoSharp.Core.Extensions;
using NeoSharp.Core.Network.Messages;
using NeoSharp.Core.Network.Serialization;
using NeoSharp.TestHelpers;

namespace NeoSharp.Core.Test.Network.Serialization
{
    [TestClass]
    public class UtMessageSerializer : TestBase
    {
        [TestMethod]
        public async Task Can_serialize_and_deserialize_messages()
        {
            // Arrange 
            var serializer = AutoMockContainer.Create<MessageSerializer>();
            var expectedVerAckMessage = new VersionAcknowledgmentMessage();
            VersionAcknowledgmentMessage actualVerAckMessage;

            // Act
            using (var memory = new MemoryStream())
            {
                await serializer.SerializeTo(expectedVerAckMessage, memory, CancellationToken.None);
                memory.Seek(0, SeekOrigin.Begin);
                actualVerAckMessage = await serializer.DeserializeFrom<VersionAcknowledgmentMessage>(memory, CancellationToken.None);
            }

            // Asset
            actualVerAckMessage.Should().NotBeNull();
            actualVerAckMessage.Command.Should().Be(expectedVerAckMessage.Command);
        }

        [TestMethod]
        public async Task Can_serialize_and_deserialize_messages_with_payload()
        {
            // Arrange 
            var serializer = AutoMockContainer.Create<MessageSerializer>();
            var expectedVersionMessage = new VersionMessage();
            var r = new Random(Environment.TickCount);
            expectedVersionMessage.Payload.Version = (uint)r.Next(0, int.MaxValue);
            expectedVersionMessage.Payload.Services = (ulong)r.Next(0, int.MaxValue);
            expectedVersionMessage.Payload.Timestamp = DateTime.UtcNow.ToTimestamp();
            expectedVersionMessage.Payload.Port = (ushort)r.Next(0, short.MaxValue);
            expectedVersionMessage.Payload.Nonce = (uint)r.Next(0, int.MaxValue);
            expectedVersionMessage.Payload.UserAgent = $"/NEO:{r.Next(1, 10)}.{r.Next(1, 100)}.{r.Next(1, 1000)}/";
            expectedVersionMessage.Payload.StartHeight = (uint)r.Next(0, int.MaxValue);
            expectedVersionMessage.Payload.Relay = false;
            VersionMessage actualVersionMessage;

            // Act
            using (var memory = new MemoryStream())
            {
                await serializer.SerializeTo(expectedVersionMessage, memory, CancellationToken.None);
                memory.Seek(0, SeekOrigin.Begin);
                actualVersionMessage = await serializer.DeserializeFrom<VersionMessage>(memory, CancellationToken.None);
            }

            // Asset
            actualVersionMessage.Should().NotBeNull();
            actualVersionMessage.Command.Should().Be(expectedVersionMessage.Command);
            actualVersionMessage.Payload.Should().NotBeNull();
            actualVersionMessage.Payload.Version.Should().Be(expectedVersionMessage.Payload.Version);
            actualVersionMessage.Payload.Services.Should().Be(expectedVersionMessage.Payload.Services);
            actualVersionMessage.Payload.Timestamp.Should().Be(expectedVersionMessage.Payload.Timestamp);
            actualVersionMessage.Payload.Port.Should().Be(expectedVersionMessage.Payload.Port);
            actualVersionMessage.Payload.Nonce.Should().Be(expectedVersionMessage.Payload.Nonce);
            actualVersionMessage.Payload.UserAgent.Should().Be(expectedVersionMessage.Payload.UserAgent);
            actualVersionMessage.Payload.StartHeight.Should().Be(expectedVersionMessage.Payload.StartHeight);
            actualVersionMessage.Payload.Relay.Should().Be(expectedVersionMessage.Payload.Relay);
        }
    }
}