using OsmSharp.Forms.Extensions;
using OsmSharp.Osm;
using OsmSharp.Osm.Data.Memory;
using OsmSharp.Osm.PBF.Streams;
using OsmSharp.UI.Map.Layers;
using OsmSharp.UI.Map.Styles.MapCSS;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Xamarin.Forms;
using Xamarin.Forms.Maps;
using OsmSharp.Math.Geo.Projections;
using OsmSharp.UI.Renderer;
using OsmSharp.UI;
using Plugin.Geolocator;
using Plugin.Geolocator.Abstractions;
using OsmSharp.Math.Geo;
using OsmSharp.UI.Animations;
using OsmSharp.Units.Angle;
using OsmSharp.Osm.Streams.Filters;

namespace OsmSharp.Forms
{
    public class OsmMap : View
    {
        public OsmMap()
        {
            VerticalOptions = LayoutOptions.CenterAndExpand;
            HorizontalOptions = LayoutOptions.CenterAndExpand;
            MinimumWidthRequest = 50;
            MinimumHeightRequest = 50;
        }

        private LayerPrimitives layerUser;
        private LayerPrimitives layerAccuracy;
        private GeoCoordinate lastKnownPosition;
        private double lastKnownAccuracy;
        private Layer layerMap;
        private Layer layerScale;
        private string tileUrl;

        //public delegate void MapChangedEvent(object sender);
        //public event MapChangedEvent MapChanged;

        /// <summary>
        /// The used MapView for this view (contains the UIView/ViewGroup)
        /// </summary>
        private OsmSharp.UI.IMapView mapView;

        /// <summary>
        /// MapView property
        /// </summary>
        public OsmSharp.UI.IMapView MapView
        {
            get
            {
                return mapView;
            }
            set
            {
                if (mapView != value)
                {
                    if (mapView != null)
                        mapView.MapMove -= MapMoved;
                    mapView = value;
                    mapView.MapMove += MapMoved;
                }
            }
        }

        /// <summary>
        /// The used Map (containing the layers)
        /// </summary>
        private OsmSharp.UI.Map.Map map;

        /// <summary>
        /// Map property
        /// </summary>
        public OsmSharp.UI.Map.Map Map
        {
            get
            {
                return map;
            }
            set
            {
                if (map != value)
                {
                    map = value;
                    MapCreated();
                }
            }
        }

        /// <summary>
        /// Gets/Sets the tile url for streets.
        /// </summary>
        public string TileUrlStreet
        {
            get { return (string)GetValue(TileUrlStreetProperty); }
            set
            {
                this.SetValue(TileUrlStreetProperty, value);

                // If active map is tile map, than clear it and update the tile layer
                if (!string.IsNullOrEmpty(tileUrl) && MapType == MapType.Street)
                {
                    tileUrl = value;

                    if (layerMap != null)
                        layerMap.Close();

                    layerMap = map?.AddLayerTile(tileUrl);
                }
            }
        }

        /// <summary>
        /// Bindable Property of <see cref="TileUrlStreet"/>
        /// </summary>
        public static readonly BindableProperty TileUrlStreetProperty = BindableProperty.Create(nameof(TileUrlStreet), typeof(string), typeof(OsmMap), default(string));

        /// <summary>
        /// Gets/Sets the tile url for satellite.
        /// </summary>
        public string TileUrlSatellite
        {
            get { return (string)GetValue(TileUrlSatelliteProperty); }
            set
            {
                this.SetValue(TileUrlSatelliteProperty, value);

                // If active map is tile map, than clear it and update the tile layer
                if (!string.IsNullOrEmpty(tileUrl) && MapType == MapType.Satellite)
                {
                    tileUrl = value;

                    if (layerMap != null)
                        layerMap.Close();

                    layerMap = map?.AddLayerTile(tileUrl);
                }
            }
        }

        /// <summary>
        /// Bindable Property of <see cref="TileUrlSatellite"/>
        /// </summary>
        public static readonly BindableProperty TileUrlSatelliteProperty = BindableProperty.Create(nameof(TileUrlSatellite), typeof(string), typeof(OsmMap), default(string));

        /// <summary>
        /// Gets/Sets the tile url for hybrid.
        /// </summary>
        public string TileUrlHybrid
        {
            get { return (string)GetValue(TileUrlHybridProperty); }
            set
            {
                this.SetValue(TileUrlHybridProperty, value);

                // If active map is tile map, than clear it and update the tile layer
                if (!string.IsNullOrEmpty(tileUrl) && MapType == MapType.Hybrid)
                {
                    tileUrl = value;

                    if (layerMap != null)
                        layerMap.Close();

                    layerMap = map?.AddLayerTile(tileUrl);
                }
            }
        }

