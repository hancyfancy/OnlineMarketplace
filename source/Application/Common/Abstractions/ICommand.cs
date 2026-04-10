using MediatR;

namespace Demo.Application.Common.Abstractions
{
    public interface ICommand<TResult> : IRequest<TResult>
    {
    }
}
