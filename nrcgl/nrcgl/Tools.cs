using System;
using nrcgl.nrcgl;
using System.Xml.Serialization;
using Java.IO;
using System.IO;
using OpenTK;
using Android.Graphics;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace nrcgl
{
	public class Tools
	{
		public static Task<VertexsIndicesData> DeserializeModelTask(Stream stream)
		{
			var deserializeModelTask = 
				Task<VertexsIndicesData>.Factory.
						StartNew(new Func<object, VertexsIndicesData>(DeserializeModel), stream);

			return deserializeModelTask;
		}
		public static VertexsIndicesData DeserializeModel(object stream)
		{
			VertexsIndicesData vertexsIndicesData = new VertexsIndicesData();

			XmlSerializer xmlSerializer = new XmlSerializer(vertexsIndicesData.GetType());
			StreamReader reader = new StreamReader(stream as Stream);
			vertexsIndicesData = (VertexsIndicesData)xmlSerializer.Deserialize(reader);

			reader.Close();

			return vertexsIndicesData;
		}

		public static double Distance(double a, double b)
		{
			return Math.Sqrt (Math.Pow (a, 2) + Math.Pow (b, 2));
		}

		public static List<SpanInfo> SpanInfos (string[] words, string text, Color color)
		{
			var spanInfos = new List<SpanInfo> ();

			foreach (var word in words) {
				
				for (int i = 0; i < (text.Length - word.Length); i++) {

					if (text.Substring (i, word.Length) == word) {
						spanInfos.Add (new SpanInfo{ 
							SpanStart = i,
							SpanEnd = i + word.Length,
							SpanColor = color
						});
					}
				}
			}

			return spanInfos;
		}
	}
}




