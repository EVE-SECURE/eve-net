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
using System.ComponentModel;
using System.Diagnostics;
using System.Linq.Expressions;

namespace AppTemplate.Utilities
{
   public class ViewModel : INotifyPropertyChanged
   {
      [field:NonSerialized]
      public event PropertyChangedEventHandler PropertyChanged;

      protected virtual void OnPropertyChanged<T>(Expression<Func<T>> propertyExpression)
      {
         MemberExpression memberExpression = (MemberExpression)propertyExpression.Body;
         string propertyName = memberExpression.Member.Name;

         OnPropertyChanged(propertyName);
      }

      protected void OnPropertyChanged(params string[] propertyNames)
      {
         foreach (string propertyName in propertyNames)
         {
            VerifyProperty(propertyName);
   
            if (PropertyChanged != null)
               PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
         }
      }

      [Conditional("DEBUG")]
      protected void VerifyProperty(string propertyName)
      {
         Type type = GetType();

         if (type.GetProperty(propertyName) == null)
            Debug.Fail(string.Format("'{0}' is not a valid property of type '{1}'", propertyName, type.FullName));
      }
   }
}
