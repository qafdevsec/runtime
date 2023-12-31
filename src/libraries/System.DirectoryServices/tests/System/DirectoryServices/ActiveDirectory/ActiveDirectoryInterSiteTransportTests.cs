// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel;
using Xunit;

namespace System.DirectoryServices.ActiveDirectory.Tests
{
    [ConditionalClass(typeof(PlatformDetection), nameof(PlatformDetection.IsNotWindowsNanoServer))]
    public class ActiveDirectoryInterSiteTransportTests
    {
        [Fact]
        public void FindByTransportType_NullContext_ThrowsArgumentNullException()
        {
            AssertExtensions.Throws<ArgumentNullException>("context", () => ActiveDirectoryInterSiteTransport.FindByTransportType(null, ActiveDirectoryTransportType.Rpc));
        }

        [Fact]
        [OuterLoop]
        public void FindByTransportType_ForestNoDomainAssociatedWithoutName_ThrowsActiveDirectoryOperationException()
        {
            var context = new DirectoryContext(DirectoryContextType.Forest);
            if (!PlatformDetection.IsDomainJoinedMachine)
            {
                Assert.Throws<ActiveDirectoryOperationException>(() => ActiveDirectoryInterSiteTransport.FindByTransportType(context, ActiveDirectoryTransportType.Rpc));
            }
        }

        [Fact]
        public void FindByTransportType_ForestNoDomainAssociatedWithName_ThrowsActiveDirectoryOperationException()
        {
            var context = new DirectoryContext(DirectoryContextType.Forest, "server:port");
            AssertExtensions.Throws<ArgumentException>("context", () => ActiveDirectoryInterSiteTransport.FindByTransportType(context, ActiveDirectoryTransportType.Rpc));
        }

        [ConditionalFact(typeof(PlatformDetection), nameof(PlatformDetection.IsNotDomainJoinedMachine))]
        public void FindByTransportType_ForestNoDomainAssociatedWithName_ThrowsActiveDirectoryOperationException_NoDomain()
        {
            var context = new DirectoryContext(DirectoryContextType.Forest, "\0");
            AssertExtensions.Throws<ArgumentException>("context", () => ActiveDirectoryInterSiteTransport.FindByTransportType(context, ActiveDirectoryTransportType.Rpc));
        }

        [Fact]
        public void FindByTransportType_DomainNoDomainAssociatedWithoutName_ThrowsArgumentException()
        {
            var context = new DirectoryContext(DirectoryContextType.Domain);
            AssertExtensions.Throws<ArgumentException>("context", () => ActiveDirectoryInterSiteTransport.FindByTransportType(context, ActiveDirectoryTransportType.Rpc));
        }

        [ConditionalTheory(typeof(PlatformDetection), nameof(PlatformDetection.IsNotWindowsNanoServer))]
        [ActiveIssue("https://github.com/dotnet/runtime/issues/34442", TestPlatforms.Windows, TargetFrameworkMonikers.Netcoreapp, TestRuntimes.Mono)]
        [OuterLoop("Takes too long on domain joined machines")]
        [InlineData(DirectoryContextType.ApplicationPartition)]
        [InlineData(DirectoryContextType.DirectoryServer)]
        [InlineData(DirectoryContextType.Domain)]
        public void FindByTransportType_InvalidContextTypeWithName(DirectoryContextType type)
        {
            var context = new DirectoryContext(type, "Name");
            Exception exception = Record.Exception(() => ActiveDirectoryInterSiteTransport.FindByTransportType(context, ActiveDirectoryTransportType.Rpc));
            Assert.NotNull(exception);
            Assert.True(exception is ArgumentException ||
                        exception is ActiveDirectoryOperationException,
                        $"We got unrecognized exception {exception}");
        }

        [Fact]
        [OuterLoop]
        public void FindByTransportType_ConfigurationSetTypeWithName_Throws()
        {
            var context = new DirectoryContext(DirectoryContextType.ConfigurationSet, "Name");
            Assert.Throws<ActiveDirectoryOperationException>(() => ActiveDirectoryInterSiteTransport.FindByTransportType(context, ActiveDirectoryTransportType.Rpc));
        }

        [Theory]
        [InlineData(ActiveDirectoryTransportType.Rpc - 1)]
        [InlineData(ActiveDirectoryTransportType.Smtp + 1)]
        public void FindByTransportType_InvalidTransport_ThrowsInvalidEnumArgumentException(ActiveDirectoryTransportType transport)
        {
            var context = new DirectoryContext(DirectoryContextType.ConfigurationSet, "Name");
            AssertExtensions.Throws<InvalidEnumArgumentException>("value", () => ActiveDirectoryInterSiteTransport.FindByTransportType(context, transport));
        }
    }
}
