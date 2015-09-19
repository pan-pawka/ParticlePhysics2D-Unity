

namespace CzlParallelFor
{ 
    /// <summary>
    /// 固定帧数的类
    /// </summary>
    public class ConstFrame
    {
        public ConstFrame() : this(30) { }
		public ConstFrame(float framePerSecond)
		{
			this.framePerSecond = framePerSecond;
			//lastTime = time;
		}
		float _framePerSecond;
		public float framePerSecond
		{
			set 
			{
				_framePerSecond = value;
				frameTime = 1f / _framePerSecond;
			}
			get { return _framePerSecond; }
		}
        //public const float frameTime = 1f / 60f;
        public float frameTime;
        public const float maxElapseTime = 0.3333f;
        float lastTime;
        public void Start()
        {
            lastTime = time;
        }
        public float time
        {
            get { return UnityEngine.Time.realtimeSinceStartup; }
        }

        /// <summary>
        /// 获取距离上次执行此函数后,流逝的帧数
        /// </summary>
        /// <returns></returns>
        public int GetDeltaFrame()
        {

            var lNewTime = time;
            var lElapse = System.Math.Min(maxElapseTime, lNewTime - lastTime);
            //Console.WriteLine(lElapse);
            //int lUpdateCount = 0;
            lastTime = lNewTime;
            //使线程每秒执行60次
            if (lElapse < 0)
                return 0;
            var lFrameCount = (int)(lElapse / frameTime) + 1;
            //while (lElapse >= 0)
            //{
            //    _update();
            //    lElapse -= frameTime;
            //    //lUpdateCount += 1;
            //}
            //Console.WriteLine(lElapse + " " + lFrameCount);
            lastTime += lFrameCount * frameTime - lElapse;
            return lFrameCount;
        }
    }

}