        /// <summary>
        /// Bindable Property of <see cref="TileUrlHybrid"/>
        /// </summary>
        public static readonly BindableProperty TileUrlHybridProperty = BindableProperty.Create(nameof(TileUrlHybrid), typeof(string), typeof(OsmMap), default(string));

        /// <summary>
        /// Gets/Sets the current center of the map.
        /// </summary>
        public MapSpan MapCenter
        {
            get
            {
                var result = (MapSpan)this.GetValue(MapCenterProperty);

                if (mapView != null && mapView.MapCenter != null && mapView.MapBoundingBox != null)
                    return result ?? new MapSpan(mapView.MapCenter.ToPosition(), mapView.MapBoundingBox.MaxLat - mapView.MapBoundingBox.MinLat, mapView.MapBoundingBox.MaxLon - mapView.MapBoundingBox.MinLon);
                else
                    return result ?? VisibleRegion;
            }
            set
            {
                this.SetValue(MapCenterProperty, value);

                if (mapView != null)
                {
                    // Set new center but don't touch zoom level
                    mapView.MapCenter = value.Center.ToGeoCoordinate();
                }
            }
        }

        /// <summary>
        /// Bindable Property of <see cref="MapCenter"/>
        /// </summary>
        public static readonly BindableProperty MapCenterProperty = BindableProperty.Create(nameof(MapCenter), typeof(MapSpan), typeof(OsmMap), default(MapSpan), BindingMode.TwoWay);

        /// <summary>
        /// Gets/Sets the flag for scrolling the map.
        /// </summary>
        public bool HasScrollEnabled
        {
            get { return (bool)this.GetValue(HasScrollEnabledProperty); }
            set
            {
                this.SetValue(HasScrollEnabledProperty, value);

                if (mapView != null)
                    mapView.MapAllowPan = value;
            }
        }

        /// <summary>
        /// Bindable Property of <see cref="HasScrollEnabled"/>
        /// </summary>
        public static readonly BindableProperty HasScrollEnabledProperty = BindableProperty.Create(nameof(HasScrollEnabled), typeof(bool), typeof(OsmMap), true);

        /// <summary>
        /// Gets/Sets the flag for zooming the map.
        /// </summary>
        public bool HasZoomEnabled
        {
            get { return (bool)this.GetValue(HasZoomEnabledProperty); }
            set
            {
                this.SetValue(HasZoomEnabledProperty, value);

                if (mapView != null)
                    mapView.MapAllowZoom = value;
            }
        }

        /// <summary>
        /// Bindable Property of <see cref="HasZoomEnabled"/>
        /// </summary>
        public static readonly BindableProperty HasZoomEnabledProperty = BindableProperty.Create(nameof(HasZoomEnabled), typeof(bool), typeof(OsmMap), true);

        /// <summary>
        /// Gets/Sets the flag for showing a scale on the map.
        /// </summary>
        public bool HasScaleEnabled
        {
            get { return (bool)this.GetValue(HasScaleEnabledProperty); }
            set
            {
                this.SetValue(HasScaleEnabledProperty, value);

                if (mapView != null)
                    mapView.MapAllowZoom = value;
            }
        }

        /// <summary>
        /// Bindable Property of <see cref="HasScaleEnabled"/>
        /// </summary>
        public static readonly BindableProperty HasScaleEnabledProperty = BindableProperty.Create(nameof(HasScaleEnabled), typeof(bool), typeof(OsmMap), true);

