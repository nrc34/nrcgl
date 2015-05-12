using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Xamarin.UITest;
using Xamarin.UITest.Android;
using Xamarin.UITest.Queries;

namespace nrcgl.UITests
{
	[TestFixture]
	public class Tests
	{
		AndroidApp app;

		[SetUp]
		public void BeforeEachTest ()
		{
			app = ConfigureApp.Android.StartApp ();
		}

		[Test]
		public void FindOpenGLView ()
		{
			AppResult[] results = app.WaitForElement (c => c.Class ("GLView1"));
			app.Screenshot ("First screen.");

			Assert.IsTrue (results.Any ());
		}
	}
}

