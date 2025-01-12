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
            .WithMessage(localizer["Username_Required"])
            .Length(3, 20)
            .WithMessage(localizer["Username_Length"]);

        RuleFor(x => x.Email)
            .Required()
            .WithMessage(localizer["Email_Required"])
            .EmailAddress()
            .WithMessage(localizer["Email_Invalid"]);

        RuleFor(x => x.Password)
            .Required()
            .WithMessage(localizer["Password_Required"])
            .MinimumLength(6)
            .WithMessage(localizer["Password_Length"])
            .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{6,}$")
            .WithMessage(localizer["Password_Complexity"]);

        RuleFor(x => x.ConfirmPassword)
            .Required()
            .WithMessage(localizer["ConfirmPassword_Required"])
            .Equal(x => x.Password)
            .WithMessage(localizer["ConfirmPassword_NotMatch"]);

        RuleFor(x => x.Age)
            .Required()
            .WithMessage(localizer["Age_Required"])
            .GreaterThanOrEqualTo(18)
            .WithMessage(localizer["Age_Range"]);
    }
} 