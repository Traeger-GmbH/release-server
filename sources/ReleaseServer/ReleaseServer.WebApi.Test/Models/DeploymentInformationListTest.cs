namespace ReleaseServer.WebApi.Test.Models
{
    using FluentAssertions;
    using ReleaseServer.WebApi.Models;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Xunit;

    public class DeploymentInformationListTest
    {
        [Fact]
        public void CreateInstance_WithOffsetOutOfRange_DoesNotThrow()
        {
            var input = new List<DeploymentInformation>();
            input.Add(new DeploymentInformation());
            input.Add(new DeploymentInformation());
            input.Add(new DeploymentInformation());

            Action act = () => new DeploymentInformationList(input, 2, 100);
            act.Should().NotThrow();
        }

        [Fact]
        public void CreateInstance_WithLimitOutOfRange_DoesNotThrow()
        {
            var input = new List<DeploymentInformation>();
            input.Add(new DeploymentInformation());
            input.Add(new DeploymentInformation());
            input.Add(new DeploymentInformation());

            Action act = () => new DeploymentInformationList(input, 100, 2);
            act.Should().NotThrow();
        }

        [Fact]
        public void CreateInstance_WithOffsetAndLimit_IsCorrect()
        {
            var input = new List<DeploymentInformation>();
            input.Add(new DeploymentInformation());
            input.Add(new DeploymentInformation());
            input.Add(new DeploymentInformation());
            input.Add(new DeploymentInformation());
            input.Add(new DeploymentInformation());

            var expectedTotalCount = 5;
            var expectedNextOffset = 3;

            var result = new DeploymentInformationList(input, 2, 1);
            result.ProductInformation.Should().BeEquivalentTo(input.Skip(1).Take(2));
            result.TotalCount.Should().Be(expectedTotalCount);
            result.NextOffset.Should().Be(expectedNextOffset);
        }
    }
}
