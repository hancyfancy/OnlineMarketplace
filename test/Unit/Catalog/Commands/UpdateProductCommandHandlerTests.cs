using Moq;
using Demo.Application.Catalog.Commands;
using Demo.Application.Catalog.Dtos;
using Demo.Domain.Abstractions;
using Demo.Domain.Models;
using Demo.Domain.Primitives;

namespace Unit.Catalog.Commands;

public class UpdateProductCommandHandlerTests
{
    private readonly Mock<IProductRepository> _repoMock = new();
    private readonly UpdateProductCommandHandler _sut;
    private readonly Guid _categoryId = Guid.NewGuid();

    public UpdateProductCommandHandlerTests()
    {
        _sut = new UpdateProductCommandHandler(_repoMock.Object);

        _repoMock
            .Setup(r => r.ResolveCategoryIdByGuidAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(_categoryId));

        _repoMock
            .Setup(r => r.UpdateAsync(It.IsAny<ProductUpdateInput>(), It.IsAny<List<ProductVariantInput>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());
    }

    [Fact]
    public async Task Handle_ReturnsSuccess_WhenProductUpdated()
    {
        var result = await _sut.Handle(BuildCommand(), CancellationToken.None);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_ReturnsFailure_WhenCategoryNotFound()
    {
        _repoMock
            .Setup(r => r.ResolveCategoryIdByGuidAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<Guid>("CategoryNotFound", "Category not found."));

        var result = await _sut.Handle(BuildCommand(), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("CategoryNotFound", result.Error.Code);
    }

    [Fact]
    public async Task Handle_ReturnsFailure_WhenUpdateFails()
    {
        _repoMock
            .Setup(r => r.UpdateAsync(It.IsAny<ProductUpdateInput>(), It.IsAny<List<ProductVariantInput>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure("UpdateFail", "Database error"));

        var result = await _sut.Handle(BuildCommand(), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("UpdateFail", result.Error.Code);
    }

    [Fact]
    public async Task Handle_DoesNotCallUpdate_WhenCategoryResolutionFails()
    {
        _repoMock
            .Setup(r => r.ResolveCategoryIdByGuidAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<Guid>("CategoryNotFound", "Category not found."));

        await _sut.Handle(BuildCommand(), CancellationToken.None);

        _repoMock.Verify(r => r.UpdateAsync(It.IsAny<ProductUpdateInput>(), It.IsAny<List<ProductVariantInput>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_PassesProductGuid_ToUpdateInput()
    {
        ProductUpdateInput? captured = null;

        _repoMock
            .Setup(r => r.UpdateAsync(It.IsAny<ProductUpdateInput>(), It.IsAny<List<ProductVariantInput>>(), It.IsAny<CancellationToken>()))
            .Callback<ProductUpdateInput, List<ProductVariantInput>, CancellationToken>((p, _, __) => captured = p)
            .ReturnsAsync(Result.Success());

        var productGuid = Guid.NewGuid();
        await _sut.Handle(BuildCommand(productGuid: productGuid), CancellationToken.None);

        Assert.Equal(productGuid, captured!.ProductGuid);
    }

    [Fact]
    public async Task Handle_PassesSellerGuid_ToUpdateInput()
    {
        ProductUpdateInput? captured = null;

        _repoMock
            .Setup(r => r.UpdateAsync(It.IsAny<ProductUpdateInput>(), It.IsAny<List<ProductVariantInput>>(), It.IsAny<CancellationToken>()))
            .Callback<ProductUpdateInput, List<ProductVariantInput>, CancellationToken>((p, _, __) => captured = p)
            .ReturnsAsync(Result.Success());

        var sellerGuid = Guid.NewGuid();
        await _sut.Handle(BuildCommand(sellerGuid: sellerGuid), CancellationToken.None);

        Assert.Equal(sellerGuid, captured!.SellerGuid);
    }

    [Fact]
    public async Task Handle_PassesCategoryId_FromResolvedCategory()
    {
        ProductUpdateInput? captured = null;

        _repoMock
            .Setup(r => r.UpdateAsync(It.IsAny<ProductUpdateInput>(), It.IsAny<List<ProductVariantInput>>(), It.IsAny<CancellationToken>()))
            .Callback<ProductUpdateInput, List<ProductVariantInput>, CancellationToken>((p, _, __) => captured = p)
            .ReturnsAsync(Result.Success());

        await _sut.Handle(BuildCommand(), CancellationToken.None);

        Assert.Equal(_categoryId, captured!.CategoryId);
    }

    [Fact]
    public async Task Handle_MapsProductFields_Correctly()
    {
        ProductUpdateInput? captured = null;

        _repoMock
            .Setup(r => r.UpdateAsync(It.IsAny<ProductUpdateInput>(), It.IsAny<List<ProductVariantInput>>(), It.IsAny<CancellationToken>()))
            .Callback<ProductUpdateInput, List<ProductVariantInput>, CancellationToken>((p, _, __) => captured = p)
            .ReturnsAsync(Result.Success());

        await _sut.Handle(BuildCommand(name: "Updated Product", brand: "BrandX", isActive: false), CancellationToken.None);

        Assert.Equal("Updated Product", captured!.Name);
        Assert.Equal("BrandX", captured.Brand);
        Assert.False(captured.IsActive);
    }

    [Theory]
    [InlineData("My Test Product", "my-test-product")]
    [InlineData("Hello World!", "hello-world")]
    [InlineData("Product #2", "product-2")]
    public async Task Handle_GeneratesSlug_FromProductName(string name, string expectedSlugPrefix)
    {
        ProductUpdateInput? captured = null;

        _repoMock
            .Setup(r => r.UpdateAsync(It.IsAny<ProductUpdateInput>(), It.IsAny<List<ProductVariantInput>>(), It.IsAny<CancellationToken>()))
            .Callback<ProductUpdateInput, List<ProductVariantInput>, CancellationToken>((p, _, __) => captured = p)
            .ReturnsAsync(Result.Success());

        await _sut.Handle(BuildCommand(name: name), CancellationToken.None);

        Assert.NotNull(captured!.Slug);
        Assert.StartsWith(expectedSlugPrefix + "-", captured.Slug);
        Assert.Equal(captured.Slug, captured.Slug.ToLowerInvariant());
    }

    [Fact]
    public async Task Handle_MapsVariants_Correctly()
    {
        List<ProductVariantInput>? capturedVariants = null;

        _repoMock
            .Setup(r => r.UpdateAsync(It.IsAny<ProductUpdateInput>(), It.IsAny<List<ProductVariantInput>>(), It.IsAny<CancellationToken>()))
            .Callback<ProductUpdateInput, List<ProductVariantInput>, CancellationToken>((_, v, __) => capturedVariants = v)
            .ReturnsAsync(Result.Success());

        var variant = new UpdateProductVariantDto
        {
            Name = "Blue / Small",
            Color = "Blue",
            Size = "S",
            Material = "Polyester",
            Condition = "new",
            UnitPrice = 19.99m,
            CompareAtPrice = 24.99m,
            UnitsInStock = 5,
            UnitsReserved = 1,
            ImageUrl = "https://example.com/blue.jpg",
            SortIndex = 2
        };

        await _sut.Handle(BuildCommand(variants: [variant]), CancellationToken.None);

        Assert.Single(capturedVariants!);
        var v = capturedVariants![0];
        Assert.Equal("Blue / Small", v.Name);
        Assert.Equal("Blue", v.Color);
        Assert.Equal("S", v.Size);
        Assert.Equal("Polyester", v.Material);
        Assert.Equal("new", v.Condition);
        Assert.Equal(19.99m, v.UnitPrice);
        Assert.Equal(24.99m, v.CompareAtPrice);
        Assert.Equal(5, v.UnitsInStock);
        Assert.Equal(1, v.UnitsReserved);
        Assert.Equal("https://example.com/blue.jpg", v.ImageUrl);
        Assert.Equal(2, v.SortIndex);
    }

    [Fact]
    public async Task Handle_UsesSortIndex_FromPosition_WhenVariantSortIndexIsZero()
    {
        List<ProductVariantInput>? capturedVariants = null;

        _repoMock
            .Setup(r => r.UpdateAsync(It.IsAny<ProductUpdateInput>(), It.IsAny<List<ProductVariantInput>>(), It.IsAny<CancellationToken>()))
            .Callback<ProductUpdateInput, List<ProductVariantInput>, CancellationToken>((_, v, __) => capturedVariants = v)
            .ReturnsAsync(Result.Success());

        var variants = new List<UpdateProductVariantDto>
        {
            new() { UnitPrice = 10m, SortIndex = 0 },
            new() { UnitPrice = 20m, SortIndex = 0 }
        };

        await _sut.Handle(BuildCommand(variants: variants), CancellationToken.None);

        Assert.Equal(0, capturedVariants![0].SortIndex);
        Assert.Equal(1, capturedVariants[1].SortIndex);
    }

    [Fact]
    public async Task Handle_CallsUpdateAsync_ExactlyOnce()
    {
        await _sut.Handle(BuildCommand(), CancellationToken.None);

        _repoMock.Verify(r => r.UpdateAsync(It.IsAny<ProductUpdateInput>(), It.IsAny<List<ProductVariantInput>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    private static UpdateProductCommand BuildCommand(
        Guid? productGuid = null,
        Guid? sellerGuid = null,
        string name = "Test Product",
        string? brand = null,
        bool isActive = true,
        List<UpdateProductVariantDto>? variants = null)
    {
        return new UpdateProductCommand(
            sellerGuid ?? Guid.NewGuid(),
            new UpdateProductDto
            {
                ProductGuid = productGuid ?? Guid.NewGuid(),
                CategoryGuid = Guid.NewGuid(),
                Name = name,
                Brand = brand,
                IsActive = isActive,
                Variants = variants ?? [new UpdateProductVariantDto { UnitPrice = 9.99m }]
            });
    }
}
