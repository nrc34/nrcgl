using System;
using Android.Content;
using OpenTK.Graphics.ES20;
using Android.Graphics;

namespace nrcgl
{
	public class Texture
	{
		public Texture ()
		{
		}

		public static void LoadTexture (Context context, int resourceId, int textureId)
		{
			GL.BindTexture (TextureTarget.Texture2D, textureId);

			// setup texture parameters
			GL.TexParameter (TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
			GL.TexParameter (TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMagFilter.Linear);
			GL.TexParameter (TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
			GL.TexParameter (TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

			Bitmap b = BitmapFactory.DecodeResource (context.Resources, resourceId);

			Android.Opengl.GLUtils.TexImage2D ((int)All.Texture2D, 0, b, 0);              
		}
	}
}

