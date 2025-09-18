using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using W2;
using W2.Application.Contracts.IMS;
using W2.Identity;
using Microsoft.AspNetCore.Authorization;
[Route("api/services/app")]
[Authorize]
public class Hrmv2AppService : W2AppService
{
    private readonly SimpleGuidGenerator _simpleGuidGenerator;
    private readonly IRepository<W2CustomIdentityUser, Guid> _userRepository;

    public Hrmv2AppService(IRepository<W2CustomIdentityUser, Guid> userRepository)
    {
        _userRepository = userRepository;
        _simpleGuidGenerator = SimpleGuidGenerator.Instance;
    }

    [HttpPost("Hrmv2/CreateUserByHRM")]
    public async Task<IActionResult> CreateUserByHRM(CreateOrUpdateUserOtherToolDto input)
    {
        var user = await GetUserByEmailAsync(input.EmailAddress);
        if (user != null)
        {
            throw new UserFriendlyException($"User with email {input.EmailAddress} already exists.");
        }

        var newUser = MapToIdentityUser(input);
        await _userRepository.InsertAsync(newUser);

        return new ObjectResult(new
        {
            Message = "User created successfully",
        });
    }

    [HttpPost("Hrmv2/UpdateUserByHRM")]
    public async Task<IActionResult> UpdateUserByHRM(CreateOrUpdateUserOtherToolDto input)
    {
        var user = await GetUserByEmailOrThrow(input.EmailAddress);

        UpdateUserFromDto(user, input);
        await _userRepository.UpdateAsync(user);

        return new ObjectResult(new
        {
            Message = "User updated successfully",
        });
    }

    [HttpPost("Hrmv2/ConfirmUserQuit")]
    public async Task<IActionResult> ConfirmUserQuit(InputToUpdateUserStatusDto input)
    {
        return await UpdateUserStatusAsync(input.EmailAddress, false, "User is confirmed as Quit successfully");
    }

    [HttpPost("Hrmv2/ConfirmUserPause")]
    public async Task<IActionResult> ConfirmUserPause(InputToUpdateUserStatusDto input)
    {
        return await UpdateUserStatusAsync(input.EmailAddress, false, "User is confirmed as Pause successfully");
    }

    [HttpPost("Hrmv2/ConfirmUserMaternityLeave")]
    public async Task<IActionResult> ConfirmUserMaternityLeave(InputToUpdateUserStatusDto input)
    {
        return await UpdateUserStatusAsync(input.EmailAddress, false, "User is confirmed as Maternity Leave successfully");
    }

    [HttpPost("Hrmv2/ConfirmUserBackToWork")]
    public async Task<IActionResult> ConfirmUserBackToWork(InputToUpdateUserStatusDto input)
    {
        return await UpdateUserStatusAsync(input.EmailAddress, true, "User is confirmed as Back to Work successfully");
    }

    [HttpPost("Public/CheckConnect")]
    public Task<GetResultConnectDto> CheckConnectToIMSAsync()
    {
        return Task.FromResult(new GetResultConnectDto
        {
            IsConnected = true,
            Message = "Connected to IMS successfully"
        });
    }

    private async Task<W2CustomIdentityUser> GetUserByEmailAsync(string email)
    {
        var query = await _userRepository.GetQueryableAsync();
        return await query.FirstOrDefaultAsync(u => u.Email == email);
    }

    private async Task<W2CustomIdentityUser> GetUserByEmailOrThrow(string email)
    {
        var user = await GetUserByEmailAsync(email);
        if (user == null)
        {
            throw new UserFriendlyException($"Could not find the user with email: {email}");
        }
        return user;
    }

    private W2CustomIdentityUser MapToIdentityUser(CreateOrUpdateUserOtherToolDto input)
    {
        var user = new W2CustomIdentityUser(
            _simpleGuidGenerator.Create(),
            input.EmailAddress,
            input.EmailAddress)
        {
            Name = input.Name,
            Surname = input.Surname,
            MezonUserId = input.MezonUserId,
        };

        user.SetUserName(input.EmailAddress);
        user.SetEmail(input.EmailAddress);
        user.SetPhoneNumber(input.EmergencyContactPhone);
        user.SetMezonUserId(input.MezonUserId);

        return user;
    }

    private void UpdateUserFromDto(W2CustomIdentityUser user, CreateOrUpdateUserOtherToolDto input)
    {
        user.SetUserName(input.EmailAddress);
        user.Name = input.Name;
        user.Surname = input.Surname;
        user.SetEmail(input.EmailAddress);
        user.SetPhoneNumber(input.EmergencyContactPhone);
        user.SetMezonUserId(input.MezonUserId);
    }

    private async Task<IActionResult> UpdateUserStatusAsync(string email, bool isActive, string message)
    {
        var user = await GetUserByEmailOrThrow(email);
        user.SetIsActive(isActive);
        await _userRepository.UpdateAsync(user);
        return new ObjectResult(new
        {
            Message = message,
        });
    }
}
