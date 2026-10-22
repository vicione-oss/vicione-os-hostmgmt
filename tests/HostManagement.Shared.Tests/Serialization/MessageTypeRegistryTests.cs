using AwesomeAssertions;
using HostManagement.Shared.Contracts;
using HostManagement.Shared.Contracts.System;
using HostManagement.Shared.Serialization;
using Xunit;

namespace HostManagement.Shared.Tests.Serialization;

public class MessageTypeRegistryTests
{
    public class AddContext
    {
        [Fact]
        public void ShouldThrow_WhenContextNull()
        {
            // Act
            var act = () => MessageTypeRegistry.AddContext(null!);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("context");
        }

        [Fact]
        public void ShouldNotThrow_WhenContextAddedTwice()
        {
            // Act
            var act = () =>
            {
                MessageTypeRegistry.AddContext(SourceGenerationContext.Default);
                MessageTypeRegistry.AddContext(SourceGenerationContext.Default);
            };

            // Assert
            act.Should().NotThrow();
        }

        [Fact]
        public void ShouldMakeContractResolvable_WhenContextAdded()
        {
            // Arrange
            MessageTypeRegistry.TryGetTypeInfo<AdditionalContract>(out _).Should().BeFalse();

            // Act
            MessageTypeRegistry.AddContext(AdditionalContractContext.Default);

            // Assert
            MessageTypeRegistry.TryGetTypeInfo<AdditionalContract>(out var typeInfo).Should().BeTrue();
            typeInfo!.Type.Should().Be<AdditionalContract>();
        }
    }

    public class TryGetTypeInfo
    {
        [Fact]
        public void ShouldReturnTrue_WhenContractRegisteredByOwningAssembly()
        {
            // Act
            var result = MessageTypeRegistry.TryGetTypeInfo<SystemConfiguration>(out var typeInfo);

            // Assert
            result.Should().BeTrue();
            typeInfo!.Type.Should().Be<SystemConfiguration>();
        }

        [Fact]
        public void ShouldReturnFalse_WhenContractNotRegistered()
        {
            // Act
            var result = MessageTypeRegistry.TryGetTypeInfo<UnregisteredContract>(out var typeInfo);

            // Assert
            result.Should().BeFalse();
            typeInfo.Should().BeNull();
        }

        [Fact]
        public void ShouldReturnFalse_WhenContractIsString()
        {
            // Act
            var result = MessageTypeRegistry.TryGetTypeInfo<string>(out var typeInfo);

            // Assert
            result.Should().BeFalse();
            typeInfo.Should().BeNull();
        }

        [Fact]
        public void ShouldReturnSameInstance_WhenCalledTwice()
        {
            // Act
            MessageTypeRegistry.TryGetTypeInfo<SignedDebianPackage>(out var first);
            MessageTypeRegistry.TryGetTypeInfo<SignedDebianPackage>(out var second);

            // Assert
            first.Should().BeSameAs(second);
        }

        [Fact]
        public void ShouldKeepSourceGenerationOptions_WhenResolvedThroughRegistry()
        {
            // The context disallows unmapped members. Resolving through the owning context must preserve that,
            // which combining contexts into shared 'JsonSerializerOptions' would not.

            // Act
            MessageTypeRegistry.TryGetTypeInfo<SignedDebianPackage>(out var typeInfo);

            // Assert
            typeInfo!.Options.Should().BeSameAs(SourceGenerationContext.Default.Options);
        }
    }

    public class GetTypeInfo
    {
        [Fact]
        public void ShouldThrow_WhenContractIsString()
        {
            // Act
            var act = () => MessageTypeRegistry.GetTypeInfo<string>();

            // Assert
            act.Should()
                .Throw<InvalidOperationException>()
                .WithMessage("*raw text*");
        }

        [Fact]
        public void ShouldThrow_WhenContractNotRegistered()
        {
            // Act
            var act = () => MessageTypeRegistry.GetTypeInfo<UnregisteredContract>();

            // Assert
            act.Should()
                .Throw<InvalidOperationException>()
                .WithMessage($"*{typeof(UnregisteredContract).FullName}*");
        }

        [Fact]
        public void ShouldReturnTypeInfo_WhenContractRegistered()
        {
            // Act
            var typeInfo = MessageTypeRegistry.GetTypeInfo<SystemConfiguration>();

            // Assert
            typeInfo.Type.Should().Be<SystemConfiguration>();
        }
    }
}
