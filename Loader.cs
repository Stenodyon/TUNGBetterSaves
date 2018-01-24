using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using PiTung_Bootstrap.Console;

namespace BetterSaves
{
    public class Loader
    {
        private static string ObjectType(Datum datum)
        {
            string result = "";
            var dict = new Dictionary<Type, Action>
            {
                { typeof(BoardDatum), () => result = "CircuitBoard" },
                { typeof(WireDatum), () => result = "Wire" },
                { typeof(InverterDatum), () => result = "Inverter" },
                { typeof(PegDatum), () => result = "Peg" },
                { typeof(DelayerDatum), () => result = "Delayer" },
                { typeof(ThroughPegDatum), () => result = "Through Peg" },
                { typeof(SwitchDatum), () => result = ((SwitchDatum)datum).panel ? "Panel Switch" : "Switch" },
                { typeof(ButtonDatum), () => result = ((ButtonDatum)datum).panel ? "Panel Button" : "Button" },
                { typeof(DisplayDatum), () => result = ((DisplayDatum)datum).panel ? "Panel Display" : "Display" },
                { typeof(LabelDatum), () => result = ((LabelDatum)datum).panel ? "Panel Label" : "Label" },
                { typeof(BlotterDatum), () => result = ((BlotterDatum)datum).through ? "Through Blotter" : "Blotter" },
            };
            dict[datum.GetType()]();
            return result;
        }

        private static GameObject UnityInstantiate(string type, Transform parent = null)
        {
            return UnityEngine.Object.Instantiate(SaveObjectsList.ObjectTypeToPrefab(type), parent);
        }

        public static void Instantiate(Datum data, Transform parent = null)
        {
            string objType = ObjectType(data);
            GameObject obj = UnityInstantiate(objType, parent);
            SaveThisObject save = obj.AddComponent<SaveThisObject>();
            save.ObjectType = objType;
            save.LocalPosition = data.localPosition;
            save.LocalEulerAngles = data.localAngles;
            save.transform.localPosition = save.LocalPosition;
            save.transform.localEulerAngles = save.LocalEulerAngles;
            var dict = new Dictionary<Type, Action>
            {
                { typeof(BoardDatum), () => OnBoard((BoardDatum)data, save, parent) },
                { typeof(WireDatum), () => OnWire((WireDatum)data, save) },
                { typeof(InverterDatum), () => OnInverter((InverterDatum)data, save) },
                { typeof(PegDatum), () => OnPeg((PegDatum)data, save) },
                { typeof(DelayerDatum), () => OnDelayer((DelayerDatum)data, save) },
                { typeof(ThroughPegDatum), () => OnThroughPeg((ThroughPegDatum)data, save) },
                { typeof(SwitchDatum), () => OnSwitch((SwitchDatum)data, save) },
                { typeof(ButtonDatum), () => OnButton((ButtonDatum)data, save) },
                { typeof(DisplayDatum), () => OnDisplay((DisplayDatum)data, save) },
                { typeof(LabelDatum), () => OnLabel((LabelDatum)data, save) },
                { typeof(BlotterDatum), () => OnBlotter((BlotterDatum)data, save) },
            };
            dict[data.GetType()]();
        }

        private static void OnBoard(BoardDatum datum, SaveThisObject save, Transform parent)
        {
            save.CustomDataArray = new object[] { datum.width, datum.height, (Color)datum.color };
            CustomData.LoadCircuitBoard(save);
            foreach (Datum child in datum.children)
                Instantiate(child, save.transform);
        }

        private static void OnWire(WireDatum datum, SaveThisObject save)
        {
            save.CustomDataArray = new object[] { datum.isInputInput, (Vector3)datum.localScale };
            CustomData.LoadWire(save);
        }

        private static void OnInverter(InverterDatum datum, SaveThisObject save)
        {
            save.CustomDataArray = new object[] { datum.inputOn, datum.outputOn };
            CustomData.LoadInverter(save);
        }

        private static void OnPeg(PegDatum datum, SaveThisObject save)
        {
            save.CustomDataArray = new object[] { datum.isOn };
            CustomData.LoadPeg(save);
        }

        private static void OnDelayer(DelayerDatum datum, SaveThisObject save)
        {
            save.CustomDataArray = new object[] { datum.inputOn, datum.outputOn, datum.delayCount };
            CustomData.LoadDelayer(save);
        }

        private static void OnThroughPeg(ThroughPegDatum datum, SaveThisObject save)
        {
            save.CustomDataArray = new object[] { datum.isOn };
            CustomData.LoadThroughPeg(save);
        }

        private static void OnSwitch(SwitchDatum datum, SaveThisObject save)
        {
            save.CustomDataArray = new object[] { datum.isOn };
            CustomData.LoadSwitch(save);
        }

        private static void OnButton(ButtonDatum datum, SaveThisObject save)
        {
            save.CustomDataArray = new object[] { datum.isOn, datum.downTime };
            CustomData.LoadButton(save);
        }

        private static void OnDisplay(DisplayDatum datum, SaveThisObject save)
        {
            save.CustomDataArray = new object[] { datum.isOn };
            CustomData.LoadDisplay(save);
        }

        private static void OnLabel(LabelDatum datum, SaveThisObject save)
        {
            save.CustomDataArray = new object[] { datum.text, datum.fontSize };
            CustomData.LoadLabel(save);
        }

        private static void OnBlotter(BlotterDatum datum, SaveThisObject save)
        {
            save.CustomDataArray = new object[] { datum.inputOn, datum.outputON };
            CustomData.LoadBlotter(save);
        }
    }
}
