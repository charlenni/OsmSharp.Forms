using Android.Views;
using System.ComponentModel;
using OsmSharp.Android.UI;
using OsmSharp.Forms;
using OsmSharp.UI.Map;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(OsmMap), typeof(OsmSharp.Forms.Android.OsmMapRenderer))]

namespace OsmSharp.Forms.Android
{
    public class OsmMapRenderer : ViewRenderer<Xamarin.Forms.View, ViewGroup>
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Xamarin.Forms.View> e)
        {
            base.OnElementChanged(e);

            // If there isn't a new Element, than we have to do nothing 
            if (e.OldElement != null || Element == null)
                return;

            // Create new Map and MapView
            var map = new Map();
            var mapView = new MapView(this.Context, new MapViewSurface(this.Context));

            // Measure only visible objects
            mapView.SetMeasureAllChildren(false);

            // Set Map of MapView to new created map
            mapView.Map = map;

            // If mapView belongs allready to another ViewGroup ...
            if (mapView != null)
            {
                // ... than remove it
                ViewGroup vg = mapView;
                var par = ((ViewGroup)vg.Parent);
                if (par != null)
                    par.RemoveView(mapView);
            }

            // Save for later use
            ((OsmMap)Element).MapView = mapView;
            ((OsmMap)Element).Map = mapView.Map;

            // Set native control to new created MapView
            SetNativeControl(mapView);
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);
        }
    }
}