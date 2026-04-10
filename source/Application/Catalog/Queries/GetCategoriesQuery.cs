using Demo.Application.Catalog.Dtos;
using Demo.Application.Common.Abstractions;
using Demo.Domain.Primitives;

namespace Demo.Application.Catalog.Queries;

public class GetCategoriesQuery : IQuery<Result<List<CategoryDto>>>
{
}
