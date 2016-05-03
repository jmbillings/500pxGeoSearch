using System;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using _500pxGeoSearch.Search;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using System.IO;
using System.Text;
using Android.Graphics;

namespace _500pxGeoSearch
{
    [Activity(Label = "PhotoDetail")]
    public class PhotoDetail : Activity
    {
        Bitmap m_Thumbnail;
        string m_PhotoTitle;
        ImageView m_Image;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            using (MemoryStream ms = new MemoryStream(Intent.GetByteArrayExtra("thumbnail")))
            {
                byte[] bytes = new byte[ms.Length];
                ms.Read(bytes, 0, (int)ms.Length);
                m_Thumbnail = BitmapFactory.DecodeByteArray(bytes, 0, bytes.Length);
            }

            SetContentView(Resource.Layout.PhotoDetail);

            m_Image = (ImageView)FindViewById(Resource.Id.photoImage);
            m_Image.SetImageBitmap(m_Thumbnail);
        }


    }
}