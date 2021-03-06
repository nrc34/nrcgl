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
using System.Linq;

namespace nrcgl
{
	public class GLView : AndroidGameView
	{
		public MainActivity mainActivity;

		Int32 updateCounter;

		int viewportWidth;
		int viewportHeight;
		int mSeekBarsHandle;

		float xTouch;
		float yTouch;

		Matrix4 mProjectionMatrix;

		float rotateY = 0f;
		float rotateX = 0f;
		float moveY = 0f;
		float moveX = 0f;

		float distTouch;
		float distTouchZero;
		int pointer1ID;
		int pointer2ID;
		float scale = 1f;

		BeginMode beginMode;

		Dictionary<string, Shape3D> Shapes3D;
		Shape3D CurrShape;
		Stack<Shape3D> Shapes2Remove;
		Camera Camera;

		int textureId;

		int touchCounter;

		int shootCoolDown = 0;
		int moveCoolDown = 0;

		Vector2 moveMainTo;


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


			//Loat textures
			GL.Enable (EnableCap.Texture2D);

			GL.GenTextures (1, out textureId);
			Texture.LoadTexture (Context, Resource.Drawable.text256x256, textureId);

			int texturePanelId;
			GL.GenTextures (1, out texturePanelId);
			Texture.LoadTexture (Context, Resource.Drawable.text256x256, texturePanelId);

			
			Shapes3D = new Dictionary<string, Shape3D> ();

			var MainShape = new Torus ("main_shape", this);
			MainShape.TextureId = textureId;
			MainShape.Scale = new Vector3 (0.2f);
			MainShape.Position = new Vector3 (0f, 0f, 0f);
			MainShape.IsVisible = true;
			MainShape.ShapeActions = new Queue<ShapeAction>();

			moveMainTo = 
				new Vector2 (Width * 0.2f,
							 Height * 0.8f);

			var Panel1 = new Panel ("panel1", this);
			Panel1.TextureId = texturePanelId;
			Panel1.Rotate (Vector3.UnitX, -MathHelper.PiOver2);
			Panel1.Scale = new Vector3 (5);
			Panel1.Position = 
				new Vector3 (Panel1.Position.X,
							 Panel1.Position.Y - 3,
							 Panel1.Position.Z);

			var Panel2 = new Panel ("panel2", this);
			Panel2.TextureId = texturePanelId;
			Panel2.Rotate (Vector3.UnitX, -MathHelper.PiOver2);
			Panel2.Scale = new Vector3 (5);
			Panel2.Position = 
				new Vector3 (Panel2.Position.X,
					Panel2.Position.Y - 3,
					Panel2.Position.Z + 10f);

			Shapes3D.Add (MainShape.Name, MainShape);
			Shapes3D.Add (Panel1.Name, Panel1);
			Shapes3D.Add (Panel2.Name, Panel2);

			Shapes2Remove = new Stack<Shape3D>();


			// Initialize GL
			viewportHeight = Height; 
			viewportWidth = Width;

			GL.Enable(EnableCap.DepthTest);

			GL.Enable (EnableCap.Blend);

			GL.BlendFunc(BlendingFactorSrc.SrcAlpha, 
						 BlendingFactorDest.OneMinusSrcAlpha);

			GL.Viewport(0, 0, viewportWidth, viewportHeight);

			float ratio = (float) viewportWidth / viewportHeight;


			// set the projection matrix

//			mProjectionMatrix = 
//				OpenTK.Matrix4.
//				 CreatePerspectiveFieldOfView(MathHelper.PiOver2,
//															ratio, 
//															0.5f, 
//															20.0f);

			mProjectionMatrix = 
				OpenTK.Matrix4.
					CreateOrthographic (viewportWidth, 
					                    viewportHeight, 
					                    0.5f, 
					                    20f);

			// Set the camera position (View matrix)

			Camera = new Camera ();


			Camera.Rotate (Vector3.UnitX, MathHelper.PiOver2);
			Camera.Rotate (Vector3.UnitY, MathHelper.Pi);
			Camera.Position = new Vector3 (0, -8, 0);
			Camera.Update ();


