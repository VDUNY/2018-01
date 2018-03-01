using System.Collections.Concurrent;
using Prism.Events;

namespace ModuleMessages
{

    /// <summary>
    /// 
    /// </summary>
    public class StatusMsg : PubSubEvent<StatusMsg>
    {
        public string Msg { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class StatusMessageEvent : PubSubEvent<StatusMsg>
    {
        private static readonly EventAggregator m_eventAggregator = null;
        private static readonly StatusMessageEvent m_push = null;

        /// <summary>
        /// 
        /// </summary>
        static StatusMessageEvent()
        {
            m_eventAggregator = new EventAggregator();
            m_push = m_eventAggregator.GetEvent<StatusMessageEvent>();
        }   //         static StatusMessageEvent()

        /// <summary>
        /// 
        /// </summary>
        public static StatusMessageEvent Instance { get { return m_push;  } }
    }

    /// <summary>
    /// PRISM mechanism for reporting settings across MEF modules, which are otherwise loosely coupled
    /// </summary>
    public class DataReportableSettings : PubSubEvent<DataReportableSettings>
    {
        public ConcurrentDictionary<string, string> ReportableSettings { get; set; }
    }   // public class DataReportableSettings

    /// <summary>
    /// 
    /// </summary>
    public class DataReportableSettingsEvent : PubSubEvent<DataReportableSettings>
    {
        private static readonly EventAggregator _eventAggregator = null;
        private static readonly DataReportableSettingsEvent _event = null;

        static DataReportableSettingsEvent()
        {
            _eventAggregator = new EventAggregator();
            _event = _eventAggregator.GetEvent<DataReportableSettingsEvent>();
        }

        public static DataReportableSettingsEvent Instance
        {
            get { return _event; }
        }

    }   // public class DataReportableSettingsEvent

}   // namespace ModuleMessages
