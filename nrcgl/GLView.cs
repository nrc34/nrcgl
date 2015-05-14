using System;
using System.Runtime.InteropServices;
using System.Text;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.ES20;
using OpenTK.Platform;
using OpenTK.Platform.Android;

using Android.Util;
using Android.Views;
using Android.Content;
using nrcgl.nrcgl;
using Android.Widget;

namespace nrcgl
{
	class GLView : AndroidGameView
	{
		TextView textView;

		int viewportWidth, viewportHeight;
		int mMVPMatrixHandle;
		float [] vertices;

		Matrix4 mProjectionMatrix;
		Matrix4 mViewMatrix;
		Matrix4 mModelViewProjectionMatrix;

		Shader shader;
		VertexFloatBuffer VertexBuffer;
		VertexsIndicesData vid;

		float rotate = 0f;

		// Set color with red, green, blue and alpha (opacity) values
		float [] color;

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
			Run (30);
		}

		// This method is called everytime the context needs
		// to be recreated. Use it to set any egl-specific settings
		// prior to context creation
		protected override void CreateFrameBuffer ()
		{
			ContextRenderingApi = GLVersion.ES2;

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

			viewportHeight = Height; 
			viewportWidth = Width;

			vid = Tools.DeserializeModel();

			textView.Text = "Costa" + vid.Vertexs.Count.ToString();

			// Set color with red, green, blue and alpha (opacity) values
			color = new float [] { 0.63671875f, 0.76953125f, 0.22265625f, 1.0f };

			// Vertex and fragment shaders
			string vertexShaderSrc = 
				"attribute vec3 vertex_position;  \n" +
				"attribute vec3 vertex_normal;  \n" +
				"attribute vec2 vertex_texcoord;  \n" +
				"attribute vec4 vertex_color;  \n" +
				"uniform mat4 uMVPMatrix;   \n" +
				"void main()                  \n" +
				"{                            \n" +
				"   gl_Position = uMVPMatrix*vec4(vertex_position, 1f);  \n" +
				"}                            \n";

			string fragmentShaderSrc = "precision mediump float;             \n" +
				"uniform vec4 vColor;                         \n" +
				"void main()                                  \n" +
				"{                                            \n" +
				"  gl_FragColor = vec4(1.0, 1.0, 1.0, 1.0);  \n" +
				"}                                            \n";

			shader = 
				new Shader(
					ref vertexShaderSrc, 
					ref fragmentShaderSrc);
			
			// initialize buffer
			VertexBuffer = 
				new VertexFloatBuffer(
					VertexFormat.XYZ_NORMAL_COLOR, 
					7650, 
					BeginMode.Lines);

			DrawBufer(VertexBuffer, VertexFormat.XYZ_NORMAL_COLOR);

			VertexBuffer.IndexFromLength();
			VertexBuffer.Load();

			VertexBuffer.Bind(shader);

			// Initialize GL
			GL.Enable(EnableCap.DepthTest);

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
			mViewMatrix = Matrix4.LookAt(0, 0, -100, 0f, 0f, 0f, 0f, 1.0f, 0.0f);

			// Calculate the projection and view transformation
			mModelViewProjectionMatrix = Matrix4.Mult(mProjectionMatrix, mViewMatrix);

			GL.UseProgram (shader.Program);

			// get handle to shape's transformation matrix
			mMVPMatrixHandle = GL.GetUniformLocation(shader.Program, "uMVPMatrix");

			// Apply the projection and view transformation
			GL.UniformMatrix4(mMVPMatrixHandle, false, ref mModelViewProjectionMatrix);

			GL.UseProgram (0);

		}



		// this is called whenever android raises the SurfaceChanged event
		protected override void OnResize (EventArgs e)
		{
			viewportHeight = Height;
			viewportWidth = Width;

			// the surface change event makes your context
			// not be current, so be sure to make it current again
			MakeCurrent ();

			// Adjust the viewport based on geometry changes,
			// such as screen rotation
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

		public void GiveTextView(TextView textView)
		{
			this.textView = textView;

		}


		protected override void OnRenderFrame (FrameEventArgs e)
		{
			base.OnRenderFrame (e);

			MakeCurrent ();

			GL.ClearColor (0.0f, 0.0f, 0.0f + rotate, 1f);
			GL.Clear(ClearBufferMask.ColorBufferBit |
				ClearBufferMask.DepthBufferBit);

			mModelViewProjectionMatrix = 
				Matrix4.Mult (Matrix4.Scale (rotate), 
							  mViewMatrix) * mProjectionMatrix;
			rotate += 0.01f;

			textView.Text = shader.VertexSource.Substring(shader.VertexSource.Length - 100);


			//mMVPMatrixHandle = GL.GetUniformLocation(shader.Program, new StringBuilder("uMVPMatrix"));


			GL.UseProgram (shader.Program);

			mMVPMatrixHandle = GL.GetUniformLocation (shader.Program, "uMVPMatrix");
			GL.UniformMatrix4(mMVPMatrixHandle, false, ref mModelViewProjectionMatrix);

			GL.UseProgram (0);





			VertexBuffer.Bind(shader);

			SwapBuffers();
		}

	}
}

