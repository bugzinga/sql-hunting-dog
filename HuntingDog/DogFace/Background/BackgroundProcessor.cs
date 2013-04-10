
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using HuntingDog.Core;

namespace HuntingDog.DogFace.Background
{
    public class BackgroundProcessor
    {
        protected readonly Log log = LogFactory.GetLog();

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

                if (i == 0)
                {
                    break;
                }

                var processRequest = true;

                while (processRequest)
                {
                    var request = GetRequest();

                    if (request != null)
                    {
                        try
                        {
                            log.Message(String.Format("Processing request: type = {0}, parameter = {{ {1} }}", request.RequestType.ToString(), request.Argument));
                            var analyzer = new PerformanceAnalyzer();

                            request.DoWorkFunction(request.Argument);
                            
                            analyzer.Stop();
                            log.Performance("Request process time", analyzer.Result);
                        }
                        catch (Exception ex)
                        {
                            if (RequestFailed != null)
                            {
                                RequestFailed.Invoke(request, ex);
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

        public void AddRequest(DoWork workingFunction, Object arg, RequestType requestType, Boolean deleteSameRequests)
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
