using System;
using nrcgl.nrcgl;
using System.Xml.Serialization;
using Java.IO;
using System.IO;

namespace nrcgl
{
	public class Tools
	{
		public static VertexsIndicesData DeserializeModel(Stream stream)
		{
			VertexsIndicesData vertexsIndicesData = new VertexsIndicesData();

			XmlSerializer xmlSerializer = new XmlSerializer(vertexsIndicesData.GetType());
			StreamReader reader = new StreamReader(stream);
			vertexsIndicesData = (VertexsIndicesData)xmlSerializer.Deserialize(reader);

			reader.Close();

			return vertexsIndicesData;
		}
	}
}




