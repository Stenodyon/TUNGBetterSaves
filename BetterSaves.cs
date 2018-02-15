using System;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.Diagnostics;
using System.Collections;
using System.Reflection;
using System.IO;
using PiTung;
using PiTung.Console;
using Harmony;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityStandardAssets.Characters.FirstPerson;

namespace BetterSaves
{
    public class BetterSaves : Mod
    {
        private static readonly Version Version = Assembly.GetExecutingAssembly().GetName().Version;

        public override string Name => "BetterSaves";
        public override string Author => "Stenodyon";
        public override Version ModVersion => Version;
        public override Version FrameworkVersion => new Version(1, 6, 0, PiTUNG.FrameworkVersion.Revision);
        public override string PackageName => "me.stenodyon.BetterSaves";

        /// <summary>
        /// If set to true, next save will not use the improved format.
        /// Useful for saving to the regular file for sharing with vanilla TUNG
        /// </summary>
        private static bool legacySave = false;

        /// <summary>
        /// Number of instanciations per frame
        /// </summary>
        private static int instancesPerFrame = 50;

        /// <summary>
        /// True if currently loading
        /// </summary>
        private static bool loading = false;

        /// <summary>
        /// Loading progress in object instances
        /// </summary>
        private static int progress = 0;

        /// <summary>
        /// Amount of objects to instanciate
        /// </summary>
        private static int maxProgress = 1;

        /// <summary>
        /// Color of the progress bar background
        /// </summary>
        private static readonly Color bgColor = new Color(0f, 0f, 0f, 0.5f);

        /// <summary>
        /// Color of the progress bar
        /// </summary>
        private static readonly Color barColor = new Color(0f, 1f, 1f);

        public override void AfterPatch()
        {
            IGConsole.RegisterCommand<Command_normalsave>(this);
            IGConsole.Log($"BetterSaves v{Version.ToString()} initialized");
        }

        public override void OnGUI()
        {
            if (loading)
                DrawProgressBar();
        }

        public static void DrawProgressBar()
        {
            int barWidth = Screen.width / 2;
            int barHeight = Screen.height / 10;
            int barX = Screen.width / 2 - barWidth / 2;
            int barY = Screen.height / 2 - barHeight / 2;
            int progressWidth = barWidth * progress / maxProgress;
            ModUtilities.Graphics.DrawRect(new Rect(barX, barY, barWidth, barHeight), bgColor);
            ModUtilities.Graphics.DrawRect(new Rect(barX, barY, progressWidth, barHeight), barColor);
        }

        public static string GetSaveDirectory()
        {
            return Application.persistentDataPath + "/saves";
        }

        /// <summary>
        /// Get the save path of the improved save file
        /// </summary>
        /// <returns>Path of the improved save file</returns>
        public static string GetSavePath()
        {
            return Application.persistentDataPath + "/saves/" + SaveManager.SaveName + ".btung";
        }

        public static string GetVanillaSave()
        {
            return Application.persistentDataPath + "/saves/" + SaveManager.SaveName + ".tung";
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

        /// <summary>
        /// Gathers all objects to save into serializable datums
        /// </summary>
        /// <returns>The serializable structures</returns>
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

        /// <summary>
        /// Load coroutine, yields every now and then to delay
        /// loading
        /// </summary>
        private static IEnumerator LoadCoroutine()
        {
            loading = true;
            Stopwatch watch = new Stopwatch();
            watch.Start();
#if DEBUG
            IGConsole.Log($"Loading better save {GetSavePath()}");
#endif

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
#if DEBUG
            IGConsole.Log($"Length of data {data.Length}");
#endif
            foreach (SaveThisObject obj in UnityEngine.Object.FindObjectsOfType<SaveThisObject>())
                UnityEngine.Object.Destroy(obj.gameObject);
            BehaviorManager.AllowedToUpdate = false;
            MegaBoardMeshManager.MegaBoardMeshesOfColor.Clear();
            SetPlayerPosition(player);
            int size = data.Length;
            maxProgress = size;
            for (int index = 0; index < size; index++)
            {
                Loader.Instantiate(data[index]);
                if ((index + 1) % instancesPerFrame == 0)
                {
                    progress = index;
                    yield return new WaitForEndOfFrame();
                }
            }
            SaveManager.RecalculateAllClustersEverywhereWithDelay();
            MegaMesh.GenerateNewMegaMesh();
            MegaBoardMeshManager.GenerateAllMegaBoardMeshes();
            watch.Stop();
            loading = false;
            if (UIManager.SomeOtherMenuIsOpen)
                SaveManager.SaveAll();
            IGConsole.Log($"Loaded save in {watch.Elapsed.ToString()}");
        }

        /// <summary>
        /// Load function replacement
        /// </summary>
        [HarmonyPatch(typeof(SaveManager), "LoadAll")]
        class LoadAllPatch
        {
            private static Stopwatch watch;
            private static bool betterSave = false;

            static bool Prefix()
            {
                watch = new Stopwatch();
                watch.Start();
                if (File.Exists(GetSavePath())) // It's a better save
                {
                    betterSave = true;
                    DummyComponent comp = UnityEngine.Object.FindObjectOfType<DummyComponent>();
                    comp.StartCoroutine(LoadCoroutine());
                    return false;
                }
                betterSave = false;
                return true;
            }

            static void Postfix()
            {
                if (watch.IsRunning)
                    watch.Stop();
                if (!betterSave)
                    IGConsole.Log($"Loaded save in {watch.Elapsed.ToString()}");
            }
        }

        /// <summary>
        /// Save function replacement
        /// </summary>
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
                if (!File.Exists(GetVanillaSave()))
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

        /// <summary>
        /// Prevents exiting before loading finished
        /// </summary>
        [HarmonyPatch(typeof(RunPauseMenu), "QuitToDesktop")]
        class NoExitPatch
        {
            static bool Prefix()
            {
                return !loading; // If loading, do not exit
            }
        }

        /// <summary>
        /// Prevents exiting before loading finished
        /// </summary>
        [HarmonyPatch(typeof(RunPauseMenu), "QuitToMainMenu")]
        class NoExitPatch2
        {
            static bool Prefix()
            {
                return !loading; // Really do not exit
            }
        }

        // ####################
        //  Commands
        // ####################

        /// <summary>
        /// Vanilla save command
        /// </summary>
        private class Command_normalsave : Command
        {
            public override string Name => "normalsave";
            public override string Usage => $"{Name}";
            public override string Description => "Unmodified save compatible with vanilla TUNG";

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
    }

    internal class Utils
    {
        public static readonly GameObject[] prefabs = new GameObject[]
        {
            SaveObjectsList.CircuitBoard,
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
    }
}
