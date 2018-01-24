using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using PiTung_Bootstrap;
using PiTung_Bootstrap.Console;
using Harmony;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;
using System.Runtime.Serialization.Formatters.Binary;
using System.Diagnostics;

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

        public static PlayerPosition GetPlayerPosition()
        {
            Transform player = FirstPersonController.Instance.transform;
            Transform camera = player.GetChild(0);
            return new PlayerPosition
            {
                pos = player.position,
                angles = new v2(camera.localEulerAngles.x, camera.localEulerAngles.y)
            };
        }

        public static void SetPlayerPosition(PlayerPosition pos)
        {
            Transform player = FirstPersonController.Instance.transform;
            player.position = pos.pos;
            FirstPersonController.Instance.m_MouseLook.m_CharacterTargetRot = Quaternion.Euler(0f, pos.angles.y, 0f);
            FirstPersonController.Instance.m_MouseLook.m_CameraTargetRot = Quaternion.Euler(pos.angles.x, 0f, 0f);
        }

        public static void SavePlayerPosition()
        {
        }

        public static void LoadPlayerPosition()
        {
        }

        public static Datum[] GetDatumsToSave()
        {
            List<Datum> objs = new List<Datum>();
            foreach (SaveThisObject save in SaveManager.SaveObjects)
            {
                if (save.transform.parent == null)
                    objs.Add(Converter.Convert(save));
            }
            return objs.ToArray();
        }

        [HarmonyPatch(typeof(SaveManager), "LoadAll")]
        class LoadAllPatch
        {
            private static Stopwatch watch;

            static bool Prefix()
            {
                watch = new Stopwatch();
                if(File.Exists(GetSavePath())) // It's a better save
                {
                    IGConsole.Log("Loading better save");
                    bool result = false;
                    FileStream fs = new FileStream(GetSavePath(), FileMode.Open);
                    BinaryFormatter formatter = new BinaryFormatter();
                    try
                    {
                        PlayerPosition player = formatter.Deserialize(fs) as PlayerPosition;
                        Datum[] data = formatter.Deserialize(fs) as Datum[];
                        SetPlayerPosition(player);
                    }
                    catch (Exception e)
                    {
                        IGConsole.Error($"Loading failed with {e.ToString()}");
                        result = true;
                    }
                    finally
                    {
                        fs.Close();
                    }
                    return result;
                }
                return true;
            }

            static void Postfix()
            {
                IGConsole.Log($"Loaded save in {watch.Elapsed.ToString()}");
            }
        }

        [HarmonyPatch(typeof(SaveManager), "SaveAll")]
        class SaveAllPatch
        {
            private static Stopwatch watch;

            static bool Prefix()
            {
                watch = new Stopwatch();
                PlayerPosition player = GetPlayerPosition();
                Datum[] data = GetDatumsToSave();
                FileStream fs = new FileStream(GetSavePath(), FileMode.Create);
                BinaryFormatter formatter = new BinaryFormatter();
                try
                {
                    formatter.Serialize(fs, player);
                    formatter.Serialize(fs, data);
                }
                catch (Exception e)
                {
                    IGConsole.Error($"Saving failed with {e.ToString()}");
                }
                finally
                {
                    fs.Close();
                }
                return false;
            }

            static void Postfix()
            {
                IGConsole.Log($"Saved game in {watch.Elapsed.ToString()}");
            }
        }
    }
}
