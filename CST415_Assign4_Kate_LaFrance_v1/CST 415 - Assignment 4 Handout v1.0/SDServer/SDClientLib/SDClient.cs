// SDClient.cs
//
// Pete Myers
// CST 415
// Fall 2019
// 

using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace SDClientLib
{
    public class SDClient
    {
        private string sdServerAddress;
        private ushort sdServerPort;
        private bool connected;
        private ulong sessionID;
        Socket clientSocket;
        NetworkStream stream;
        StreamReader reader;
        StreamWriter writer;

        public SDClient(string sdServerAddress, ushort sdServerPort)
        {
            // save server address/port
            this.sdServerAddress = sdServerAddress;
            this.sdServerPort = sdServerPort;

            // initialize to not connected to server
            connected = false;
            clientSocket = null;
            stream = null;
            reader = null;
            writer = null;

            // no session open at this time
            sessionID = 0;
        }

        public ulong SessionID { get { return sessionID; } set { sessionID = value; } }

        public void Connect()
        {
            ValidateDisconnected();

            // create a client socket and connect to the FT Server's IP address and port
            clientSocket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            clientSocket.Connect(new IPEndPoint(IPAddress.Parse(sdServerAddress), sdServerPort));

            // establish the network stream, reader and writer
            stream = new NetworkStream(clientSocket);
            reader = new StreamReader(stream);
            writer = new StreamWriter(stream);

            // now connected
            connected = true;
        }

        public void Disconnect()
        {
            ValidateConnected();

            // close writer, reader and stream
            writer.Close();
            reader.Close();
            stream.Close();

            // disconnect and close socket
            clientSocket.Disconnect(false);
            clientSocket.Close();

            // now disconnected
            connected = false;
        }

        public void OpenSession()
        {
            ValidateConnected();

            // send open command to server
            SendOpen();

            // receive server's response, hopefully with a new session id
            sessionID = ReceiveSessionResponse();
        }

        public void ResumeSession(ulong trySessionID)
        {
            ValidateConnected();

            // send resume session to the server
            SendResume(trySessionID);

            // receive server's response, hopefully confirming our sessionId
            ulong receivedSessionId = ReceiveSessionResponse();

            // verify that we received the same session ID that we requested
            if (trySessionID != receivedSessionId)
            {
                throw new Exception("Server resumed wrong session ID: " + receivedSessionId.ToString());
            }

            // save opened session
            sessionID = receivedSessionId;        
        }

        public void CloseSession()
        {
            ValidateConnected();

            // send close session to the server
            SendClose(sessionID);

            // Verify closed response
            ulong closedId = ReceiveSessionResponse();
            if (closedId != sessionID)
            {
                throw new Exception("Server closed id " + closedId.ToString() + " but we asked it to close " + sessionID.ToString());
            }

            // no session open
            sessionID = 0;
        }

        public string GetDocument(string documentName)
        {
            ValidateConnected();

            // send get to the server
            SendGet(documentName);

            // get the server's response
            return ReceiveGetResponse();
        }

        public void PostDocument(string documentName, string documentContents)
        {
             ValidateConnected();

            // send the document to the server
            SendPost(documentName, documentContents);

            // get the server's response
            ReceivePostResponse();
        }

        private void ValidateConnected()
        {
            if (!connected)
                throw new Exception("Connot perform action. Not connected to server!");
        }

        private void ValidateDisconnected()
        {
            if (connected)
                throw new Exception("Connot perform action. Already connected to server!");
        }

        private void SendOpen()
        {
            // send open message to SD server
            writer.Write("open\n");
            writer.Flush();
            Console.WriteLine("Sent open to server");          
        }

        private void SendClose(ulong sessionId)
        {
            // send close message to SD server
            writer.Write("close\n" + sessionId.ToString() + "\n");
            writer.Flush();
            Console.WriteLine("Sent close to server: " + sessionId.ToString());
        }

        private void SendResume(ulong sessionId)
        {
             // send resume message to SD server
            writer.Write("resume\n" + sessionId.ToString() + "\n");
            writer.Flush();
            Console.WriteLine("Sent resume to server: " + sessionId.ToString());
        }

        private ulong ReceiveSessionResponse()
        {
            // get SD server's response to our last session request (open or resume)
            string line = reader.ReadLine();
            if (line == "accepted")
            {
                // yay, server accepted our session!
                // get the sessionID
                line = reader.ReadLine();
                Console.WriteLine("Received accepted: " + line);
                return ulong.Parse(line);
            }
            else if (line == "rejected")
            {
                // boo, server rejected us!
                line = reader.ReadLine();
                Console.WriteLine("Received rejected: " + line);
                throw new Exception(line);
            }
            else if (line == "closed")
            {
                // yay, server closed the session
                line = reader.ReadLine();
                Console.WriteLine("Received closed: " + line);
                return ulong.Parse(line);
            }
            else if (line == "error")
            {
                // boo, server sent us an error!
                line = reader.ReadLine();
                Console.WriteLine("Received error: " + line);
                throw new Exception(line);
            }
            else
            {
                throw new Exception("Expected to receive a valid session response, instead got... " + line);
            }
        }

        private void SendPost(string documentName, string documentContents)
        {
            // send post message to SD erer, including document name, length and contents
            writer.Write("post\n" +  documentName + "\n" + documentContents.Length.ToString() + "\n");
            writer.Write(documentContents);
            writer.Flush();
            Console.WriteLine("Sent post to server for document: " + documentName);
        }

        private void SendGet(string documentName)
        {
            // send get message to SD server
            writer.Write("get\n" + documentName + "\n");
            writer.Flush();
            Console.WriteLine("Sent get to server for document: " + documentName);
        }

        private void ReceivePostResponse()
        {
            // get server's response to our last post request
            string line = reader.ReadLine();
            if (line == "success")
            {
                // yay, server accepted our request!
                Console.WriteLine("Successfully posted document!");
            }
            else if (line == "error")
            {
                // boo, server sent us an error!
                line = reader.ReadLine(); // error msg
                throw new Exception("SDC.ReceivePostResponse() - Error posting document: " + line);
            }
            else
            {
                throw new Exception("Expected to receive a valid post response, instead got... " + line);
            }
        }

        private string ReceiveGetResponse()
        {
            // get server's response to our last get request and return the content received
            string line = reader.ReadLine();
            if (line == "success")
            {
                // yay, server accepted our request!

                // read the document name, content length and content
                string documentName = reader.ReadLine();  // Note: we ignore the document name
                int contentLength = int.Parse(reader.ReadLine());
                Console.WriteLine("Received get response for " + documentName);

                // return the content
                return ReceiveDocumentContent(contentLength);
            }
            else if (line == "error")
            {
                // boo, server sent us an error!
                string msg = reader.ReadLine();
                throw new Exception(msg);
            }
            else
            {
                throw new Exception("Expected to receive a valid get response, instead got... " + line);
            }
        }

        private string ReceiveDocumentContent(int length)
        {
            // read from the reader until we've received the expected number of characters
            // accumulate the characters into a string and return those when we received enough

            StringBuilder builder = new StringBuilder();
            int charactersToRead = length;
            while (charactersToRead > 0)
            {
                char[] buffer = new char[charactersToRead];
                int charactersRead = reader.Read(buffer, 0, charactersToRead);
                charactersToRead -= charactersRead;
                builder.Append(buffer, 0, charactersRead);
            }
            Console.WriteLine("SDC.ReceiveDocumentContent() - received " + length.ToString() + " characters from server");

            return builder.ToString();
        }
    }
}
