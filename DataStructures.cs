using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BetterSaves
{
    [Serializable]
    public class v2
    {
        public float x, y;
        public v2(float x, float y) { this.x = x; this.y = y; }
        public static implicit operator Vector2(v2 o) { return new Vector2(o.x, o.y); }
        public static implicit operator v2(Vector2 o) { return new v2(o.x, o.y); }
    }

    [Serializable]
    public class v3
    {
        public float x, y, z;
        public v3(float x, float y, float z) { this.x = x; this.y = y; this.z = z; }
        public static implicit operator Vector3(v3 o) { return new Vector3(o.x, o.y, o.z); }
        public static implicit operator v3(Vector3 o) { return new v3(o.x, o.y, o.z); }
    }

    [Serializable]
    public class color
    {
        float r, g, b, a;
        public color(float r, float g, float b, float a) { this.r = r; this.g = g; this.b = b; this.a = a; }
        public static implicit operator Color(color o) { return new Color(o.r, o.g, o.b, o.a); }
        public static implicit operator color(Color o) { return new color(o.r, o.g, o.b, o.a); }
    }

    [Serializable]
    public class PlayerPosition
    {
        public v3 pos;
        public v2 angles;
    }

    [Serializable]
    public class Datum
    {
        public v3 localPosition;
        public v3 localAngles;
    }

    [Serializable]
    public class BoardDatum : Datum
    {
        public Datum[] children;
        public int width, height;
        public color color;
    }

    [Serializable]
    public class WireDatum : Datum
    {
        public bool isInputInput;
        public v3 localScale;
    }

    [Serializable]
    public class InverterDatum : Datum
    {
        public bool inputOn, outputOn;
    }

    [Serializable]
    public class PegDatum : Datum
    {
        public bool isOn;
    }

    [Serializable]
    public class DelayerDatum : Datum
    {
        public bool inputOn, outputOn;
        public int delayCount;
    }

    [Serializable]
    public class ThroughPegDatum : Datum
    {
        public bool isOn;
    }

    [Serializable]
    public class SwitchDatum : Datum
    {
        public bool isOn;
    }

    [Serializable]
    public class ButtonDatum : Datum
    {
        public bool isOn;
        public int downTime;
    }

    [Serializable]
    public class DisplayDatum : Datum
    {
        public bool isOn;
    }

    [Serializable]
    public class LabelDatum : Datum
    {
        public string text;
        public float fontSize;
    }

    [Serializable]
    public class BlotterDatum : Datum
    {
        public bool inputOn, outputON;
    }

}
