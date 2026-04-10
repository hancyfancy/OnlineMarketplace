using MediatR;

namespace Demo.Application.Common.Abstractions
{
    public interface IQuery<out TResult> : IRequest<TResult>
    {
    }
}
