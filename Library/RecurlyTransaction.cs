﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Xml;
using System.Text;

namespace Recurly
{
    public class RecurlyTransaction
    {
        public string Id { get; private set; }
        public int AmountInCents { get; private set; }
        public string Description { get; private set; }
        public DateTime Date { get; private set; }
        public string Message { get; private set; }
        public string Status { get; private set; }
        public bool Success { get; private set; }
        public bool Voidable { get; private set; }
        public bool Refundable { get; private set; }
        public TransactionType Type { get; private set; }
        public RecurlyAccount Account { get; private set; }

        public enum TransactionType : short
        {
            Unknown = 0,
            Authorization,
            Payment,
            Refund
        }

        internal RecurlyTransaction()
        { }

        internal RecurlyTransaction(XmlTextReader reader)
        {
            ReadXml(reader);
        }

        private const string UrlPrefix = "/transactions/";

        public static void Create(RecurlyAccount acct, int amountInCents, string description)
        {
            RecurlyTransaction toCreate = new RecurlyTransaction();

            toCreate.Account = acct;
            toCreate.AmountInCents = amountInCents;
            toCreate.Description = description;

            RecurlyClient.PerformRequest(RecurlyClient.HttpRequestMethod.Post,
                UrlPrefix,
                new RecurlyClient.WriteXmlDelegate(toCreate.WriteXml));
        }

        public static RecurlyTransaction Get(string transactionId)
        {
            RecurlyTransaction transaction = new RecurlyTransaction();

            HttpStatusCode statusCode = RecurlyClient.PerformRequest(RecurlyClient.HttpRequestMethod.Get,
                UrlPrefix + System.Web.HttpUtility.UrlEncode(transactionId),
                new RecurlyClient.ReadXmlDelegate(transaction.ReadXml));

            if (statusCode == HttpStatusCode.NotFound)
                return null;

            return transaction;
        }

        #region Read and Write XML documents

        internal void ReadXml(XmlTextReader reader)
        {
            while (reader.Read())
            {
                // End of account element, get out of here
                if ((reader.Name == "transaction" || reader.Name == "payment" || reader.Name == "refund") && 
                    reader.NodeType == XmlNodeType.EndElement)
                    break;

                if (reader.NodeType == XmlNodeType.Element)
                {
                    bool boolVal;
                    switch (reader.Name)
                    {
                        case "id":
                            this.Id = reader.ReadElementContentAsString();
                            break;

                        case "action":
                            string action = reader.ReadElementContentAsString();
                            switch (action)
                            {
                                case "purchase":
                                    this.Type = TransactionType.Payment;
                                    break;
                                case "credit":
                                    this.Type = TransactionType.Refund;
                                    break;
                                case "authorization":
                                    this.Type = TransactionType.Authorization;
                                    break;
                                default:
                                    this.Type = TransactionType.Unknown;
                                    break;
                            }
                            break;

                        case "date":
                            DateTime date;
                            if (DateTime.TryParse(reader.ReadElementContentAsString(), out date))
                                this.Date = date;
                            break;

                        case "amount_in_cents":
                            int amount;
                            if (Int32.TryParse(reader.ReadElementContentAsString(), out amount))
                                this.AmountInCents = amount;
                            break;

                        case "message":
                            this.Message = reader.ReadElementContentAsString();
                            break;

                        case "success":
                            if (Boolean.TryParse(reader.ReadElementContentAsString(), out boolVal))
                                Success = boolVal;
                            break;

                        case "voidable":
                            if (Boolean.TryParse(reader.ReadElementContentAsString(), out boolVal))
                                Voidable = boolVal;
                            break;

                        case "refundable":
                            if (Boolean.TryParse(reader.ReadElementContentAsString(), out boolVal))
                                Refundable = boolVal;
                            break;
                    }
                }
            }
        }

        internal void WriteXml(XmlTextWriter xmlWriter)
        {
            xmlWriter.WriteStartElement("transaction");
            
            xmlWriter.WriteElementString("amount_in_cents", this.AmountInCents.ToString());

            xmlWriter.WriteElementString("description", this.Description);

            this.Account.WriteXml(xmlWriter);

            xmlWriter.WriteEndElement();
        }

        #endregion

        #region Object Overrides

        public override string ToString()
        {
            return "Recurly Transaction: " + this.Id;
        }

        public override bool Equals(object obj)
        {
            if (obj is RecurlyTransaction)
                return Equals((RecurlyTransaction)obj);
            else
                return false;
        }

        public bool Equals(RecurlyTransaction transaction)
        {
            return this.Id == transaction.Id;
        }

        public override int GetHashCode()
        {
            return this.Id.GetHashCode();
        }

        #endregion
    }
}