// SessionTable.cs
//
// Pete Myers
// CST 415
// Fall 2019
// 

using System;
using System.Collections.Generic;
using System.Threading;

namespace SDServer
{
    class SessionException : Exception
    {
        public SessionException(string msg) : base(msg)
        {
        }
    }

    class SessionTable
    {
        // thread safe collection
        // represents the SDServer's session table, where we track session data per client
        // client sessions are identified by an unsigned long session ID
        // session IDs are never reused
        // when the session table is first created, it is empty, with no client session data
        // client session data is made up of arbitrary key/value pairs, where each are text

        private class Session
        {
            public ulong sessionId;
            public Dictionary<string, string> values;
            // Note: any other info about the session we want to remember can go here

            public Session(ulong sessionId)
            {
                this.sessionId = sessionId;
                values = new Dictionary<string, string>();
            }
        }

        private Dictionary<ulong, Session> sessions;    // sessionId --> Session instance
        private ulong nextSessionId;                    // next value to use for the next new session
        private Mutex mutex;                            // synchronize access to sessions

        public SessionTable()
        {
            sessions = new Dictionary<ulong, Session>();
            nextSessionId = 1;
            mutex = new Mutex(false);
        }

        private ulong NextSessionId()
        {
            // watch out for multiple threads trying to get the next sessionId!!!
            ulong sessionId = 0;
            mutex.WaitOne();
            sessionId = nextSessionId++;
            mutex.ReleaseMutex();

            return sessionId;
        }

        public ulong OpenSession()
        {
            // allocate and return a new session to the caller
            // find a free sessionId
            ulong sessionId = NextSessionId();

            // allocate a new session instance
            Session session = new Session(sessionId);

            // save the session for later
            mutex.WaitOne();
            sessions.Add(sessionId, session);
            mutex.ReleaseMutex();

            return sessionId;
        }

        public bool ResumeSession(ulong sessionID)
        {
            // returns true only if sessionID is a valid and open sesssion, false otherwise
            bool result = false;

            mutex.WaitOne();
            if (sessions.ContainsKey(sessionID))
            {
                result = true;
            }
            mutex.ReleaseMutex();

            return result;
        }

        public void CloseSession(ulong sessionID)
        {
            // closes the session, will no longer be open and cannot be reused
            mutex.WaitOne();

            // throws a session exception if the session is not open
            if (!sessions.ContainsKey(sessionID))
            {
                mutex.ReleaseMutex();
                throw new SessionException("Cannot close session that doesn't exist!");
            }

            // Remove the session from the tab;e
            sessions.Remove(sessionID);
            mutex.ReleaseMutex();
        }

        public string GetSessionValue(ulong sessionID, string key)
        {
            // retrieves a session value, given session ID and key
            mutex.WaitOne();

            // throws a session exception if the session is not open or if the value does not exist by that key
            if (!sessions.ContainsKey(sessionID))
            {
                mutex.ReleaseMutex();
                throw new SessionException("ST.GetSessionValue() - Cannot get value for the session that doesn't exist!");
            }

            // Throws a session exception if the value does not exsist by that key
            if (!sessions[sessionID].values.ContainsKey(key)) 
            {
                mutex.ReleaseMutex();
                throw new SessionException("Session key not found!");
            }

            string value = sessions[sessionID].values[key];

            mutex.ReleaseMutex();

            return value;
        }

        public void PutSessionValue(ulong sessionID, string key, string value)
        {
            // stores a session value by session ID and key, replaces value if it already exists
            mutex.WaitOne();

            // throws a session exception if the session is not open
            if (!sessions.ContainsKey(sessionID))
            {
                mutex.ReleaseMutex();
                throw new SessionException("ST.PutSessionValue() - Cannot put value for the session that doesn't exist!");
            }

            // Store the value in the session
            sessions[sessionID].values[key] = value;

            mutex.ReleaseMutex();
        }
    }
}
