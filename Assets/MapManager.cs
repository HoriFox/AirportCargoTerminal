using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace at
{
    public class MapManager : MonoBehaviour
    {
        public static MapManager Instance;

        public Transform[] point1;
        public Transform[] point2;
        public Transform[] point3;
        public Transform[] point4;
        public Transform[] point5;
        public Transform[] point6;
        public Transform[] point7;
        public Transform[] point8;
        public Transform[] point9;
        public Transform[] point10;
        public Transform[] point11;
        public Transform[] point12;
        public Transform[] point13;

        public Transform[] parkingPoint1;
        public Transform[] parkingPoint2;
        public Transform[] parkingPoint3;
        public Transform[] parkingPoint4;
        public Transform[] parkingPoint5;
        public Transform[] parkingPoint6;
        public Transform[] parkingPoint7;
        public Transform[] parkingPoint8;
        public Transform[] parkingPoint9;
        public Transform[] parkingPoint10;

        public Transform[] pointAll;

        public Dictionary<Transform, Transform[]> mapWays = new Dictionary<Transform, Transform[]>();

        private void Awake()
        {
            Instance = this;
        }
        public static MapManager GetInstance()
        {
            return Instance;
        }

        public void Start()
        {
            mapWays.Add(pointAll[0], point1);
            mapWays.Add(pointAll[1], point2);
            mapWays.Add(pointAll[2], point3);
            mapWays.Add(pointAll[3], point4);
            mapWays.Add(pointAll[4], point5);
            mapWays.Add(pointAll[5], point6);
            mapWays.Add(pointAll[6], point7);
            mapWays.Add(pointAll[7], point8);
            mapWays.Add(pointAll[8], point9);
            mapWays.Add(pointAll[9], point10);
            mapWays.Add(pointAll[10], point11);
            mapWays.Add(pointAll[11], point12);
            mapWays.Add(pointAll[12], point13);

            mapWays.Add(pointAll[13], parkingPoint1);
            mapWays.Add(pointAll[14], parkingPoint2);
            mapWays.Add(pointAll[15], parkingPoint3);
            mapWays.Add(pointAll[16], parkingPoint4);
            mapWays.Add(pointAll[17], parkingPoint5);
            mapWays.Add(pointAll[18], parkingPoint6);
            mapWays.Add(pointAll[19], parkingPoint7);
            mapWays.Add(pointAll[20], parkingPoint8);
            mapWays.Add(pointAll[21], parkingPoint9);
            mapWays.Add(pointAll[22], parkingPoint10);
        }

        public List<Transform> GetWay(Transform currentPlace, Transform destination)
        {
            List<Transform> techWay = new List<Transform>();
            NextWay(null, currentPlace, destination, ref techWay);
            techWay.Reverse();

            return techWay;
        }

        private bool NextWay(Transform lastPos, Transform currentPlace, Transform destination, ref List<Transform> techWay)
        {
            if (currentPlace.name == destination.name)
            {
                techWay.Add(currentPlace);
                return true;
            }
            else
            {
                foreach (var part in mapWays[currentPlace])
                {
                    // Чтобы не идти назад.
                    if (lastPos == null || (lastPos != null && lastPos.name != part.name))
                    {
                        if (NextWay(currentPlace, part, destination, ref techWay))
                        {
                            techWay.Add(currentPlace);
                            return true;
                        }
                    }
                }
                return false;
            }
        }
    }
}
