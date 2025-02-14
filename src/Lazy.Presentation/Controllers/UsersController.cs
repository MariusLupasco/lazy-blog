﻿using Lazy.Application.Posts.GetPostByUserId;
using Lazy.Application.Users.CheckIfUserNameIsUnique;
using Lazy.Application.Users.CreateUser;
using Lazy.Application.Users.GetUserById;
using Lazy.Application.Users.Login;
using Lazy.Application.Users.UpdateUser;
using Lazy.Application.Users.UploadUserAvatar;
using Lazy.Domain.Shared;
using Lazy.Presentation.Abstractions;
using Lazy.Presentation.Contracts.Users;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Lazy.Presentation.Controllers;

[Authorize]
[Route("api/users")]
public class UsersController : ApiController
{
    public UsersController(
        ISender sender, 
        ILogger<UsersController> logger)
        : base(sender, logger)
    {
    }

    [AllowAnonymous]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetUserById(Guid id, CancellationToken ct)
    {
        var query = new GetUserByIdQuery(id);

        Result<UserResponse> response = await Sender.Send(query, ct);

        return response.IsSuccess ? Ok(response.Value) : NotFound(response.Error);
    }

    [AllowAnonymous]
    [HttpGet("{username}/available")]
    public async Task<IActionResult> CheckIfUserNameIsAvailable(string username, CancellationToken ct)
    {
        var query = new CheckIfUserNameIsUnique(username);

        var result = await Sender.Send(query, ct);

        return result.IsFailure ? HandleFailure(result) : Ok(result.Value);
    }

    [HttpPost("{id:guid}/avatar")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UploadAvatar(
        [FromRoute] Guid id,
        [FromForm] IFormFile file, 
        CancellationToken ct)
    {
        var command = new UploadUserAvatarCommand(id, file);

        var result = await Sender.Send(command, ct);

        return result.IsFailure ? HandleFailure(result) : NoContent();
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> LoginUser(
        [FromBody] LoginRequest request,
        CancellationToken ct)
    {
        var command = new LoginCommand(request.Email, request.Password);

        Result<LoginResponse> tokenResult = await Sender.Send(command, ct);

        return tokenResult.IsFailure ? HandleFailure(tokenResult) : Ok(tokenResult.Value);
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> RegisterUser(
        [FromBody] RegisterUserRequest request,
        CancellationToken ct)
    {
        var command = new CreateUserCommand(
            request.Email,
            request.FirstName,
            request.LastName,
            request.UserName,
            request.Password);

        Result<Guid> result = await Sender.Send(command, ct);

        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        return CreatedAtAction(
            nameof(GetUserById),
            new { id = result.Value },
            result.Value);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateUser(
        Guid id,
        [FromBody] UpdateUserRequest request,
        CancellationToken ct)
    {
        var command = new UpdateUserCommand(
            id,
            request.FirstName,
            request.LastName,
            request.Username);

        Result result = await Sender.Send(
            command,
            ct);

        if (result.IsFailure)
        {
            HandleFailure(result);
        }

        return NoContent();
    }

    [AllowAnonymous]
    [HttpGet("{id:guid}/posts")]
    public async Task<IActionResult> GetPostByUserId(
        Guid id,
        [FromQuery] int offset, 
        CancellationToken ct)
    {
        var query = new GetPostByUserIdQuery(id, offset);

        Result<UserPostResponse> response = await Sender.Send(query, ct);

        return response.IsSuccess ? Ok(response.Value) : NotFound(response.Error);
    }
}