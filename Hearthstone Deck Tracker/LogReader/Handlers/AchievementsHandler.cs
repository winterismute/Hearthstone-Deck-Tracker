#region

using System;
using System.Collections.Generic;
using Hearthstone_Deck_Tracker.Enums;
using static Hearthstone_Deck_Tracker.LogReader.HsLogReaderConstants;
using Hearthstone_Deck_Tracker.Hearthstone;
#endregion

namespace Hearthstone_Deck_Tracker.LogReader.Handlers
{
    public class AchievementsHandler
    {
        class AchieveNotificationInfo
        {
            public AchieveNotificationInfo(/*int iID, */ bool iCompleted, bool iNew, bool iRemoved)
            {
                //ID = iID;
                Completed = iCompleted;
                isNew = iNew;
                Removed = iRemoved;
            }

            //public int ID;
            public bool Completed;
            public bool isNew;
            public bool Removed;
        }

        private Dictionary<long, AchieveNotificationInfo> achieveNotificationsCache;

        public void Handle(LogLineItem logLine, IGame game)
        {
            if (game.CurrentGameMode == GameMode.Spectator)
                return;

            if (AchieveNotificationRegex.IsMatch(logLine.Line))
            {
                var match = AchieveNotificationRegex.Match(logLine.Line);
                long ID = int.Parse(match.Groups["id"].Value);
                if (match.Groups["completed"].Value == "True")
                {
                    achieveNotificationsCache.Add(ID, new AchieveNotificationInfo(true, false, false));
                }
                else if (match.Groups["new"].Value == "True")
                {
                    achieveNotificationsCache.Add(ID, new AchieveNotificationInfo(false, true, false));
                }
                else if (match.Groups["remove"].Value == "True")
                {
                    achieveNotificationsCache.Add(ID, new AchieveNotificationInfo(false, false, true));
                }
            }
            else if (AchievementNotificationRegex.IsMatch(logLine.Line))
            {
                var match = AchievementNotificationRegex.Match(logLine.Line);
                long ID = long.Parse(match.Groups["id"].Value);
                if (achieveNotificationsCache.ContainsKey(ID))
                {
                    AchieveNotificationInfo info = achieveNotificationsCache[ID];
                    string name = match.Groups["name"].Value;
                    string description = match.Groups["description"].Value;
                    if (info.isNew)
                    {
                        long dategiven = long.Parse(match.Groups["dategiven"].Value);
                        Stats.QuestStats.Instance.AddNewQuest(ID, name, description, dategiven.ToString());
                    }
                    else if (info.Removed)
                    {
                        Stats.QuestStats.Instance.RemoveCurrentQuest(ID, name);
                    }
                    else if (info.Completed)
                    {
                        long datecompleted = long.Parse(match.Groups["datecompleted"].Value);
                        Stats.QuestStats.Instance.RemoveCompletedQuest(ID, name, description, datecompleted.ToString());
                    }
                }
            }

        }
    }
}
