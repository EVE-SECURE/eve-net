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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using AppTemplate.Utilities;

namespace AppTemplate
{
   public class PluginLoader
   {
      string basePath;

      private PluginLoader() { }

      public PluginLoader(string path)
      {
         basePath = path;
      }

      public List<T> FindPlugins<T>(bool recurse) where T : class
      {
         List<T> foundTypes = new List<T>();

         foreach (string file in Directory.GetFiles(basePath, "*.dll", recurse ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly))
         {
            List<T> pluginTypes = GetPluginInstances<T>(file);

            if (pluginTypes.Count > 0)
               foundTypes.AddRange(pluginTypes);
         }

         return foundTypes;
      }

      private Assembly GetAssembly(string file)
      {
         Assembly asm = AssemblyHelper.FindAssembly(file);

         if (asm == null)
            asm = Assembly.LoadFrom(file);

         return asm;
      }

      private List<T> GetPluginInstances<T>(string file) where T : class
      {
         Type[] types = null;

         try
         {
            Assembly assembly = GetAssembly(file);
            types = assembly.GetTypes();
         }
         catch (ReflectionTypeLoadException)
         {
            //_log.Info("Loader Exceptions: {0}", e.LoaderExceptions.ToString());
            return null;
         }
         catch (Exception)
         {

            //_log.Info("Unable to load plugin dll '{0}'.  Missing Dependencies?. Exception: {1}", file, e.ToString());
            return null;
         }

         Type PluginType = typeof(T);
         List<T> pluginTypes = new List<T>();

         foreach (Type type in types)
         {
            if (PluginType.IsAssignableFrom(type))
            {
               ObjectFactory<T> factory = ObjectFactory<T>.Instance;

               if (factory.GetTypesLoaded().FirstOrDefault(t => t.GUID == type.GUID) == null)
                  factory.LoadAdditionalAssembly(type);

               pluginTypes.Add( ObjectFactory<T>.Instance.Create(type.Name) );
            }
         }

         return pluginTypes;
      }
   }
}
