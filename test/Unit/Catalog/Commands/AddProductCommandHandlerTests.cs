using Moq;
using Demo.Application.Catalog.Commands;
using Demo.Application.Catalog.Dtos;
using Demo.Domain.Abstractions;
using Demo.Domain.Models;
using Demo.Domain.Primitives;

namespace Unit.Catalog.Commands;

public class AddProductCommandHandlerTests
{
    private readonly Mock<IProductRepository> _repoMock = new();
    private readonly AddProductCommandHandler _sut;
    private readonly Guid _categoryId = Guid.NewGuid();

    public AddProductCommandHandlerTests()
    {
        _sut = new AddProductCommandHandler(_repoMock.Object);

        _repoMock
            .Setup(r => r.ResolveCategoryIdByGuidAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(_categoryId));

        _repoMock
            .Setup(r => r.AddAsync(It.IsAny<ProductAddInput>(), It.IsAny<List<ProductVariantInput>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());
    }

    [Fact]
    public async Task Handle_ReturnsSuccess_WhenProductAdded()
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
    public async Task Handle_ReturnsFailure_WhenAddFails()
    {
        _repoMock
            .Setup(r => r.AddAsync(It.IsAny<ProductAddInput>(), It.IsAny<List<ProductVariantInput>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure("AddFail", "Database error"));

        var result = await _sut.Handle(BuildCommand(), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("AddFail", result.Error.Code);
    }

    [Fact]
    public async Task Handle_DoesNotCallAdd_WhenCategoryResolutionFails()
    {
        _repoMock
            .Setup(r => r.ResolveCategoryIdByGuidAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<Guid>("CategoryNotFound", "Category not found."));

        await _sut.Handle(BuildCommand(), CancellationToken.None);

        _repoMock.Verify(r => r.AddAsync(It.IsAny<ProductAddInput>(), It.IsAny<List<ProductVariantInput>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_SetsIsActive_ToTrue()
    {
        ProductAddInput? captured = null;

        _repoMock
            .Setup(r => r.AddAsync(It.IsAny<ProductAddInput>(), It.IsAny<List<ProductVariantInput>>(), It.IsAny<CancellationToken>()))
            .Callback<ProductAddInput, List<ProductVariantInput>, CancellationToken>((p, _, __) => captured = p)
            .ReturnsAsync(Result.Success());

        await _sut.Handle(BuildCommand(), CancellationToken.None);

        Assert.True(captured!.IsActive);
    }

    [Fact]
    public async Task Handle_PassesSellerGuid_ToProductInput()
    {
        ProductAddInput? captured = null;

        _repoMock
            .Setup(r => r.AddAsync(It.IsAny<ProductAddInput>(), It.IsAny<List<ProductVariantInput>>(), It.IsAny<CancellationToken>()))
            .Callback<ProductAddInput, List<ProductVariantInput>, CancellationToken>((p, _, __) => captured = p)
            .ReturnsAsync(Result.Success());

        var sellerGuid = Guid.NewGuid();
        await _sut.Handle(BuildCommand(sellerGuid: sellerGuid), CancellationToken.None);

        Assert.Equal(sellerGuid, captured!.SellerGuid);
    }

    [Fact]
    public async Task Handle_PassesCategoryId_FromResolvedCategory()
    {
        ProductAddInput? captured = null;

        _repoMock
            .Setup(r => r.AddAsync(It.IsAny<ProductAddInput>(), It.IsAny<List<ProductVariantInput>>(), It.IsAny<CancellationToken>()))
            .Callback<ProductAddInput, List<ProductVariantInput>, CancellationToken>((p, _, __) => captured = p)
            .ReturnsAsync(Result.Success());

        await _sut.Handle(BuildCommand(), CancellationToken.None);

        Assert.Equal(_categoryId, captured!.CategoryId);
    }

    [Fact]
    public async Task Handle_MapsProductFields_Correctly()
    {
        ProductAddInput? captured = null;

        _repoMock
            .Setup(r => r.AddAsync(It.IsAny<ProductAddInput>(), It.IsAny<List<ProductVariantInput>>(), It.IsAny<CancellationToken>()))
            .Callback<ProductAddInput, List<ProductVariantInput>, CancellationToken>((p, _, __) => captured = p)
            .ReturnsAsync(Result.Success());

        await _sut.Handle(BuildCommand(name: "My Product", brand: "Acme", shortDesc: "Short desc"), CancellationToken.None);

        Assert.Equal("My Product", captured!.Name);
        Assert.Equal("Acme", captured.Brand);
        Assert.Equal("Short desc", captured.ShortDescription);
    }

    [Theory]
    [InlineData("My Test Product", "my-test-product")]
    [InlineData("Hello World!", "hello-world")]
    [InlineData("Product #1", "product-1")]
    [InlineData("  Spaces   Around  ", "spaces-around")]
    public async Task Handle_GeneratesSlug_FromProductName(string name, string expectedSlugPrefix)
    {
        ProductAddInput? captured = null;

        _repoMock
            .Setup(r => r.AddAsync(It.IsAny<ProductAddInput>(), It.IsAny<List<ProductVariantInput>>(), It.IsAny<CancellationToken>()))
            .Callback<ProductAddInput, List<ProductVariantInput>, CancellationToken>((p, _, __) => captured = p)
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
            .Setup(r => r.AddAsync(It.IsAny<ProductAddInput>(), It.IsAny<List<ProductVariantInput>>(), It.IsAny<CancellationToken>()))
            .Callback<ProductAddInput, List<ProductVariantInput>, CancellationToken>((_, v, __) => capturedVariants = v)
            .ReturnsAsync(Result.Success());

        var variant = new AddProductVariantDto
        {
            Name = "Red / Large",
            Color = "Red",
            Size = "L",
            Material = "Cotton",
            Condition = "new",
            UnitPrice = 29.99m,
            CompareAtPrice = 39.99m,
            UnitsInStock = 10,
            UnitsReserved = 2,
            ImageUrl = "https://example.com/img.jpg",
            SortIndex = 1
        };

        await _sut.Handle(BuildCommand(variants: [variant]), CancellationToken.None);

        Assert.Single(capturedVariants!);
        var v = capturedVariants![0];
        Assert.Equal("Red / Large", v.Name);
        Assert.Equal("Red", v.Color);
        Assert.Equal("L", v.Size);
        Assert.Equal("Cotton", v.Material);
        Assert.Equal("new", v.Condition);
        Assert.Equal(29.99m, v.UnitPrice);
        Assert.Equal(39.99m, v.CompareAtPrice);
        Assert.Equal(10, v.UnitsInStock);
        Assert.Equal(2, v.UnitsReserved);
        Assert.Equal("https://example.com/img.jpg", v.ImageUrl);
        Assert.Equal(1, v.SortIndex);
    }

    [Fact]
    public async Task Handle_UsesSortIndex_FromPosition_WhenVariantSortIndexIsZero()
    {
        List<ProductVariantInput>? capturedVariants = null;

        _repoMock
            .Setup(r => r.AddAsync(It.IsAny<ProductAddInput>(), It.IsAny<List<ProductVariantInput>>(), It.IsAny<CancellationToken>()))
            .Callback<ProductAddInput, List<ProductVariantInput>, CancellationToken>((_, v, __) => capturedVariants = v)
            .ReturnsAsync(Result.Success());

        var variants = new List<AddProductVariantDto>
        {
            new() { UnitPrice = 10m, SortIndex = 0 },
            new() { UnitPrice = 20m, SortIndex = 0 },
            new() { UnitPrice = 30m, SortIndex = 0 }
        };

        await _sut.Handle(BuildCommand(variants: variants), CancellationToken.None);

        Assert.Equal(0, capturedVariants![0].SortIndex);
        Assert.Equal(1, capturedVariants[1].SortIndex);
        Assert.Equal(2, capturedVariants[2].SortIndex);
    }

    [Fact]
    public async Task Handle_PreservesExplicitSortIndex_WhenGreaterThanZero()
    {
        List<ProductVariantInput>? capturedVariants = null;

        _repoMock
            .Setup(r => r.AddAsync(It.IsAny<ProductAddInput>(), It.IsAny<List<ProductVariantInput>>(), It.IsAny<CancellationToken>()))
            .Callback<ProductAddInput, List<ProductVariantInput>, CancellationToken>((_, v, __) => capturedVariants = v)
            .ReturnsAsync(Result.Success());

        var variants = new List<AddProductVariantDto>
        {
            new() { UnitPrice = 10m, SortIndex = 5 },
            new() { UnitPrice = 20m, SortIndex = 10 }
        };

        await _sut.Handle(BuildCommand(variants: variants), CancellationToken.None);

        Assert.Equal(5, capturedVariants![0].SortIndex);
        Assert.Equal(10, capturedVariants[1].SortIndex);
    }

    [Fact]
    public async Task Handle_CallsAddAsync_ExactlyOnce()
    {
        await _sut.Handle(BuildCommand(), CancellationToken.None);

        _repoMock.Verify(r => r.AddAsync(It.IsAny<ProductAddInput>(), It.IsAny<List<ProductVariantInput>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    private static AddProductCommand BuildCommand(
        Guid? sellerGuid = null,
        string name = "Test Product",
        string? brand = null,
        string? shortDesc = null,
        List<AddProductVariantDto>? variants = null)
    {
        return new AddProductCommand(
            sellerGuid ?? Guid.NewGuid(),
            new AddProductDto
            {
                CategoryGuid = Guid.NewGuid(),
                Name = name,
                Brand = brand,
                ShortDescription = shortDesc,
                Variants = variants ?? [new AddProductVariantDto { UnitPrice = 9.99m }]
            });
    }
}
