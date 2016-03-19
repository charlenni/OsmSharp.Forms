using OsmSharp.Math.Geo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms.Maps;

namespace OsmSharp.Forms
{
    public static class PositionExtensions
    {
        public static GeoCoordinate ToGeoCoordinate(this Position pos)
        {
            return new GeoCoordinate(pos.Latitude, pos.Longitude);
        }
    }
}
