using System;
using System.Runtime.Serialization;
using System.IO;
using System.Xml;

namespace DataAccess
{
   [DataContract(Name="Settings")]
   internal class Settings
   {
      [DataMember(Name="TimeOfLastRefTypesUpdate")]
      internal DateTime TimeOfLastRefTypesUpdate = DateTime.MinValue;

      [DataMember(Name="ReferenceTypes")]
      internal System.Collections.Generic.List<EVE.Net.RefTypes.ReferenceType> ReferenceTypes = new System.Collections.Generic.List<EVE.Net.RefTypes.ReferenceType>();

      public Settings() { }

      public static void Load(string settingsFile, ref Settings settings)
      {
         try
         {
            using (FileStream reader = new FileStream(settingsFile, FileMode.Open, FileAccess.Read))
            {
               DataContractSerializer xml = new DataContractSerializer(typeof(Settings));
               settings = xml.ReadObject(reader) as Settings;
            }
         }
         catch { }
         finally
         {
            if (settings == null)
               settings = new Settings();
         }
      }

      public static void Save(string settingsFile, ref Settings settings)  
      {
         try
         {
            if (settings == null)
               return;

            XmlWriterSettings xmlSettings = new XmlWriterSettings { Indent = true };
            using (XmlWriter writer = XmlWriter.Create(settingsFile, xmlSettings))
            {
               DataContractSerializer xml = new DataContractSerializer(typeof(Settings));
               xml.WriteObject(writer, settings);
            }
         }
         catch { }
      }
   }
}
