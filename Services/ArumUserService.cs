using MSLC.ArumProxy;
using MSLC.Portal.Configuration;
using MSLC.Portal.Utility.Proxies;

namespace OhfiApi.Services;

public interface IARUMUserService
{
  ArumServiceProxy GetArumServiceProxy(ClassificationDTO? classification);
}

public class ARUMUserService: IARUMUserService
{
  public ArumServiceProxy GetArumServiceProxy(ClassificationDTO? classification)
  {
    return ARUMUser.GetArumServiceProxy(classification);
  }
}