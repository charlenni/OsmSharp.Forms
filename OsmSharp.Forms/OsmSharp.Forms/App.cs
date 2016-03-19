using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Xamarin.Forms;
using Xamarin.Forms.Maps;

namespace OSMSharp.Forms
{
    public class App : Application
    {
        public App()
        {
            var map = new OsmSharp.Forms.OsmMap();

            map.AddLayerTile();

            // The root page of your application
            MainPage = new ContentPage
            {
                //Content = map,
                Content = new StackLayout
                {
                    Orientation = StackOrientation.Vertical,
                    HorizontalOptions = LayoutOptions.FillAndExpand,
                    VerticalOptions = LayoutOptions.FillAndExpand,
                    Children = {
                        map,
                        new Button
                        {
                            Command = new Command(() => map.MapCenter = MapSpan.FromCenterAndRadius(new Position(48.5, 9.25), new Distance(500))),
                        },
                    },
                }
            };
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
