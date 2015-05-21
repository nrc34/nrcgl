using System;
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

namespace nrcgl
{
	public class GLView : AndroidGameView
	{
		public MainActivity mainActivity;

		bool move2ShaderEditor = false;

		int viewportWidth, viewportHeight;
		int mMVPMatrixHandle;
		int mMVMatrixHandle;
		int mPMatrixHandle;
		int mMMatrixHandle;
		int mSeekBarsHandle;

		float xTouch;
		float yTouch;

		Matrix4 mProjectionMatrix;
		Matrix4 mViewMatrix;
		Matrix4 mModelViewProjectionMatrix;

		Shader shader;
		bool IsShaderOK;

		VertexFloatBuffer VertexBuffer;
		VertexsIndicesData vid;

		float rotateY = 0f;
		float rotateX = 0f;

		float distTouch;
		float distTouchZero;
		int pointer1ID;
		int pointer2ID;
		float scale = 1f;

		BeginMode beginMode;

		Shape3D Shape;
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

			Shape = new Torus (this);


			vid = Tools.DeserializeModel(MainActivity.input);

			mainActivity.textView.Text = "Costa" + vid.Vertexs.Count.ToString();

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


			// initialize buffer
			VertexBuffer = 
				new VertexFloatBuffer(
					VertexFormat.XYZ_NORMAL_COLOR, 
					7650, 
					beginMode);

			DrawBufer(VertexBuffer, VertexFormat.XYZ_NORMAL_COLOR);

			VertexBuffer.IndexFromLength();
			VertexBuffer.Load();

			VertexBuffer.Bind(shader);

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

			// Calculate the projection and view transformation
			mModelViewProjectionMatrix = Matrix4.Mult(mProjectionMatrix, mViewMatrix);

			var mModelMatrix = Matrix4.Scale (1);
			var mModelViewMatrix = Matrix4.Mult(mModelMatrix, mViewMatrix);

			GL.UseProgram (shader.Program);

			mMVMatrixHandle = GL.GetUniformLocation(shader.Program, "modelview_matrix");
			mPMatrixHandle = GL.GetUniformLocation(shader.Program, "projection_matrix");
			mMMatrixHandle = GL.GetUniformLocation(shader.Program, "model_matrix");
			mMVPMatrixHandle = GL.GetUniformLocation(shader.Program, "mvp_matrix");
			mSeekBarsHandle = GL.GetUniformLocation(shader.Program, "sb");

			GL.UniformMatrix4 (mMVMatrixHandle, false, ref mModelViewMatrix);
			GL.UniformMatrix4 (mPMatrixHandle, false, ref mProjectionMatrix);
			GL.UniformMatrix4 (mMMatrixHandle, false, ref mModelMatrix);
			GL.UniformMatrix4 (mMVPMatrixHandle, false, ref mModelViewProjectionMatrix);

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


		public virtual void DrawBufer(VertexFloatBuffer buffer, 
									  VertexFormat vertexFormat)
		{
			switch (vertexFormat)
			{
			case VertexFormat.XY:
				break;
			case VertexFormat.XY_COLOR:
				break;
			case VertexFormat.XY_UV:
				break;
			case VertexFormat.XY_UV_COLOR:
				break;
			case VertexFormat.XYZ:
				break;
			case VertexFormat.XYZ_COLOR:
				break;
			case VertexFormat.XYZ_UV:
				break;
			case VertexFormat.XYZ_UV_COLOR:
				break;
			case VertexFormat.XYZ_NORMAL_COLOR:
				#region xyz_normal_color
				foreach (Vertex vertex in vid.Vertexs)
				{
					buffer.AddVertex(vertex.Position.X, vertex.Position.Y, vertex.Position.Z,
						vertex.Normal.X, vertex.Normal.Y, vertex.Normal.Z,
						vertex.Color.R, vertex.Color.G, vertex.Color.B, vertex.Color.A);
				}
				#endregion
				break;
			case VertexFormat.XYZ_NORMAL_UV:
				#region xyz_normal_uv
				foreach (Vertex vertex in vid.Vertexs)
				{
					buffer.AddVertex(vertex.Position.X, vertex.Position.Y, vertex.Position.Z,
						vertex.Normal.X, vertex.Normal.Y, vertex.Normal.Z,
						vertex.TexCoord.X, vertex.TexCoord.Y);
				}
				#endregion
				break;
			case VertexFormat.XYZ_NORMAL:
				#region xyz_normal
				foreach (Vertex vertex in vid.Vertexs)
				{
					buffer.AddVertex(vertex.Position.X, vertex.Position.Y, vertex.Position.Z,
						new Vector3(vertex.Normal.X, vertex.Normal.Y, vertex.Normal.Z));
				}
				#endregion
				break;
			case VertexFormat.XYZ_NORMAL_UV_COLOR:
				#region xyz_normal_uv_color
				foreach (Vertex vertex in vid.Vertexs)
				{
					buffer.AddVertex(vertex.Position.X, vertex.Position.Y, vertex.Position.Z,
						vertex.Normal.X, vertex.Normal.Y, vertex.Normal.Z,
						vertex.TexCoord.X, vertex.TexCoord.Y,
						vertex.Color.R, vertex.Color.G, vertex.Color.B, vertex.Color.A);
				}
				#endregion
				break;
			default:
				break;
			}
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


			GL.UseProgram (shader.Program);

			if (mainActivity.mRadioBTriangle.Checked)
				beginMode = BeginMode.Triangles;

			if (mainActivity.mRadioBLine.Checked)
				beginMode = BeginMode.Lines;

			if (mainActivity.mRadioBPoint.Checked)
				beginMode = BeginMode.Points;

			VertexBuffer.DrawMode = beginMode;

			VertexBuffer.Bind(shader);

			GL.UseProgram (0);

			Shape.Render ();

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

			//textView.Text = "ups: "+(1/e.Time).ToString ();

			var mModelMatrix = Matrix4.Scale(scale) *
				Matrix4.Rotate (Quaternion.FromAxisAngle(Vector3.UnitY, rotateY * 3) * 
					Quaternion.FromAxisAngle(Vector3.UnitX, rotateX * 3));

			var mModelViewMatrix = Matrix4.Mult(mModelMatrix, mViewMatrix);

			mModelViewProjectionMatrix = 
				Matrix4.Mult (mModelMatrix,  mViewMatrix) * mProjectionMatrix;


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

			GL.UniformMatrix4 (mMVMatrixHandle, false, ref mModelViewMatrix);
			GL.UniformMatrix4 (mMMatrixHandle, false, ref mModelMatrix);

			GL.UniformMatrix4 (mMVPMatrixHandle, false, ref mModelViewProjectionMatrix);

			VertexBuffer.Bind(shader);

			GL.UseProgram (0);

			Shape.Rotate (Vector3.UnitY, 0.05f);

			Shape.Update (Camera, mProjectionMatrix);
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

