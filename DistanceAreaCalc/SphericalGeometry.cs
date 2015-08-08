// Converted from  http://tubalmartin.github.io/spherical-geometry-php/
// Test results look same (This C# Version and PHP Version have good agreement)
// However, it doesn't look same as what google map / Earth does.
// Especially if you have dent or crossover in polygone
// Don't assume it's perfect.
using System;
using System.Collections.Generic;

namespace DistanceAreaCalc {

	public class SphericalGeometry {

		// This is original copyright. and this C# code was Converted by me.
		/*!
		 * Spherical Geometry PHP Library v1.1.1
		 * http://tubalmartin.github.com/spherical-geometry-php/
		 *
		 * Copyright 2012, Túbal Martín
		 * Dual licensed under the MIT or GPL Version 2 licenses.
		 *
		 * This code is a port of some classes from the Google Maps Javascript API version 3.x
		 */



		/** 
		* Static class SphericalGeometry
		* Utility functions for computing geodesic angles, distances and areas.
			*/  

			public static double EQUALS_MARGIN_ERROR = 1.0E-9;

		// Earth's radius (at the Ecuator) of 6378137 meters.
		public static double EARTH_RADIUS = 6378137; 


		public static double getEarthRadius()
		{
			return EARTH_RADIUS;
		}

		public static double deg2rad(double angle)
		{
			return Math.PI * angle / 180.0;
		}

		public static double rad2deg(double angle)
		{
			return Math.PI * angle / 180.0;
		}

		/**
	     * Computes a bounding rectangle (LatLngBounds instance) from a point and a given radius.
	     * Reference: http://www.movable-type.co.uk/scripts/latlong-db.html
	     *
	     *  -------------NE
	     * |              |
	     * |        radius|
	     * |       o------|
	     * |              |
	     * |              |
	     * SW-------------
	     *
	     * @param object LatLng
	     * @param int|float radius (In meters)
	     */
		public static LatLngBounds computeBounds(LatLng latlng, double radius)
		{
			double latRadiansDistance = radius / EARTH_RADIUS;
			double latDegreesDistance = rad2deg(latRadiansDistance);
			double lngDegreesDistance = rad2deg(latRadiansDistance / Math.Cos(deg2rad(latlng.Lat)));

			// SW point
			double swLat = latlng.Lat - latDegreesDistance;
			double swLng = latlng.Lng - lngDegreesDistance;
			LatLng sw = new LatLng(swLat, swLng);

			// NE point
			double neLat = latlng.Lat + latDegreesDistance;
			double neLng = latlng.Lng + lngDegreesDistance;
			LatLng ne = new LatLng(neLat, neLng);

			return new LatLngBounds(sw, ne);
		}

		public static double computeHeading(LatLng fromLatLng, LatLng toLatLng)
		{
			double fromLat = deg2rad(fromLatLng.Lat);
			double toLat = deg2rad(toLatLng.Lat);
			double lng = deg2rad(toLatLng.Lng) - deg2rad(fromLatLng.Lng);

			return wrapLongitude(rad2deg(Math.Atan2(Math.Sin(lng) * Math.Cos(toLat), Math.Cos(fromLat) 
				* Math.Sin(toLat) - Math.Sin(fromLat) * Math.Cos(toLat) * Math.Cos(lng))));
		}

		public static LatLng computeOffset(LatLng fromLatLng, double distance, double heading) 
		{
			distance /= EARTH_RADIUS;
			heading = deg2rad(heading);
			double fromLat = deg2rad(fromLatLng.Lat);
			double cosDistance = Math.Cos(distance);
			double sinDistance = Math.Sin(distance);
			double sinFromLat = Math.Sin(fromLat);
			double cosFromLat = Math.Cos(fromLat);
			double sc = cosDistance * sinFromLat + sinDistance * cosFromLat * Math.Cos(heading);

			double lat = rad2deg(Math.Asin(sc));
			double lng = rad2deg(deg2rad(fromLatLng.Lng) + Math.Atan2(sinDistance * cosFromLat 
				* Math.Sin(heading), cosDistance - sinFromLat * sc));

			return new LatLng(lat, lng);
		}

