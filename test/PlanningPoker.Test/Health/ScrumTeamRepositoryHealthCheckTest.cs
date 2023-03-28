using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PlanningPoker.Data;
using PlanningPoker.Health;

namespace PlanningPoker.Test.Health
{
    [TestClass]
    public class ScrumTeamRepositoryHealthCheckTest
    {
        [TestMethod]
        public async Task CheckHealthAsync_RepositoryReturnsEmptyTeamsList_Healthy()
        {
            // Arrange
            var scrumTeamRepository = new Mock<IScrumTeamRepository>();
            scrumTeamRepository.SetupGet(o => o.ScrumTeamNames).Returns(Enumerable.Empty<string>());
            var target = new ScrumTeamRepositoryHealthCheck(scrumTeamRepository.Object);

            // Act
            var result = await target.CheckHealthAsync(new HealthCheckContext(), default);

            // Verify
            Assert.AreEqual(HealthStatus.Healthy, result.Status);
        }

        [TestMethod]
        public async Task CheckHealthAsync_RepositoryReturns1Team_Healthy()
        {
            // Arrange
            var scrumTeamRepository = new Mock<IScrumTeamRepository>();
            scrumTeamRepository.SetupGet(o => o.ScrumTeamNames).Returns(new[] { "MyTeam" });
            var target = new ScrumTeamRepositoryHealthCheck(scrumTeamRepository.Object);

            // Act
            var result = await target.CheckHealthAsync(new HealthCheckContext(), default);

            // Verify
            Assert.AreEqual(HealthStatus.Healthy, result.Status);
        }

        [TestMethod]
        public async Task CheckHealthAsync_RepositoryThrowsException_Unhealthy()
        {
            // Arrange
            var scrumTeamRepository = new Mock<IScrumTeamRepository>();
            scrumTeamRepository.SetupGet(o => o.ScrumTeamNames).Throws(new InvalidOperationException());
            var target = new ScrumTeamRepositoryHealthCheck(scrumTeamRepository.Object);

            // Act
            var result = await target.CheckHealthAsync(new HealthCheckContext(), default);

            // Verify
            Assert.AreEqual(HealthStatus.Unhealthy, result.Status);
        }
    }
}
