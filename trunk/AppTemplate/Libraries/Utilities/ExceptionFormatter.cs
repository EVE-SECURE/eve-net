using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Security;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Xml;

namespace AppTemplate.Utilities
{
   /// <summary>
   /// Exstension methods to the base Exception class.
   /// </summary>
   public static class ExceptionExtensions
   {
      public static void Print(this Exception exception)
      {

         string exceptionData = Format(exception);

         Console.WriteLine(exceptionData);
         Console.Error.WriteLine(exceptionData);
         Debug.WriteLine(exceptionData);
      }


      public static string Format(this Exception exception)
      {
         return Format(exception, null);
      }

      public static string Format(this Exception exception, NameValueCollection additionalInfo)
      {
         if (additionalInfo == null)
            additionalInfo = GetAdditionalInfo(exception, additionalInfo);

         var output = new StringBuilder();

         if (exception == null)
         {
            output.AppendFormat("{0}{0}No Exception object has been provided.{0}", Environment.NewLine);
            return output.ToString();
         }

         Exception iexception = exception;
         int icount = 1;

         do
         {
            // Write title information for the exception object.
            output.AppendFormat(" {3,2}) {2,15}: {1}{0}",
                  Environment.NewLine, iexception.GetType().FullName, "Exception Type", icount.ToString());

            PropertyInfo[] aryPublicProperties = iexception.GetType().GetProperties();
            NameValueCollection currentAdditionalInfo;

            // Loop through all public properties in order to record their values
            foreach (PropertyInfo p in aryPublicProperties)
            {

               try
               {
                  var val = p.GetValue(iexception, null);

                  // Ignore InnerException and StackTrace. These are added to the output later.
                  if (p.Name != "InnerException" && p.Name != "StackTrace")
                  {
                     if (val == null)
                     {

                        output.AppendFormat("{1,20}: NULL{0}", Environment.NewLine, p.Name);

                     }
                     else if (val is XmlElement)
                     {

                        // For XmlElements, parse the property.
                        // Often error details lie inside these properties.
                        var eval = (XmlElement)val;
                        var sb = new StringBuilder();
                        var xsettings = new XmlWriterSettings();
                        xsettings.Indent = true;
                        xsettings.OmitXmlDeclaration = true;

                        using (var xwriter = XmlWriter.Create(sb, xsettings))
                        {
                           xwriter.WriteRaw(eval.OuterXml);
                        }

                        output.AppendFormat("{1,20}: {0}{2}{0}", Environment.NewLine, p.Name, sb.ToString());

                     }
                     else
                     {
                        if (p.Name == "LoaderExceptions")
                        {
                           output.AppendLine();
                           output.AppendFormat("Loader Exceptions");

                           foreach (Exception err in (iexception as ReflectionTypeLoadException).LoaderExceptions)
                           {
                              output.AppendLine();
                              output.AppendFormat("{0}", err.Message);
                              output.AppendLine();
                           }
                        }
                        else
                           if (p.Name == "Message")
                           {
                              string message = p.GetValue(iexception, null).ToString();
                              message = message.Replace("\r\n", " ");

                              output.AppendFormat("{1,20}: {2}{0}", Environment.NewLine, p.Name, message);
                           }
                           else
                              // Loop through the collection of AdditionalInformation if the exception type is a BaseApplicationException.
                              if (p.Name == "AdditionalInformation")
                              {
                                 // Verify the collection is not null.
                                 if (p.GetValue(iexception, null) != null)
                                 {
                                    // Cast the collection into a local variable.
                                    currentAdditionalInfo = (NameValueCollection)p.GetValue(iexception, null);

                                    if (currentAdditionalInfo.Count > 0)
                                    {
                                       output.AppendFormat("Additional Information:{0}", Environment.NewLine);

                                       // Loop through the collection adding the information to the string builder.
                                       for (int i = 0; i < currentAdditionalInfo.Count; i++)
                                       {
                                          output.AppendFormat("{1,20}: {2}{0}", Environment.NewLine, currentAdditionalInfo.GetKey(i), currentAdditionalInfo[i]);
                                       }
                                    }
                                 }
                              }
                              else
                              {
                                 // Otherwise just write the ToString() value of the property.
                                 output.AppendFormat("{1,20}: {2}{0}", Environment.NewLine, p.Name, p.GetValue(iexception, null));
                              }
                     }
                  }


               }
               catch
               {
                  // Ignore exceptions from a single property
               }
            }

            if (iexception.StackTrace != null)
            {
               output.AppendFormat("{1,20}:{0}{2}{0}{0}", Environment.NewLine, "Trace", iexception.StackTrace);
            }

            // Reset the current exception
            iexception = iexception.InnerException;
            icount++;

         } while (iexception != null);


         // Record the contents of the AdditionalInfo collection, if available.
         if (additionalInfo != null)
         {
            foreach (string i in additionalInfo) output.AppendFormat(" {1,-20}: {2}{0}", Environment.NewLine, i, additionalInfo.Get(i));
         }

         return output.ToString();
      }