		public static LatLng interpolate(LatLng fromLatLng, LatLng toLatLng, double fraction)
		{
			double radFromLat = deg2rad(fromLatLng.Lat);
			double radFromLng = deg2rad(fromLatLng.Lng);
			double radToLat = deg2rad(toLatLng.Lat);
			double radToLng = deg2rad(toLatLng.Lng);
			double cosFromLat = Math.Cos(radFromLat);
			double cosToLat = Math.Cos(radToLat);
			double radDist = _computeDistanceInRadiansBetween(fromLatLng, toLatLng);
			double sinRadDist = Math.Sin(radDist);

			if (sinRadDist < 1.0E-6)
			{
				return new LatLng(fromLatLng.Lat, fromLatLng.Lng);
			}

			double a = Math.Sin((1 - fraction) * radDist) / sinRadDist;
			double b = Math.Sin(fraction * radDist) / sinRadDist;
			double c = a * cosFromLat * Math.Cos(radFromLng) + b * cosToLat * Math.Cos(radToLng);
			double d = a * cosFromLat * Math.Sin(radFromLng) + b * cosToLat * Math.Sin(radToLng);

			double lat = rad2deg(Math.Atan2(a * Math.Sin(radFromLat) + b * Math.Sin(radToLat),  Math.Sqrt(Math.Pow(c,2) + Math.Pow(d,2))));
			double lng = rad2deg(Math.Atan2(d, c));

			return new LatLng(lat, lng);
		}

		public static double computeDistanceBetween(LatLng LatLng1, LatLng LatLng2)
		{
			return _computeDistanceInRadiansBetween(LatLng1, LatLng2) * EARTH_RADIUS;
		}

		public static double computeLength(List<LatLng> LatLngsArray) 
		{
			double length = 0;

			for (int i = 0, l = LatLngsArray.Count - 1; i < l; ++i) 
			{
				length += computeDistanceBetween(LatLngsArray[i], LatLngsArray[i + 1]);
			}    

			return length;
		}

		public static double computeArea(List<LatLng> LatLngsArray)
		{
			return  Math.Abs(computeSignedArea(LatLngsArray, false));
		}

		public static double computeSignedArea(List<LatLng> LatLngsArray, bool signed = true)
		{
			if (LatLngsArray.Count == 0 || LatLngsArray.Count < 3) return 0;

			double e = 0;
			double r2 = Math.Pow(EARTH_RADIUS, 2);

			for (int i = 1, l = LatLngsArray.Count - 1; i < l; ++i) 
			{
				e += _computeSphericalExcess(LatLngsArray[0], LatLngsArray[i], LatLngsArray[i + 1], signed);
			}

			return e * r2;
		}

		// Clamp latitude
		public static double clampLatitude(double lat)
		{
			return Math.Min(Math.Max(lat, -90), 90); 
		}

		// Wrap longitude
		public static double wrapLongitude(double lng)
		{
			return lng == 180 ? lng : ((((lng - -180)%360.0) + 360)%360.0) + -180;
		}

		/**
     * Computes the great circle distance (in radians) between two points.
     * Uses the Haversine formula.
     */
		protected static double _computeDistanceInRadiansBetween(LatLng LatLng1, LatLng LatLng2)
		{
			double p1RadLat = deg2rad(LatLng1.Lat);
			double p1RadLng = deg2rad(LatLng1.Lng);
			double p2RadLat = deg2rad(LatLng2.Lat);
			double p2RadLng = deg2rad(LatLng2.Lng);
			return 2 * Math.Asin( Math.Sqrt(Math.Pow(Math.Sin((p1RadLat - p2RadLat) / 2), 2) + Math.Cos(p1RadLat) 
				* Math.Cos(p2RadLat) * Math.Pow(Math.Sin((p1RadLng - p2RadLng) / 2), 2)));
		}

