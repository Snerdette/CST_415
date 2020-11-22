// ContentFetcher.cs
//
// Pete Myers
// CST 415
// Fall 2019
// 

using System;
using System.Collections.Generic;


namespace SDBrowser
{
    class ContentFetcher
    {
        private Dictionary<string, IProtocolClient> protocols;  // protocol name --> protocol client instance

        public ContentFetcher()
        {
            // initially empty protocols dictionary
            protocols = new Dictionary<string, IProtocolClient>();
            
        }

        public void Close()
        {
            // close each protocol client
            foreach (IProtocolClient client in protocols.Values)
            {
                client.Close();
            }
            protocols.Clear();
        }

        public void AddProtocol(string name, IProtocolClient client)
        {
            // save the protocol client under the given name
            protocols.Add(name, client);
        }

        public string Fetch(string address)
        {
            // TODO: ContentFetcher.Fetch()
            // parse the address
            // Address format:
            //    < type >:< server IP >:< resource >
            //    Where…
            //      < type > is one of “SD” and “FT”
            //      < server IP > is the IP address of the server to contact
            //      < resource > is the name of the resource to request from the server
            if (String.IsNullOrWhiteSpace(address))
            {
                throw new Exception("Empty Address!");
            }

            string[] parts = address.Split(':');
            if (parts.Length != 3)
            {
                throw new Exception("Invalid address format!");
            }

            foreach (string part in parts)
            {
                if (String.IsNullOrWhiteSpace(part))
                {
                    throw new Exception("");
                }
            }

            string protocalName = parts[0];
            string serverIP = parts[1];
            string resourceName = parts[2];

            // retrieve the correct protocol client for the requested protocol
            // watch out for invalid type
            if (!protocols.ContainsKey(protocalName))
            {
                throw new Exception("Unrecognized protocol!");
            }
            IProtocolClient client = protocols[protocalName];

            // get the content from the protocol client, using the given IP address and resource name
            string content = client.GetDocument(serverIP, resourceName);
            
            // return the content
            return content;
        }
    }
}
