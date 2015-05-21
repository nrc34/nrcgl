using System;
using OpenTK;
using OpenTK.Graphics.ES20;
using nrcgl.nrcgl;
using System.IO;

namespace nrcgl
{
	public class Shape3D : IWGameable
	{
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

		public VertexFloatBuffer VertexBuffer {
			get;
			set;
		}

		public static VertexsIndicesData VertexsIndicesData {
			get;
			set;
		}

		public VertexFormat VertexFormat {
			get;
			set;
		}


		public Shape3D ()
		{
			Position = Vector3.Zero;
			Scale = Vector3.One;
			Quaternion = Quaternion.Identity;
			IsVisible = true;
		}


		public void Rotate(Vector3 axis, float angle)
		{
			Quaternion q = Quaternion.FromAxisAngle(axis, angle);
			Quaternion = q * Quaternion;
		}




		#region IWGameable implementation

		public void Load (Stream stream)
		{
			if (VertexsIndicesData is VertexsIndicesData)
				return;

			VertexsIndicesData = Tools.DeserializeModel(stream);
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

			VertexBuffer.Bind(Shader);

			GL.UseProgram (0);

		}

		public virtual void Render ()
		{
			if (!IsVisible) return;

			GL.UseProgram (Shader.Program);

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