		/**
     * Computes the spherical excess.
     * Uses L'Huilier's Theorem.
     */
		protected static double _computeSphericalExcess(LatLng LatLng1, LatLng LatLng2, LatLng LatLng3, bool signed)
		{
			LatLng[] latLngsArray = new LatLng[]{LatLng1, LatLng2, LatLng3, LatLng1};
			double[] distances = new double[3];
			double sumOfDistances = 0;

			for (int i = 0; i < 3; ++i) 
			{
				distances[i] = _computeDistanceInRadiansBetween(latLngsArray[i], latLngsArray[i + 1]);
				sumOfDistances += distances[i];
			}

			double semiPerimeter = sumOfDistances / 2;
			double tan = Math.Tan(semiPerimeter / 2);

			for (int i = 0; i < 3; ++i) 
			{ 
				tan *= Math.Tan((semiPerimeter - distances[i]) / 2);
			}

			double sphericalExcess = 4 * Math.Atan( Math.Sqrt( Math.Abs(tan)));

			if (!signed) 
			{
				return sphericalExcess;
			}

			// Negative or positive sign?
			//latLngsArray.RemoveAt(latLngsArray.Length-1);
			Array.Copy (latLngsArray, latLngsArray, latLngsArray.Length - 1);
			double[,] v = new double[3,3];

			for (int i = 0; i < 3; ++i) 
			{ 
				LatLng LatLng = latLngsArray[i];
				double lat = deg2rad(LatLng.Lat);
				double lng = deg2rad(LatLng.Lng);

				//v[i] = array();
				v[i,0] = Math.Cos(lat) * Math.Cos(lng);
				v[i,1] = Math.Cos(lat) * Math.Sin(lng);
				v[i,2] = Math.Sin(lat);
			}

			double sign = (v[0,0] * v[1,1] * v[2,2] 
				+ v[1,0] * v[2,1] * v[0,2] 
				+ v[2,0] * v[0,1] * v[1,2] 
				- v[0,0] * v[2,1] * v[1,2] 
				- v[1,0] * v[0,1] * v[2,2] 
				- v[2,0] * v[1,1] * v[0,2]) > 0 ? 1 : -1;

			return sphericalExcess * sign;
		}
	}



	public class LatLng
	{
		protected double _lat;
		protected double _lng;

		public double Lat{
			get {
				return this._lat;
			}
		}
		public double Lng{
			get {
				return this._lng;
			}
		}

		public LatLng(double lat, double lng, bool noWrap = false)
		{
			lat = (float) lat;
			lng = (float) lng;


			if (noWrap == false)
			{
				lat = SphericalGeometry.clampLatitude(lat);
				lng = SphericalGeometry.wrapLongitude(lng);
			}

			this._lat = lat;
			this._lng = lng;
		}

		public double getLat()
		{
			return this._lat;
		}

		public double getLng()
		{
			return this._lng;
		}

		public bool equals(LatLng LatLng)
		{			
			if (LatLng == null) {
				return false;
			}
			return  (Math.Abs(this._lat-LatLng.Lat) +  Math.Abs(this._lng-LatLng.Lng)) <= SphericalGeometry.EQUALS_MARGIN_ERROR;             
		}

		public string toString()
		{
			return "("+this._lat+", "+ this._lng +")";
		}

		public string toUrlValue(int precision = 6)
		{
			precision = (int) precision;
			return Math.Round(this._lat, precision) +","+ Math.Round(this._lng, precision);
		}
	}



	public class LatLngBounds
	{
		LatBounds _LatBounds;
		LngBounds _LngBounds;

