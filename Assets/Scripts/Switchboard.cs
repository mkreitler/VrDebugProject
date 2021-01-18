using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assertions;

namespace PalletBuilder
{
    public class Switchboard : MonoBehaviour
    {
        public static Switchboard THIS = null;

        public delegate object fnListener(object args);

        public delegate void fnCallback(object listener, object retVal);

        private static List<ListenInfo> deferredRegistrationList = new List<ListenInfo>();
        private static List<ListenInfo> deferredDeregistrationList = new List<ListenInfo>();

        private class ListenInfo
        {
            public string message = null;
            public object listener = null;
            public fnListener fn = null;

            public ListenInfo(string theMessage, object theListener, fnListener theFn)
            {
                Assert.Test(theMessage != null, "Invalid message!");
                Assert.Test(theFn != null, "Invalid delegate!");

                message = theMessage;
                listener = theListener;
                fn = theFn;
            }
        }

        private class MessageRequest
        {
            public string message = null;
            public object listener = null;
            public object args = null;
            public fnCallback callback = null;

            public MessageRequest(string theMessage, object theListener, object theArgs, fnCallback theCallback)
            {
                message = theMessage;
                listener = theListener;
                args = theArgs;
                callback = theCallback;
            }
        }

        private List<ListenInfo> registrationList = new List<ListenInfo>();
        private List<ListenInfo> deregistrationList = new List<ListenInfo>();

        private List<MessageRequest> unusedMessageRequests = new List<MessageRequest>();
        private List<MessageRequest> usedMessageRequests = new List<MessageRequest>();
        private List<MessageRequest> messageRequests = new List<MessageRequest>();

        private Hashtable switchboard = new Hashtable();

        public static void Clear()
        {
            deferredRegistrationList.Clear();
            deferredDeregistrationList.Clear();

            if (THIS != null)
            {
                THIS._Clear();
            }
        }

        public static void Broadcast(string theMessage, object args, fnCallback callback)
        {
            Assert.Test(THIS != null, "No switchboard found!");

            THIS._AddBroadcastRequest(theMessage, args, callback);
        }

        public static void Broadcast(string theMessage, object args)
        {
            Assert.Test(THIS != null, "No switchboard found!");

            THIS._AddBroadcastRequest(theMessage, args, null);
        }

        public static void Message(string theMessage, object theListener, object args, fnCallback callback)
        {
            Assert.Test(THIS != null, "No switchboard found!");
            Assert.Test(theListener != null, "Invalid listener!");

            THIS._AddMessageRequest(theMessage, theListener, args, callback);
        }

        public static void Message(string theMessage, object theListener, object args)
        {
            Assert.Test(THIS != null, "No switchboard found!");
            Assert.Test(theListener != null, "Invalid listener!");

            THIS._AddMessageRequest(theMessage, theListener, args, null);
        }

        public static void Listen(string theMessage, object theListener, fnListener theFn)
        {
            Assert.Test(theMessage != null, "Invalid message!");
            Assert.Test(theListener != null, "Invalid listener!");
            Assert.Test(theFn != null, "Invalid delegate!");

            if (THIS != null)
            {
                THIS._Listen(theMessage, theListener, theFn);
            }
            else
            {
                deferredRegistrationList.Add(new ListenInfo(theMessage, theListener, theFn));
            }
        }

        public static void Unlisten(string theMessage, object theListener, fnListener theFn)
        {
            // null message indicates we should unlisten for every message for this object.
            Assert.Test(theListener != null, "Invalid listener!");
            Assert.Test(theFn != null, "Invalid delegate!");

            if (THIS != null)
            {
                THIS._Unlisten(theMessage, theListener, theFn);
            }
            else
            {
                deferredDeregistrationList.Add(new ListenInfo(theMessage, theListener, theFn));
            }
        }

        private void _AddBroadcastRequest(string theMessage, object theArgs, fnCallback callback)
        {
            MessageRequest mr = null;

            if (unusedMessageRequests.Count > 0)
            {
                mr = unusedMessageRequests[0];
                unusedMessageRequests.RemoveAt(0);
            }
            else
            {
                mr = new MessageRequest(null, null, null, null);
            }

            mr.message = theMessage;
            mr.listener = null;
            mr.args = theArgs;
            mr.callback = callback;

            messageRequests.Add(mr);
        }

        public void SendBroadcast(object args) {
            _Broadcast("SetText", args, null);
        }

        public void OnDestroy()
        {
            Clear();
            THIS = null;
        }

