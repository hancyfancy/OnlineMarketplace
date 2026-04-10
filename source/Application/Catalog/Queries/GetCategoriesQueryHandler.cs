using Demo.Application.Catalog.Dtos;
using Demo.Application.Common.Abstractions;
using Demo.Domain.Abstractions;
using Demo.Domain.Primitives;

namespace Demo.Application.Catalog.Queries;

public class GetCategoriesQueryHandler : IQueryHandler<GetCategoriesQuery, Result<List<CategoryDto>>>
{
    private readonly IProductRepository _productRepository;

    public GetCategoriesQueryHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<Result<List<CategoryDto>>> Handle(GetCategoriesQuery query, CancellationToken cancellationToken)
    {
        var result = await _productRepository.GetCategoriesAsync(cancellationToken);

        if (!result.IsSuccess)
            return Result.Failure<List<CategoryDto>>(result.Error.Code, result.Error.Description);

        var dtos = result.Value.Select(c => new CategoryDto
        {
            CategoryId       = c.CategoryId,
            CategoryGuid     = c.CategoryGuid,
            Name             = c.Name,
            Slug             = c.Slug,
            ParentCategoryId = c.ParentCategoryId,
            Level            = c.Level
        }).ToList();

        return Result.Success(dtos);
    }
}
