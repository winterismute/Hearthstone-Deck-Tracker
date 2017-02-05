#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.IO;
using Hearthstone_Deck_Tracker.Utility.Logging;

#endregion

namespace Hearthstone_Deck_Tracker.Stats
{
    public class QuestStats
    {
        private const string FileName = "QuestsLog.xml";

        private static Lazy<QuestStats> _instance = new Lazy<QuestStats>(Load);

        [XmlArray(ElementName = "CurrentQuests")]
        private List<QuestInfo> _currentQuests;

        [XmlArray(ElementName = "CompletedQuests")]
        private List<QuestInfo> _completedQuests;

        /*
        static QuestStats()
        {
        }
        */

        private QuestStats()
        {
            _currentQuests = new List<QuestInfo>();
            _completedQuests = new List<QuestInfo>();
        }

        public static QuestStats Instance => _instance.Value;

        private static string FilePath => Path.Combine(Config.AppDataPath, FileName);

        //public List<GameInfo> GameInfos => _currentQuests ?? ();

        private static QuestStats Load()
        {
            QuestStats instance;
            if (!File.Exists(FilePath))
            {
                instance = new QuestStats();
            }
            else
            {
                try
                {
                    instance =  XmlManager<QuestStats>.Load(FilePath);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                    instance = new QuestStats();
                }
            }

            // Check whether one of the 2 lists is empty, add it empty if yes, and save

            return instance;
        }

        public static void Save() => XmlManager<QuestStats>.Save(FileName, _instance);

        internal static void Reload() => _instance = new Lazy<QuestStats>(Load);

        public void RemoveCompletedQuest(long id, string name, string desc, string datecompleted)
        {
            var retrieved = _currentQuests.Where(q => q.QuestName == name);
            System.Diagnostics.Debug.Assert(retrieved.Count() == 1);
            _currentQuests.Remove(retrieved.First());
            QuestInfo info = new QuestInfo(id, name, desc, retrieved.First().DateGiven, datecompleted);
            _completedQuests.Add(info);
        }

        public void AddNewQuest(long id, string name, string desc, string dategiven)
        {
            QuestInfo info = new QuestInfo(id, name, desc, dategiven, null);
            var retrieved = _currentQuests.Where(q => q.QuestName == name);
            System.Diagnostics.Debug.Assert(retrieved.Count() == 0);
            _currentQuests.Add(info);
        }

        public void RemoveCurrentQuest(long id, string name)
        {
            var retrieved = _currentQuests.Where(q => q.QuestName == name);
            if (retrieved.Count() > 0)
            {
                _currentQuests.Remove(retrieved.First());
            }
        }
    }

    public class QuestInfo
    {
        public QuestInfo()
        {
        }

        public QuestInfo(long qid, string qname, string qdesc, string qdateGiven, string qdateCompleted)
        {
            QuestId = qid;
            QuestName = qname;
            QuestDescription = qdesc;
            DateGiven = qdateGiven;
            DateCompleted = qdateCompleted;
        }

        [XmlAttribute("questId")]
        //public Guid QuestId { get; set; }
        public long QuestId { get; set; }

        [XmlAttribute("questName")]
        public string QuestName { get; set; }

        [XmlAttribute("questDescription")]
        public string QuestDescription { get; set; }

        [XmlAttribute("dateGiven")]
        public string DateGiven { get; set; }

        [XmlAttribute("dateCompleted")]
        public string DateCompleted { get; set; }

        // DateGiven=131264657750000000 DateCompleted=130573554750000000
    }
}
