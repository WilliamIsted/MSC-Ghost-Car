using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

using HutongGames.PlayMaker;
using MSCLoader;

namespace GhostCar
{
    public static class SettingsManager
    {

        private static SettingsHeader groupReplays;
        private static SettingsHeader groupRecordings;
        private static SettingsHeader groupCurrentReplay;

        private static SettingsButton settingReplayFolder;
        private static SettingsCheckBox settingGuestReplays;
        private static SettingsCheckBox settingAutoRecordRallyOne;
        private static SettingsCheckBox settingAutoRecordRallyTwo;
        private static SettingsCheckBox settingAutoRecordDragRace;

        public static bool showGuestReplays => settingGuestReplays.GetValue();
        public static bool autoRecordRallyOne => settingAutoRecordRallyOne.GetValue();
        public static bool autoRecordRallyTwo => settingAutoRecordRallyTwo.GetValue();
        public static bool autoRecordDragRace => settingAutoRecordDragRace.GetValue();

        public static void initSettings()
        {

            groupReplays = Settings.AddHeader("Replays");
            replayFolder();
            Settings.AddText("");
            guestReplays();

            groupRecordings = Settings.AddHeader("Recordings");
            automaticRecording();

            groupCurrentReplay = Settings.AddHeader("Current Replay", false, true);

            List<Dictionary<string, string>> metadataList = Helper.GetReplayMetadata(10);
            foreach (var meta in metadataList)
            {
                ModConsole.Print("Replay:");
                foreach (var kvp in meta)
                    ModConsole.Print($"  {kvp.Key} = {kvp.Value}");
            }

        }

        private static void replayFolder()
        {

            settingReplayFolder = Settings.AddButton("Open Replay Folder", new Action(() =>
            {
                showReplayDir();
            }));

        }

        private static void guestReplays()
        {

            settingGuestReplays = Settings.AddCheckBox("settingGuestReplays", "Show Guest Practice Replays", true, new Action(() =>
            {

                ModConsole.Print($"Show Guest Replays: {showGuestReplays}");

            }));

        }

        private static void automaticRecording()
        {

            settingAutoRecordRallyOne = Settings.AddCheckBox("autoRecordRallyOne", "Automatically Record Rally - Stage One", true);
            settingAutoRecordRallyTwo = Settings.AddCheckBox("autoRecordRallyTwo", "Automatically Record Rally - Stage Two", true);
            settingAutoRecordDragRace = Settings.AddCheckBox("autoRecordDragRace", "Automatically Record Drag Race", true);

        }

        /*
         * 
         * 
         * 
         */

        public static void showReplayDir(string filename = "")
        {

            filename = Path.Combine(GhostCar.replayFolder, filename);

            if (File.Exists(filename))
            {
                Process.Start("explorer.exe", "/select,\"" + filename + "\"");
            }
            else if (Directory.Exists(filename))
            {
                Process.Start("explorer.exe", GhostCar.replayFolder);
            }

        }

    }

}
