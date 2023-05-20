﻿using SolutionTwo.Common.Interfaces;

namespace SolutionTwo.Identity.Configuration;

public class IdentityConfiguration : IVerifiableConfiguration
{
    public string? JwtKey { get; set; }

    public int? JwtExpiresMinutes { get; set; }

    public string? JwtIssuer { get; set; }

    public string? JwtAudience { get; set; }
}