        /// <summary>
        /// Gets/Sets the flag for showing position of user on map.
        /// </summary>
        public bool IsShowingUser
        {
            get { return (bool)this.GetValue(IsShowingUserProperty); }
            set
            {
                this.SetValue(IsShowingUserProperty, value);

                if (mapView != null)
                {
                    if (value)
                    {
                        DrawUserIcon(lastKnownPosition, 50);

                        CrossGeolocator.Current.PositionChanged += UserPositionChanged;

                        if (!CrossGeolocator.Current.IsListening)
                            CrossGeolocator.Current.StartListeningAsync(100, 2.0);
                    }
                    else
                    {
                        if (layerUser != null)
                        {
                            map.RemoveLayer(layerUser);

                            layerUser.Close();
                            layerUser = null;
                        }

                        if (layerAccuracy != null)
                        {
                            map.RemoveLayer(layerAccuracy);

                            layerAccuracy.Close();
                            layerAccuracy = null;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Bindable Property of <see cref="IsShowingUser"/>
        /// </summary>
        public static readonly BindableProperty IsShowingUserProperty = BindableProperty.Create(nameof(IsShowingUser), typeof(bool), typeof(OsmMap), false);

        /// <summary>
        /// Gets/Sets the flag for showing position of user on map.
        /// </summary>
        public bool IsShowingUserInCenter
        {
            get { return (bool)this.GetValue(IsShowingUserInCenterProperty); }
            set
            {
                this.SetValue(IsShowingUserInCenterProperty, value);

                if (value)
                {
                    mapView.MapCenter = lastKnownPosition;
                }
            }
        }

        /// <summary>
        /// Bindable Property of <see cref="IsShowingUser"/>
        /// </summary>
        public static readonly BindableProperty IsShowingUserInCenterProperty = BindableProperty.Create(nameof(IsShowingUserInCenter), typeof(bool), typeof(OsmMap), false);

        /// <summary>
        /// Gets/Sets the type of the map.
        /// </summary>
        public MapType MapType
        {
            get { return (MapType)this.GetValue(MapTypeProperty); }
            set
            {
                this.SetValue(MapTypeProperty, value);

                var newUrl = "";

                // Which tile url should be used for the map layer
                switch(value)
                {
                    case MapType.Street:
                        newUrl = TileUrlStreet;
                        break;
                    case MapType.Satellite:
                        newUrl = TileUrlSatellite;
                        break;
                    case MapType.Hybrid:
                        newUrl = TileUrlHybrid;
                        break;
                }

                // No new tile url found
                if (string.IsNullOrEmpty(newUrl))
                    return;

                // Save new tile url for later use
                tileUrl = newUrl;

                // If a map layer is active, remove it
                if (layerMap != null)
                    layerMap.Close();

                // Set new map layer
                layerMap = map?.AddLayerTile(tileUrl);
            }
        }

        /// <summary>
        /// Bindable Property of <see cref="MapType"/>
        /// </summary>
        public static readonly BindableProperty MapTypeProperty = BindableProperty.Create(nameof(MapType), typeof(MapType), typeof(OsmMap), default(MapType));

        /// <summary>
        /// Returns a MapSpan with the actuell visible region of the map
        /// </summary>
        public MapSpan VisibleRegion
        {
            get
            {
                double latDegrees;
                double lonDegrees;

                if (mapView?.MapBoundingBox != null)
                {
                    latDegrees = mapView.MapBoundingBox.MaxLat - mapView.MapBoundingBox.MinLat;
                    lonDegrees = mapView.MapBoundingBox.MaxLon - mapView.MapBoundingBox.MinLon;
                }
                else
                {
                    latDegrees = 360.0 / System.Math.Pow(2, (int)System.Math.Ceiling(mapView.MapZoom));
                    lonDegrees = 360.0 / System.Math.Pow(2, (int)System.Math.Ceiling(mapView.MapZoom));
                }

                return new MapSpan(mapView.MapCenter.ToPosition(), latDegrees, lonDegrees);
            }
        }

        #region Methods

        /// <summary>
        ///  Move map to region defined by a MapSpan
        /// </summary>
        /// <param name="mapSpan">MapSpan of the new region to display</param>
        public void MoveToRegion(MapSpan mapSpan)
        {
            if (map == null)
                return;

            // Calc new zoom level
            var zoom = System.Math.Floor(System.Math.Log(360.0 / mapSpan.LatitudeDegrees, 2));

            mapView.SetMapView(mapSpan.Center.ToGeoCoordinate(), mapView.MapTilt, (float)zoom);
        }

        //public void AddPin(GeoCoordinate coord, string text)
        //{
        //    var layer = new LayerPrimitives();
        //    layer.AddFeature(new Geo.Features.Feature( () { })
        //}

        public void AddLayerTile(string url = "")
        {
            // Add MBTiles Layer.
            // any stream will do or any path on the device to a MBTiles SQLite databas
            // in this case the data is taken from the resource stream, written to disk and then opened.
            //map.AddLayer(new LayerMBTile(SQLiteConnection.CreateFrom(
            //            Assembly.GetExecutingAssembly().GetManifestResourceStream(@"YOUR MB TILES FILE HERE"), "map")));

            if (string.IsNullOrEmpty(url))
                return;

            tileUrl = url;

            if (layerMap != null)
                layerMap.Close();

            layerMap = map?.AddLayerTile(tileUrl);
        }

        public void AddLayerOsm(Stream stream, Stream cssStream)
        {
            if (stream == null)
                return;

            // Create the MapCSS image source, which is used as image source for areas of the map
            var imageSource = new MapCSSDictionaryImageSource();

			// Load mapcss style interpreter.
			var mapCSSInterpreter = new MapCSSInterpreter(cssStream, imageSource);

            // Load data from pbf file into a memory data source
            var input = new PBFOsmStreamSource(stream);
            //var filter = new OsmStreamFilterBoundingBox(mapView.MapBoundingBox);
            //filter.RegisterSource(input);
            //var source = MemoryDataSource.CreateFromPBFStream(input);
            var source = MemoryDataSource.CreateFrom(input);
            //var source = new MemoryDataSource();

            // If there is allready a layer with map, close it
            if (layerMap != null)
                layerMap.Close();

            tileUrl = "";

            // Add new map layer
            layerMap = map?.AddLayerOsm(source, mapCSSInterpreter);
        }

        /// <summary>
        /// Method which is called after native map is created
        /// </summary>
        public void MapCreated()
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                // Set some initial values
                mapView.MapTilt = 0;
                mapView.MapZoom = 14;
                mapView.MapAllowTilt = false;

                MapCenter = new MapSpan(new Math.Geo.GeoCoordinate(48.487, 9.215).ToPosition(), 
                    360.0/System.Math.Pow(2, System.Math.Ceiling(mapView.MapZoom)), 
                    360.0/System.Math.Pow(2, System.Math.Ceiling(mapView.MapZoom)));

                // Create map layer
                if (!string.IsNullOrWhiteSpace(tileUrl))
                    layerMap = map?.AddLayerTile(tileUrl);
            });
        }

        #endregion

        private void UserPositionChanged(object sender, PositionEventArgs e)
        {
            // Calculate new center
            var center = new GeoCoordinate(e.Position.Latitude, e.Position.Longitude);

            if (IsShowingUser)
                DrawUserIcon(center, e.Position.Accuracy);

            // Save for later use
            lastKnownPosition = center;
            lastKnownAccuracy = e.Position.Accuracy;
        }

        private void DrawUserIcon(GeoCoordinate coord, double accuracy)
        {
            // If there no coordiantes, we have nothing to do
            if (coord == null)
                return;

            if (layerAccuracy == null)
            {
                // Create layer for accuracy
                layerAccuracy = new LayerPrimitives(map.Projection);
                // Add new accuracy layer
                map.AddLayer(layerAccuracy);
            }

            if (layerUser == null)
            {
                // Create layer for user icon
                layerUser = new LayerPrimitives(map.Projection);
                // Add layer for user icon
                map.AddLayer(layerUser);
            }

            var repaintUserIcon = coord.Latitude != lastKnownPosition?.Latitude || coord.Longitude != lastKnownPosition?.Longitude;
            var repaintAccuracy = true;

            // Stop painting
            layerAccuracy.Pause();
            layerUser.Pause();

            if (repaintAccuracy)
                layerAccuracy.Clear();

            if (repaintUserIcon)
                layerUser.Clear();

            if (IsShowingUserInCenter)
                mapView.MapCenter = coord;

            if (repaintAccuracy)
            {
                var size = 2.2f + mapView.Density * (float)accuracy;
                layerAccuracy.AddPoint(coord, size > 40 ? 40 : size, SimpleColor.FromArgb(32, 0, 0, 255).Value);
            }

            if (repaintUserIcon)
            {
                layerUser.AddPoint(coord, mapView.Density * 2.2f, SimpleColor.FromKnownColor(KnownColor.Blue).Value);
                layerUser.AddPoint(coord, mapView.Density * 1.8f, SimpleColor.FromKnownColor(KnownColor.White).Value);
                layerUser.AddPoint(coord, mapView.Density * 1.2f, SimpleColor.FromKnownColor(KnownColor.Blue).Value);
            }

            layerAccuracy.Resume();
            layerUser.Resume();
        }

        private void MapMoved(IMapView mapView, float newZoom, Degree newTilt, GeoCoordinate newCenter)
        {
            IsShowingUserInCenter = false;
        }

        protected override SizeRequest OnSizeRequest(double widthConstrain, double heightConstrain)
        {
            var maxWidth = widthConstrain;
            var maxHeight = heightConstrain;

            if (double.IsPositiveInfinity(maxWidth))
                maxWidth = ((View)Parent).Width;

            if (double.IsPositiveInfinity(maxHeight))
                maxHeight = ((View)Parent).Height;

            return new SizeRequest(new Size(maxWidth, maxHeight), new Size(maxWidth, maxHeight));
        }
    }
}
