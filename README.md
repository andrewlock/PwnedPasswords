![HaveIBeenPwnedValidator logo](https://raw.githubusercontent.com/andrewlock/HaveIBeenPwnedValidator/master/logo.png)

*Pwned Passwords are hundreds of millions of real world passwords exposed in data breaches. This exposure makes them unsuitable for ongoing use as they're at much greater risk of being used to take over other accounts. They're searchable online below as well as being downloadable for use in other online system*

# HaveIBeenPwnedValidator

[![Build status](https://ci.appveyor.com/api/projects/status/xramjpbrgj36fwmr?svg=true)](https://ci.appveyor.com/project/andrewlock/haveibeenpwnedvalidator)
<!--[![Travis](https://img.shields.io/travis/andrewlock/HaveIBeenPwnedValidator.svg?maxAge=3600&label=travis)](https://travis-ci.org/andrewlock/HaveIBeenPwnedValidator)-->
[![NuGet](https://img.shields.io/nuget/v/HaveIBeenPwnedValidator.svg)](https://www.nuget.org/packages/HaveIBeenPwnedValidator/)
[![MyGet CI](https://img.shields.io/myget/andrewlock-ci/v/HaveIBeenPwnedValidator.svg)](http://myget.org/gallery/acndrewlock-ci)

An implementation of an [ASP.NET Core Identity](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/identity) `IPasswordValidator` that verifies the provided password is not one of the most passwords exposed by [Troy Hunt](https://twitter.com/troyhunt)'s [Have I Been Pwned passwords service](https://haveibeenpwned.com/Passwords).

## Why should you care?

As per Troy Hunt's website:

>### Password reuse and credential stuffing
>Password reuse is normal. It's extremely risky, but it's so common because it's easy and people aren't aware of the potential impact. Attacks such as credential stuffing take advantage of reused credentials by automating login attempts against systems using known emails and password pairs.

> ### NIST's guidance: check passwords against those obtained from previous data breaches
> The Pwned Passwords service was created after NIST released guidance specifically recommending that user-provided passwords be checked against existing data breaches . The rationale for this advice and suggestions for how applications may leverage this data is described in detail in the blog post titled Introducing 306 Million Freely Downloadable Pwned Passwords.

This package provides an `IPasswordValidator` for ASP.NET Core Identity that checks whether the provided password appears on the have I been pwned list. You can call the public API directly, or use files download from the Have I Been Pwned website, and added manually to your application.

## Quick start

Install into your project using

```
dotnet add package HaveIBeenPwnedValidator
```

You can add the password validator to you ASP.NET Core Identity configuration using one of the `IdentityBuilder` extension methods: 

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddIdentity<ApplicationUser, IdentityRole>()
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders()
        .AddPwnedPasswordApiValidator<ApplicationUser>() // <-- Add this line to use the api
        .AddPwnedPasswordFileValidator<ApplicationUser>("pwned-passwords-1.0.txt", "pwned-passwords-update-2.txt"); // <-- Or this line to load from files instead

    // other configuration
}
```

## Installing 

>*NOTE* This package is currently for ASP.NET Core Identity 2.0 only, so requires .NET Core 2.0 is installed.

Install using the [HaveIBeenPwnedValidator NuGet package](https://www.nuget.org/packages/HaveIBeenPwnedValidator):

```
PM> Install-Package HaveIBeenPwnedValidator
```

or

```
dotnet add package HaveIBeenPwnedValidator
```

## Usage 

When you install the package, it should be added to your `csproj`. Alternatively, you can add it directly by adding:

```xml
<PackageReference Include="NetEscapades.HaveIBeenPwnedValidator" Version="1.0.0" />
```

Extension methods exist for validating whether the password is listed as a breached password on have I been pwned, either by calling the API directly, or by searching files stored in the `IHostingEnvironment.ContentRootPath`.

### API Service

To call the API service directly, use the `AddPwnedPasswordApiValidator` extension method on `IdentityBuilder` in `Startup.ConfigureServices`:

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddIdentity<ApplicationUser, IdentityRole>()
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders()
        .AddPwnedPasswordApiValidator<ApplicationUser>(); // <-- Add this line to use the api

    // other configuration
}
```

By default, the API validator service calls the public API (at a maximum rate of 1 request every 1500ms) to check if the password has been pwned, with a User-Agent of `HaveIBeenPwnedValidator.PwnedPasswordApiService`.

### File Service

To use the file service directly, use the `AddPwnedPasswordFileValidator` extension method on `IdentityBuilder` in `Startup.ConfigureServices`:

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddIdentity<ApplicationUser, IdentityRole>()
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders()
        .AddPwnedPasswordFileValidator<ApplicationUser>("pwned-passwords-1.0.txt", "pwned-passwords-update-2.txt"); // <-- Add this line to load from files

    // other configuration
}
```

The file service uses the downloaded password lists from https://haveibeenpwned.com/Passwords. As they are so large, you'll need to download them separately, and add them to the `ContentRootPath` of your app. 



## Additional Resources
* [Have I been pwned API](https://haveibeenpwned.com/Passwords)
* [Introducing 306 Million Freely Downloadable Pwned Passwords](https://www.troyhunt.com/introducing-306-million-freely-downloadable-pwned-passwords/)

* [Password Rules Are Bullshit](https://blog.codinghorror.com/password-rules-are-bullshit/)
* [Introduction to ASP.NET Core Identity](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/identity)
