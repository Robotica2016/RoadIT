﻿using Android.App;
using Android.Widget;
using Android.Hardware;
using Android.Locations;
using Android.Content;
using Android.OS;
using Android.Gms.Common;
using Android.Util;
using System;
using Android.Support.V4.App;
using Android.Support.Design.Widget;
using Android.Views;

using System.Threading;

namespace RoadIT
{
	[Activity(Label = "TruckTrack", MainLauncher = true, Icon = "@mipmap/icon", ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation)]
	public class MainActivity : Activity
	{
		public static readonly int InstallGooglePlayServicesId = 1000;
		private bool _isGooglePlayServicesInstalled;

		//View layout;

		static readonly int REQUEST_COARSELOCATION = 0;
		static readonly int REQUEST_FINELOCATION = 1;
		static readonly int REQUEST_INTERNET = 2;

		//static string[] PERMISSIONS_CONTACT = {

		//	Android.Manifest.Permission.AccessCoarseLocation,
		//	Android.Manifest.Permission.AccessFineLocation,
		//	Android.Manifest.Permission.Internet
		//};

		protected override void OnCreate(Bundle bundle)
		{
			base.OnCreate(bundle);
			_isGooglePlayServicesInstalled = TestIfGooglePlayServicesIsInstalled();

			SetContentView(Resource.Layout.Setup);

			initLocationManager();
			RequestInternetPermission();
			confirmSettings();


		}

		public void confirmSettings()
		{
			Button startButton = FindViewById<Button>(Resource.Id.startButton);
			startButton.Click += delegate {
				EditText broker = FindViewById<EditText>(Resource.Id.editTextbroker);
				string brokerstring = broker.Text;

				EditText name = FindViewById<EditText>(Resource.Id.editTextname);
				string namestring = name.Text;

				EditText username = FindViewById<EditText>(Resource.Id.editTextusername);
				string usernamestring = username.Text;

				EditText pass = FindViewById<EditText>(Resource.Id.editTextpassword);
				string passtring = username.Text;

				RadioButton truckfin = FindViewById<RadioButton>(Resource.Id.radiotruck);
				string truck;
				if (truckfin.Checked==true){
					truck = "true";
				}else{
					truck="false";
				}
				SampleActivity activityfin = new SampleActivity(1, 2, typeof(OwnVehicle));
				var ownvec = new Intent(this, typeof(OwnVehicle));
				ownvec.PutExtra("broker",brokerstring );
				ownvec.PutExtra("name",namestring );
				ownvec.PutExtra("username",usernamestring );
				ownvec.PutExtra("pass",passtring );
				ownvec.PutExtra("truck",truck);
				StartActivity(ownvec);

			};
		}

		private bool TestIfGooglePlayServicesIsInstalled()
		{
			int queryResult = GoogleApiAvailability.Instance.IsGooglePlayServicesAvailable(this);
			if (queryResult == ConnectionResult.Success)
			{
				Log.Info("Road IT", "Google Play Services is installed on this device.");
				return true;
			}

			if (GoogleApiAvailability.Instance.IsUserResolvableError(queryResult))
			{
				string errorString = GoogleApiAvailability.Instance.GetErrorString(queryResult);
				Log.Error("Road IT", "There is a problem with Google Play Services on this device: {0} - {1}", queryResult, errorString);
				Dialog errorDialog = GoogleApiAvailability.Instance.GetErrorDialog(this, queryResult, InstallGooglePlayServicesId);
				//ErrorDialogFragment dialogFrag = new ErrorDialogFragment(errorDialog);

				//dialogFrag.Show(FragmentManager, "GooglePlayServicesDialog");
			}
			return false;
		}

		public void initLocationManager()
		{
			
			if (ActivityCompat.CheckSelfPermission(this, Android.Manifest.Permission.AccessCoarseLocation) != (int)Android.Content.PM.Permission.Granted)
			{

				// CoarseLocation permission has not been granted
				RequestCoarsePermission();
			}
			if (ActivityCompat.CheckSelfPermission(this, Android.Manifest.Permission.AccessFineLocation) != (int)Android.Content.PM.Permission.Granted)
			{

				// FineLocation permission has not been granted
				RequestFinePermission();
			}
			if (ActivityCompat.CheckSelfPermission(this, Android.Manifest.Permission.Internet) != (int)Android.Content.PM.Permission.Granted)
			{

				// Internet permission has not been granted
				RequestInternetPermission();
			}
		}

