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

		public static Stream vShader;
		public static Stream fShader;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);


			input = Assets.Open ("Torus3D_smooth.xml");
			vShader = Assets.Open ("vShader_Torus.vs");
			fShader = Assets.Open ("fShader_Torus.fs");
			/// Inflate our UI from its XML layout description
			// - should match filename res/layout/main.axml ?
			SetContentView (Resource.Layout.Main);

			var textView = FindViewById<TextView> (Resource.Id.textView1);
			textView.Text = "Nuno Ramalho"; //vid.Vertexs.Count.ToString();

			// Load the view
			var glView = FindViewById<GLView> (Resource.Id.glview);

			glView.GiveTextView (textView);



			
		}

		protected override void OnPause ()
		{
			// never forget to do this!
			base.OnPause ();
			(FindViewById (Resource.Id.glview) as GLView).Pause ();
		}

		protected override void OnResume ()
		{
			// never forget to do this!
			base.OnResume ();
			(FindViewById (Resource.Id.glview) as GLView).Resume ();
		}
	}
}


