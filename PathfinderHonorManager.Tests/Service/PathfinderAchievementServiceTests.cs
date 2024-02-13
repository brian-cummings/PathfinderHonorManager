﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using Moq;
using PathfinderHonorManager.DataAccess;
using Incoming = PathfinderHonorManager.Dto.Incoming;
using Outgoing = PathfinderHonorManager.Dto.Outgoing;
using PathfinderHonorManager.Mapping;
using PathfinderHonorManager.Model;
using PathfinderHonorManager.Service;
using PathfinderHonorManager.Service.Interfaces;
using PathfinderHonorManager.Tests.Helpers;
using PathfinderHonorManager.Dto.Outgoing;

namespace PathfinderHonorManager.Tests.Service
{
    public class PathfinderAchievementServiceTests
    {
        private static readonly DbContextOptions<PathfinderContext> SharedContextOptions =
            new DbContextOptionsBuilder<PathfinderContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        private List<Achievement> _achievements;
        private List<Pathfinder> _pathfinders;
        private List<PathfinderAchievement> _pathfinderAchievements;
        private PathfinderContext _dbContext;
        private PathfinderAchievementService _pathfinderAchievementService;

        [SetUp]
        public async Task Setup()
        {
            await DatabaseSeeder.SeedDatabase(SharedContextOptions);
            _dbContext = new PathfinderContext(SharedContextOptions);
            var mapperConfiguration = new MapperConfiguration(cfg => cfg.AddProfile<AutoMapperConfig>());
            IMapper mapper = mapperConfiguration.CreateMapper();
            
            var logger = NullLogger<PathfinderAchievementService>.Instance;
            var validator = new Mock<IValidator<Incoming.PathfinderAchievementDto>>();

            _pathfinderAchievementService = new PathfinderAchievementService(_dbContext, mapper, logger, validator.Object);
            _achievements = await _dbContext.Achievements.ToListAsync();
            _pathfinders = await _dbContext.Pathfinders.ToListAsync();
            _pathfinderAchievements = await _dbContext.PathfinderAchievements.ToListAsync();
        }

        [TestCase]
        public async Task GetAllAsync_ReturnsAllAchievements()
        {
            // Arrange
            var cancellationToken = new CancellationToken();
            var expectedCount = _achievements.Count;

            // Act
            var result = await _pathfinderAchievementService.GetAllAsync(cancellationToken);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<ICollection<PathfinderAchievementDto>>());
            Assert.That(result.Count, Is.EqualTo(expectedCount));
        }

        [TestCase]
        public async Task GetByIdAsync_ValidId_ReturnsAchievement()
        {
            // Arrange
            var cancellationToken = new CancellationToken();
            var expectedPathfinderAchievement = _pathfinderAchievements.First();
            var expectedAchievementId = expectedPathfinderAchievement.AchievementID;
            var expectedPathfinderId = expectedPathfinderAchievement.PathfinderID;

            // Act
            var result = await _pathfinderAchievementService.GetByIdAsync(expectedPathfinderId, expectedAchievementId, cancellationToken);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.AchievementID, Is.EqualTo(expectedAchievementId));
            Assert.That(result.PathfinderID, Is.EqualTo(expectedPathfinderId));
            Assert.That(result, Is.InstanceOf<PathfinderAchievementDto>());
        }

        [TestCase]
        public async Task AddAsync_AddsNewPathfinderAchievementAndReturnsDto()
        {
            // Arrange
            var newAchievementDto = new Incoming.PostPathfinderAchievementDto
            {
                AchievementID = _achievements.First().AchievementID
            };
            var pathfinderId = _pathfinders.Last().PathfinderID;
            var cancellationToken = new CancellationToken();

            // Act
            var result = await _pathfinderAchievementService.AddAsync(pathfinderId, newAchievementDto, cancellationToken);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.PathfinderID, Is.EqualTo(pathfinderId));
            Assert.That(result.AchievementID, Is.EqualTo(newAchievementDto.AchievementID));
        }
        
        [TearDown]
        public async Task TearDown()
        {
            await DatabaseCleaner.CleanDatabase(_dbContext);
            _dbContext.Dispose();
        }
    }
}