		/**
     	* Requests the CoarseLocation permission.
		* If the permission has been denied previously, a SnackBar will prompt the user to grant the
		* permission, otherwise it is requested directly.
		*/
		void RequestCoarsePermission()
		{
			//Log.Info (TAG, "COARSE permission has NOT been granted. Requesting permission.");

			if (ActivityCompat.ShouldShowRequestPermissionRationale(this, Android.Manifest.Permission.AccessCoarseLocation))
			{
				// Provide an additional rationale to the user if the permission was not granted
				// and the user would benefit from additional context for the use of the permission.
				// For example if the user has previously denied the permission.
				//Log.Info (TAG, "Displaying COARSE permission rationale to provide additional context.");

				/*Snackbar
					.Make(layout, "Message sent", Snackbar.LengthLong)
  					.SetAction("OK", (view) => { ActivityCompat.RequestPermissions(this, new String[] { Android.Manifest.Permission.AccessCoarseLocation }, REQUEST_COARSELOCATION); })
  					.Show(); */
			}
			else {
				// Camera permission has not been granted yet. Request it directly.
				ActivityCompat.RequestPermissions(this, new String[] { Android.Manifest.Permission.AccessCoarseLocation }, REQUEST_COARSELOCATION);
			}
		}

		/**
     	* Requests the FineLocation permission.
		* If the permission has been denied previously, a SnackBar will prompt the user to grant the
		* permission, otherwise it is requested directly.
		*/
		void RequestFinePermission()
		{
			//Log.Info (TAG, "Fine permission has NOT been granted. Requesting permission.");

			if (ActivityCompat.ShouldShowRequestPermissionRationale(this, Android.Manifest.Permission.AccessCoarseLocation))
			{
				// Provide an additional rationale to the user if the permission was not granted
				// and the user would benefit from additional context for the use of the permission.
				// For example if the user has previously denied the permission.
				//Log.Info (TAG, "Displaying Fine permission rationale to provide additional context.");

				/*Snackbar
					.Make(layout, "Message sent", Snackbar.LengthLong)
  					.SetAction("OK", (view) => {ActivityCompat.RequestPermissions(this, new String[] { Android.Manifest.Permission.AccessFineLocation }, REQUEST_FINELOCATION); })
  					.Show();*/
			}
			else {
				// Camera permission has not been granted yet. Request it directly.
				ActivityCompat.RequestPermissions(this, new String[] { Android.Manifest.Permission.AccessFineLocation }, REQUEST_FINELOCATION);
			}
		}

		/**
     	* Requests the Internet permission.
		* If the permission has been denied previously, a SnackBar will prompt the user to grant the
		* permission, otherwise it is requested directly.
		*/
		void RequestInternetPermission()
		{
			//Log.Info (TAG, "Internet permission has NOT been granted. Requesting permission.");

			if (ActivityCompat.ShouldShowRequestPermissionRationale(this, Android.Manifest.Permission.AccessCoarseLocation))
			{
				// Provide an additional rationale to the user if the permission was not granted
				// and the user would benefit from additional context for the use of the permission.
				// For example if the user has previously denied the permission.
				//Log.Info (TAG, "Displaying Intenet permission rationale to provide additional context.");

				/*Snackbar
					.Make(layout, "Message sent", Snackbar.LengthLong)
  					.SetAction("OK", (view) => { ActivityCompat.RequestPermissions(this, new String[] { Android.Manifest.Permission.Internet }, REQUEST_INTERNET); })
  					.Show();*/
			}
			else {
				// Camera permission has not been granted yet. Request it directly.
				ActivityCompat.RequestPermissions(this, new String[] { Android.Manifest.Permission.Internet }, REQUEST_INTERNET);
			}
		}


	}
}