		/**
	     * LatLngSw South West LatLng object
	     * LatLngNe North East LatLng object
	     */
		public LatLngBounds(LatLng LatLngSw = null, LatLng LatLngNe = null) 
		{   


			if (LatLngSw != null)
			{
				LatLngNe = (LatLngNe!=null) ? LatLngSw : LatLngNe;
				double sw = SphericalGeometry.clampLatitude(LatLngSw.Lat);
				double ne = SphericalGeometry.clampLatitude(LatLngNe.Lat);
				this._LatBounds = new LatBounds(sw, ne);

				sw = LatLngSw.Lng;
				ne = LatLngNe.Lng;

				if (360 <= ne - sw)
				{
					this._LngBounds = new LngBounds(-180, 180);
				}
				else 
				{
					sw = SphericalGeometry.wrapLongitude(sw);
					ne = SphericalGeometry.wrapLongitude(ne);
					this._LngBounds = new LngBounds(sw, ne);
				}
			} 
			else 
			{
				this._LatBounds = new LatBounds(1, -1);
				this._LngBounds = new LngBounds(180, -180);
			}
		}

		public LatBounds getLatBounds()
		{
			return this._LatBounds;
		}

		public LngBounds getLngBounds()
		{
			return this._LngBounds;
		}

		public LatLng getCenter()
		{
			return new LatLng(this._LatBounds.getMidpoint(), this._LngBounds.getMidpoint());
		}

		public bool isEmpty()
		{
			return this._LatBounds.isEmpty() || this._LngBounds.isEmpty();
		}

		public LatLng getSouthWest()
		{
			return new LatLng(this._LatBounds.getSw(), this._LngBounds.getSw(), true);
		}

		public LatLng getNorthEast()
		{
			return new LatLng(this._LatBounds.getNe(), this._LngBounds.getNe(), true);
		}

		public LatLng toSpan()
		{
			double lat = this._LatBounds.isEmpty() ? 0 : this._LatBounds.getNe() - this._LatBounds.getSw();
			double lng = this._LngBounds.isEmpty() 
				? 0 
				: (this._LngBounds.getSw() > this._LngBounds.getNe() 
					? 360 - (this._LngBounds.getSw() - this._LngBounds.getNe())
					: this._LngBounds.getNe() - this._LngBounds.getSw());

			return new LatLng(lat, lng, true);
		}

		public string toString()
		{
			return "("+ this.getSouthWest().toString() +", "+ this.getNorthEast().toString() +")";
		}

		public string toUrlValue(int precision = 6)
		{
			return this.getSouthWest().toUrlValue(precision) +","+
				this.getNorthEast().toUrlValue(precision);
		}

		public bool equals(LatLngBounds LatLngBounds)
		{
			return !(LatLngBounds!=null) 
				? false 
					: this._LatBounds.equals(LatLngBounds.getLatBounds()) 
				&& this._LngBounds.equals(LatLngBounds.getLngBounds());
		}

		public bool intersects(LatLngBounds latlngBounds)
		{
			return this._LatBounds.intersects(latlngBounds.getLatBounds()) 
				&& this._LngBounds.intersects(latlngBounds.getLngBounds());
		}

		public LatLngBounds union(LatLngBounds LatLngBounds)
		{
			this.extend(LatLngBounds.getSouthWest());
			this.extend(LatLngBounds.getNorthEast());
			return this;
		}

		public bool contains(LatLng LatLng)
		{
			return this._LatBounds.contains(LatLng.Lat) 
				&& this._LngBounds.contains(LatLng.Lng);
		}

		public LatLngBounds extend(LatLng LatLng)
		{
			this._LatBounds.extend(LatLng.Lat);
			this._LngBounds.extend(LatLng.Lng);
			return this;    
		}
	}


	// DO NOT USE THE CLASSES BELOW DIRECTLY


	public class LatBounds
	{
		protected double _swLat;
		protected double _neLat;

		public LatBounds(double swLat,double neLat) 
		{
			this._swLat = swLat;
			this._neLat = neLat;
		}

		public double getSw()
		{
			return this._swLat;
		}

		public double getNe()
		{
			return this._neLat;
		}

		public double getMidpoint()
		{
			return (this._swLat + this._neLat) / 2;
		}

