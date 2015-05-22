﻿using System;
using System.Runtime.InteropServices;
using System.Text;

using OpenTK;
using OpenTK.Graphics.ES20;
using OpenTK.Platform;
using OpenTK.Platform.Android;

using Android.Util;
using Android.Views;
using Android.Content;
using nrcgl.nrcgl;
using Android.Widget;
using System.IO;
using System.Threading;
using OpenTK.Graphics;
using nrcgl.nrcgl.shapes;
using System.Collections.Generic;

namespace nrcgl
{
	public class GLView : AndroidGameView
	{
		public MainActivity mainActivity;

		bool move2ShaderEditor = false;

		Int32 updateCounter;

		int viewportWidth, viewportHeight;
		int mSeekBarsHandle;

		float xTouch;
		float yTouch;

		Matrix4 mProjectionMatrix;
		Matrix4 mViewMatrix;

		Shader shader;
		bool IsShaderOK;

		float rotateY = 0f;
		float rotateX = 0f;

		float distTouch;
		float distTouchZero;
		int pointer1ID;
		int pointer2ID;
		float scale = 1f;

		BeginMode beginMode;

		Dictionary<string, Shape3D> Shapes3D;
		Stack<Shape3D> Shapes2Remove;
		Camera Camera;



		public GLView (Context context, IAttributeSet attrs) :
		base (context, attrs)
		{
			Init ();
		}

		public GLView (IntPtr handle, Android.Runtime.JniHandleOwnership transfer)
			: base (handle, transfer)
		{
			Init ();
		}

		void Init ()
		{
			updateCounter = 0;
			Run (60);
		}

		// This method is called everytime the context needs
		// to be recreated. Use it to set any egl-specific settings
		// prior to context creation
		protected override void CreateFrameBuffer ()
		{
			ContextRenderingApi = OpenTK.Graphics.GLVersion.ES2;

			// Set the graphics mode to use 32bpp colour format, 24bpp depth, 8bpp stencil and 4x MSAA
			//GraphicsMode = new GraphicsMode(new ColorFormat(32), 24, 8, 4); 
			GraphicsMode = new AndroidGraphicsMode (new ColorFormat(32), 24, 8, 4, 2, false);
			// the default GraphicsMode that is set consists of (16, 16, 0, 0, 2, false)
			try {
				Log.Verbose ("GLTriangle", "Loading with default settings");

				// if you don't call this, the context won't be created
				base.CreateFrameBuffer ();
				return;
			} catch (Exception ex) {
				Log.Verbose ("GLTriangle", "{0}", ex);
			}

			// this is a graphics setting that sets everything to the lowest mode possible so
			// the device returns a reliable graphics setting.
			try {
				Log.Verbose ("GLTriangle", "Loading with custom Android settings (low mode)");
				GraphicsMode = new AndroidGraphicsMode (0, 0, 0, 0, 0, false);

				// if you don't call this, the context won't be created
				base.CreateFrameBuffer ();
				return;
			} catch (Exception ex) {
				Log.Verbose ("GLTriangle", "{0}", ex);
			}
			throw new Exception ("Can't load egl, aborting");
		}

