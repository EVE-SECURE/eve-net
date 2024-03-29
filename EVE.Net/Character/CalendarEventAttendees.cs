﻿//
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
using System.Text;
using System.ComponentModel;

namespace EVE.Net.Character
{
   // http://wiki.eveonline.com/en/wiki/EVE_API_Character_Calendar_Event_Attendees
   public sealed class CalendarEventAttendees : APIObject
   {
      private int eventID_;

      public CalendarEventAttendees() { }

      public CalendarEventAttendees(string keyid, string vcode, string actorid, int eventID) 
         : base(keyid, vcode, actorid)
      {
         eventID_ = eventID;
      }

      #region Overrides

      public override void Query()
      {
         APIReader reader = new APIReader();
         reader.Query(Uri, this, "keyID={0}&vCode={1}&characterID={2}&eventID={3}", keyID, vCode, actorID, eventID_);
      }

      public override string Uri { get { return BaseUrls.charEventAttendeesUri; } }

      #endregion

      #region Nested Classes

      public sealed class Attendee
      {
         public Attendee() { }

         public Int64 characterID { get; set; }
         public string characterName { get; set; }
         public string response { get; set; }
      }

      #endregion

      private List<Attendee> eventAttendees_ = new List<Attendee>();

      [TypeConverter(typeof(ExpandableObjectConverter))]
      public List<Attendee> eventAttendees
      {
         get { return eventAttendees_; }
         set { eventAttendees_ = value; }
      }
   }
}
