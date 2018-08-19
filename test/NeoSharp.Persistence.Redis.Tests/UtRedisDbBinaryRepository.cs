using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using NeoSharp.BinarySerialization;
using NeoSharp.Core.Models;
using NeoSharp.Core.Persistence;
using NeoSharp.Core.Types;
using NeoSharp.Persistence.RedisDB;
using NeoSharp.Persistence.RedisDB.Helpers;
using NeoSharp.TestHelpers;
using StackExchange.Redis;

namespace NeoSharp.Persistence.Redis.Tests
{
    [TestClass]
    public class UtRedisDbBinaryRepository : TestBase
    {
        private Mock<IBinaryDeserializer> _deserializerMock;
        private Mock<IBinarySerializer> _serializerMock;

        [TestInitialize]
        public void TestInit()
        {
            _deserializerMock = AutoMockContainer.GetMock<IBinaryDeserializer>();
            _serializerMock = AutoMockContainer.GetMock<IBinarySerializer>();
        }

        [TestMethod]
        public void Ctor_CreateValidRedisDbRepository()
        {
            var testee = AutoMockContainer.Create<RedisDbBinaryRepository>();

            testee.Should().BeOfType<RedisDbBinaryRepository>();
        }

        #region IRepository System Members

        [TestMethod]
        public async Task GetTotalBlockHeight_NoValueFound_ReturnsUIntMinValue()
        {
            var redisDbContextMock = AutoMockContainer.GetMock<IRedisDbContext>();
            redisDbContextMock
                .Setup(x => x.Get(DataEntryPrefix.SysCurrentBlock.ToString()))
                .ReturnsAsync(RedisValue.Null);
            var testee = AutoMockContainer.Create<RedisDbBinaryRepository>();

            var result = await testee.GetTotalBlockHeight();

            result.Should().Be(uint.MinValue);
        }

        [TestMethod]
        public async Task GetTotalBlockHeight_ValueFound_ReturnsValue()
        {
            var expectedResult = (uint)RandomInt();
            var expectedValue = (RedisValue) expectedResult;
            var redisDbContextMock = AutoMockContainer.GetMock<IRedisDbContext>();
            redisDbContextMock
                .Setup(x => x.Get(DataEntryPrefix.SysCurrentBlock.ToString()))
                .ReturnsAsync(expectedValue);
            var testee = AutoMockContainer.Create<RedisDbBinaryRepository>();

            var result = await testee.GetTotalBlockHeight();

            result.Should().Be(expectedResult);
        }

        [TestMethod]
        public async Task SetTotalBlockHeight_SaveCorrectKeyValue()
        {
            var input = (uint)RandomInt();
            var expectedValue = (RedisValue) input;
            var redisDbContextMock = AutoMockContainer.GetMock<IRedisDbContext>();
            var testee = AutoMockContainer.Create<RedisDbBinaryRepository>();

            await testee.SetTotalBlockHeight(input);

            redisDbContextMock.Verify(m => m.Set(DataEntryPrefix.SysCurrentBlock.ToString(), expectedValue));
        }

        [TestMethod]
        public async Task GetVersion_NoValueFound_ReturnsUIntMinValue()
        {
            var redisDbContextMock = AutoMockContainer.GetMock<IRedisDbContext>();
            redisDbContextMock
                .Setup(x => x.Get(DataEntryPrefix.SysVersion.ToString()))
                .ReturnsAsync(RedisValue.Null);
            var testee = AutoMockContainer.Create<RedisDbBinaryRepository>();

            var result = await testee.GetVersion();

            result.Should().BeNull();
        }

        [TestMethod]
        public async Task GetVersion_ValueFound_ReturnsValue()
        {
            var expectedResult = RandomString(RandomInt(10));

            var redisDbContextMock = AutoMockContainer.GetMock<IRedisDbContext>();
            redisDbContextMock
                .Setup(x => x.Get(DataEntryPrefix.SysVersion.ToString()))
                .ReturnsAsync(expectedResult);

            var testee = AutoMockContainer.Create<RedisDbBinaryRepository>();

            var result = await testee.GetVersion();

            result.Should().Be(expectedResult);
        }

        [TestMethod]
        public async Task SetVersion_SaveCorrectKeyValue()
        {
            var input = RandomString(RandomInt(10));

            var redisDbContextMock = AutoMockContainer.GetMock<IRedisDbContext>();
            var testee = AutoMockContainer.Create<RedisDbBinaryRepository>();

            await testee.SetVersion(input);

            redisDbContextMock.Verify(m => m.Set(DataEntryPrefix.SysVersion.ToString(), input));
        }

        [TestMethod]
        public async Task GetTotalBlockHeaderHeight_NoValueFound_ReturnsUIntMinValue()
        {
            var redisDbContextMock = AutoMockContainer.GetMock<IRedisDbContext>();
            redisDbContextMock
                .Setup(x => x.Get(DataEntryPrefix.SysCurrentHeader.ToString()))
                .ReturnsAsync((RedisValue.Null));
            var testee = AutoMockContainer.Create<RedisDbBinaryRepository>();

            var result = await testee.GetTotalBlockHeaderHeight();

            result.Should().Be(uint.MinValue);
        }

        [TestMethod]
        public async Task GetTotalBlockHeaderHeight_ValueFound_ReturnsValue()
        {
            var expectedResult = (uint)RandomInt();
            var expectedValue = (RedisValue) expectedResult;
            var redisDbContextMock = AutoMockContainer.GetMock<IRedisDbContext>();
            redisDbContextMock
                .Setup(x => x.Get(DataEntryPrefix.SysCurrentHeader.ToString()))
                .ReturnsAsync(expectedValue);
            var testee = AutoMockContainer.Create<RedisDbBinaryRepository>();

            var result = await testee.GetTotalBlockHeaderHeight();

            result.Should().Be(expectedResult);
        }

