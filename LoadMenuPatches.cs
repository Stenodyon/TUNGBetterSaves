using System;
using System.Collections.Generic;
using System.IO;
using Harmony;
using UnityEngine;

namespace BetterSaves
{
    class LoadMenuPatches
    {
        /// <summary>
        /// Extensions to the load menu
        /// </summary>
        [HarmonyPatch(typeof(LoadGame), "GenerateLoadGamesMenu")]
        class LoadMenuPatch
        {
            static void Postfix(LoadGame __instance)
            {
                foreach(var saveFile in __instance.UISaveFiles) // Color BetterSaved games
                {
                    string fileName = BetterSaves.GetSaveDirectory() + "/" + saveFile.FileName + ".btung";
                    if (File.Exists(fileName))
                    {
                        saveFile.Title.text = $"<color=#15A51A>{saveFile.Title.text}</color>";
                        FileInfo info = new FileInfo(fileName);
                        long kBsize = info.Length / 1000;
                        DateTime time = info.LastWriteTime;
                        saveFile.Info.text = $"{kBsize} kB | {time}";
                    }
                }
                string[] files = Directory.GetFiles(BetterSaves.GetSaveDirectory());
                List<string> toAdd = new List<string>();
                foreach(string file in files) // Find bettersaves without regular save
                {
                    string basename = Path.GetFileNameWithoutExtension(file);
                    string tungName = BetterSaves.GetSaveDirectory() + "/" + basename + ".tung";
                    if (file.EndsWith(".btung") && !File.Exists(tungName))
                        toAdd.Add(basename);
                }
                int index = __instance.UISaveFiles.Count;
                foreach(string name in toAdd)
                {
                    GameObject obj = UnityEngine.Object.Instantiate(__instance.LoadGamePrefab, __instance.Parent);
                    obj.name = name;
                    UISaveFile entry = obj.GetComponent<UISaveFile>();
                    entry.FileName = name;
                    entry.Title.text = $"<color=\"blue\">{name}</color>";
                    entry.SetPosition(index);
                    FileInfo info = new FileInfo(BetterSaves.GetSaveDirectory() + "/" + name + ".btung");
                    long kBSize = info.Length / 1000;
                    DateTime time = info.LastWriteTime;
                    entry.Info.text = $"{kBSize} kB | {time}";
                    __instance.UISaveFiles.Add(entry);
                    index++;
                }
                __instance.Parent.sizeDelta = new Vector2(784f, (float)(10 + index * 110));
            }
        }

        /// <summary>
        /// Renaming support
        /// </summary>
        [HarmonyPatch(typeof(LoadGame), "SetNewName")]
        class RenamePatch
        {
            static bool Prefix(LoadGame __instance)
            {
                string path = $"{BetterSaves.GetSaveDirectory()}/{__instance.SelectedSaveFile.FileName}.tung";
                if (!File.Exists(path))
                    return false;
                return true;
            }

            static void Postfix(LoadGame __instance)
            {
                string oldName = __instance.SelectedSaveFile.FileName;
                string newName = NewGame.ValidatedUniqueSaveName(__instance.RenameInput.text);
                string oldPath = $"{BetterSaves.GetSaveDirectory()}/{oldName}.btung";
                string newPath = $"{BetterSaves.GetSaveDirectory()}/{newName}.btung";
                if (File.Exists(oldPath))
                    File.Move(oldPath, newPath);
                __instance.GenerateLoadGamesMenu();
            }
        }

        private static string GetUniqueSaveName(string originalName)
        {
            string newName = originalName;
            while (File.Exists($"{BetterSaves.GetSaveDirectory()}/{newName}.btung"))
                newName += "-";
            return newName;
        }

        /// <summary>
        /// Save duplication support
        /// </summary>
        [HarmonyPatch(typeof(LoadGame), "DuplicateGame")]
        class DuplicatePatch
        {
            private static string newName;

            static bool Prefix(LoadGame __instance)
            {
                string path = $"{BetterSaves.GetSaveDirectory()}/{__instance.SelectedSaveFile.FileName}.tung";
                if (!File.Exists(path))
                {
                    newName = GetUniqueSaveName(__instance.SelectedSaveFile.FileName);
                    return false;
                }
                newName = NewGame.ValidatedUniqueSaveName(__instance.SelectedSaveFile.FileName);
                return true;
            }

            static void Postfix(LoadGame __instance)
            {
                string oldPath = $"{BetterSaves.GetSaveDirectory()}/{__instance.SelectedSaveFile.FileName}.btung";
                string newPath = $"{BetterSaves.GetSaveDirectory()}/{newName}.btung";
                if (File.Exists(oldPath))
                    File.Copy(oldPath, newPath);
                __instance.GenerateLoadGamesMenu();
            }
        }

        /// <summary>
        /// Deletion support
        /// </summary>
        [HarmonyPatch(typeof(LoadGame), "DeleteGame")]
        class DeletePatch
        {
            static void Postfix(LoadGame __instance)
            {
                if (__instance.SelectedSaveFile.FileName == null)
                    return;
                string fileName = $"{BetterSaves.GetSaveDirectory()}/{__instance.SelectedSaveFile.FileName}.btung";
                if (File.Exists(fileName))
                    File.Delete(fileName);
                __instance.GenerateLoadGamesMenu();
            }
        }
    }
}
