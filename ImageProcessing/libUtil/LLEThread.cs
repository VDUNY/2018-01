using System;
using System.Threading;

namespace LLE
{
    namespace Util
	{
		public class LLEThread : Disposable
		{
			public delegate void ThreadStart(LLEThread aThread);

			private ThreadStart m_threadStart;
			private Thread m_thread;
            private bool m_started = false;
			private bool m_stopped = true;
			private bool m_paused = false;
			private bool m_woke = false;
            private bool m_joinable = true;
            private ApartmentState m_apartmentState = ApartmentState.STA;
            private Object m_monitor = null;

			public LLEThread()
			{
                m_monitor = this;
			}

            public LLEThread(Object aMonitor)
            {
                m_monitor = aMonitor;
            }

            public LLEThread(ApartmentState aApartmentState)
            {
                m_apartmentState = aApartmentState;
                m_monitor = this;
            }

            public LLEThread(ApartmentState aApartmentState, Object aMonitor)
            {
                m_apartmentState = aApartmentState;
                m_monitor = aMonitor;
            }

            private void ThreadWrapper()
            {
                try
                {
                    m_threadStart(this);
                }
                catch (Exception e)
                {
                    Log.Instance.WriteLine(LogLevel.Err, "Unhandled exception caught by LLEThread: " + e.Message);
                }

                lock (m_monitor)
                {
                    m_stopped = true;
                    m_joinable = true;

                    Monitor.PulseAll(m_monitor);
                }
            }

            protected virtual void ThreadFunc(LLEThread aThread)
            {
            }

            public void Start(ThreadStart aThreadStart)
			{
                lock (m_monitor)
                {
                    if (!m_joinable)
                    {
                        throw new LLEException(LogLevel.Err, "previous thread has not exited", "LLEThread.Start");
                    }
                    Join();

                    m_threadStart = aThreadStart;
                    m_started = true;
                    m_stopped = false;
                    m_woke = false;
                    m_joinable = false;

                    m_thread = new Thread(new System.Threading.ThreadStart(this.ThreadWrapper));
                    m_thread.SetApartmentState(m_apartmentState);
                    m_thread.Start();
                }
			}

            public void Start()
            {
                Start(new ThreadStart(this.ThreadFunc));
            }

            public bool IsStarted()
            {
                lock (m_monitor)
                {
                    return m_started;
                }
            }

            public void Stop()
			{
				lock (m_monitor)
				{
					// stop the thread
					m_stopped = true;

					// wake up the thread
                    Monitor.PulseAll(m_monitor);
				}
			}

			public void Wake()
			{
                lock (m_monitor)
				{
					m_woke = true;
                    Monitor.PulseAll(m_monitor);
				}
			}

            public bool Join(int aTimeout)
            {
                lock (m_monitor)
                {
                    return Join(m_monitor, aTimeout);
                }
            }

            public void Join()
            {
                lock (m_monitor)
                {
                    Join(m_monitor);
                }
            }

            public bool Join(Object aMonitor, int aTimeout)
            {
                if (aMonitor != m_monitor)
                {
                    throw new LLEException(LogLevel.Err, "invalid monitor specified", "LLEThread.Join");
                }

                if (!IsStarted())
                {
                    return true;
                }

                // wait until the thread is joinable
                while (!m_joinable)
                {
                    try
                    {
                        if (!Monitor.Wait(aMonitor, aTimeout))
                        {
                            return false;
                        }
                    }
                    catch (System.Exception)
                    {
                    }
                }

                if (!m_thread.Join(aTimeout))
                {
                    return false;
                }

                lock (m_monitor)
                {
                    m_started = false;
                }

                return true;
            }

            public bool Join(Object aMonitor)
            {
                if (aMonitor != m_monitor)
                {
                    throw new LLEException(LogLevel.Err, "invalid monitor specified", "LLEThread.Join");
                }

                if (!IsStarted())
                {
                    return true;
                }

                // wait until the thread is joinable
                while (!m_joinable)
                {
                    try
                    {
                        Monitor.Wait(aMonitor);
                    }
                    catch (System.Exception)
                    {
                    }
                }

                m_thread.Join();

                lock (m_monitor)
                {
                    m_started = false;
                }

                return true;
            }

            public void Pause(bool aPaused)
			{
                lock (m_monitor)
				{
                    if (m_paused == aPaused)
                    {
                        return;
                    }

					// pause the thread
					m_paused = aPaused;

					// if resuming, wake up the thread
                    if (!m_paused)
                    {
                        Monitor.PulseAll(m_monitor);
                    }
				}
			}

            public bool IsPaused()
            {
                lock (m_monitor)
                {
                    return m_paused;
                }
            }

			public bool IsStopped()
			{
				return IsStopped(0, false);
			}

			public bool IsStopped(bool aPauseCheck)
			{
				return IsStopped(0, aPauseCheck);
			}

			public bool IsStopped(int aTimeout)
			{
				return IsStopped(aTimeout, false);
			}

			public bool IsStopped(int aTimeout, bool aPauseCheck)
			{
                lock (m_monitor)
				{
                    while (!(m_stopped || m_woke))
                    {
                        try
                        {
                            if (!Monitor.Wait(m_monitor, aTimeout))
                            {
                                break;
                            }
                        }
                        catch (System.Exception)
                        {
                        }
                    }

                    if (aPauseCheck && m_paused)
                    {
                        while (!(m_stopped || m_woke || m_paused))
                        {
                            try
                            {
                                if (!Monitor.Wait(m_monitor))
                                {
                                    break;
                                }
                            }
                            catch (System.Exception)
                            {
                            }
                        }
                    }

                    // clear the woke flag
                    m_woke = false;

					// are we stopped?
					return m_stopped;
				}
			}
		}
	}
}
