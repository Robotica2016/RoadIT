﻿using System;
using Android.App;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.OS;
using Android.Locations;
using Android.Util;
using Android.Widget;
using Android.Content;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Org.Eclipse.Paho.Client.Mqttv3;
using Org.Eclipse.Paho.Client.Mqttv3.Persist;

namespace RoadIT
{
	[Activity(Label = "Finisher")]
	public class Finisher : Activity, ILocationListener
	{
		//static readonly LatLng truck1loc = new LatLng(51.229241, 4.404648);
		LatLng finisherloc = new LatLng(0, 0);
		GoogleMap map;
		MapFragment mapFragment;
		LocationManager locMgr;
		string ownlocstring;
		//string truckstring;
		//string cinestring;
		//static string varloc = "";
		string durationString;
		JObject _Jobj;
		string tag = "MainActivity";
		Truck trucktoadd;

		PolylineOptions polylineOptions = new PolylineOptions();

		List<Truck> trucklist;

		MarkerOptions markertruck = new MarkerOptions();
		public static string broker = "tcp://iot.eclipse.org:1883";
		public static string clientId = "JavaSample";

		public static MemoryPersistence persistence = new MemoryPersistence();
		public static MqttClient Client = new MqttClient(broker, clientId, persistence);

		bool firstloc = true;

		public void OnLocationChanged(Android.Locations.Location location)
		{
			//Toast.MakeText(this, "Location changed", ToastLength.Long).Show();
			finisherloc = new LatLng(location.Latitude, location.Longitude);
			if (firstloc == true)
			{
				InitMarkers();
				ZoomOnLoc();
				locsToString();
				firstloc = false;
			}
			locsToString();
			RefreshMarkers();

			//Thread MapsAPICallThread = new Thread(() => mapAPICall(truckstring,"red"));
			//MapsAPICallThread.Start();

			ownlocstring = location.Latitude.ToString().Replace(",", ".") + "," + location.Longitude.ToString().Replace(",", ".");

			Thread PublishMQTT = new Thread(() => MQTTPublish(ownlocstring + ",0"));
			PublishMQTT.Start();
		}

		public void addToList(Truck truck)
		{
			trucklist.Add(truck);
		}

		public void MQTTPublish(string content)
		{

			string topic = "fin";
			int qos = 2;
			MemoryPersistence persistence = new MemoryPersistence();

			try
			{
				byte[] bytes = System.Text.Encoding.ASCII.GetBytes(content);
				MqttMessage message = new MqttMessage(bytes);
				message.Qos = qos;
				Client.Publish(topic, message);
				Log.Debug("MQTTPublish", message.ToString());
			}
			catch (MqttException me)
			{
				me.PrintStackTrace();
			}
		}

		public void MQTTupdate(string mqttmessage)
		{
			Char delimiter = ',';
			String[] substrings = mqttmessage.Split(delimiter);
			if (substrings.Length == 3)
			{
				try
				{
					if (Convert.ToDouble(substrings[2]) != 0)
					{
						//truck1loc.Latitude = Convert.ToDouble(substrings[0]);
						//truck1loc.Longitude = Convert.ToDouble(substrings[1]);
						//TODO todouble kapot nederlands?? punten verdwijnen?
						Log.Debug("mqttsubstring0", Convert.ToDouble(substrings[0]).ToString());
						Log.Debug("mqttsubstring1", Convert.ToDouble(substrings[1]).ToString());
						trucktoadd = new Truck(new LatLng(Convert.ToDouble(substrings[0]), Convert.ToDouble(substrings[1])), "red", Int32.Parse(substrings[2]));
						trucktoadd.display();
						trucklist.Add(trucktoadd);
						Log.Debug("mqttupdate", "truck added");

						//trucklist.Add(new Truck(new LatLng(Convert.ToDouble(substrings[0]), Convert.ToDouble(substrings[1])), "red", Int32.Parse(substrings[2])));
						Log.Debug("trucklist", trucklist.ToString());
						Log.Debug("MQTTinput", "Accept");
					}
				}
				catch
				{
					Log.Debug("MQTTinput", "input not right");
				}

			}
		}

		protected override void OnCreate(Bundle bundle)
		{
			base.OnCreate(bundle);
			Log.Debug(tag, "OnCreate called");
			SetContentView(Resource.Layout.Main);
			InitMapFragment();
			SetupAnimateToButton();
			Client.SetCallback(new MqttSubscribe());
			ConfigMQTT();
		}

