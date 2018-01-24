using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using PiTung_Bootstrap;
using PiTung_Bootstrap.Console;
using Harmony;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityStandardAssets.Characters.FirstPerson;
using System.Runtime.Serialization.Formatters.Binary;
using System.Diagnostics;
using System.Collections;

namespace BetterSaves
{
    public class BetterSaves : Mod
    {
        public override string Name => "BetterSaves";
        public override string Author => "Stenodyon";
        public override Version ModVersion => new Version(0, 1, 0);
        public override Version FrameworkVersion => PiTung.FrameworkVersion;

        private static bool legacySave = false;
        private static bool init = false;
        private static int instancesPerFrame = 50;

        public override void LodingWorld(string worldName)
        {
            if(!init)
            {
                IGConsole.RegisterCommand<Command_normalsave>();
                IGConsole.RegisterCommand<Command_lscol>();
                init = true;
            }
        }

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

        private static IEnumerator LoadCoroutine()
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();
            IGConsole.Log("Loading better save");

            FileStream fs = new FileStream(GetSavePath(), FileMode.Open);
            BinaryFormatter formatter = new BinaryFormatter();

            PlayerPosition player;
            Datum[] data;
            try
            {
                player = formatter.Deserialize(fs) as PlayerPosition;
                data = formatter.Deserialize(fs) as Datum[];
            }
            catch (Exception e)
            {
                IGConsole.Error($"Loading failed with {e.ToString()}");
                yield break;
            }
            finally
            {
                fs.Close();
            }
            foreach (SaveThisObject obj in UnityEngine.Object.FindObjectsOfType<SaveThisObject>())
                UnityEngine.Object.Destroy(obj.gameObject);
            BehaviorManager.AllowedToUpdate = false;
            MegaBoardMeshManager.MegaBoardMeshesOfColor.Clear();
            SetPlayerPosition(player);
            int size = data.Length;
            for (int index = 0; index < size; index++)
            {
                Loader.Instantiate(data[index]);
                if ((index + 1) % instancesPerFrame == 0)
                {
                    yield return new WaitForEndOfFrame();
                }
            }
            SaveManager.RecalculateAllClustersEverywhereWithDelay();
            MegaMesh.GenerateNewMegaMesh();
            MegaBoardMeshManager.GenerateAllMegaBoardMeshes();
            watch.Stop();
            IGConsole.Log($"Loaded save in {watch.Elapsed.ToString()}");
        }

        [HarmonyPatch(typeof(SaveManager), "LoadAll")]
        class LoadAllPatch
        {
            private static Stopwatch watch;

            static bool Prefix()
            {
                watch = new Stopwatch();
                watch.Start();
                if(File.Exists(GetSavePath())) // It's a better save
                {
                    DummyComponent comp = UnityEngine.Object.FindObjectOfType<DummyComponent>();
                    comp.StartCoroutine(LoadCoroutine());
                    return false;
                }
                return true;
            }

            static void Postfix()
            {
                if(watch.IsRunning)
                    watch.Stop();
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
                watch.Start();
                if (legacySave)
                    return true;
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
                watch.Stop();
                IGConsole.Log($"Saved game in {watch.Elapsed.ToString()}");
                legacySave = false;
            }
        }

        private class Command_normalsave : Command
        {
            public override string Name => "normalsave";
            public override string Usage => $"{Name}";

            public override bool Execute(IEnumerable<string> arguments)
            {
                if (SceneManager.GetActiveScene().name != "gameplay")
                {
                    IGConsole.Error("Not currently in a world");
                    return false;
                }
                legacySave = true;
                SaveManager.SaveAll();
                return true;
            }
        }

        private class Command_lscol : Command
        {
            public override string Name => "lscol";
            public override string Usage => $"{Name}";

            public override bool Execute(IEnumerable<string> arguments)
            {
                GameObject[] prefabs = new GameObject[]
                {
                    SaveObjectsList.Wire,
                    SaveObjectsList.Inverter,
                    SaveObjectsList.Peg,
                    SaveObjectsList.Delayer,
                    SaveObjectsList.ThroughPeg,
                    SaveObjectsList.Switch,
                    SaveObjectsList.Button,
                    SaveObjectsList.Display,
                    SaveObjectsList.Label,
                    SaveObjectsList.PanelSwitch,
                    SaveObjectsList.PanelButton,
                    SaveObjectsList.PanelDisplay,
                    SaveObjectsList.PanelLabel,
                    SaveObjectsList.Blotter,
                    SaveObjectsList.ThroughBlotter
                };
                foreach(var obj in prefabs)
                {
                    //do stuff
                }
                return true;
            }
        }
    }
}
