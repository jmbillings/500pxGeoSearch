using System;

namespace _500pxGeoSearch.Search
{
    public class PhotoProcessedEventArgs : EventArgs
    {
        public int TotalItems { get; private set; }
        public string PhotoTitle { get; private set; }
        public string PhotoAuthor { get; private set; }
        public double PhotoLatitude { get; private set; }
        public double PhotoLongitude { get; private set; }
        public string PhotoThumbnailUrl { get; private set; }
        public string PhotoId { get; private set; }

        public PhotoProcessedEventArgs(int totalItems, string photoTitle, string photoAuthor, double photoLatitude, double photoLongitude, string photoThumbnailUrl, string photoid)
        {
            TotalItems = totalItems;
            PhotoTitle = photoTitle;
            PhotoAuthor = photoAuthor;
            PhotoLatitude = photoLatitude;
            PhotoLongitude = photoLongitude;
            PhotoThumbnailUrl = photoThumbnailUrl.Replace("\"","");
            PhotoId = photoid;
        }

        public override string ToString()
        {
            return string.Format("Photo id {0} - '{1}'", PhotoId, PhotoTitle);
        }
   
    }
}