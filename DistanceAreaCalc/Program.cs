using System;
using System.Collections.Generic;
namespace DistanceAreaCalc {
	class MainClass {
		public static void Main (string[] args) {
			// colorado
			var poly = new List<LatLng> {
				new LatLng (41.0296433871664, -109.05029296875),
				new LatLng (40.94671366508, -102.01904296875),
				new LatLng (37.0200982013681, -102.041015625),
				new LatLng (36.985003092856, -109.05029296875)
			};

			//			var poly = new List<LatLng> {
			//				new LatLng(47.9899216674142,61.171875),
			//				new LatLng(50.2893392532918,68.90625),
			//				new LatLng(52.9089020477703,78.046875),
			//				new LatLng(56.5594824837622,95.625),
			//				new LatLng(45.5832897560063,93.515625),
			//				new LatLng(49.3823727870096,101.953125),
			//				new LatLng(38.272688535981,94.21875),
			//				new LatLng(34.8859309407532,80.15625),
			//				new LatLng(32.5468131735152,57.65625),
			//				new LatLng(43.0688877741696,52.03125)
			//			};

			//			var poly = new List<LatLng> {
			//				new LatLng(41.0130657870063,-109.1162109375),
			//				new LatLng(36.985003092856,-109.2041015625),
			//				new LatLng(40.4803814290817,-95.4931640625),
			//				new LatLng(37.3701571840575,-94.4384765625)				
			//			};

			double area = SphericalGeometry.computeArea(poly);
			Console.WriteLine("Area: " + area/1000000 + " km^2");
			double distance = SphericalGeometry.computeLength (poly);
			Console.WriteLine ("Length: " + distance/1000 + " km");
		}
	}
}
