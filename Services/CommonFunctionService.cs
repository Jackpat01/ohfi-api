namespace OhfiApi.Services;

public interface ICommonFunctionService
{
    List<Guid> GetARUMTokensFromClassifications { get; }
}
public class CommonFunctionService: ICommonFunctionService
{
    private readonly IPortalConfigurationService _portalConfiguration;

    public CommonFunctionService(IPortalConfigurationService portalConfiguration)
    {
        _portalConfiguration = portalConfiguration;
    }

    public List<Guid> GetARUMTokensFromClassifications
    {
        get
        {
            var listClassifications = _portalConfiguration.GetAllClassifications();
            if (listClassifications == null)
            {
                throw new Exception("No classifications returned from this application.");
            }
            return listClassifications.Select(c => c.ARUMToken).ToList();
        }
    }

}