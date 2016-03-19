using OsmSharp.Math.Geo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms.Maps;

namespace OsmSharp.Forms.Extensions
{
    public static class GeoCoordinateExtensions
    {
        public static Position ToPosition(this GeoCoordinate coord)
        {
            return new Position(coord.Latitude, coord.Longitude);
        }
    }
}
