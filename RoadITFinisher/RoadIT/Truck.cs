﻿using System;
using Android.Gms.Maps.Model;
using Android.Util;

namespace RoadIT
{
	public class Truck
	{
		private LatLng location;
		private string color;
		private int duration;
		private int id;
		private string locstring;
		private PolylineOptions polylineOptions;
		private bool nearest = false;
		private Random rnd = new Random();
		private string[] colorarray = new string[] { "red", "blue", "orange", "purple" };

		public Truck(LatLng location, int id)
		{
			this.location = location;
			color = colorarray[rnd.Next(0, colorarray.Length)];
			this.id = id;
			locstring = location.Latitude.ToString().Replace(",", ".") + "," + location.Longitude.ToString().Replace(",", ".");//+ "," + id;
			polylineOptions = new PolylineOptions();
		}

		public void setNearest(bool nearest)
		{
			this.nearest = nearest;
		}

		public bool getNearest()
		{
			return nearest;
		}

		public string getcolor()
		{
			return color;
		}

		public void setcolor(string color)
		{
			this.color = color;
		}

		public int getid()
		{
			return id;
		}

		public void setDuration(int duration)
		{
			this.duration = duration;
		}

		public int getDuration()
		{
			return duration;
		}

		public string getlocstring()
		{
			return locstring;
		}

		public void setPolylineOptions(PolylineOptions poly)
		{
			polylineOptions = poly;
		}

		public PolylineOptions getPolylineOptions()
		{
			return polylineOptions;
		}

		public LatLng getLocation()
		{
			return location;
		}

		public void setLocation(LatLng location)
		{
			this.location = location;
			locstring = location.Latitude.ToString().Replace(",", ".") + "," + location.Longitude.ToString().Replace(",", ".");
		}

		public void display()
		{
			Log.Debug("truckdisploc", locstring);
			Log.Debug("truckdispcoolor", color);
			Log.Debug("truckdsipid", id.ToString());
		}


	}
}

