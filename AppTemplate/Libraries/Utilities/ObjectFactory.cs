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
using System.Reflection;

namespace AppTemplate.Utilities
{
   public class ObjectFactoryException : Exception
   {
      public ObjectFactoryException(String msg) : base(msg) { }

      public ObjectFactoryException(String msg, Exception innerException) : base(msg, innerException) { }
   }

   public sealed class ObjectFactoryUnknownTypeException : ObjectFactoryException
   {
      public ObjectFactoryUnknownTypeException(String msg) : base(msg) { }

      public ObjectFactoryUnknownTypeException(String msg, Exception innerException) : base(msg) { }
   }

   public sealed class ObjectFactoryMissingConstructorException : ObjectFactoryException
   {
      public ObjectFactoryMissingConstructorException(String msg) : base(msg) { }

      public ObjectFactoryMissingConstructorException(String msg, Exception innerException) : base(msg, innerException) { }
   }


   public sealed class ObjectFactory<TBaseClassType>
   {
      private static volatile ObjectFactory<TBaseClassType> instance_;
      private static object syncRoot_ = new Object();

      private Dictionary<string, RuntimeTypeHandle> instanceHandlers_;
      private bool alreadyLoaded_ = false;

      public static ObjectFactory<TBaseClassType> Instance
      {
         get
         {
            if (instance_ == null)
            {
               lock (syncRoot_)
               {
                  if (instance_ == null)
                     instance_ = new ObjectFactory<TBaseClassType>();
               }
            }

            return instance_;
         }
      }

      private ObjectFactory()
      {
         instanceHandlers_ = new Dictionary<string, RuntimeTypeHandle>(StringComparer.OrdinalIgnoreCase);

         ConstructHandlers(Assembly.GetEntryAssembly());
      }

      private RuntimeTypeHandle GetInstanceHandler(string id)
      {
         if (instanceHandlers_.ContainsKey(id))
         {
            return instanceHandlers_[id];
         }
         else
         {
            // unknown type.  Most likely, the type either is not a 'TBaseClassType' type, or 
            // it exists in an assembly other than the assembly used by this instance of the factory.
            // By default, the searched assembly is the 'GetEntryAssembly()'.  You can load additional
            // search assemblies with the 'LoadAdditionalAssembly' call
            throw new ObjectFactoryUnknownTypeException("Unable to acquire instance handler. Unknown type: \'" + id + "\'");
         }
      }

      private void ConstructHandlers(Assembly assembly)
      {
         if (assembly == null)
            return;

         try
         {
            foreach (Type oType in assembly.GetTypes())
            {
               if (typeof(TBaseClassType).IsAssignableFrom(oType))
               {
                  AddTypeToInstanceHandlerMap(oType);
               }
            }
         }
         catch (ReflectionTypeLoadException ex)
         {
            Exception[] loaderExceptions = ex.LoaderExceptions;

            foreach (Exception err in loaderExceptions)
            {
               Debug.WriteLine(err.Message);
            }
         }
      }

      private void AddTypeToInstanceHandlerMap(Type type)
      {
         // Adding both the type.Name and type.FullName here allows the client to reference the type by either it's short name, or it's fully qualified assembly name.
         // This essentially adds support for types of the same short name that exist within different assemblies.  

         if (!instanceHandlers_.ContainsKey(type.Name))
            instanceHandlers_.Add(type.Name, type.TypeHandle);

         if (!instanceHandlers_.ContainsKey(type.FullName))
            instanceHandlers_.Add(type.FullName, type.TypeHandle);
      }

      public TBaseClassType Create(string id, params object[] arguments)
      {
         if (!alreadyLoaded_)
         {
            if (!alreadyLoaded_)
            {
               LoadAdditionalAssembly(typeof(TBaseClassType));
               alreadyLoaded_ = true;
            }
         }

         BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
         RuntimeTypeHandle instanceHandler = GetInstanceHandler(id);

         try
         {
            return (TBaseClassType)Activator.CreateInstance(Type.GetTypeFromHandle(instanceHandler), bindingFlags, null, arguments, null);
         }
         catch (MissingMethodException ex)
         {
            throw new ObjectFactoryMissingConstructorException("Unable to find an accessible constructor for '" + id + "'.  Possibly a private or protected constructor.", ex);
         }
      }

      public void LoadAdditionalAssembly(Type t)
      {
         ConstructHandlers(Assembly.GetAssembly(t));
      }

      public List<Type> GetTypesLoaded()
      {
         List<Type> toreturn = new List<Type>();

         foreach (System.Collections.Generic.KeyValuePair<string, RuntimeTypeHandle> pair in instanceHandlers_)
         {
            Type current = Type.GetTypeFromHandle(pair.Value);

            toreturn.Add(current);
         }

         return toreturn;
      }

      public Type GetTypeByName(string TypeName)
      {
         foreach (System.Collections.Generic.KeyValuePair<string, RuntimeTypeHandle> pair in instanceHandlers_)
         {
            if (pair.Key.Equals(TypeName, StringComparison.OrdinalIgnoreCase))
            {
               Type toload = Type.GetTypeFromHandle(pair.Value);

               return toload;
            }
         }

         return null;
      }
   }
}
