using OsmSharp.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

using Xamarin.Forms;
using Xamarin.Forms.Maps;

namespace OSMSharp.Forms
{
    public class App : Application
    {
        private OsmSharp.Forms.OsmMap map;
        private Button btnOnlineOffline;

        public App()
        {
            map = new OsmSharp.Forms.OsmMap();

            btnOnlineOffline = new Button
            {
                Text = "Offline",
                Command = new Command(() => HandleOnlineOffline()),
            };


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
                        new StackLayout
                        {
                            Orientation = StackOrientation.Horizontal,
                            Children =
                            {
                                btnOnlineOffline,
                                new Button
                                {
                                    Text = "User",
                                    Command = new Command(() => { map.IsShowingUser = !map.IsShowingUser; map.IsShowingUserInCenter = true; }),
                                },
                                new Button
                                {
                                    Text = "Center",
                                    Command = new Command(() => map.MoveToRegion(MapSpan.FromCenterAndRadius(new Position(48.487, 9.215), new Distance(200)))),
                                },
                            }
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

        private void HandleOnlineOffline()
        {
            if (btnOnlineOffline.Text == "Online")
            {
                btnOnlineOffline.Text = "Offline";
                map.AddLayerTile();
            }
            else
            {
                btnOnlineOffline.Text = "Online";

                // Get assembly
                var assembly = typeof(OsmMap).GetTypeInfo().Assembly;

                // Initialize the data source.
                var stream = assembly.GetManifestResourceStream("OsmSharp.Forms.Maps.test.osm.pbf");

                map.AddLayerOsm(stream);
            }
        }
    }
}