      [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "exception")]
      private static NameValueCollection GetAdditionalInfo(this Exception exception, NameValueCollection additionalInfo)
      {
         // Create the Additional Information collection if it does not exist.
         if (additionalInfo == null) additionalInfo = new NameValueCollection();

         additionalInfo.Add("MachineName", Environment.MachineName);
         additionalInfo.Add("TimeStamp", DateTime.Now.ToString());
         additionalInfo.Add("AppDomainName", AppDomain.CurrentDomain.FriendlyName);
         additionalInfo.Add("ThreadIdentity", Thread.CurrentPrincipal.Identity.Name);
         additionalInfo.Add("WindowsIdentity", WindowsIdentity.GetCurrent().Name);

         return additionalInfo;
      }

      public static string Format(StackTrace trace)
      {
         if (trace == null) return null;

         string resourceString = "at";
         string format = "in {0}:line {1}";

         bool flag = true;
         StringBuilder builder = new StringBuilder(0xff);

         for (int i = 0; i < trace.FrameCount; i++)
         {
            StackFrame frame = trace.GetFrame(i);
            MethodBase method = frame.GetMethod();
            if (method != null)
            {
               if (flag)
               {
                  flag = false;
               }
               else
               {
                  builder.Append(Environment.NewLine);
               }
               builder.AppendFormat(CultureInfo.InvariantCulture, "{0} ", resourceString);
               Type declaringType = method.DeclaringType;
               if (declaringType != null)
               {
                  builder.Append(declaringType.FullName.Replace('+', '.'));
                  builder.Append(".");
               }
               builder.Append(method.Name);
               if ((method is MethodInfo) && ((MethodInfo)method).IsGenericMethod)
               {
                  Type[] genericArguments = ((MethodInfo)method).GetGenericArguments();
                  builder.Append("[");
                  int index = 0;
                  bool flag2 = true;
                  while (index < genericArguments.Length)
                  {
                     if (!flag2)
                     {
                        builder.Append(",");
                     }
                     else
                     {
                        flag2 = false;
                     }
                     builder.Append(genericArguments[index].Name);
                     index++;
                  }
                  builder.Append("]");
               }
               builder.Append("(");
               ParameterInfo[] parameters = method.GetParameters();
               bool flag3 = true;
               for (int j = 0; j < parameters.Length; j++)
               {
                  if (!flag3)
                  {
                     builder.Append(", ");
                  }
                  else
                  {
                     flag3 = false;
                  }
                  string name = "<UnknownType>";
                  if (parameters[j].ParameterType != null)
                  {
                     name = parameters[j].ParameterType.Name;
                  }
                  builder.Append(name + " " + parameters[j].Name);
               }
               builder.Append(")");
               if (frame.GetILOffset() != -1)
               {
                  string fileName = null;
                  try
                  {
                     fileName = frame.GetFileName();
                  }
                  catch (SecurityException)
                  {
                  }
                  if (fileName != null)
                  {
                     builder.Append(' ');
                     builder.AppendFormat(CultureInfo.InvariantCulture, format, new object[] { fileName, frame.GetFileLineNumber() });
                  }
               }
            }
         }
         builder.Append(Environment.NewLine);

         return builder.ToString();
      }
   }
}
