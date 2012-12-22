
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace DatabaseObjectSearcherUI
{
    public interface IRequestCompleted
    {
        void Completed(Object result);
    }

    public enum ReqType : int
    {
        LoadObjects,
        Search,
        Details,
        Dependencies,
        Refresh,
        Navigate
    }

    [SuppressMessage("Microsoft.Design", "CA1001")]
    public class BackgroundProcessor
    {
        public class Request
        {
            public Object Argument;

            public Int32 RequestType;

            public DoWork DoWorkFunction
            {
                get;
                set;
            }
        }

        public delegate void DoWork(Object arg);

        //public delegate RequestFailedDelegate(Request rec,Exception ex);
        public event Action<Request, Exception> RequestFailed;

        AutoResetEvent _doWork = new AutoResetEvent(false);

        AutoResetEvent _stop = new AutoResetEvent(false);

        Thread _thread;

        List<Request> _requests = new List<Request>();

        public void AddRequest(DoWork workingFunction, Object arg, Int32 reqType, Boolean deleteSameRequests)
        {
            lock (this)
            {
                if (deleteSameRequests)
                {
                    var sameType = _requests.FindAll(x => x.RequestType == reqType);

                    foreach (Request req in sameType)
                    {
                        _requests.Remove(req);
                    }
                }

                var newReq = new Request();
                newReq.DoWorkFunction = workingFunction;
                newReq.RequestType = reqType;
                newReq.Argument = arg;
                _requests.Add(newReq);

                _doWork.Set();
            }
        }

        public void Run()
        {
            while (true)
            {
                var i = WaitHandle.WaitAny(new WaitHandle[] { _stop, _doWork });

                // stop event
                if (i == 0)
                {
                    break;
                }

                var processRequest = true;

                while (processRequest)
                {
                    Request req = GetRequest();

                    if (req != null)
                    {
                        try
                        {
                            req.DoWorkFunction(req.Argument);
                        }
                        catch (Exception ex)
                        {
                            if (RequestFailed != null)
                            {
                                RequestFailed.Invoke(req, ex);
                            }
                        }
                    }
                    else
                    {
                        processRequest = false;
                    }
                }
            }
        }

        private Request GetRequest()
        {
            Request req = null;

            lock (this)
            {
                if (_requests.Count > 0)
                {
                    req = _requests[0];
                    _requests.RemoveAt(0);
                }
            }

            return req;
        }

        public BackgroundProcessor()
        {
            _thread = new Thread(Run);
            _thread.Start();
        }

        public void Stop()
        {
            _stop.Set();
            _thread.Join();
        }
    }
}
