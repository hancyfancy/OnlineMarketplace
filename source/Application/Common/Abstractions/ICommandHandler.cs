using MediatR;

namespace Demo.Application.Common.Abstractions
{
    public interface ICommandHandler<in TCommand, TResult>
        : IRequestHandler<TCommand, TResult> where TCommand : ICommand<TResult>
    {
    }
}
