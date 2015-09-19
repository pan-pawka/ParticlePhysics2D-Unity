using System;
using System.Collections.Generic;
using System.Threading;

namespace CzlParallelFor
{
  
    //多线程并行处理
    public class ParallelProcess
    {
        public ParallelProcess(ParallelProcessManager manager)
        {
            this.manager = manager;
        }

        public void Start()
        {
            if (thread == null)
            {
                thread = new Thread(Process);
                thread.Start();
            }
        }
        public Thread thread;
        //唤醒此线程的事件
        public AutoResetEvent resetEvent = new AutoResetEvent(false);
        //线程处理结束的事件
        public AutoResetEvent updateEndEvent = new AutoResetEvent(false);

        public void Quit()
        {
            _quit = true;
            resetEvent.Set();
            updateEndEvent.Close();
        }
        bool _quit;

        volatile bool _running = false;
        public bool running
        {
            get { return _running; }
        }

        volatile int _runCount = 0;

        int doProcessCount;
        public void TryProcess(int runCount)
        {
            //if (runCount>0)
            //{
            _runCount = runCount;
            updateEndEvent.Reset();
            resetEvent.Set();
            //}
            //else
            //{
            //    updateEndEvent.Set();
            //}
            //UnityEngine.Debug.Log(thread.ManagedThreadId + " TryProcess " + (++doProcessCount) + " Time " + time);
        }
        //int processCount;
        void Process()
        {
            //lastTime = time;
            //线程阻塞,等待重新唤醒
            Wait();
            while (!_quit)
            {
                _running = true;

                var lRunCount = _runCount;
                var lUpdate = manager.updateAction;
                var lProcessIndex = processIndex;
                var lProcessCount = processCount;
                try {
                	while (lRunCount > 0)
                	{
                    	//_update(processIndex,processCount);
                    	lUpdate(lProcessIndex, lProcessCount);
	                    //lElapse -= frameTime;
                    //lUpdateCount += 1;
    	                lRunCount -= 1;
        	        }
                } catch (System.Exception e) {
                	UnityEngine.Debug.LogError(e);
                }

                Wait();

            }
            resetEvent.Close();
        }
        int doWaitCount;
        void Wait()
        {
            //activeTime = targetTime;
            _running = false;
            //Debug.Log("Thread " + thread.ManagedThreadId + " End " + (++processCount));
            //UnityEngine.Debug.Log(thread.ManagedThreadId + " Wait EndEvent.Set " + (doWaitCount + 1));
            //激活更新结束的事件
            updateEndEvent.Set();
            //UnityEngine.Debug.Log(thread.ManagedThreadId + " Wait Start " + (doWaitCount + 1) + " Time " + time);
            //线程阻塞,等待重新唤醒
            resetEvent.WaitOne();
            //UnityEngine.Debug.Log(thread.ManagedThreadId + " Wait End " + (++doWaitCount) + " Time " + time);
        }


        //System.Action<int,int> _update;
        public ParallelProcessManager manager;

        //public System.Action<int, int> Update
        //{
        //    set { _update = value; }
        //}

        volatile public int processIndex;
        volatile public int processCount;
    }

    public class ParallelProcessManager
    {

        ParallelProcess[] parallelProcess = new ParallelProcess[] { };
        int parallelProcessCount;
        //设置更新的函数,两个参数分别为索引开始位置和数量
        public System.Action<int, int> updateAction;
        int waitUpdateEndCount;
        WaitHandle[] waitHandles = new WaitHandle[] { };
        public int maxThreadCount = int.MaxValue;
        public void WaitUpdateEnd()
        {
            if (waitHandles.Length != 0)
            {
                //UnityEngine.Debug.Log(" WaitUpdateEnd Start " + (waitUpdateEndCount + 1) + " Time " + ParallelProcess.time);
                WaitHandle.WaitAll(waitHandles, 1000);
                //UnityEngine.Debug.Log(" WaitUpdateEnd End " + (++waitUpdateEndCount) + " Time " + ParallelProcess.time);
            }
        }

        static int[] processCountBuffer = new int[8];

