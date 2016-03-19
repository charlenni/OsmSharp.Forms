using System.ComponentModel;
using OsmSharp.Forms;
using OsmSharp.UI.Map;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using UIKit;
using OsmSharp.iOS.UI;

[assembly: ExportRenderer(typeof(OsmMap), typeof(OsmSharp.Forms.iOS.OsmMapRenderer))]

namespace OsmSharp.Forms.iOS
{
    public class OsmMapRenderer : ViewRenderer<Xamarin.Forms.View, UIView>
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Xamarin.Forms.View> e)
        {
            base.OnElementChanged(e);

            // If there isn't a new Element, than we have to do nothing 
            if (e.OldElement != null || Element == null)
                return;

            // Create new Map and MapView
            var map = new Map();
            var mapView = new MapView();

            // Set Map of MapView to new created map
            mapView.Map = map;

            // If mapView belongs allready to another UIView ...
            if (mapView != null)
            {
                // ... than remove it
                UIView view = mapView;
                view.RemoveFromSuperview();
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