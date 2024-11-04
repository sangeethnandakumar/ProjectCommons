```cs
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

public class GetHccDatasetByDateQueryHandlerTests
{
    private readonly Mock<ILogger<GetHccDatasetByDateQueryHandler>> _mockLogger;
    private readonly Mock<IDbContextFactory> _mockDbContextFactory;
    private readonly Mock<IValidator<GetHccDatasetByDateQuery>> _mockValidator;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ITelemetryService> _mockTelemetryService;
    private readonly Mock<ICorrelationContextAccessor> _mockCorrelationContext;
    private readonly GetHccDatasetByDateQueryHandler _handler;
    private readonly Mock<DbContext> _mockDbContext;

    public GetHccDatasetByDateQueryHandlerTests()
    {
        _mockLogger = new Mock<ILogger<GetHccDatasetByDateQueryHandler>>();
        _mockDbContextFactory = new Mock<IDbContextFactory>();
        _mockValidator = new Mock<IValidator<GetHccDatasetByDateQuery>>();
        _mockMapper = new Mock<IMapper>();
        _mockTelemetryService = new Mock<ITelemetryService>();
        _mockCorrelationContext = new Mock<ICorrelationContextAccessor>();
        _mockDbContext = new Mock<DbContext>();

        _mockDbContextFactory.Setup(x => x.GetDbContextAsync())
            .ReturnsAsync(_mockDbContext.Object);

        _handler = new GetHccDatasetByDateQueryHandler(
            _mockLogger.Object,
            _mockDbContextFactory.Object,
            _mockValidator.Object,
            _mockMapper.Object,
            _mockTelemetryService.Object,
            _mockCorrelationContext.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldThrowValidationException_WhenValidationFails()
    {
        var query = new GetHccDatasetByDateQuery();
        _mockValidator
            .Setup(v => v.ValidateAsync(query, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(new List<ValidationFailure>
            {
                new ValidationFailure("HccDate", "HccDate is required")
            }));

        await Assert.ThrowsAsync<ValidationException>(() => _handler.Handle(query, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldExecuteCountAndFetchQueries_WhenValidationPasses()
    {
        var query = new GetHccDatasetByDateQuery { HccDate = "01-01-2024", PageNumber = 1, PageSize = 10 };
        _mockValidator
            .Setup(v => v.ValidateAsync(query, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _mockTelemetryService
            .Setup(t => t.RunAndTrackMetricsAsync(It.IsAny<Task<int>>(), It.IsAny<string>()))
            .ReturnsAsync(100);

        _mockTelemetryService
            .Setup(t => t.RunAndTrackMetricsAsync(It.IsAny<Task<List<Dtos.HccDataset>>>(), It.IsAny<string>()))
            .ReturnsAsync(new List<Dtos.HccDataset>());

        await _handler.Handle(query, CancellationToken.None);

        _mockTelemetryService.Verify(t => t.RunAndTrackMetricsAsync(It.IsAny<Task<int>>(), MetricName.MI_COUNT_QUERY_DURATION), Times.Once);
        _mockTelemetryService.Verify(t => t.RunAndTrackMetricsAsync(It.IsAny<Task<List<Dtos.HccDataset>>>(), MetricName.MI_FETCH_QUERY_DURATION), Times.Once);
    }

    [Fact]
    public async Task ExecuteFetchQuery_ShouldReturnData_WithAppliedFilters()
    {
        var query = new GetHccDatasetByDateQuery { HccDate = "01-01-2024" };
        var mockSet = new Mock<DbSet<GoldMaHcc>>();
        _mockDbContext.Setup(c => c.GOLD_MA_HCC).Returns(mockSet.Object);

        var result = await _handler.ExecuteFetchQuery(query);

        Assert.NotNull(result);
        mockSet.Verify(m => m.Skip(It.IsAny<int>()).Take(It.IsAny<int>()).ToListAsync(), Times.Once);
    }

    [Fact]
    public async Task ExecuteFetchQuery_ShouldReturnEmptyList_WhenNoDataFound()
    {
        var query = new GetHccDatasetByDateQuery { HccDate = "01-01-2024" };
        var mockSet = new Mock<DbSet<GoldMaHcc>>();
        mockSet.Setup(m => m.ToListAsync()).ReturnsAsync(new List<GoldMaHcc>());
        _mockDbContext.Setup(c => c.GOLD_MA_HCC).Returns(mockSet.Object);

        var result = await _handler.ExecuteFetchQuery(query);

        Assert.Empty(result);
    }

    [Fact]
    public async Task ExecuteCountQuery_ShouldReturnCount_WithAppliedFilters()
    {
        var query = new GetHccDatasetByDateQuery { HccDate = "01-01-2024" };
        var mockSet = new Mock<DbSet<GoldMaHcc>>();
        _mockDbContext.Setup(c => c.GOLD_MA_HCC).Returns(mockSet.Object);

        var result = await _handler.ExecuteCountQuery(query);

        Assert.Equal(0, result);
        mockSet.Verify(m => m.CountAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldLogError_WhenDatabaseExceptionOccurs()
    {
        var query = new GetHccDatasetByDateQuery { HccDate = "01-01-2024" };
        _mockValidator
            .Setup(v => v.ValidateAsync(query, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _mockDbContextFactory
            .Setup(f => f.GetDbContextAsync())
            .ThrowsAsync(new Exception("Database error"));

        await Assert.ThrowsAsync<Exception>(() => _handler.Handle(query, CancellationToken.None));
        _mockLogger.Verify(l => l.LogError(It.IsAny<Exception>(), "Error executing query"), Times.Once);
    }

    [Theory]
    [InlineData("01-01-2024", 1, 10)]
    [InlineData("invalid-date", 1, 10)]
    public async Task Handle_ShouldHandleVariousDateFormats(string hccDate, int pageNumber, int pageSize)
    {
        var query = new GetHccDatasetByDateQuery { HccDate = hccDate, PageNumber = pageNumber, PageSize = pageSize };
        _mockValidator
            .Setup(v => v.ValidateAsync(query, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        await _handler.Handle(query, CancellationToken.None);

        _mockTelemetryService.Verify(t => t.RunAndTrackMetricsAsync(It.IsAny<Task<int>>(), MetricName.MI_COUNT_QUERY_DURATION), Times.Once);
        _mockTelemetryService.Verify(t => t.RunAndTrackMetricsAsync(It.IsAny<Task<List<Dtos.HccDataset>>>(), MetricName.MI_FETCH_QUERY_DURATION), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldRespectPaginationBoundaries()
    {
        var query = new GetHccDatasetByDateQuery { HccDate = "01-01-2024", PageNumber = -1, PageSize = 0 };
        _mockValidator
            .Setup(v => v.ValidateAsync(query, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        await _handler.Handle(query, CancellationToken.None);

        _mockTelemetryService.Verify(t => t.RunAndTrackMetricsAsync(It.IsAny<Task<int>>(), MetricName.MI_COUNT_QUERY_DURATION), Times.Once);
        _mockTelemetryService.Verify(t => t.RunAndTrackMetricsAsync(It.IsAny<Task<List<Dtos.HccDataset>>>(), MetricName.MI_FETCH_QUERY_DURATION), Times.Once);
    }
}

```