		// This gets called when the drawing surface has been created
		// There is already a GraphicsContext and Surface at this point,
		// following the standard OpenTK/GameWindow logic
		//
		// Android will only render when it refreshes the surface for
		// the first time, so if you don't call Run, you need to hook
		// up the Resize delegate or override the OnResize event to
		// get the updated bounds and re-call your rendering code.
		// This will also allow non-Run-loop code to update the screen
		// when the device is rotated.
		protected override void OnLoad (EventArgs e)
		{
			// This is completely optional and only needed
			// if you've registered delegates for OnLoad
			base.OnLoad (e);

			GL.Enable (EnableCap.Texture2D);

			int textureId;
			GL.GenTextures (1, out textureId);

			Texture.LoadTexture (Context, Resource.Drawable.text256x256, textureId);


			var Shape = new Torus ("shape1", this);
			Shape.TextureId = textureId;
			Shape.LifeTime.Max = 1000;
			Shape.ShapeActions = new Queue<ShapeAction>();
			Shape.ShapeActions.Enqueue(new ShapeAction(
				new Action<Shape3D, LifeTime>(
					(shape, lifeTime) => {
						shape.Rotate(Vector3.UnitY,0.01f);

						shape.Scale = 
							new Vector3(1 - Tween.Solve(
									Tween.Function.Cubic,
									Tween.Ease.Out,
									0f,
									1f,
									lifeTime.Max,
									lifeTime.Counter));

					}),
				new LifeTime(500)));
			Shape.ShapeActions.Enqueue(new ShapeAction(
				new Action<Shape3D, LifeTime>(
					(shape, lifeTime) => {
						shape.Rotate(Vector3.UnitY,0.01f);

						shape.Scale = 
							new Vector3(Tween.Solve(
								Tween.Function.Cubic,
								Tween.Ease.Out,
								0f,
								1f,
								lifeTime.Max,
								lifeTime.Counter));

					}),
				new LifeTime(500)));
			
			Shapes3D = new Dictionary<string, Shape3D> ();

			Shapes3D.Add (Shape.Name, Shape);

			var MainShape = new Torus ("main_shape", this);
			MainShape.TextureId = textureId;

			Shapes3D.Add (MainShape.Name, MainShape);

			Shapes2Remove = new Stack<Shape3D>();

			// Vertex and fragment shaders
			StreamReader vsReader = new StreamReader(MainActivity.vShader);
			string vertexShaderSrc = vsReader.ReadToEnd ();

			StreamReader fsReader = new StreamReader(MainActivity.fShader);
			string fragmentShaderSrc = fsReader.ReadToEnd ();

			// shaders from editor
			string editorVShader = 
				mainActivity.Intent.GetStringExtra ("vShader");
			string editorFShader = 
				mainActivity.Intent.GetStringExtra ("fShader");

			if (editorVShader != null) {
				vertexShaderSrc = editorVShader;
				fragmentShaderSrc = editorFShader;
			}

			// initialize shader
			bool success;
			string infoVShader;
			string infoFShader;

			shader = 
				new Shader(
					ref vertexShaderSrc, 
					ref fragmentShaderSrc,
					out success,
					out infoVShader,
					out infoFShader);
			
			Shapes3D ["main_shape"].Shader = shader;
			
			IsShaderOK = success;

			if (!IsShaderOK) {
				mainActivity.textView.Text ="Error compiling shaders.";
				mainActivity.mTextViewInfoVShader.Text = infoVShader;
				mainActivity.mTextViewInfoFShader.Text = infoFShader;
				return;
			}

			beginMode = new BeginMode();

			if (mainActivity.mRadioBTriangle.Checked)
				beginMode = BeginMode.Triangles;

			if (mainActivity.mRadioBLine.Checked)
				beginMode = BeginMode.Lines;

			if (mainActivity.mRadioBPoint.Checked)
				beginMode = BeginMode.Points;

			Shapes3D ["main_shape"].BeginMode = beginMode;

			// Initialize GL
			viewportHeight = Height; 
			viewportWidth = Width;

			GL.Enable(EnableCap.DepthTest);

			GL.Enable (EnableCap.Blend);

			GL.BlendFunc(BlendingFactorSrc.SrcAlpha, 
						 BlendingFactorDest.OneMinusSrcAlpha);

			GL.Viewport(0, 0, viewportWidth, viewportHeight);

			float ratio = (float) viewportWidth / viewportHeight;

			// this projection matrix is applied to object coordinates
			// in the onDrawFrame() method
			mProjectionMatrix = 
				OpenTK.Matrix4.
					CreatePerspectiveFieldOfView(MathHelper.PiOver4,
															ratio, 
															0.5f, 
															1000.0f);

			// Set the camera position (View matrix)
			mViewMatrix = Matrix4.LookAt(0, 3, -5, 0f, 0f, 0f, 0f, 1.0f, 0.0f);

			Camera = new Camera ();
			Camera.View = mViewMatrix;

			GL.UseProgram (shader.Program);

			mSeekBarsHandle = GL.GetUniformLocation(shader.Program, "sb");

			GL.UseProgram (0);

		}

		// this is called whenever android raises the SurfaceChanged event
		protected override void OnResize (EventArgs e)
		{
			// the surface change event makes your context
			// not be current, so be sure to make it current again
			MakeCurrent ();

			viewportHeight = Height;
			viewportWidth = Width;

			// Adjust the viewport based on geometry changes,
			// such as screen rotation
			GL.Viewport(0, 0, viewportWidth, viewportHeight);

			float ratio = (float) viewportWidth / viewportHeight;

			mProjectionMatrix = 
				OpenTK.Matrix4.
				CreatePerspectiveFieldOfView(MathHelper.PiOver4,
					ratio, 
					0.5f, 
					1000.0f);
			
		}


		public void SetActivity(MainActivity mainActivity)
		{
			this.mainActivity = mainActivity;
		}


