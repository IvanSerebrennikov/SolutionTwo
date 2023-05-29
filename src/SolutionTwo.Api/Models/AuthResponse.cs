﻿using SolutionTwo.Business.Identity.Models.Auth.Outgoing;

namespace SolutionTwo.Api.Models;

public class AuthResponse
{
    public AuthResponse(TokensPairModel tokens, string firstName, string lastName)
    {
        Tokens = tokens;
        FirstName = firstName;
        LastName = lastName;
    }

    public TokensPairModel Tokens { get; }

    public string FirstName { get; }

    public string LastName { get; }
}