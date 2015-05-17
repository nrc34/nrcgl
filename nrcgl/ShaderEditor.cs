
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Views.InputMethods;

namespace nrcgl
{
	[Activity (Label = "ShaderEditor")]			
	public class ShaderEditor : Activity
	{
		public TextView mTextViewVShader;
		public TextView mTextViewFShader;
		public Button mButton;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			// Create your application here

			SetContentView (Resource.Layout.ShaderEditor);

			mTextViewVShader = FindViewById<TextView> (Resource.Id.editTextVShader);
			mTextViewFShader = FindViewById<TextView> (Resource.Id.editTextFShader);


			mTextViewVShader.Text = Intent.GetStringExtra ("vShader");
			mTextViewFShader.Text = Intent.GetStringExtra ("fShader");

			mButton = FindViewById<Button> (Resource.Id.button1);
			mButton.Click += MButton_Click;
		}

		void MButton_Click (object sender, EventArgs e)
		{
			Intent intent = new Intent (this, typeof(MainActivity));
			intent.PutExtra ("vShader", mTextViewVShader.Text);
			intent.PutExtra ("fShader", mTextViewFShader.Text);

			StartActivity (intent);

			Finish ();
		}
	}
}

