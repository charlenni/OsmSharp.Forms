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
            map = new OsmSharp.Forms.OsmMap
            {
                // lyrs values for Google
                // h = roads only
                // m = standard roadmap
                // p = terrain
                // r = somehow altered roadmap
                // s = satellite only
                // t = terrain only
                // y = hybrid

                TileUrlStreet = "http://a.tile.opencyclemap.org/cycle/{z}/{x}/{y}.png",
                TileUrlHybrid = "http://mt1.google.com/vt/lyrs=y&x={x}&y={y}&z={z}",
                TileUrlSatellite = "http://mt1.google.com/vt/lyrs=p&x={x}&y={y}&z={z}",
                MapType = MapType.Satellite,
            };

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
                                    Text = "Show User",
                                    Command = new Command(() => map.IsShowingUser = !map.IsShowingUser),
                                },
                                new Button
                                {
                                    Text = "Follow",
                                    Command = new Command(() => map.IsShowingUserInCenter = !map.IsShowingUserInCenter),
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
                map.MapType = MapType.Street;
            }
            else
            {
                btnOnlineOffline.Text = "Online";

                // Get assembly
                var assembly = typeof(App).GetTypeInfo().Assembly;

				// Initialize the data source.
				var cssStream = assembly.GetManifestResourceStream("OsmSharp.Forms.Demo.MapCSS.Default.mapcss");
				var stream = assembly.GetManifestResourceStream("OsmSharp.Forms.Demo.Maps.test.osm.pbf");

                map.AddLayerOsm(stream, cssStream);
            }
        }
    }
}
