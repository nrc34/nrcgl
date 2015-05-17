
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
using Android.Text;
using Android.Text.Style;
using Android.Graphics;

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

			#region multicolor vShadorText
			var span = new SpannableString (Intent.GetStringExtra ("vShader"));

			var types = new string[]{"vec2", "vec3", "vec4", "mat3", 
									        "mat4", "float", "int" };

			var spanInfosTypes = 
				Tools.SpanInfos (types, 
								 Intent.GetStringExtra ("vShader"), 
								 Color.Brown);

			foreach (var item in spanInfosTypes) {

				span.SetSpan (new ForegroundColorSpan (item.SpanColor),
							  item.SpanStart, 
							  item.SpanEnd, 
							  0);
			}

			var typesInit = new string[]{"uniform", "varying", "attribute"};

			var spanTypesInit = 
				Tools.SpanInfos (typesInit, 
					Intent.GetStringExtra ("vShader"), 
					Color.CadetBlue);

			foreach (var item in spanTypesInit) {

				span.SetSpan (new ForegroundColorSpan (item.SpanColor),
					item.SpanStart, 
					item.SpanEnd, 
					0);
			}

			var operators = new string[]{"*", "+", "-", "/", 
										 "=", "(", ")", ",", "{", "}"};

			var spanOperators = 
				Tools.SpanInfos (operators, 
					Intent.GetStringExtra ("vShader"), 
					Color.Yellow);

			foreach (var item in spanOperators) {

				span.SetSpan (new ForegroundColorSpan (item.SpanColor),
					item.SpanStart, 
					item.SpanEnd, 
					0);
			}


			#endregion


			mTextViewVShader.SetText(span, TextView.BufferType.Spannable);
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

