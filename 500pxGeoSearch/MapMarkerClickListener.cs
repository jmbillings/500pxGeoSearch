using System;
using Android.Gms.Maps.Model;

namespace _500pxGeoSearch
{
    class MapMarkerClickListener : Android.Gms.Maps.GoogleMap.IOnMarkerClickListener
    {
        public IntPtr Handle
        {
            get
            {
                return new IntPtr(0);
            }
        }

        public void Dispose()
        {
            //throw new NotImplementedException();
        }

        public bool OnMarkerClick(Marker marker)
        {
            throw new NotImplementedException();
        }
    }
}