using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BetterSaves
{
    public class Converter
    {
        public static Datum Convert(SaveThisObject obj)
        {
            Datum result = null;
            switch(obj.ObjectType)
            {
                case "CircuitBoard":
                    List<Datum> children = new List<Datum>(obj.transform.childCount);
                    CircuitBoard comp = obj.GetComponent<CircuitBoard>();
                    foreach(Transform child in obj.transform)
                    {
                        SaveThisObject save = child.GetComponent<SaveThisObject>();
                        if (save != null)
                            children.Add(Convert(save));
                    }
                    Renderer renderer = obj.GetComponent<Renderer>();
                    result = new BoardDatum
                    {
                        width = comp.x, height = comp.z,
                        children = children.ToArray(),
                        color = renderer.material.color
                    };
                    break;
                case "Wire":
                    result = new WireDatum
                    {
                        isInputInput = obj.GetComponent<InputInputConnection>() != null,
                        localScale = obj.transform.localScale
                    };
                    break;
                case "Inverter":
                    NotGate gate = obj.GetComponent<NotGate>();
                    result = new InverterDatum
                    {
                        inputOn = gate.Input.On,
                        outputOn = gate.Output.On
                    };
                    break;
                case "Peg":
                    result = new PegDatum
                    {
                        isOn = obj.GetComponent<CircuitInput>().On
                    };
                    break;
                case "Delayer":
                    Delayer delayer = obj.GetComponent<Delayer>();
                    result = new DelayerDatum
                    {
                        inputOn = delayer.Input.On,
                        outputOn = delayer.Output.On,
                        delayCount = delayer.DelayCount
                    };
                    break;
                case "Through Peg":
                    CircuitInput[] inputs = obj.GetComponentsInChildren<CircuitInput>();
                    result = new ThroughPegDatum
                    {
                        isOn = inputs[0].On
                    };
                    break;
                case "Switch":
                case "Panel Switch":
                    result = new SwitchDatum
                    {
                        isOn = obj.GetComponent<Switch>().On
                    };
                    break;
                case "Button":
                case "Panel Button":
                    Button button = obj.GetComponent<Button>();
                    result = new ButtonDatum
                    {
                        isOn = button.output.On,
                        downTime = button.ButtonDownTime
                    };
                    break;
                case "Display":
                case "Panel Display":
                    result = new DisplayDatum
                    {
                        isOn = obj.GetComponent<global::Display>().Input.On
                    };
                    break;
                case "Label":
                case "Panel Label":
                    Label label = obj.GetComponent<Label>();
                    result = new LabelDatum
                    {
                        text = label.text.text,
                        fontSize = label.text.fontSize
                    };
                    break;
                case "Blotter":
                case "Through Blotter":
                    Blotter blotter = obj.GetComponent<Blotter>();
                    result = new BlotterDatum
                    {
                        inputOn = blotter.Input.On,
                        outputON = blotter.Output.On
                    };
                    break;
            }
            result.localPosition = obj.transform.localPosition;
            result.localAngles = obj.transform.localEulerAngles;
            return result;
        }
    }
}