			GL.UseProgram (Shapes3D["main_shape"].Shader.Program);

			mSeekBarsHandle = GL.GetUniformLocation(Shapes3D["main_shape"].Shader.Program, "sb");

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

			if (mainActivity.mRadioBTriangle.Checked)
				beginMode = BeginMode.Triangles;

			if (mainActivity.mRadioBLine.Checked)
				beginMode = BeginMode.Lines;

			if (mainActivity.mRadioBPoint.Checked)
				beginMode = BeginMode.Points;

			//CurrShape.BeginMode = beginMode;


			GL.ClearColor (0.0f, 0.0f, 0.0f, 1f);
			GL.Clear(ClearBufferMask.ColorBufferBit |
				ClearBufferMask.DepthBufferBit);


			foreach (var shape in Shapes3D) {
				shape.Value.BeginMode = beginMode;
				shape.Value.Render ();
			}

			SwapBuffers();
		}


		protected override void OnUpdateFrame (FrameEventArgs e)
		{
			base.OnUpdateFrame (e);

			GL.UseProgram (Shapes3D["main_shape"].Shader.Program);

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

//			CurrShape.Quaternion = (Quaternion.FromAxisAngle (Vector3.UnitY, rotateY * 3) *
//				Quaternion.FromAxisAngle (Vector3.UnitX, rotateX * 3));
//			
//			CurrShape.Scale = new Vector3 (scale);

			Shapes3D ["main_shape"].Rotate (Vector3.UnitZ, 0.01f);

			Shapes3D ["panel1"].Position = 
				new Vector3 (
				Shapes3D ["panel1"].Position.X,
				Shapes3D ["panel1"].Position.Y,
				Shapes3D ["panel1"].Position.Z - 0.01f);

			Shapes3D ["panel2"].Position = 
				new Vector3 (
					Shapes3D ["panel2"].Position.X,
					Shapes3D ["panel2"].Position.Y,
					Shapes3D ["panel2"].Position.Z - 0.01f);

			if (Shapes3D ["panel2"].Position.Z < -10f)
				Shapes3D ["panel2"].Position = new Vector3 (
					Shapes3D ["panel2"].Position.X,
					Shapes3D ["panel2"].Position.Y,
					Shapes3D ["panel1"].Position.Z + 10); 

			if (Shapes3D ["panel1"].Position.Z < -10f)
				Shapes3D ["panel1"].Position = new Vector3 (
					Shapes3D ["panel1"].Position.X,
					Shapes3D ["panel1"].Position.Y,
					Shapes3D ["panel2"].Position.Z + 10); 

			mainActivity.mTextViewInfoVShader.Text = Shapes3D ["main_shape"].Position.ToString ();

			#region shoot
			if(shootCoolDown == 0) {
				var bullet = new Sphere (
					"bullet" + Guid.NewGuid ().ToString (), 
					this);
				bullet.TextureId = textureId;
				bullet.Scale = new Vector3 (0.05f);
				bullet.Position = new Vector3 (
					Shapes3D ["main_shape"].Position.X,
					Shapes3D ["main_shape"].Position.Y,
					Shapes3D ["main_shape"].Position.Z);
				bullet.LifeTime.Max = 500;
				bullet.ShapeActions = new Queue<ShapeAction> ();
				bullet.ShapeActions.Enqueue (new ShapeAction (
					new Action<Shape3D, LifeTime, Object> (
						(shape, lifeTime, Object) => {

							shape.Position = 
								new Vector3 (shape.Position.X,
									shape.Position.Y,
									shape.Position.Z +
									Tween.Solve (
										Tween.Function.Cubic,
										Tween.Ease.Out,
										0f,
										0.7f,
										lifeTime.Max,
										lifeTime.Counter));
						}),
					new LifeTime (500)));
				Shapes3D.Add (bullet.Name, bullet);
				shootCoolDown = 5;
			}
			shootCoolDown--;
			#endregion

			if (moveMainTo != 
				new Vector2 (Shapes3D ["main_shape"].Position.X, 
					         Shapes3D ["main_shape"].Position.Z)
				&& moveCoolDown == 0) {

				var to = new Vector2 (moveMainTo.X, moveMainTo.Y);
				var from = new Vector2 (Shapes3D ["main_shape"].Position.X, 
					Shapes3D ["main_shape"].Position.Z);

				Shapes3D ["main_shape"].ShapeActions.Enqueue (
					GameActions.Move2XY(
						new Move2XY(from, to, Width, Height)));

				moveCoolDown = 6;
			}

			if (updateCounter % 20 ==0) {

				Shape3D rock = new Rock ("rock" + Guid.NewGuid ().ToString (),
				   	                     this);
				var randX = new Random ();
				float randomX = 3.8f * (float)randX.NextDouble () - 1.9f;
				rock.Position = new Vector3 ((float) randomX, 0, 3f);
				rock.Scale = new Vector3 (0.2f);
				rock.TextureId = textureId;
				rock.LifeTime.Max = 500;
				rock.ShapeActions = new Queue<ShapeAction>();
				rock.ShapeActions.Enqueue(new ShapeAction(
					new Action<Shape3D, LifeTime, Object>(
						(shape, lifeTime, Object) => {
							shape.Rotate(Vector3.UnitX, 0.04f);
							shape.Rotate(Vector3.UnitY, 0.03f);

							shape.Position = 
								new Vector3(shape.Position.X,
											shape.Position.Y,
											shape.Position.Z -0.03f);

						}),
					new LifeTime(500)));
				Shapes3D.Add (rock.Name, rock);
			}


			
			while (Shapes2Remove.Count > 0)
				Shapes3D.Remove(Shapes2Remove.Pop().Name);

			foreach (var shape in Shapes3D) {

				if (shape.Key.Contains ("rock")) {

					var bullets = 
						new List <string> (Shapes3D.Keys).
							Where((st) => (st.Contains("bullet")));
					
					foreach (var item in bullets) {
						bool hitX = Math.Abs (Shapes3D [item].Position.X - shape.Value.Position.X) < 0.2f;
						bool hitZ = Math.Abs (Shapes3D [item].Position.Z - shape.Value.Position.Z) < 0.2f;

						if (hitX && hitZ && shape.Value.HitCount == 0) {
							shape.Value.HitCount++;
							shape.Value.LifeTime.Max = shape.Value.LifeTime.Counter + 10;
							shape.Value.ShapeActions = new Queue<ShapeAction> ();
							shape.Value.ShapeActions.Enqueue(new ShapeAction(
								new Action<Shape3D, LifeTime, Object>(
									(shape1, lifeTime, Object) => {
										if(lifeTime.Counter < 4)
											shape1.Scale -= new Vector3(0.025f);
										else
											shape1.Scale += new Vector3(0.025f);
									}),
								new LifeTime(10)));
						}
					}

				}
				
				ShapeManager.Manage(shape.Value, 
					Shapes3D, 
					Shapes2Remove);


				shape.Value.Update (Camera, mProjectionMatrix);
			}

			updateCounter++;
			touchCounter++;
			if(moveCoolDown > 0) moveCoolDown--;
		}


		public override bool OnTouchEvent (MotionEvent e)
		{
			//mainActivity.textView.Text = e.PointerCount.ToString ();

			if (e.PointerCount == 1) {

				switch (e.Action) {

				case MotionEventActions.Down:
					touchCounter = 0;
					xTouch = e.GetX (0);
					yTouch = e.GetY (0);

					moveMainTo =  new Vector2 (xTouch, yTouch);

					break;

				case MotionEventActions.Move:

					try {

						xTouch = e.GetX (e.FindPointerIndex(pointer1ID));
						yTouch = e.GetY (e.FindPointerIndex(pointer1ID));

						moveMainTo =  new Vector2 (xTouch, yTouch);

					} catch (Exception) {

						pointer1ID = e.GetPointerId (0);
						
					}

					break;

				case MotionEventActions.Up:


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

			} else if(e.PointerCount == 3){

				mainActivity.mTextViewInfoFShader.Text = Width.ToString () + ":" + Height.ToString ();


			}

			return true;
		}



	}
}

