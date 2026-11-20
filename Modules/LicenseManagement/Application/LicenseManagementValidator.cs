#nullable enable
using Modular.Modules.Contracts; namespace Modular.Modules.LicenseManagement.Application;
public sealed class LicenseManagementValidator { public ValidationResult Validate(LicenseInput x){var e=new List<string>();if(string.IsNullOrWhiteSpace(x.Key)||x.Key.Length>256)e.Add("License key is required and must be at most 256 characters.");if(string.IsNullOrWhiteSpace(x.Product))e.Add("Product is required.");if(string.IsNullOrWhiteSpace(x.Customer))e.Add("Customer is required.");if(x.ExpiresUtc<=x.IssuedUtc)e.Add("Expiration must be later than issue time.");if(x.Features.Any(string.IsNullOrWhiteSpace))e.Add("Feature codes cannot be empty.");return new(e);}}
public sealed record LicenseInput(string Key,string Product,string Customer,string? TenantId,DateTimeOffset IssuedUtc,DateTimeOffset ExpiresUtc,IReadOnlyCollection<string> Features);
public sealed record ValidationResult(IReadOnlyCollection<string> Errors){public bool IsValid=>Errors.Count==0;}
