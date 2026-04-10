using Demo.Application.Common.Abstractions;
using Demo.Domain.Primitives;

namespace Demo.Application.Catalog.Commands;

public record ArchiveProductCommand(Guid ProductGuid, Guid SellerGuid) : ICommand<Result>;
