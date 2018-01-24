using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using PiTung_Bootstrap;
using PiTung_Bootstrap.Console;
using Harmony;
using UnityEngine;

namespace BetterSaves
{
    public class BetterSaves : Mod
    {
        public override string Name => "BetterSaves";
        public override string Author => "Stenodyon";
        public override Version ModVersion => new Version(0, 1, 0);
        public override Version FrameworkVersion => PiTung.FrameworkVersion;

        /*
        [HarmonyPatch(typeof(IGConsole), "Init")]
        class ConsoleInitPatch
        {
            static void Postfix()
            {
                // Register new commands here
            }
        }
        */

        public static string GetSavePath()
        {
            return Application.persistentDataPath + "/saves/" + SaveManager.SaveName + ".btung";
        }

        [HarmonyPatch(typeof(SaveManager), "LoadAll")]
        class LoadAllPatch
        {
            static bool Prefix()
            {
                if(ES3.KeyExists("BetterSave")) // It's a better save
                {
                    return false;
                }
                return true;
            }
        }


        [HarmonyPatch(typeof(SaveManager), "SaveAll")]
        class SaveAllPatch
        {
            static bool Prefix()
            {
                IGConsole.Log(GetSavePath());
                return true;
            }
        }
    }
}
