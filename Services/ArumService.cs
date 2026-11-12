using MSLC.ArumProxy;
using MSLC.ArumProxy.Models;
using OhfiApi.Models;
using Omu.ValueInjecter;

namespace OhfiApi.Services;

public class ArumService : IArumService
{
  private readonly ILogger<ArumService> _logger;
  private readonly ICommonFunctionService _function;
  private readonly IPortalConfigurationService _portalConfiguration;
  private readonly IARUMUserService _arumUser;
  private ArumServiceProxy _arumUserProxy;

  public ArumService(ILogger<ArumService> logger, ICommonFunctionService function,
    IPortalConfigurationService portalConfiguration, IARUMUserService arumUser)
  {
    _logger = logger;
    _function = function;
    _portalConfiguration = portalConfiguration;
    _arumUser=arumUser;
    var allClassifications = _portalConfiguration.GetAllClassifications();
    _arumUserProxy = _arumUser.GetArumServiceProxy(allClassifications.First());
  }

  public LoginToPageResponse GetUserRolesByEmailAndPassword(
    string email,
    string password,
    string operationName,
    string ipAddress)
  {
    var redirect = false;
    var redirectToTerms = false;

    var arumValidations = _arumUserProxy.GetUserRolesByEmailAndPasswordAndApplications(
      _function.GetARUMTokensFromClassifications,
      email,
      password,
      operationName,
      ipAddress
    );

    var response = new LoginToPageResponse();
    if (!arumValidations.Results.Successful)
    {
      response.Successful = false;
      response.ErrorMessage = "Username/password invalid";
      return response;
    }

    if (arumValidations.Results.Successful)
    {
      if (!arumValidations.lstARUMUsers.Any())
      {
        response.Successful = false;
        response.ErrorMessage = "Username/password invalid";
        return response;
      }

      var allClassifications = _portalConfiguration.GetAllClassifications();

      foreach (var singleARUMValidation in arumValidations.lstARUMUsers)
      {
        _arumUserProxy = _arumUser.GetArumServiceProxy(
            allClassifications.FirstOrDefault(c => c.ARUMToken == singleARUMValidation.Token));

        if (singleARUMValidation.IPAddress != ipAddress)
        {
          var myARUMResults =
            _arumUserProxy.UpdateUserIPAddress(singleARUMValidation.UserID, ipAddress, "Home - Login");
          if (!myARUMResults.Successful)
          {
            response.ErrorMessage = myARUMResults.Message;
          }
        }

        response.ARUMUser = new ARUMProxy_User().InjectFrom(singleARUMValidation) as ARUMProxy_User;
        response.Email = singleARUMValidation.Email;
        response.UserName = singleARUMValidation.Email;
        response.Successful = true;
      }

      if (arumValidations.lstARUMUsers.Any(c => !c.AcceptedTerms.HasValue))
      {
        response.RedirectToTerm = true;
      }
    }
    else
    {
      response.ErrorMessage = arumValidations.Results.Message;
    }

    return response;
  }
}