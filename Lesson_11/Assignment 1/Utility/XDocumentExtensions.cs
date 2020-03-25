using System;
using System.IO;
using System.Xml;
using System.Xml.Linq;

namespace Utility
{
    public static class XDocumentExtensions
    {
        public static Byte[] ToByteArray(this XDocument xDocument)
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var xmlWriter = XmlWriter.Create(memoryStream))
                {
                    xDocument.WriteTo(xmlWriter);
                }
                return memoryStream.ToArray();
            }
        }

        public static XDocument ToXDocument(this Byte[] bytes)
        {
            using (var memoryStream = new MemoryStream(bytes))
            {
                return XDocument.Load(memoryStream);
            }
        }
    }
}