        static int CaculateProcess(int totalProcess, int maxThreadCount)
        {
            if (totalProcess == 0)
                return 0;
            var lThreadCount = Math.Min(System.Environment.ProcessorCount - 1, maxThreadCount);
            if (lThreadCount <= 0)
                lThreadCount = 1;
            if(lThreadCount>processCountBuffer.Length)
                processCountBuffer = new int[lThreadCount];
            var lProcessCountFloat = (float)totalProcess / (float)lThreadCount;
            var lProcessCount = (int)lProcessCountFloat;
            if (lProcessCountFloat > lProcessCount)
                lProcessCount += 1;
            int i = 0;
            for (; i < lThreadCount; ++i)
            {
                if (totalProcess <= lProcessCount)
                {
                    processCountBuffer[i] = totalProcess;
                    break;
                }
                processCountBuffer[i] = lProcessCount;
                totalProcess -= lProcessCount;
            }
            return i+1;
        }

        public void SetProcess(int totalProcess)
        {
            var lThreadCount = CaculateProcess(totalProcess, maxThreadCount);
            SetProcess(0, processCountBuffer, lThreadCount);
        }

        public void SetProcess(int startIndex, int totalProcess)
        {
            var lThreadCount = CaculateProcess(totalProcess, maxThreadCount);
            SetProcess(startIndex, processCountBuffer, lThreadCount);
        }

        public void SetProcess(int[] processCounts)
        {
            SetProcess(0, processCounts, processCounts.Length);
        }

        WaitHandle[][] waitHandlesBuffer = new WaitHandle[][]{};
        WaitHandle[] CreateWaitHandlesBuffer(int index)
        {
            var lOut = new WaitHandle[index];
            for (int i = 0; i < index; ++i)
                lOut[i] = parallelProcess[i].updateEndEvent;
            return lOut;
        }
        void ExtendWaitHandlesBuffer(int count)
        {
            if (count > waitHandlesBuffer.Length-1)
            {
                var lBuffer = new WaitHandle[count+1][];
                System.Array.Copy(waitHandlesBuffer, lBuffer, waitHandlesBuffer.Length);
                for (int i = waitHandlesBuffer.Length; i < count + 1; ++i)
                {
                    lBuffer[i] = CreateWaitHandlesBuffer(i);
                }
                waitHandlesBuffer = lBuffer;
            }
        }
        //WaitHandle[] GetWaitHandles(int count)
        //{
        //    if(waitHandlesBuffer>)
        //}

        /// <summary>
        /// 设置每个线程所处理的数据数
        /// </summary>
        /// <param name="startIndex">开始的索引</param>
        /// <param name="processCounts">每个线程处理的单元数量</param>
        /// <param name="count">processCounts中使用数据的数量</param>
        public void SetProcess(int startIndex, int[] processCounts, int count)
        {
            var lNeedWait = false;
            if (parallelProcessCount != count)
            {
				//UnityEngine.Debug.Log(count);
                if (count > parallelProcess.Length)
                {
                    ExtendParallelProcess(count);
                    ExtendWaitHandlesBuffer(count);
                    lNeedWait = true;
                }
                waitHandles = waitHandlesBuffer[count];
                parallelProcessCount = count;
            }
            var lIndex = startIndex;
            for (int i = 0; i < count; ++i)
            {
                parallelProcess[i].processIndex = lIndex;
                parallelProcess[i].processCount = processCounts[i];
                lIndex += processCounts[i];
            }
            //有创建新线程的时候等待新线程执行完Wait
            if (lNeedWait)
                WaitUpdateEnd();
        }

        private void ExtendParallelProcess(int count)
        {
            var lNewParallelProcess = new ParallelProcess[count];
            System.Array.Copy(parallelProcess, lNewParallelProcess, parallelProcess.Length);
            for (int i = parallelProcess.Length; i < count; ++i)
            {
                lNewParallelProcess[i] = new ParallelProcess(this);
                lNewParallelProcess[i].Start();
            }
            parallelProcess = lNewParallelProcess;
        }
        public void Update()
        {
            Update(1);
        }

        //让所有线程执行count次数
        public void Update(int count)
        {
            if (count > 0)
            {
                for (int i = 0; i < parallelProcessCount; ++i)
                    parallelProcess[i].TryProcess(count);
            }
        }
        /// <summary>
        /// 销毁子线程
        /// </summary>
        public void Destroy()
        {
            if (parallelProcess != null)
            {
                for (int i = 0; i < parallelProcess.Length; ++i)
                    parallelProcess[i].Quit();
                parallelProcess = null;
            }
        }
    }


}