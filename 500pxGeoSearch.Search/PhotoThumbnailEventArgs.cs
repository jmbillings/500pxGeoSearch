using System;
using Android.Graphics;

namespace _500pxGeoSearch.Search
{
    [Serializable]
    public class PhotoThumbnailEventArgs : EventArgs
    {
        public Bitmap Thumbnail { get; private set; }
        public PhotoProcessedEventArgs PhotoDetails { get; private set; }

        public PhotoThumbnailEventArgs(Bitmap thumbnail, PhotoProcessedEventArgs photoDetails)
        {
            Thumbnail = thumbnail;
            PhotoDetails = photoDetails;
        }
    }
}