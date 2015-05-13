using System;
using nrcgl.nrcgl;
using System.Xml.Serialization;
using Java.IO;
using System.IO;

namespace nrcgl
{
	public class Tools
	{
		public static VertexsIndicesData DeserializeModel()
		{
			VertexsIndicesData vertexsIndicesData = new VertexsIndicesData();

			XmlSerializer xmlSerializer = new XmlSerializer(vertexsIndicesData.GetType());
			StreamReader reader = new StreamReader(MainActivity.input);
			vertexsIndicesData = (VertexsIndicesData)xmlSerializer.Deserialize(reader);

			reader.Close();

			return vertexsIndicesData;
		}
	}
}




