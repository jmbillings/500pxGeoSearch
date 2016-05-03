using Android.Graphics;
using Android.Util;
using System;
using System.IO;
using System.Json;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace _500pxGeoSearch.Search
{
    public class Core : IDisposable
    {
        private string c_apiKey;
        const string c_searchUrl = "https://api.500px.com/v1/photos/search";
        private SearchResultParser m_ResultParser;
        public event EventHandler<PhotoProcessedEventArgs> PhotoParsed;
        public event EventHandler<PhotoThumbnailEventArgs> ThumbnailDownloaded;
        //private bool m_SearchInProgress;
        private CancellationTokenSource cts;

        public Core(string mapKey)
        {
            c_apiKey = mapKey;
            cts = new CancellationTokenSource();
            m_ResultParser = new SearchResultParser();
            m_ResultParser.PhotoParsed += ResultParser_PhotoParsed;
        }

        public async void DoSearch(string latitude, string longitude, string radius)
        {
            //Build up the search request URL
            string searchUrl = string.Format("{0}?consumer_key={1}&geo={2},{3},{4}&rpp=30", c_searchUrl, c_apiKey, latitude, longitude, radius);
            //m_SearchInProgress = true;
            JsonValue searchResult = await PictureSearch(searchUrl);

            //Parse the result
            m_ResultParser.Parse(searchResult, cts.Token);
        }

        public async void GetPhotoThumbnail(PhotoProcessedEventArgs photoDetails)
        {
            try {
                Bitmap thumbnail = await Task.Run(() => GetThumbnail(photoDetails, cts.Token)).ConfigureAwait(false);
                OnThumbnailDownloaded(new PhotoThumbnailEventArgs(thumbnail, photoDetails));
            }
            catch (OperationCanceledException cancelledException)
            {
                Log.Warn("500pxGeoSearch", "Cancellation token occured");
            }
        }

        private Bitmap GetThumbnail(PhotoProcessedEventArgs photoDetails, CancellationToken cancelled)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(new Uri(photoDetails.PhotoThumbnailUrl));
            Bitmap outputBitmap;

            Log.Debug("500pxGeoSearch", "ThumbnailGrabber for photoid " + photoDetails.PhotoId + "::" + photoDetails.PhotoThumbnailUrl);

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                using (BinaryReader reader = new BinaryReader(response.GetResponseStream()))
                {
                    int length = Convert.ToInt32(reader.BaseStream.Length);
                    byte[] bytes = reader.ReadBytes(length);
                    outputBitmap = BitmapFactory.DecodeByteArray(bytes, 0, length);
                }
            }
            return outputBitmap;
        }

        public void Cancel()
        {
            if (cts != null)
            {
                Log.Debug("500pxGeoSearch", "Cancellation token sent");
                cts.Cancel();
            }
           // m_SearchInProgress = false;
        }

        private void ResultParser_PhotoParsed(object sender, PhotoProcessedEventArgs e)
        {
             GetPhotoThumbnail(e);
        }


        protected virtual void OnPhotoParsed(PhotoProcessedEventArgs e)
        {
            EventHandler<PhotoProcessedEventArgs> handler = PhotoParsed;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnThumbnailDownloaded(PhotoThumbnailEventArgs e)
        {
            EventHandler<PhotoThumbnailEventArgs> handler = ThumbnailDownloaded;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private async Task<JsonValue> PictureSearch(string url)
        {
            Log.Debug("500pxGeoSearch", "PictureSearch::" + url);
            // Create an HTTP web request using the URL:
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(new Uri(url));
            request.ContentType = "application/json";
            request.Method = "GET";

            try {
                // Send the request to the server and wait for the response:
                using (WebResponse response = await request.GetResponseAsync())
                {
                    // Get a stream representation of the HTTP web response:
                    using (Stream stream = response.GetResponseStream())
                    {
                        // Use this stream to build a JSON document object:
                        JsonValue jsonDoc = await Task.Run(() => JsonObject.Load(stream));

                        // Return the JSON document:
                        return jsonDoc;
                    }
                }
            }
            catch(Exception ex)
            {
                Log.Error("500pxGeoSearch", "Error in PictureSearch... " + ex.Message);
                return null;
            }
        }

        public void Dispose()
        {
            m_ResultParser.PhotoParsed -= ResultParser_PhotoParsed;
        }
    }
}