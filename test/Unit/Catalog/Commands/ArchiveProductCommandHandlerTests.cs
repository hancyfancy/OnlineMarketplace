using Moq;
using Demo.Application.Catalog.Commands;
using Demo.Domain.Abstractions;
using Demo.Domain.Primitives;

namespace Unit.Catalog.Commands;

public class ArchiveProductCommandHandlerTests
{
    private readonly Mock<IProductRepository> _repoMock = new();
    private readonly ArchiveProductCommandHandler _sut;

    public ArchiveProductCommandHandlerTests()
    {
        _sut = new ArchiveProductCommandHandler(_repoMock.Object);
    }

    [Fact]
    public async Task Handle_ReturnsSuccess_WhenArchiveSucceeds()
    {
        _repoMock
            .Setup(r => r.ArchiveAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        var result = await _sut.Handle(BuildCommand(), CancellationToken.None);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_ReturnsFailure_WhenProductNotFound()
    {
        _repoMock
            .Setup(r => r.ArchiveAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure("ProductNotFound", "Product not found."));

        var result = await _sut.Handle(BuildCommand(), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("ProductNotFound", result.Error.Code);
        Assert.Equal("Product not found.", result.Error.Description);
    }

    [Fact]
    public async Task Handle_ReturnsFailure_WhenArchiveFails()
    {
        _repoMock
            .Setup(r => r.ArchiveAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure("ArchiveFail", "Database error"));

        var result = await _sut.Handle(BuildCommand(), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("ArchiveFail", result.Error.Code);
    }

    [Fact]
    public async Task Handle_PassesProductGuid_ToRepository()
    {
        Guid? capturedProductGuid = null;

        _repoMock
            .Setup(r => r.ArchiveAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .Callback<Guid, Guid, CancellationToken>((pg, _, __) => capturedProductGuid = pg)
            .ReturnsAsync(Result.Success());

        var expectedGuid = Guid.NewGuid();
        await _sut.Handle(BuildCommand(productGuid: expectedGuid), CancellationToken.None);

        Assert.Equal(expectedGuid, capturedProductGuid);
    }

    [Fact]
    public async Task Handle_PassesSellerGuid_ToRepository()
    {
        Guid? capturedSellerGuid = null;

        _repoMock
            .Setup(r => r.ArchiveAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .Callback<Guid, Guid, CancellationToken>((_, sg, __) => capturedSellerGuid = sg)
            .ReturnsAsync(Result.Success());

        var expectedSellerGuid = Guid.NewGuid();
        await _sut.Handle(BuildCommand(sellerGuid: expectedSellerGuid), CancellationToken.None);

        Assert.Equal(expectedSellerGuid, capturedSellerGuid);
    }

    [Fact]
    public async Task Handle_CallsArchiveAsync_ExactlyOnce()
    {
        _repoMock
            .Setup(r => r.ArchiveAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        await _sut.Handle(BuildCommand(), CancellationToken.None);

        _repoMock.Verify(r => r.ArchiveAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    private static ArchiveProductCommand BuildCommand(
        Guid? productGuid = null,
        Guid? sellerGuid = null)
    {
        return new ArchiveProductCommand(productGuid ?? Guid.NewGuid(), sellerGuid ?? Guid.NewGuid());
    }
}
