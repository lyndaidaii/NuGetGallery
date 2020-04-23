using System;
using System.Web.Routing;

namespace NuGetGallery.Helpers
{
    public static class LatestPackageRouteVerifier
    {
        public static bool IsLatestRoute(RouteBase route, out bool preRelease)
        {
            preRelease = false;
            if (route is Route r)
            {
                if (r.Url.Equals(GalleryConstants.LatestUrlString, StringComparison.InvariantCultureIgnoreCase))
                {
                    return true;
                }
                if (r.Url.Equals(GalleryConstants.LatestUrlWithPreleaseString, StringComparison.InvariantCultureIgnoreCase))
                {
                    preRelease = true;
                    return true;
                }
                if (r.Url.Equals(GalleryConstants.LatestUrlWithPreleaseAndVersionString, StringComparison.InvariantCultureIgnoreCase))
                {
                    preRelease = true;
                    return true;
                }
            }

            return false;
        }
    }
}