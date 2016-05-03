using System;
using Android.App;
using Android.OS;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.Locations;
using _500pxGeoSearch.Search;
using System.Collections.Generic;
using Android.Support.V7.App;
using Android.Widget;
using Android.Views;
using Android.Content;
using System.IO;
using Android.Graphics;

namespace _500pxGeoSearch
{
    [Activity(Label = "500pxGeoSearch", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : AppCompatActivity, IOnMapReadyCallback, ILocationListener
    {
        private MapFragment m_MapFragment;
        private LinearLayout m_MapContainer;
        private GoogleMap m_Map;
        private LocationManager m_LocationManager;
        private Core m_Core;
        private Dictionary<string, PhotoThumbnailEventArgs> m_MapMarkers;
        private MapMarkerClickListener m_MapMarkerClickListener;
        private LatLng m_LastCameraLocation;
        private bool m_UserPannedMap;
        private Android.Support.V7.Widget.Toolbar toolbarMenu;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);
            m_MapContainer = (LinearLayout)FindViewById(Resource.Id.mapContainer);
            m_MapMarkerClickListener = new MapMarkerClickListener();
            toolbarMenu = (Android.Support.V7.Widget.Toolbar)FindViewById(Resource.Id.toolbarMenu);
            SetSupportActionBar(toolbarMenu);

            AddMap();
        }

        public override bool OnPrepareOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menu, menu);
            return base.OnPrepareOptionsMenu(menu);
        }

        protected override void OnPause()
        {
            base.OnPause();

            //Unhook things so we don't sit in the background being bothersome
            if (m_LocationManager != null)
                m_LocationManager.RemoveUpdates(this);

            if (m_Core != null)
            {
                m_Core.Cancel();
                m_Core.ThumbnailDownloaded -= core_ThumbnailDownloaded;
            }
        }

        protected override void OnPostResume()
        {
            base.OnPostResume();
            m_MapFragment.GetMapAsync(this);
        }

        private void AddMap()
        {
            m_MapFragment = MapFragment.NewInstance();
            FragmentTransaction tx = FragmentManager.BeginTransaction();
            tx.Add(Resource.Id.mapContainer, m_MapFragment);
            tx.Commit();
        }

        public void OnMapReady(GoogleMap googleMap)
        {
            if (m_Core == null)
                m_Core = new Core(Resources.GetString(Resource.String.MapKey));

            m_Core.ThumbnailDownloaded += core_ThumbnailDownloaded;

            m_Map = googleMap;

            m_LocationManager = GetSystemService(LocationService) as LocationManager;

            InitLocation();

            // Hook up map-move event
            m_Map.CameraChange += Map_CameraChange;

            // Hook up Marker click handler
            m_Map.MarkerClick += Map_MarkerClick; ;
        }

        private void InitLocation()
        {
            // Try to get last decent network location while the GPS warms up...
            m_LocationManager.GetLastKnownLocation(LocationManager.NetworkProvider);
            m_LocationManager.RequestLocationUpdates(LocationManager.NetworkProvider, 2000, 100, this);

            // Hook up GPS provider
            if (m_LocationManager.IsProviderEnabled(LocationManager.GpsProvider))
            {
                m_LocationManager.RequestLocationUpdates(LocationManager.GpsProvider, 2000, 5, this);
            }
            else
            {
                Toast.MakeText(this, "Cannot access GPS!", ToastLength.Long).Show();
            }
        }

        private void Map_CameraChange(object sender, GoogleMap.CameraChangeEventArgs e)
        {
            //If there's no previous location, this is probably a GPS triggred initial move
            if (m_LastCameraLocation == null)
                return;

            //Do a search if the position is different
            if (m_LastCameraLocation != e.Position.Target)
            {
                m_UserPannedMap = true; //Set flag to ignore GPS updates to we don't keep moving the user back to their current position.
                m_Core.Cancel(); //Cancel any current search
                m_Core.DoSearch(e.Position.Target.Latitude.ToString(), e.Position.Target.Longitude.ToString(), "5km");
            }
        }

        private void Map_MarkerClick(object sender, GoogleMap.MarkerClickEventArgs e)
        {
            PhotoThumbnailEventArgs photoDetails;
            if (m_MapMarkers.TryGetValue(e.Marker.Id, out photoDetails))
            {
                var photoDetail = new Intent(this, typeof(PhotoDetail));

                //Serialize the thumbnail
                MemoryStream ms = new MemoryStream();
                photoDetails.Thumbnail.Compress(Bitmap.CompressFormat.Png, 100, ms);
                photoDetail.PutExtra("thumbnail", ms.ToArray());

                //PutExtra the other details


                StartActivity(photoDetail);
                ms.Dispose();
            }
        }

        private void core_ThumbnailDownloaded(object sender, PhotoThumbnailEventArgs e)
        {
            RunOnUiThread(() => { AddMarker(e); });
        }

        private void AddMarker(PhotoThumbnailEventArgs e)
        {
            try {
                //New up dictionary if it doesn't exist yet
                if (m_MapMarkers == null) m_MapMarkers = new Dictionary<string, PhotoThumbnailEventArgs>();

                //Get a map marker
                MarkerOptions markerOptions = new MarkerOptions().SetPosition(new LatLng(e.PhotoDetails.PhotoLatitude, e.PhotoDetails.PhotoLongitude))
                    .SetIcon(BitmapDescriptorFactory.FromBitmap(e.Thumbnail));

                Marker marker = m_Map.AddMarker(markerOptions);

                //Chuck it in the collection with the photo details
                if (!m_MapMarkers.ContainsKey(marker.Id))
                    m_MapMarkers.Add(marker.Id, e);
            }
            catch (Exception ex)
            {
                string em = ex.Message;
            }
        }

        private void DoSearch(Location location)
        {
            m_Core.DoSearch(location.Latitude.ToString(), location.Longitude.ToString(), "5km");
        }

        
        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.action_userlocation:
                    m_UserPannedMap = false;
                    InitLocation();
                    return true;

                case Resource.Id.action_settings:
                    //Settings
                    return true;

                default:
                    // If we got here, the user's action was not recognized.
                    // Invoke the superclass to handle it.
                    return base.OnOptionsItemSelected(item);

            }
        }

        void ILocationListener.OnLocationChanged(Location location)
        {
            if (m_UserPannedMap) return;

            CameraPosition.Builder builder = CameraPosition.InvokeBuilder();
            builder.Target(new LatLng(location.Latitude, location.Longitude));
            builder.Zoom(15);
            CameraPosition cameraPosition = builder.Build();
            CameraUpdate cameraUpdate = CameraUpdateFactory.NewCameraPosition(cameraPosition);
            m_Map.MoveCamera(cameraUpdate);
            m_LastCameraLocation = new LatLng(location.Latitude, location.Longitude);

            DoSearch(location);
        }

        void ILocationListener.OnProviderDisabled(string provider)
        {
            // Not sure we need to warn the user or not here... they most likely know they just turned it off...
        }

        void ILocationListener.OnProviderEnabled(string provider)
        {
            // Wait for updates naturally.
        }

        void ILocationListener.OnStatusChanged(string provider, Availability status, Bundle extras)
        {
            // Probably nothing to do here...
        }
    }
}

