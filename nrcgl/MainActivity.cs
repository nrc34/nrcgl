﻿using System;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Content.PM;

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
		GLView view;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			// Create our OpenGL view, and display it
			view = new GLView (this);
			SetContentView (view);
		}

		protected override void OnPause ()
		{
			// never forget to do this!
			base.OnPause ();
			view.Pause ();
		}

		protected override void OnResume ()
		{
			// never forget to do this!
			base.OnResume ();
			view.Resume ();
		}
	}
}


