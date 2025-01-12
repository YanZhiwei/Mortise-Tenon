using FluentValidation;
using FluentValidationSample.Models;
using FluentValidationSample.Resources;
using Microsoft.Extensions.Localization;
using Tenon.FluentValidation.Extensions;

namespace FluentValidationSample.Validators;

/// <summary>
/// 用户注册请求验证器
/// </summary>
public class UserRegistrationValidator : AbstractValidator<UserRegistrationRequest>
{
    public UserRegistrationValidator(IStringLocalizer<ValidationMessages> localizer)
    {
        RuleFor(x => x.Username)
            .Required()
            .WithMessage(x => localizer["Username_Required"].Value)
            .Length(3, 20)
            .WithMessage(x => localizer["Username_Length", 3, 20].Value);

        RuleFor(x => x.Email)
            .Required()
            .WithMessage(x => localizer["Email_Required"].Value)
            .EmailAddress()
            .WithMessage(x => localizer["Email_Invalid"].Value);

        RuleFor(x => x.Password)
            .Required()
            .WithMessage(x => localizer["Password_Required"].Value)
            .MinimumLength(6)
            .WithMessage(x => localizer["Password_Length", 6].Value)
            .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{6,}$")
            .WithMessage(x => localizer["Password_Complexity"].Value);

        RuleFor(x => x.ConfirmPassword)
            .Required()
            .WithMessage(x => localizer["ConfirmPassword_Required"].Value)
            .Equal(x => x.Password)
            .WithMessage(x => localizer["ConfirmPassword_NotMatch"].Value);

        RuleFor(x => x.Age)
            .Required()
            .WithMessage(x => localizer["Age_Required"].Value)
            .GreaterThanOrEqualTo(18)
            .WithMessage(x => localizer["Age_Range", 18].Value);
    }
} 