		public bool isEmpty()
		{
			return this._swLat > this._neLat;
		}

		public bool intersects(LatBounds LatBounds)
		{
			return this._swLat <= LatBounds.getSw() 
				? LatBounds.getSw() <= this._neLat && LatBounds.getSw() <= LatBounds.getNe() 
					: this._swLat <= LatBounds.getNe() && this._swLat <= this._neLat;
		}

		public bool equals(LatBounds LatBounds)
		{
			return this.isEmpty() 
				? LatBounds.isEmpty() 
					:  Math.Abs(LatBounds.getSw() - this._swLat) 
				+  Math.Abs(this._neLat - LatBounds.getNe()) <= SphericalGeometry.EQUALS_MARGIN_ERROR;
		}

		public bool contains(double lat)
		{
			return lat >= this._swLat && lat <= this._neLat;
		}

		public void extend(double lat)
		{
			if (this.isEmpty()) 
			{
				this._neLat = this._swLat = lat;
			}
			else if (lat < this._swLat) 
			{ 
				this._swLat = lat;
			}
			else if (lat > this._neLat) 
			{
				this._neLat = lat;
			}
		}
	}



	public class LngBounds
	{
		protected double _swLng;
		protected double _neLng;

		public LngBounds(double swLng, double neLng) 
		{
			swLng = swLng == -180 && neLng != 180 ? 180 : swLng;
			neLng = neLng == -180 && swLng != 180 ? 180 : neLng;

			this._swLng = swLng;
			this._neLng = neLng;
		}

		public double getSw()
		{
			return this._swLng;
		}

		public double getNe()
		{
			return this._neLng;
		}

		public double getMidpoint()
		{
			double midPoint = (this._swLng + this._neLng) / 2;

			if (this._swLng > this._neLng) 
			{
				midPoint = SphericalGeometry.wrapLongitude(midPoint + 180);
			}

			return midPoint;
		}

		public bool isEmpty()
		{
			return this._swLng - this._neLng == 360;
		}

		public bool intersects(LngBounds LngBounds)
		{
			if (this.isEmpty() || LngBounds.isEmpty()) 
			{
				return false;
			}
			else if (this._swLng > this._neLng) 
			{
				return LngBounds.getSw() > LngBounds.getNe() 
					|| LngBounds.getSw() <= this._neLng 
					|| LngBounds.getNe() >= this._swLng;
			}
			else if (LngBounds.getSw() > LngBounds.getNe()) 
			{
				return LngBounds.getSw() <= this._neLng || LngBounds.getNe() >= this._swLng;
			}
			else 
			{
				return LngBounds.getSw() <= this._neLng && LngBounds.getNe() >= this._swLng;
			}
		}

		public bool equals(LngBounds LngBounds)
		{
			return this.isEmpty () 
				? LngBounds.isEmpty () 
					: (Math.Abs (LngBounds.getSw () - this._swLng) % 360.0f
						+ Math.Abs (LngBounds.getNe () - this._neLng) % 360.0f)
				<= SphericalGeometry.EQUALS_MARGIN_ERROR;   
		}

		public bool contains(double lng)
		{
			lng = lng == -180 ? 180 : lng;

			return this._swLng > this._neLng 
				? (lng >= this._swLng || lng <= this._neLng) && !this.isEmpty()
					: lng >= this._swLng && lng <= this._neLng;
		}

		public void extend(double lng)
		{
			if (this.contains(lng)) 
			{
				return;
			}

			if (this.isEmpty())
			{
				this._swLng = this._neLng = lng;
			}
			else 
			{
				double swLng = this._swLng - lng;
				swLng = swLng >= 0 ? swLng : this._swLng + 180 - (lng - 180);
				double neLng = lng - this._neLng;
				neLng = neLng >= 0 ? neLng : lng + 180 - (this._neLng - 180);

				if (swLng < neLng) 
				{
					this._swLng = lng;
				}
				else 
				{
					this._neLng = lng;
				}
			}
		}

	}
}

