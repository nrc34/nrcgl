using System;
using OpenTK;
using OpenTK.Graphics.ES20;
using nrcgl.nrcgl;
using System.IO;
using OpenTK.Platform.Android;
using System.Collections.Generic;

namespace nrcgl.nrcgl.shapes
{
	public class Shape3D : IWGameable
	{
		public GLView GLView {
			get;
			set;
		}

		public string Name {
			get;
			set;
		}

		public Vector3 Position {
			get;
			set;
		}

		public Vector3 Scale {
			get;
			set;
		}

		public Quaternion Quaternion {
			get;
			set;
		}

		public bool IsVisible {
			get;
			set;
		}

		public Shader Shader {
			get;
			set;
		}

		public int TextureId {
			get;
			set;
		}

		public BeginMode BeginMode {
			get;
			set;
		}

		public VertexFloatBuffer VertexBuffer {
			get;
			set;
		}

		public VertexsIndicesData VertexsIndicesData {
			get;
			set;
		}

		public VertexFormat VertexFormat {
			get;
			set;
		}

		public Queue<ShapeAction> ShapeActions {
			get; 
			set; 
		}

		public LifeTime LifeTime {
			get;
			set;
		}



		public Shape3D (string name,GLView gLView)
		{
			GLView = gLView;
			Name = name;
			BeginMode = BeginMode.Triangles;
			VertexFormat = VertexFormat.XYZ_NORMAL_UV_COLOR;
			Position = Vector3.Zero;
			Scale = Vector3.One;
			Quaternion = Quaternion.Identity;
			// lifetime with max = 0 (shape is imortal) 
			LifeTime = new LifeTime(0);
			IsVisible = true;
		}


		public void Rotate(Vector3 axis, float angle)
		{
			Quaternion q = Quaternion.FromAxisAngle(axis, angle);
			Quaternion = q * Quaternion;
		}


		#region IWGameable implementation

		public void Load (string vShaderName, string fShaderName)
		{
			
			// initialize shader
			bool success;
			string infoVShader;
			string infoFShader;

			// Vertex and fragment shaders
			var vShaderStream = GLView.mainActivity.Assets.Open (vShaderName);
			var fShaderStream = GLView.mainActivity.Assets.Open (fShaderName);

			StreamReader vsReader = new StreamReader(vShaderStream);
			string vertexShaderSrc = vsReader.ReadToEnd ();

			StreamReader fsReader = new StreamReader(fShaderStream);
			string fragmentShaderSrc = fsReader.ReadToEnd ();

			Shader = 
				new Shader(
					ref vertexShaderSrc, 
					ref fragmentShaderSrc,
					out success,
					out infoVShader,
					out infoFShader);


			// initialize buffer
			VertexBuffer = 
				new VertexFloatBuffer(
					VertexFormat, 
					7650, 
					BeginMode);
			
			DrawBufer ();

			VertexBuffer.IndexFromLength();
			VertexBuffer.Load();

			VertexBuffer.Bind(Shader);

		}

		public virtual void Update (Camera camera, Matrix4 ProjectionMatrix)
		{
			Matrix4 SM = Matrix4.Scale(Scale);

			Matrix4 RM = Matrix4.Rotate(Quaternion);

			Matrix4 TM = Matrix4.CreateTranslation(Position);

			var ModelMatrix = SM * RM * TM;

			var ModelViewMatrix = ModelMatrix * camera.View;

			var ModelViewProjectionMatrix = 
						ModelViewMatrix * ProjectionMatrix;

			GL.UseProgram (Shader.Program);

			GL.UniformMatrix4 (Shader.MVMatrixHandle, false, ref ModelViewMatrix);
			GL.UniformMatrix4 (Shader.MMatrixHandle, false, ref ModelMatrix);
			GL.UniformMatrix4 (Shader.MVPMatrixHandle, false, ref ModelViewProjectionMatrix);

			GL.UseProgram (0);
		}

		public virtual void Render ()
		{
			if (!IsVisible) return;

			GL.UseProgram (Shader.Program);

			GL.ActiveTexture(TextureUnit.Texture0);
			GL.BindTexture(TextureTarget.Texture2D, TextureId);
			GL.Uniform1(GL.GetUniformLocation(Shader.Program, "TextureUnit0"), TextureUnit.Texture0 - TextureUnit.Texture0);

			VertexBuffer.DrawMode = BeginMode;

			VertexBuffer.Bind(Shader);
		}

		#endregion

		public virtual void DrawBufer()
		{
			switch (VertexFormat)
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
				foreach (Vertex vertex in VertexsIndicesData.Vertexs)
				{
					VertexBuffer.AddVertex(vertex.Position.X, vertex.Position.Y, vertex.Position.Z,
						vertex.Normal.X, vertex.Normal.Y, vertex.Normal.Z,
						vertex.Color.R, vertex.Color.G, vertex.Color.B, vertex.Color.A);
				}
				#endregion
				break;
			case VertexFormat.XYZ_NORMAL_UV:
				#region xyz_normal_uv
				foreach (Vertex vertex in VertexsIndicesData.Vertexs)
				{
					VertexBuffer.AddVertex(vertex.Position.X, vertex.Position.Y, vertex.Position.Z,
						vertex.Normal.X, vertex.Normal.Y, vertex.Normal.Z,
						vertex.TexCoord.X, vertex.TexCoord.Y);
				}
				#endregion
				break;
			case VertexFormat.XYZ_NORMAL:
				#region xyz_normal
				foreach (Vertex vertex in VertexsIndicesData.Vertexs)
				{
					VertexBuffer.AddVertex(vertex.Position.X, vertex.Position.Y, vertex.Position.Z,
						new Vector3(vertex.Normal.X, vertex.Normal.Y, vertex.Normal.Z));
				}
				#endregion
				break;
			case VertexFormat.XYZ_NORMAL_UV_COLOR:
				#region xyz_normal_uv_color
				foreach (Vertex vertex in VertexsIndicesData.Vertexs)
				{
					VertexBuffer.AddVertex(vertex.Position.X, vertex.Position.Y, vertex.Position.Z,
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
	}
}