		protected override void OnResume()
		{
			base.OnResume();
			Log.Debug(tag, "OnResume called");

			// initialize location manager
			locMgr = GetSystemService(Context.LocationService) as LocationManager;

			// pass in the provider (GPS),
			// the minimum time between updates (in seconds),
			// the minimum distance the user needs to move to generate an update (in meters),
			// and an ILocationListener (recall that this class impletents the ILocationListener interface)
			if (locMgr.AllProviders.Contains(LocationManager.NetworkProvider)
				&& locMgr.IsProviderEnabled(LocationManager.NetworkProvider))
			{
				locMgr.RequestLocationUpdates(LocationManager.NetworkProvider, 2000, 1, this);
			}
			else {
				Toast.MakeText(this, "The Network Provider does not exist or is not enabled!", ToastLength.Long).Show();
			}
		}

		public static void ConfigMQTT()
		{
			try
			{
				Client.Connect();
				Client.Subscribe("fin");
				Log.Debug("MqttSubscribe", "connect");
				//Toast.MakeText(this, "Subscribe(\"fin\")!", ToastLength.Long).Show();

			}
			catch (MqttException me)
			{
				Log.Debug("MqttSubscribe", "(re)connect failed");
				//Toast.MakeText(this, "Error: Subscribe(\"fin\")!\n" + me, ToastLength.Long).Show();

			}
		}


		protected override void OnStart()
		{
			base.OnStart();
			Log.Debug(tag, "OnStart called");
		}

		void InitMapFragment()
		{
			mapFragment = FragmentManager.FindFragmentByTag("map") as MapFragment;

			if (mapFragment == null)
			{
				GoogleMapOptions mapOptions = new GoogleMapOptions()
					.InvokeMapType(GoogleMap.MapTypeNormal)
					.InvokeZoomControlsEnabled(true)
					.InvokeCompassEnabled(true);
				
				FragmentTransaction fragTx = FragmentManager.BeginTransaction();
				mapFragment = MapFragment.NewInstance(mapOptions);
				fragTx.Add(Resource.Id.map, mapFragment, "map");
				fragTx.Commit();
			}

		}

		void SetupAnimateToButton()
		{
			Button RouteButton = FindViewById<Button>(Resource.Id.routeButton);
			RouteButton.Click += (sender, e) =>
			{
				//Toast.MakeText(this, "Button Pressed", ToastLength.Long).Show();
				//Thread drawRouteThread2 = new Thread(() => drawRoute(varloc, "blue"));

				//trucklist.Add(trucktoadd);
				if (trucklist != null)
				{
					if (trucklist.Count() != 0)
					{
						foreach (Truck aTruck in trucklist)
						{
							Thread mapAPICall2 = new Thread(() => mapAPICall(aTruck.getlocstring(), "blue"));
							mapAPICall2.Start();
						}
					}
					else {
						Toast.MakeText(this, "List empty", ToastLength.Long).Show();
					}
				}
				else {
					Toast.MakeText(this, "List null", ToastLength.Long).Show();
				}


			};
		}

		//niet meer nodig
		void ZoomOnLoc()
		{
			CameraPosition.Builder builder = CameraPosition.InvokeBuilder();
			builder.Target(finisherloc);
			builder.Zoom(12);
			builder.Bearing(0);
			builder.Tilt(0);
			CameraPosition cameraPosition = builder.Build();

			// AnimateCamera provides a smooth, animation effect while moving
			// the camera to the the position.
			map.AnimateCamera(CameraUpdateFactory.NewCameraPosition(cameraPosition));
		}

		void InitMarkers()
		{
			map = mapFragment.Map;
			BitmapDescriptor truck = BitmapDescriptorFactory.FromResource(Resource.Drawable.truck);
			//markertruck.SetPosition(truck1loc);
			markertruck.SetTitle("Truck");
			markertruck.SetIcon(truck);
			//map.AddMarker(markertruck);

			//blue location
			map.MyLocationEnabled = true;
		}

		void RefreshMarkers()
		{
			//markertruck.SetPosition(truck1loc);
		}

		public void OnProviderDisabled(string provider)
		{
			Log.Debug(tag, provider + " disabled by user");
		}
		public void OnProviderEnabled(string provider)
		{
			Log.Debug(tag, provider + " enabled by user");
		}
		public void OnStatusChanged(string provider, Availability status, Bundle extras)
		{
			Log.Debug(tag, provider + " availability has changed to " + status.ToString());
		}

		void locsToString()
		{
			//TODO , vervangen door punt
			//truckstring = truck1loc.Latitude.ToString().Replace(",",".") + "," + truck1loc.Longitude.ToString().Replace(",",".");
			//cinestring = cineloc.Latitude.ToString().Replace(",",".") + "," + cineloc.Longitude.ToString().Replace(",",".");
		}