		protected override void OnRenderFrame (FrameEventArgs e)
		{
			
			base.OnRenderFrame (e);

			if (!IsShaderOK) {

				GL.ClearColor (0.2f, 0.0f, 0.0f, 1f);
				GL.Clear(ClearBufferMask.ColorBufferBit |
					ClearBufferMask.DepthBufferBit);
				
				SwapBuffers ();

				return;
			}


			GL.ClearColor (0.0f, 0.0f, 0.0f, 1f);
			GL.Clear(ClearBufferMask.ColorBufferBit |
				ClearBufferMask.DepthBufferBit);


			foreach (var shape in Shapes3D) {
				shape.Value.Render ();
			}

			SwapBuffers();
		}


		protected override void OnUpdateFrame (FrameEventArgs e)
		{
			base.OnUpdateFrame (e);

			if(move2ShaderEditor){

				move2ShaderEditor = false;

				Intent intent = new Intent(mainActivity, typeof(ShaderEditor));

				intent.PutExtra ("vShader", shader.VertexSource);
				intent.PutExtra ("fShader", shader.FragmentSource);

				mainActivity.StartActivity (intent);
				mainActivity.Finish ();
			}

			if (!IsShaderOK) return;


			GL.UseProgram (shader.Program);

			GL.Uniform4(mSeekBarsHandle, 
				new Vector4(mainActivity.mSeekBar1.Progress / 255f,
							mainActivity.mSeekBar2.Progress / 255f, 
							mainActivity.mSeekBar3.Progress / 255f, 
							mainActivity.mSeekBar4.Progress / 255f));
			
			mainActivity.textView.Text = "s1:"+ mainActivity.mSeekBar1.Progress.ToString();
			mainActivity.textView.Text += " s2:" + mainActivity.mSeekBar2.Progress.ToString();
			mainActivity.textView.Text += " s3:" + mainActivity.mSeekBar3.Progress.ToString();
			mainActivity.textView.Text += " s4:" + mainActivity.mSeekBar4.Progress.ToString();

			GL.UseProgram (0);

			Shapes3D ["main_shape"].Quaternion = (Quaternion.FromAxisAngle (Vector3.UnitY, rotateY * 3) *
				Quaternion.FromAxisAngle (Vector3.UnitX, rotateX * 3));
			
			Shapes3D ["main_shape"].Scale = new Vector3 (scale);

			while (Shapes2Remove.Count > 0)
				Shapes3D.Remove(Shapes2Remove.Pop().Name);

			foreach (var shape in Shapes3D) {
				
				ShapeManager.Manage(shape.Value, 
					Shapes3D, 
					Shapes2Remove);

				shape.Value.Update (Camera, mProjectionMatrix);
			}

			updateCounter++;
		}


		public override bool OnTouchEvent (MotionEvent e)
		{
			//mainActivity.textView.Text = e.PointerCount.ToString ();

			if (e.PointerCount == 1) {

				float speed = 0.004f;


				switch (e.Action) {

				case MotionEventActions.Down:
					xTouch = e.GetX ();
					yTouch = e.GetY ();
					break;

				case MotionEventActions.Move:
					rotateY += speed * (e.GetX () - xTouch);
					xTouch = e.GetX ();
					rotateX -= speed * (e.GetY () - yTouch);
					yTouch = e.GetY ();
					break;

				default:
					break;
				}
			} else if (e.PointerCount == 2) {

				switch (e.Action) {

				case MotionEventActions.Pointer2Down:
					distTouchZero = 
						(float)Tools.Distance(e.GetX (0) - e.GetX (1),
									   e.GetY (0) - e.GetY (1))
									/ scale;
					
					pointer1ID = e.GetPointerId (0);
					pointer2ID = e.GetPointerId (1);
					break;

				case MotionEventActions.Move:
					
					try {

						distTouch = 
							(float)Tools.Distance(
								e.GetX (e.FindPointerIndex(pointer1ID)) - 
								e.GetX (e.FindPointerIndex(pointer2ID)),
								e.GetY (e.FindPointerIndex(pointer1ID)) - 
								e.GetY (e.FindPointerIndex(pointer2ID)));

					} catch (Exception) {

						pointer1ID = e.GetPointerId (0);
						pointer2ID = e.GetPointerId (1);
					}

					scale = (distTouch / distTouchZero);

					break;

				default:
					break;
				}

//				mainActivity.textView.Text += ": " + 
//					distTouchZero.ToString() + 
//					" : "+ distTouch.ToString ();
			} else if(e.PointerCount == 3){

				move2ShaderEditor = true;
			}

			return true;
		}



	}
}

