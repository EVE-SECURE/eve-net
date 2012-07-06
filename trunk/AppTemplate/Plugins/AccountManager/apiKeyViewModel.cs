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
using AppTemplate.Utilities;

namespace Plugins.AccountManager
{
   public class apiKeyViewModel : ViewModel
   {
      public apiKeyViewModel() { }
      public apiKeyViewModel(string id, string vcode)
      {
         key_.keyID = id;
         key_.vCode = vcode;
      }

      private EVE.Net.APIKeyInfo key_ = new EVE.Net.APIKeyInfo();
      private string name_;

      public string ID { get { return key_.keyID; } set { key_.keyID = (value as string).Trim(); OnPropertyChanged(() => ID); } }
      public string ActorID { get { return key_.actorID; } set { key_.actorID = (value as string).Trim(); OnPropertyChanged(() => ActorID); } }
      public string Name { get { return name_; } set { name_ = (value as string).Trim(); OnPropertyChanged(() => Name); } }
      public string vCode { get { return key_.vCode; } set { key_.vCode = (value as string).Trim(); OnPropertyChanged(() => vCode); } }
      public string KeyType { get { return key_.type; } set { key_.type = value; OnPropertyChanged(() => KeyType); } }
      public int AccessMask { get { return key_.accessMask; } set { key_.accessMask = value; OnPropertyChanged(() => AccessMask); } }
      public DateTime Expires { get { return key_.expires; } set { key_.expires = value; OnPropertyChanged(() => Expires); } }
      public List<EVE.Net.APIKeyInfo.Character> Characters { get { return key_.characters; } set { key_.characters = value; OnPropertyChanged(() => Characters); } }

      public void Refresh()
      {
         key_.Query();

         OnPropertyChanged(() => Characters);
      }
   }
}
