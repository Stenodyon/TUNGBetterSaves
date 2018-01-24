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
                    return new BoardDatum {
                        width = comp.x, height = comp.z,
                        children = children.ToArray(),
                        color = renderer.material.color
                    };
            }
            return null;
        }
    }
}
