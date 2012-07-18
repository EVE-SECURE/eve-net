//
// The MIT License (MIT)
//
// Copyright (C) 2012 Gary McNickle
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal 
// in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
// copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
// FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace AppTemplate.Utilities
{
    public static class Serializer
    {
        public static void Serialize(string path, object data)
        {
            if (!Directory.Exists(Directory.GetDirectoryRoot(path)))
                Directory.CreateDirectory(Directory.GetDirectoryRoot(path));

            XmlSerializer xml = new XmlSerializer(data.GetType());

            using (StreamWriter streamWriter = new StreamWriter(path))
            {
                using (XmlTextWriter textWriter = new XmlTextWriter(streamWriter))
                {
                    textWriter.Formatting = Formatting.Indented;

                    xml.Serialize(textWriter, data);

                    streamWriter.Flush();
                    streamWriter.Close();
                }
            }
        }

        public static object Deserialize(string path, Type t)
        {
            object data = null;

            try
            {
                XmlSerializer xml = new XmlSerializer(t);

                using (StreamReader streamReader = new StreamReader(path))
                {
                    using (XmlTextReader textReader = new XmlTextReader(streamReader))
                    {
                        textReader.WhitespaceHandling = WhitespaceHandling.None;

                        data = xml.Deserialize(textReader);
                    }

                    streamReader.Close();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception stack", e.StackTrace);
            }

            return data;
        }

        public static T DeepCopy<T>(object data)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                XmlSerializer xml = new XmlSerializer(data.GetType());
                xml.Serialize(memoryStream, data);

                memoryStream.Position = 0;
                return (T)xml.Deserialize(memoryStream);
            }
        }
    }
}
