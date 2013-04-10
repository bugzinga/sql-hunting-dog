
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;

namespace HuntingDog.DogFace.Background
{
    [SuppressMessage("Microsoft.Design", "CA1001")]
    public class BackgroundProcessor
    {
        public delegate void DoWork(Object arg);

        public event Action<Request, Exception> RequestFailed;

        private AutoResetEvent doWork = new AutoResetEvent(false);

        private AutoResetEvent stop = new AutoResetEvent(false);

        private Thread thread;

        private LinkedList<Request> requests = new LinkedList<Request>();

        public BackgroundProcessor()
        {
            thread = new Thread(Run);
            thread.Start();
        }

        public void Run()
        {
            while (true)
            {
                var i = WaitHandle.WaitAny(new WaitHandle[] { stop, doWork });

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

        public void Stop()
        {
            stop.Set();
            thread.Join();
        }

        public void AddRequest(DoWork workingFunction, Object arg, Int32 requestType, Boolean deleteSameRequests)
        {
            lock (this)
            {
                if (deleteSameRequests)
                {
                    var sameTypeRequests = requests.Where(x => (x.RequestType == requestType));

                    foreach (Request request in sameTypeRequests)
                    {
                        requests.Remove(request);
                    }
                }

                var newReq = new Request
                {
                    DoWorkFunction = workingFunction,
                    RequestType = requestType,
                    Argument = arg
                };

                requests.AddLast(newReq);
                doWork.Set();
            }
        }

        private Request GetRequest()
        {
            Request request = null;

            lock (this)
            {
                if (requests.Any())
                {
                    request = requests.First.Value;
                    requests.RemoveFirst();
                }
            }

            return request;
        }
    }
}
