using FluentValidation;
using FluentValidationSample.Models;

namespace FluentValidationSample.Validators;

/// <summary>
/// 用户注册请求验证器
/// </summary>
public class UserRegistrationValidator : AbstractValidator<UserRegistrationRequest>
{
    /// <summary>
    /// 构造函数
    /// </summary>
    public UserRegistrationValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("用户名不能为空")
            .Length(3, 20).WithMessage("用户名长度必须在3-20个字符之间");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("邮箱不能为空")
            .EmailAddress().WithMessage("邮箱格式不正确");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("密码不能为空")
            .MinimumLength(6).WithMessage("密码长度不能少于6个字符")
            .Matches("[A-Z]").WithMessage("密码必须包含至少一个大写字母")
            .Matches("[a-z]").WithMessage("密码必须包含至少一个小写字母")
            .Matches("[0-9]").WithMessage("密码必须包含至少一个数字")
            .Matches("[^a-zA-Z0-9]").WithMessage("密码必须包含至少一个特殊字符");

        RuleFor(x => x.ConfirmPassword)
            .Equal(x => x.Password).WithMessage("两次输入的密码不一致");

        RuleFor(x => x.Age)
            .GreaterThanOrEqualTo(18).WithMessage("年龄必须大于或等于18岁");
    }
} 