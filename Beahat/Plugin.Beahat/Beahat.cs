using Plugin.Beahat.Abstractions;
using System;

namespace Plugin.Beahat
{
  /// <summary>
  /// Cross platform Beahat implemenations
  /// </summary>
  public class Beahat
  {
    static Lazy<IBeahat> Implementation = new Lazy<IBeahat>(() => CreateBeahat(), System.Threading.LazyThreadSafetyMode.PublicationOnly);

    /// <summary>
    /// Current settings to use
    /// </summary>
    public static IBeahat Current
    {
      get
      {
        var ret = Implementation.Value;
        if (ret == null)
        {
          throw NotImplementedInReferenceAssembly();
        }
        return ret;
      }
    }

    static IBeahat CreateBeahat()
    {
#if PORTABLE
        return null;
#else
        return new BeahatImplementation();
#endif
    }

    internal static Exception NotImplementedInReferenceAssembly()
    {
      return new NotImplementedException("This functionality is not implemented in the portable version of this assembly.  You should reference the NuGet package from your main application project in order to reference the platform-specific implementation.");
    }
  }
}
