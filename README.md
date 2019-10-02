![PwnedPasswords logo](https://raw.githubusercontent.com/andrewlock/PwnedPasswords/master/logo.png)

*Pwned Passwords are hundreds of millions of real world passwords exposed in data breaches. This exposure makes them unsuitable for ongoing use as they're at much greater risk of being used to take over other accounts. They're searchable online below as well as being downloadable for use in other online system*

[![Build status](https://ci.appveyor.com/api/projects/status/xramjpbrgj36fwmr?svg=true)](https://ci.appveyor.com/project/andrewlock/PwnedPasswords)

[![client-nuget][client-nuget-badge]][client-nuget] [![validator-nuget][validator-nuget-badge]][validator-nuget]

[client-nuget]: https://www.nuget.org/packages/PwnedPasswords.Client/
[client-nuget-badge]: https://img.shields.io/nuget/v/PwnedPasswords.Client.svg?style=flat-square&label=PwnedPasswords.Client

[validator-nuget]: https://www.nuget.org/packages/PwnedPasswords.Validator/
[validator-nuget-badge]: https://img.shields.io/nuget/v/PwnedPasswords.Validator.svg?style=flat-square&label=PwnedPasswords.Validator


# [PwnedPasswords.Client](#PwnedPasswords-Client) and [PwnedPasswords.Validator](#PwnedPasswords-Validator)

This repository contains two libraries, _PwnedPasswords.Client_ and _PwnedPasswords.Validator_. 

* [PwnedPasswords.Validator](#PwnedPasswords-Validator) contains a [.NET Core 2.1 Typed HttpClient](https://blogs.msdn.microsoft.com/webdev/2018/02/28/asp-net-core-2-1-preview1-introducing-httpclient-factory/) (compatible with .NET Standard 2.0 and .NET Core 3.0) for accessing [Troy Hunt](https://twitter.com/troyhunt)'s [Have I Been Pwned passwords service](https://haveibeenpwned.com/Passwords).
* _PwnedPasswords.Validator_ contains an implementation of an [ASP.NET Core Identity](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/identity) `IPasswordValidator` that verifies the provided password has not been exposed in a known security breach.

## Why should you care?

As per Troy Hunt's website:

>### Password reuse and credential stuffing
>Password reuse is normal. It's extremely risky, but it's so common because it's easy and people aren't aware of the potential impact. Attacks such as credential stuffing take advantage of reused credentials by automating login attempts against systems using known emails and password pairs.

> ### NIST's guidance: check passwords against those obtained from previous data breaches
> The Pwned Passwords service was created after NIST released guidance specifically recommending that user-provided passwords be checked against existing data breaches . The rationale for this advice and suggestions for how applications may leverage this data is described in detail in the blog post titled Introducing 306 Million Freely Downloadable Pwned Passwords.

This package provides an `IPasswordValidator` for ASP.NET Core Identity that checks whether the provided password appears on the have I been pwned list. 



## PwnedPasswords.Client

.NET Core 2.1 introduces HTTPClient factory, an "opinionated factory for creating HttpClient instances". It allows easy configuration of `HttpClient` instances, manages their lifetime, and enables easy addition of common functionality, such as retry logic for transient HTTP errors.

_PwnedPasswords.Client_ provides the `IPwnedPasswordsClient` type, which can be used to easily access the PwnedPasswords API. It hooks into the _Microsoft.Extensions.DependencyInjection_ / ASP.NET Core DI container, and can be configured with optional fault handling etc as required.

### Getting started

Install the [_PwnedPasswords.Client_ NuGet package](https://www.nuget.org/packages/_PwnedPasswords.Client) into your project using:

```
dotnet add package PwnedPasswords.Client
```

When you install the package, it should be added to your `csproj`. Alternatively, you can add it directly by adding:

```xml
<PackageReference Include="PwnedPasswords.Client" Version="1.2.0" />
```


Add to your dependency injection container in `Startup.ConfigureServices` using the `AddPwnedPasswordHttpClient()` extension method. You can further configure the `IHttpClientBuilder` to add fault handling for example.

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddPwnedPasswordHttpClient()                      // add the client to the container
        .AddTransientHttpErrorPolicy(p => p.RetryAsync(3))     //configure the HttpClient used by the IPwnedPasswordsClient
        .AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(2)));

    // other configuration
}
```

You can also choose the minimum number of times a password must have appeared in a breach for it to be considered "pwned". So for example, if you only want to consider passwords that have appeared 20 times as pwned you can use the overload on `AddPwnedPasswordHttpClient()`:


```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddPwnedPasswordHttpClient(minimumFrequencyToConsiderPwned: 20);
}
```

You can also configure this using the standard Options pattern in ASP.NET Core, for example by loading the required value from a JSON value.

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddPwnedPasswordHttpClient();
    services.Configure<PwnedPasswordsClientOptions>(Configuration.GetSection("PwnedPasswords"));
}
```

## PwnedPasswords.Validator

_PwnedPasswords.Validator_ contains an implementation of an [ASP.NET Core Identity](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/identity) `IPasswordValidator` that verifies the provided password has not been exposed in a known security breach.


### Getting started

Install the [_PwnedPasswords.Validator_ NuGet package](https://www.nuget.org/packages/_PwnedPasswords.Validator_) into your project using:

```
dotnet add package PwnedPasswords.Validator
```

When you install the package, it should be added to your `csproj`. Alternatively, you can add it directly by adding:

```xml
<PackageReference Include="PwnedPasswords.Validator" Version="1.2.0" />
```

You can add the PwnedPasswords ASP.NET Core Identity Validator to your `IdentityBuilder` in `Startup.ConfigureServices` using the `AddPwnedPasswordValidator()` extension method. 

```csharp
public void ConfigureServices(IServiceCollection services)
{
     services.AddDefaultIdentity<IdentityUser>()
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddPwnedPasswordValidator<IdentityUser>(); // add the validator

    // other configuration
}
```

As for the `PwnedPasswordsClient` library, you can customize the minimum number of times a password must have appeared in a breach for it to be considered invalid by the validator. For example:

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddDefaultIdentity<IdentityUser>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddPwnedPasswordValidator<IdentityUser>(); // add the validator

    // set the minimum password to consider the password pwned
    services.Configure<PwnedPasswordsClientOptions>(Configuration.GetSection("PwnedPasswords"));
}
```


## Additional Resources
* [Have I been pwned API](https://haveibeenpwned.com/Passwords)
* [Password Rules Are Bullshit](https://blog.codinghorror.com/password-rules-are-bullshit/)
* [Introduction to ASP.NET Core Identity](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/identity)
* [Steve Gordon's series on HttpClientFactory](https://www.stevejgordon.co.uk/introduction-to-httpclientfactory-aspnetcore)
* [ASP.NET Core 2.1-preview1: Introducing HTTPClient factory](https://blogs.msdn.microsoft.com/webdev/2018/02/28/asp-net-core-2-1-preview1-introducing-httpclient-factory/)