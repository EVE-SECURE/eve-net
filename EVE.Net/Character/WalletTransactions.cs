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
using System.Text;
using System.ComponentModel;
using System.Globalization;

namespace EVE.Net.Character
{
   // http://wiki.eveonline.com/en/wiki/EVE_API_Character_Wallet_Transactions
   public sealed class WalletTransactions : APIObject
   {
      private int accountKey_ = 1000;
      private Int64 fromID_ = -1;
      private int rowCount_ = -1;

      public WalletTransactions() { }

      public WalletTransactions(string keyid, string vcode, string actorid)
         : base(keyid, vcode, actorid) { }

      public WalletTransactions(string keyid, string vcode, string actorid, int accountKey, Int64 fromID, int rowCount)
         : base(keyid, vcode, actorid)
      {
         accountKey_ = accountKey;
         fromID_ = fromID;
         rowCount_ = rowCount;
      }

      #region Overrides


      public override void Query()
      {
         APIReader reader = new APIReader();

         StringBuilder sb = new StringBuilder();

         sb.AppendFormat(CultureInfo.InvariantCulture, "keyID={0}&vCode={1}&characterID={2}&accountKey={3}", keyID, vCode, actorID, accountKey_);

         if (fromID_ != -1)
            sb.AppendFormat("&fromID={0}", fromID_);

         if (rowCount_ != -1)
            sb.AppendFormat("&rowCount={0}", rowCount_);

         reader.Query(Uri, this, sb.ToString());
      }

      public override string Uri
      {
         get
         {
            return IsCorpKey ? BaseUrls.corpWalletTransactionsUri : BaseUrls.charWalletTransactionsUri;
         }
      }

      #endregion

      #region Nested Classes

      public sealed class Transaction
      {
         public Transaction() { }

         public DateTime transactionDateTime { get; set; }
         public Int64 transactionID { get; set; }
         public int quantity { get; set; }
         public string typeName { get; set; }
         public Int64 typeID { get; set; }
         public decimal price { get; set; }
         public Int64 clientID { get; set; }
         public string clientName { get; set; }
         public Int64 stationID { get; set; }
         public string stationName { get; set; }
         public string transactionType { get; set; }
         public string transactionFor { get; set; }
         public Int64 journalTransactionID { get; set; }
      }

      #endregion

      private List<Transaction> transactions_ = new List<Transaction>();

      [TypeConverter(typeof(ExpandableObjectConverter))]
      public List<Transaction> transactions
      {
         get { return transactions_; }
         set { transactions_ = value; }
      }
   }
}