        private void _AddMessageRequest(string theMessage, object theListener, object theArgs, fnCallback callback)
        {
            MessageRequest mr = null;

            if (unusedMessageRequests.Count > 0)
            {
                mr = unusedMessageRequests[0];
                unusedMessageRequests.RemoveAt(0);
            }
            else
            {
                mr = new MessageRequest(null, null, null, null);
            }

            mr.message = theMessage;
            mr.listener = theListener;
            mr.args = theArgs;
            mr.callback = callback;

            messageRequests.Add(mr);
        }

        private void _Listen(string theMessage, object theListener, fnListener theFn)
        {
            registrationList.Add(new ListenInfo(theMessage, theListener, theFn));
        }

        private void _Unlisten(string theMessage, object theListener, fnListener theFn)
        {
            deregistrationList.Add(new ListenInfo(theMessage, theListener, theFn));
        }

        private void _Broadcast(string theMessage, object args, fnCallback callback)
        {
            List<ListenInfo> list = switchboard[theMessage] as List<ListenInfo>;

            if (list != null)
            {
                foreach (ListenInfo li in list)
                {
                    GameObject go = li.listener as GameObject;
                    if (go == null || go.activeSelf)
                    {
                        object retVal = li.fn(args);

                        if (callback != null)
                        {
                            callback(li.listener, retVal);
                        }
                    }
                }
            }
            else if (callback != null)
            {
                // Indicate Broadcast failed (no receivers).
                callback(null, null);
            }
        }

        private void _Clear()
        {
            registrationList.Clear();
            deregistrationList.Clear();

            unusedMessageRequests.Clear();
            usedMessageRequests.Clear();
            messageRequests.Clear();

            switchboard.Clear();
        }

        private void _Message(string theMessage, object theListener, object args, fnCallback callback)
        {
            List<ListenInfo> list = switchboard[theMessage] as List<ListenInfo>;

            if (list != null)
            {
                foreach (ListenInfo li in list)
                {
                    if (li.listener == theListener)
                    {
                        GameObject go = theListener as GameObject;
                        if (go == null || go.activeSelf)
                        {
                            object retVal = li.fn(args);

                            if (callback != null)
                            {
                                callback(li.listener, retVal);
                            }
                        }
                    }
                }
            }
            else if (callback != null)
            {
                // Indicate Message failed (no receivers).
                callback(null, null);
            }
        }

        void Awake()
        {
            Assert.Test(THIS == null, "Multiple switchboard!");

            deferredRegistrationList.ForEach(x => registrationList.Add(x));
            deferredDeregistrationList.ForEach(x => deregistrationList.Add(x));

            deferredRegistrationList.Clear();
            deferredDeregistrationList.Clear();

            THIS = this;
        }

        // Update is called once per frame
        void Update()
        {
            registerListeners();
            deregisterListeners();

            // Transfer pending messages into the 'usedMessages' queue.
            // That way, if these messages generate more messages, the
            // new incoming messages won't pollute the current list.
            foreach (MessageRequest mr in messageRequests)
            {
                usedMessageRequests.Add(mr);
            }
            messageRequests.Clear();

            // Execute all pending messages and recycle the message data.
            foreach (MessageRequest mr in usedMessageRequests)
            {
                if (mr.listener == null)
                {
                    _Broadcast(mr.message, mr.args, mr.callback);
                    unusedMessageRequests.Add(mr);
                }
                else
                {
                    _Message(mr.message, mr.listener, mr.args, mr.callback);
                    unusedMessageRequests.Add(mr);
                }
            }

            usedMessageRequests.Clear();
        }

        private void registerListeners()
        {
            foreach (ListenInfo info in registrationList)
            {
                List<ListenInfo> list = switchboard[info.message] as List<ListenInfo>;

                if (list == null)
                {
                    list = new List<ListenInfo>();
                }

                list.Add(info);
                switchboard[info.message] = list;
            }

            registrationList.Clear();
        }

        private void deregisterListeners()
        {
            foreach (ListenInfo info in deregistrationList)
            {
                if (info.message == null)
                {
                    // Deregister this object entirely.
                    foreach (DictionaryEntry entry in switchboard)
                    {
                        List<ListenInfo> list = entry.Value as List<ListenInfo>;
                        list.RemoveAll(li => li.listener == info.listener);
                    }
                }
                else
                {
                    // Deregister for a single message.
                    List<ListenInfo> list = switchboard[info.message] as List<ListenInfo>;
                    if (list != null)
                    {
                        list.RemoveAll(li => li.listener == info.listener && li.fn == info.fn);
                    }
                }
            }

            deregistrationList.Clear();
        }
    }
}
