﻿using Microsoft.Extensions.Configuration;
using SolutionTwo.Api.Exceptions;

namespace SolutionTwo.Common.Extensions;

public static class ConfigurationExtensions
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="configuration"></param>
    /// <param name="withValidation"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    /// <exception cref="ConfigurationVerificationException">Throws if any property of T is null or empty</exception>
    public static T GetSection<T>(this IConfiguration configuration, bool withValidation = true)
        where T : class
    {
        var type = typeof(T);
        var configurationSection = configuration.GetSection(type.Name).Get(type) as T;

        if (configurationSection == null)
            throw new ConfigurationVerificationException(type.Name);

        if (withValidation)
        {
            var invalidProperties = new List<string>();
            var properties = type.GetProperties();
            foreach (var property in properties)
            {
                var value = property.GetValue(configurationSection);
                if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                {
                    invalidProperties.Add(property.Name);
                }
            }
        
            if (invalidProperties.Any())
                throw new ConfigurationVerificationException(type.Name, invalidProperties);
        }
        
        return configurationSection;
    }
}