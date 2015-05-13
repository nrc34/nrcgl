using System;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Content.PM;
using OpenTK.Platform.Android;
using System.IO;

namespace nrcgl
{
	// the ConfigurationChanges flags set here keep the EGL context
	// from being destroyed whenever the device is rotated or the
	// keyboard is shown (highly recommended for all GL apps)
	[Activity (Label = "nrcgl",
				#if __ANDROID_11__
				HardwareAccelerated=false,
				#endif
		ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.KeyboardHidden,
		MainLauncher = true,
		Icon = "@drawable/icon")]
	public class MainActivity : Activity
	{
		public static Stream input;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);


			input = Assets.Open ("Torus.xml");

			/// Inflate our UI from its XML layout description
			// - should match filename res/layout/main.axml ?
			SetContentView (Resource.Layout.Main);

			// Load the view
			FindViewById (Resource.Id.glview);
		}

		protected override void OnPause ()
		{
			// never forget to do this!
			base.OnPause ();
			(FindViewById (Resource.Id.glview) as AndroidGameView).Pause ();
		}

		protected override void OnResume ()
		{
			// never forget to do this!
			base.OnResume ();
			(FindViewById (Resource.Id.glview) as AndroidGameView).Resume ();
		}
	}
}


