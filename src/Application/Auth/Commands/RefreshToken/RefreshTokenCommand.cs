using Application.Auth.Dtos;
using MediatR;

namespace Application.Auth.Commands.RefreshToken;

public record RefreshTokenCommand(string RefreshToken) : IRequest<AuthResultDto>;
