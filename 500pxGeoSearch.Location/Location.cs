using Android.Content;
using Android.Locations;

namespace _500pxGeoSearch.Location
{
    public class Location
    {
        private LocationManager m_LocationManager;

        public Location(Context context)
        {
            m_LocationManager = context.GetSystemService(Context.LocationService) as LocationManager;

            string Provider = LocationManager.GpsProvider;

            if (m_LocationManager.IsProviderEnabled(Provider))
            {
                m_LocationManager.RequestLocationUpdates(Provider, 2000, 1, context.getA);
            }
            else
            {
                Log.Info(tag, Provider + " is not available. Does the device have location services enabled?");
            }

        }
    }
}