		void getDuration()
		{
			//animateButton.Text = "Duration: " + getDistanceTo(ownlocstring,truckstring);
			durationString = "ETA of nearest truck: " + getDistanceTo() + "s";

			TextView durationtextfield = FindViewById<TextView>(Resource.Id.durationText);

			//update textfield in main UI thread
			RunOnUiThread(() => durationtextfield.Text = durationString);
		}

		void updateUI()
		{
			map.Clear();
			//map.AddMarker(markertruck);
			map.AddPolyline(polylineOptions);
		}

		public int getDistanceTo()
		{
			//System.Threading.Thread.Sleep(50);

			int duration = -1;
			try
			{
				duration = (int)_Jobj.SelectToken("routes[0].legs[0].duration.value");
				return duration;
			}
			catch
			{
				return duration;
			}
		}

		void mapAPICall(string origin, string color)
		{
			//System.Threading.Thread.Sleep(50);

			try
			{
				string url = "http://maps.googleapis.com/maps/api/directions/json?origin=" + origin + "&destination=" + ownlocstring + "&sensor=false";
				string requesturl = url; string content = fileGetJSON(requesturl);
				//Log.Debug("httpcontent", content);
				_Jobj = JObject.Parse(content);
			}
			catch { }

			Thread durationThread = new Thread(() => getDuration());
			durationThread.Start();

			Thread drawRouteThread = new Thread(() => drawRoute(color));
			drawRouteThread.Start();

			////draw route in main UI thread
			//RunOnUiThread(() => updateUI());
		}

		void drawRoute(string color)
		{
			Log.Debug("http", "drawroutestart");

			//TODO zorgen dat andere routes niet weggaa
			polylineOptions = new PolylineOptions();
			if (color == "blue")
			{
				polylineOptions.InvokeColor(0x66000099);
			}
			else if (color == "red")
			{
				polylineOptions.InvokeColor(0x66ff0000);
			}
			else
			{
				polylineOptions.InvokeColor(0x66000099);
			}

			polylineOptions.InvokeWidth(9);
			try
			{
				string polyPoints = (string)_Jobj.SelectToken("routes[0].overview_polyline.points");
				List<LatLng> drawCoordinates;
				drawCoordinates = DecodePolylinePoints(polyPoints);
				foreach (var position in drawCoordinates)
				{
					polylineOptions.Add(new LatLng(position.Latitude, position.Longitude));
				}
			}
			catch
			{}
			//draw route in main UI thread
			RunOnUiThread(() => updateUI());
		}

		List<LatLng> DecodePolylinePoints(string encodedPoints)
		{
			if (encodedPoints == null || encodedPoints == "") return null;
			List<LatLng> poly = new List<LatLng>();
			char[] polylinechars = encodedPoints.ToCharArray();
			int index = 0;

			int currentLat = 0;
			int currentLng = 0;
			int next5bits;
			int sum;
			int shifter;

			try
			{
				while (index < polylinechars.Length)
				{
					// calculate next latitude
					sum = 0;
					shifter = 0;
					do
					{
						next5bits = (int)polylinechars[index++] - 63;
						sum |= (next5bits & 31) << shifter;
						shifter += 5;
					} while (next5bits >= 32 && index < polylinechars.Length);

					if (index >= polylinechars.Length)
						break;

					currentLat += (sum & 1) == 1 ? ~(sum >> 1) : (sum >> 1);

					//calculate next longitude
					sum = 0;
					shifter = 0;
					do
					{
						next5bits = (int)polylinechars[index++] - 63;
						sum |= (next5bits & 31) << shifter;
						shifter += 5;
					} while (next5bits >= 32 && index < polylinechars.Length);

					if (index >= polylinechars.Length && next5bits >= 32)
						break;

					currentLng += (sum & 1) == 1 ? ~(sum >> 1) : (sum >> 1);

					double latdouble = Convert.ToDouble(currentLat) / 100000.0;
					double lngdouble = Convert.ToDouble(currentLng) / 100000.0;
					LatLng p = new LatLng(latdouble, lngdouble);
					poly.Add(p);
				}
			}
			catch (Exception ex)
			{
				// logo it
			}
			return poly;
		}

		protected string fileGetJSON(string fileName)
		{
			string _sData = string.Empty;
			string me = string.Empty;
			try
			{
				if (fileName.ToLower().IndexOf("http:") > -1)
				{
					System.Net.WebClient wc = new System.Net.WebClient();
					byte[] response = wc.DownloadData(fileName);
					_sData = System.Text.Encoding.ASCII.GetString(response);
				}
				else
				{
					System.IO.StreamReader sr = new System.IO.StreamReader(fileName);
					_sData = sr.ReadToEnd();
					sr.Close();
				}
			}
			catch { _sData = "unable to connect to server "; }
			return _sData;
		}
	}
}

