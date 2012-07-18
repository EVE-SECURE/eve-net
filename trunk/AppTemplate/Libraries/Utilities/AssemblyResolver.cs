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
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace AppTemplate.Utilities
{
   public static class AssemblyHelper
   {
      private static Dictionary<string, Assembly> _searchedTypes = new Dictionary<string, Assembly>();
      private static Dictionary<string, bool> resolvePaths = new Dictionary<string, bool>();

      private static void Log(string fmt, params object[] args)
      {
         string msg = string.Format(fmt, args);
         Debug.WriteLine(msg);
      }

      public static Assembly FindAssembly(string file)
      {
         string fileNameOnly = Path.GetFileName(file);

         AppDomain domain = AppDomain.CurrentDomain;
         Assembly[] loaded_assemblies = domain.GetAssemblies();

         foreach (Assembly asm in loaded_assemblies)
         {
            if (asm.ManifestModule.Name.Equals(fileNameOnly, StringComparison.OrdinalIgnoreCase))
               return asm;
         }

         foreach (Assembly asm in _searchedTypes.Values)
         {
            if (asm != null)
            {
               if (asm.ManifestModule.Name.Equals(fileNameOnly, StringComparison.OrdinalIgnoreCase))
                  return asm;
            }
         }

         return null;
      }

      private static void InitializeSearchPaths()
      {
         Assembly executingAssembly = Assembly.GetExecutingAssembly();
         Assembly callingAssembly = Assembly.GetCallingAssembly();
         Assembly entryAssembly = Assembly.GetEntryAssembly();

         if (!resolvePaths.ContainsKey(Path.GetDirectoryName(executingAssembly.Location)))
            resolvePaths[Path.GetDirectoryName(executingAssembly.Location)] = false;

         if (!resolvePaths.ContainsKey(Path.GetDirectoryName(callingAssembly.Location)))
            resolvePaths[Path.GetDirectoryName(callingAssembly.Location)] = false;

         if (!resolvePaths.ContainsKey(Path.GetDirectoryName(entryAssembly.Location)))
            resolvePaths[Path.GetDirectoryName(entryAssembly.Location)] = false;

         string dependenciesPath = Path.Combine(Path.GetDirectoryName(entryAssembly.Location), "Dependencies");

         Log("Dependencies path: {0}", dependenciesPath);
         resolvePaths[dependenciesPath] = true;


         Log("");
         Log("Resolover Paths---------------------------");
         foreach (string key in resolvePaths.Keys)
         {
            Log("path: {0}", key);
         }
         Log("------------------------------------------");
         Log("");
      }

      private static Assembly LoadFromResources(string assemblyName)
      {
         String[] resourceNames = Assembly.GetExecutingAssembly().GetManifestResourceNames();
         foreach (string resource in resourceNames)
         {
            if (resource.Contains(assemblyName))
            {
               using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resource))
               {
                  Byte[] assemblyData = new Byte[stream.Length];
                  stream.Read(assemblyData, 0, assemblyData.Length);

                  return Assembly.Load(assemblyData);
               }
            }
         }

         return null;
      }

      private static Assembly LoadFromPath(string dllname, string path, bool recurse)
      {
         try
         {
            if (!recurse)
            {
               //Log("Looking for {0} in {1}", dllname, path);
               string fullname = System.IO.Path.Combine(path, dllname);

               if (!File.Exists(fullname))
                  return null;

               return AppDomain.CurrentDomain.Load(System.IO.File.ReadAllBytes(fullname));
            }

            List<string> search_paths = new List<string>(Directory.GetDirectories(path, "*", SearchOption.AllDirectories));
            search_paths.Add(path);

            foreach (string dir in search_paths)
            {
               //Log("Looking for {0} in {1}", dllname, dir);
               string assemblypath = System.IO.Path.Combine(dir, dllname);

               if (File.Exists(assemblypath))
                  return Assembly.LoadFrom(assemblypath);
            }
         }
         catch (Exception ex)
         {
            Log("Error trying to load assembly: {0}.  Error details: {1}", dllname, ex.ToString());
         }

         return null;
      }

      public static void AddAssemblySearchPath(string searchPath, bool searchRecursively)
      {
         if (!resolvePaths.ContainsKey(Path.GetDirectoryName(searchPath)))
            resolvePaths[Path.GetDirectoryName(searchPath)] = searchRecursively;
      }

      public static Assembly ResolveAssemblies(object sender, ResolveEventArgs args)
      {
         string dllName = args.Name.Split(',')[0] + ".dll";

         if (_searchedTypes.ContainsKey(dllName))
         {
            return _searchedTypes[dllName];
         }

         Log("!!! Resolving Assembly " + args.Name);

         Assembly loadedAssembly = FindAssembly(dllName);

         if (loadedAssembly == null)
         {
            if (resolvePaths.Count == 0)
               InitializeSearchPaths();

            foreach (string path in resolvePaths.Keys)
            {
               bool recurse = resolvePaths[path];

               if ((loadedAssembly = LoadFromPath(dllName, path, recurse)) != null)
                  break;
            }

            if (loadedAssembly == null)
               loadedAssembly = LoadFromResources(dllName);
         }

         _searchedTypes[dllName] = loadedAssembly;

         if (loadedAssembly != null)
         {
            Log("    Assembly resolved as: " + loadedAssembly.CodeBase);
         }
         else
         {
            Log("    Unable to resolve assembly.");
         }

         return loadedAssembly;
      }

      public static void AssemblyLoaded(object sender, AssemblyLoadEventArgs args)
      {
         string assemblyName = args.LoadedAssembly.GetName().Name;
         
         if (_searchedTypes.ContainsKey(assemblyName))
         {
            Log("!!! Assembly '"+ assemblyName + "' was loaded multiple times.");
            Log("    First Location: " + _searchedTypes[assemblyName].CodeBase);
            Log("    This Location : " + args.LoadedAssembly.CodeBase);
         }

         _searchedTypes[assemblyName] = args.LoadedAssembly;
      }
   }
}