        [TestMethod]
        public async Task SetTotalBlockHeaderHeight_SaveCorrectKeyValue()
        {
            var input = (uint)RandomInt();
            var redisDbContextMock = AutoMockContainer.GetMock<IRedisDbContext>();
            var testee = AutoMockContainer.Create<RedisDbBinaryRepository>();

            await testee.SetTotalBlockHeaderHeight(input);

            redisDbContextMock.Verify(m => m.Set(DataEntryPrefix.SysCurrentHeader.ToString(), input));
        }

        #endregion

        [TestMethod]
        public async Task GetIndexHeight_NoValueFound_ReturnsZero()
        {
            var contextMock = AutoMockContainer.GetMock<IRedisDbContext>();
            contextMock.Setup(m => m.Get(DataEntryPrefix.IxIndexHeight.ToString())).ReturnsAsync(RedisValue.Null);
            var testee = AutoMockContainer.Create<RedisDbBinaryRepository>();

            var result = await testee.GetIndexHeight();

            result.Should().Be(uint.MinValue);
        }

        [TestMethod]
        public async Task GetIndexHeight_ValueFound_ReturnsValue()
        {
            var expectedHeight = (uint) RandomInt();
            var contextMock = AutoMockContainer.GetMock<IRedisDbContext>();
            contextMock.Setup(m => m.Get(DataEntryPrefix.IxIndexHeight.ToString())).ReturnsAsync(expectedHeight);
            var testee = AutoMockContainer.Create<RedisDbBinaryRepository>();

            var result = await testee.GetIndexHeight();

            result.Should().Be(expectedHeight);
        }

        [TestMethod]
        public async Task GetIndexConfirmed_NoValueFound_ReturnsEmptySet()
        {
            var input = UInt160.Parse(RandomInt().ToString("X40"));
            var contextMock = AutoMockContainer.GetMock<IRedisDbContext>();
            contextMock.Setup(m => m.Get(input.BuildIxConfirmedKey())).ReturnsAsync(RedisValue.Null);
            var testee = AutoMockContainer.Create<RedisDbBinaryRepository>();

            var result = await testee.GetIndexConfirmed(input);

            result.Should().BeOfType(typeof(HashSet<CoinReference>));
            result.Count.Should().Be(0);
        }

        [TestMethod]
        public async Task GetIndexConfirmed_BinaryValueFound_ReturnsValue()
        {
            var input = UInt160.Parse(RandomInt().ToString("X40"));
            var expectedBytes = new byte[1];
            var expectedSet = new HashSet<CoinReference>
            {
                new CoinReference
                {
                    PrevHash = UInt256.Parse(RandomInt().ToString("X64")),
                    PrevIndex = 0
                },
                new CoinReference
                {
                    PrevHash = UInt256.Parse(RandomInt().ToString("X64")),
                    PrevIndex = 0
                }
            };
            var contextMock = AutoMockContainer.GetMock<IRedisDbContext>();
            contextMock.Setup(m => m.Get(input.BuildIxConfirmedKey())).ReturnsAsync(expectedBytes);

            _deserializerMock.Setup(m => m.Deserialize<HashSet<CoinReference>>(expectedBytes, null))
                .Returns(expectedSet);
            var testee = AutoMockContainer.Create<RedisDbBinaryRepository>();

            BinaryDeserializer.Initialize(_deserializerMock.Object);
            var result = await testee.GetIndexConfirmed(input);

            result.SetEquals(expectedSet).Should().BeTrue();
        }

        [TestMethod]
        public async Task GetIndexClaimable_NoValueFound_ReturnsEmptySet()
        {
            var input = UInt160.Parse(RandomInt().ToString("X40"));
            var contextMock = AutoMockContainer.GetMock<IRedisDbContext>();
            contextMock.Setup(m => m.Get(input.BuildIxClaimableKey())).ReturnsAsync(RedisValue.Null);
            var testee = AutoMockContainer.Create<RedisDbBinaryRepository>();

            var result = await testee.GetIndexClaimable(input);

            result.Should().BeOfType(typeof(HashSet<CoinReference>));
            result.Count.Should().Be(0);
        }

        [TestMethod]
        public async Task GetIndexClaimable_BinaryValueFound_ReturnsValue()
        {
            var input = UInt160.Parse(RandomInt().ToString("X40"));
            var expectedBytes = new byte[1];
            var expectedSet = new HashSet<CoinReference>
            {
                new CoinReference
                {
                    PrevHash = UInt256.Parse(RandomInt().ToString("X64")),
                    PrevIndex = 0
                },
                new CoinReference
                {
                    PrevHash = UInt256.Parse(RandomInt().ToString("X64")),
                    PrevIndex = 0
                }
            };
            var contextMock = AutoMockContainer.GetMock<IRedisDbContext>();
            contextMock.Setup(m => m.Get(input.BuildIxClaimableKey())).ReturnsAsync(expectedBytes);

            _deserializerMock.Setup(m => m.Deserialize<HashSet<CoinReference>>(expectedBytes, null))
                .Returns(expectedSet);
            BinaryDeserializer.Initialize(_deserializerMock.Object);
            var testee = AutoMockContainer.Create<RedisDbBinaryRepository>();

            var result = await testee.GetIndexClaimable(input);

            result.SetEquals(expectedSet).Should().BeTrue();
        }

        //TODO: Test JSON values
    }
}