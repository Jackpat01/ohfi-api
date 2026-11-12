using MSLC.Portal.Configuration;
using System.Configuration;

namespace OhfiApi.Services;

public interface IPortalConfigurationService
{
  ClassificationDTO[] GetAllClassifications();
  ClassificationDTO GetClassification(string classification);
  int GetClassificationID(string classification);
  Guid GetClassificationARUMToken(string classification);
  string GetClassificationDisplayName(string classification);
  string GetClassificationServer(string classification);
  Guid GetClassificationEDSToken(string classification);
  string GetClassificationDatabase(string classification);
  string GetClassificationURL(string classification);
  string GetClassificationStateAbbreviation(string classification);
  string GetClassificationStateName(string classification);
}

public class PortalConfigurationService : IPortalConfigurationService
{
  private readonly IConfiguration _configuration;

  public PortalConfigurationService(IConfiguration configuration)
  {
    _configuration = configuration;
  }

  public ClassificationDTO[] GetAllClassifications()
  {
    var classifications = new List<ClassificationDTO>();
    var classificationsSection = _configuration.GetSection("PortalConfiguration:Classifications");

    foreach (var child in classificationsSection.GetChildren())
    {
      var classification = GetClassification(child.Key);
      classifications.Add(classification);
    }
    return classifications.ToArray();
  }

  public int GetClassificationID(string classification)
  {
    string settingName = $"PortalConfiguration:Classifications:{classification}:ID";
    string environment = _configuration["ASPNETCORE_ENVIRONMENT"] ?? _configuration["DOTNET_ENVIRONMENT"] ?? "Unknown";
    return _configuration.GetValue<int?>(settingName)
           ?? throw new InvalidOperationException(
             $"(Environment) {environment} or (SettingName) {settingName} is null or empty.");
  }

  public ClassificationDTO GetClassification(string classification)
  {
    return new ClassificationDTO()
    {
      ID = GetClassificationID(classification),
      Type = classification,
      ARUMToken = GetClassificationARUMToken(classification),
      DisplayName = GetClassificationDisplayName(classification),
      Server = GetClassificationServer(classification),
      EDSToken = GetClassificationEDSToken(classification),
      Database = GetClassificationDatabase(classification),
      URL = GetClassificationURL(classification),
      StateAbbreviation = GetClassificationStateAbbreviation(classification),
      StateName = GetClassificationStateName(classification)
    };
  }

  public Guid GetClassificationARUMToken(string classification)
  {
    string settingName = $"PortalConfiguration:Classifications:{classification}:ARUMToken";
    string environment = _configuration["ASPNETCORE_ENVIRONMENT"] ?? _configuration["DOTNET_ENVIRONMENT"] ?? "Unknown";
    return _configuration.GetValue<Guid?>(settingName)
           ?? throw new InvalidOperationException(
             $"(Environment) {environment} or (SettingName) {settingName} is null or empty.");
  }

  public string GetClassificationDisplayName(string classification)
  {
    string settingName = $"PortalConfiguration:Classifications:{classification}:DisplayName";
    string environment = _configuration["ASPNETCORE_ENVIRONMENT"] ?? _configuration["DOTNET_ENVIRONMENT"] ?? "Unknown";
    return _configuration[settingName] ?? throw new InvalidOperationException($"{settingName} is missing from environment ({environment}) configuration.");
  }

  public string GetClassificationServer(string classification)
  {
    string settingName = $"PortalConfiguration:Classifications:{classification}:Server";
    string environment = _configuration["ASPNETCORE_ENVIRONMENT"] ?? _configuration["DOTNET_ENVIRONMENT"] ?? "Unknown";
    return _configuration[settingName] ?? throw new InvalidOperationException($"{settingName} is missing from environment ({environment}) configuration.");
  }

  public Guid GetClassificationEDSToken(string classification)
  {
    string settingName = $"PortalConfiguration:Classifications:{classification}:EDSToken";
    string environment = _configuration["ASPNETCORE_ENVIRONMENT"] ?? _configuration["DOTNET_ENVIRONMENT"] ?? "Unknown";
    return _configuration.GetValue<Guid?>(settingName)
           ?? throw new InvalidOperationException(
             $"(Environment) {environment} or (SettingName) {settingName} is null or empty.");
  }

  public string GetClassificationDatabase(string classification)
  {
    string settingName = $"PortalConfiguration:Classifications:{classification}:Database";
    string environment = _configuration["ASPNETCORE_ENVIRONMENT"] ?? _configuration["DOTNET_ENVIRONMENT"] ?? "Unknown";
    return _configuration[settingName] ?? throw new InvalidOperationException($"{settingName} is missing from environment ({environment}) configuration.");
  }

  public string GetClassificationURL(string classification)
  {
    string settingName = $"PortalConfiguration:Classifications:{classification}:URL";
    string environment = _configuration["ASPNETCORE_ENVIRONMENT"] ?? _configuration["DOTNET_ENVIRONMENT"] ?? "Unknown";
    return _configuration[settingName] ?? throw new InvalidOperationException($"{settingName} is missing from environment ({environment}) configuration.");
  }

  public string GetClassificationStateAbbreviation(string classification)
  {
    string settingName = $"PortalConfiguration:Classifications:{classification}:StateAbbreviation";
    string environment = _configuration["ASPNETCORE_ENVIRONMENT"] ?? _configuration["DOTNET_ENVIRONMENT"] ?? "Unknown";
    return _configuration[settingName] ?? throw new InvalidOperationException($"{settingName} is missing from environment ({environment}) configuration.");
  }

  public string GetClassificationStateName(string classification)
  {
    string settingName = $"PortalConfiguration:Classifications:{classification}:StateName";
    string environment = _configuration["ASPNETCORE_ENVIRONMENT"] ?? _configuration["DOTNET_ENVIRONMENT"] ?? "Unknown";
    return _configuration[settingName] ?? throw new InvalidOperationException($"{settingName} is missing from environment ({environment}) configuration.");
  }
}