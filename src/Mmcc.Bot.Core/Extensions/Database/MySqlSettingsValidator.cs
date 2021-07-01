﻿using FluentValidation;
using Mmcc.Bot.Database.Settings;

namespace Mmcc.Bot.Core.Extensions.Database
{
    public class MySqlSettingsValidator : AbstractValidator<MySqlSettings>
    {
        public MySqlSettingsValidator()
        {
            RuleFor(s => s.MySqlVersionString)
                .NotEmpty();

            RuleFor(s => s.RetryAmount)
                .NotEmpty()
                .GreaterThanOrEqualTo(0);

            RuleFor(s => s.ServerIp)
                .NotEmpty();

            RuleFor(s => s.Port)
                .NotEmpty()
                .GreaterThanOrEqualTo(0);

            RuleFor(s => s.DatabaseName)
                .NotEmpty();

            RuleFor(s => s.Username)
                .NotEmpty();

            RuleFor(s => s.Password)
                .NotEmpty();
        }
    }
}