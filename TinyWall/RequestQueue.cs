﻿using System.Collections.Generic;
using System.Threading;

namespace PKSoft
{
    internal class RequestQueue : DisposableObject
    {
        private List<ReqResp> FwReqQueue = new List<ReqResp>();
        private Semaphore FwReqSem = new Semaphore(0, 64);

        protected override void DisposeManaged()
        {
            FwReqSem.Close();
            base.DisposeManaged();
        }

        internal void Enqueue(ReqResp req)
        {
            lock (FwReqQueue)
            {
                FwReqQueue.Add(req);
            }

            RetrySema:
            try
            {
                FwReqSem.Release();
            }
            catch (SemaphoreFullException)
            {
                Thread.Sleep(10);
                goto RetrySema;
            }
        }

        internal ReqResp Dequeue()
        {
            ReqResp ret;

            FwReqSem.WaitOne();
            lock (FwReqQueue)
            {
                ret = FwReqQueue[0];
                FwReqQueue.RemoveAt(0);
            }

            return ret;
        }

        internal bool HasRequest(TinyWallCommands cmd)
        {
            lock (FwReqQueue)
            {
                for (int i = 0; i < FwReqQueue.Count; ++i)
                {
                    if (FwReqQueue[i].Request.Command == cmd)
                        return true;
                }
            }

            return false;
        }
    }
}