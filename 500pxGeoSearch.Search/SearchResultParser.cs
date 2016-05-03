using Android.Util;
using System;
using System.Json;
using System.Threading;

namespace _500pxGeoSearch.Search
{
    internal class SearchResultParser
    {
        int m_NumberOfItems;
        internal event EventHandler<PhotoProcessedEventArgs> PhotoParsed;

        internal void Parse(JsonValue searchResults, CancellationToken cancellationToken)
        {
            if (searchResults == null)
                return;

            if (!int.TryParse(searchResults["total_items"].ToString(), out m_NumberOfItems))
                return;

            Log.Debug("500pxGeoSearch", "Parsing first page (max 30) of " + m_NumberOfItems + " results...");
            JsonArray photos = (JsonArray)searchResults["photos"];

            for(int i=0; i < Math.Min(30,m_NumberOfItems); i++)
            {
                try {
                    JsonValue photoDetails = photos[i];

                    PhotoProcessedEventArgs photoProcessedEventArgs = new PhotoProcessedEventArgs(m_NumberOfItems, photoDetails["name"].ToString(), photoDetails["user_id"].ToString(),
                        double.Parse(photoDetails["latitude"].ToString()), double.Parse(photoDetails["longitude"].ToString()), photoDetails["image_url"].ToString(), photoDetails["id"].ToString());

                    Log.Debug("500pxGeoSearch", "Sending PhotoParsed event: " + photoProcessedEventArgs.ToString());
                    OnPhotoParsed(photoProcessedEventArgs);
                }
                catch(OperationCanceledException)
                {
                    Log.Debug("500pxGeoSearch", "Parser cancelled");
                    break;
                }
                catch(Exception ex)
                {
                    Log.Error("500pxGeoSearch", "Error parsing photo search results::" + ex.Message);
                }
            }
        }

        protected virtual void OnPhotoParsed(PhotoProcessedEventArgs e)
        {
            EventHandler<PhotoProcessedEventArgs> handler = PhotoParsed;
            if (handler != null)
            {
                handler(this, e);
            }
            else
            {
                Log.Warn("500pxGeoSearch", "No event handler hooked up in SearchResultParser");
            }
        }
    }
}