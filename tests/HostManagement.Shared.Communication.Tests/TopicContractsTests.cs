using System.Reflection;
using AwesomeAssertions;
using HostManagement.Shared.Communication.Contracts;
using HostManagement.Shared.Serialization;
using Xunit;

namespace HostManagement.Shared.Communication.Tests;

public class TopicContractsTests
{
    [Fact]
    public void ShouldDeclareContract_ForEveryTopic()
    {
        // Arrange
        var declaredTopics = GetDeclaredContracts(typeof(TopicContracts)).Select(contract => contract.Topic);

        // Act
        var topicsWithoutContract = Topics.GetAllTopics().Except(declaredTopics, StringComparer.Ordinal);

        // Assert
        topicsWithoutContract.Should().BeEmpty();
    }

    [Fact]
    public void ShouldDeclareContract_ForEveryEventTopic()
    {
        // Arrange
        var declaredTopics = GetDeclaredContracts(typeof(EventTopicContracts))
            .Select(contract => contract.Topic)
            .ToList();

        var eventTopics = typeof(EventTopics)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(field => field.IsLiteral && field.FieldType == typeof(string))
            .Select(field => (string)field.GetRawConstantValue()!);

        // Act
        var topicsWithoutContract = eventTopics.Except(declaredTopics, StringComparer.Ordinal);

        // Assert
        topicsWithoutContract.Should().BeEmpty();
    }

    [Fact]
    public void ShouldNotDeclareUnknownTopic_WhenContractDeclared()
    {
        // Arrange
        var allTopics = Topics.GetAllTopics().ToList();

        // Act
        var unknownTopics = GetDeclaredContracts(typeof(TopicContracts))
            .Select(contract => contract.Topic)
            .Except(allTopics, StringComparer.Ordinal);

        // Assert
        unknownTopics.Should().BeEmpty();
    }

    [Fact]
    public void ShouldResolveTypeInfoForEveryContractType_WhenContractDeclared()
    {
        // Every declared contract type must be serializable, otherwise the topic fails only when it is first used.

        // Arrange
        var contractTypes = GetDeclaredContractProperties(typeof(TopicContracts))
            .Concat(GetDeclaredContractProperties(typeof(EventTopicContracts)))
            .SelectMany(property => property.PropertyType.GetGenericArguments())
            .Where(type => type != typeof(string))
            .Distinct();

        // Act
        var unresolvableTypes = contractTypes.Where(type => !TryResolveTypeInfo(type));

        // Assert
        unresolvableTypes.Should().BeEmpty();
    }

    [Theory]
    [InlineData("GetSystemConfiguration")]
    [InlineData("SetSystemConfiguration")]
    [InlineData("echo")]
    public void ShouldReturnTopic_WhenConvertedToString(string topic)
    {
        // Arrange
        var contract = GetDeclaredContracts(typeof(TopicContracts))
            .Single(declared => declared.Topic == topic);

        // Act
        var result = contract.ToString();

        // Assert
        result.Should().Be(topic);
    }

    private static IEnumerable<MessageContract> GetDeclaredContracts(Type declaringType)
        => GetDeclaredContractProperties(declaringType)
            .Select(property => (MessageContract)property.GetValue(null)!);

    private static IEnumerable<PropertyInfo> GetDeclaredContractProperties(Type declaringType)
        => declaringType
            .GetProperties(BindingFlags.Public | BindingFlags.Static)
            .Where(property => typeof(MessageContract).IsAssignableFrom(property.PropertyType));

    private static bool TryResolveTypeInfo(Type contractType)
    {
        var method = typeof(MessageTypeRegistry)
            .GetMethod(nameof(MessageTypeRegistry.TryGetTypeInfo))!
            .MakeGenericMethod(contractType);

        return (bool)method.Invoke(null, [null])!;
    }
}
