using Application.Auth.Dtos;
using MediatR;

namespace Application.Auth.Commands.Login;

public record LoginCommand(string Username, string Password) : IRequest<AuthResultDto>;
