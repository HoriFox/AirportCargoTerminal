using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace at
{
    public class ControllerTerminal : MonoBehaviour
    {
        //public string[] location;
        public List<GameObject> currentLoadersFreeUnloading;
        public List<GameObject> currentLoadersFreeLoading;

        public float delayWagon = 30f;  // Время задержек перед приездом фуры.
        public float importedWeightMin = 2000f; // Минимальный привозимый вес фурой.
        public float importedWeightMax = 6000f; // Максимальный привозимый вес фурой.

        public float delayPlane = 30f;  // Время задержек перед прилётом ТЕСТОВОГО самолёта.

        public float timeLoadUnloadLoader = 4f; // Время загрузки/разгрузки подгрузчиком.

        // ТЕСТ СКОРОСТИ РАЗГРУЗКИ ПОГРУЗЧИКОМ.
        //public float delayLoader = 2f;
        public float maxLiftingWeight = 3000f;  // Максимальный вес поднимаемый погрузчиком.

        public Dictionary<int, GameObject> approachingTracks = new Dictionary<int, GameObject>();
        public Dictionary<int, GameObject> tracks = new Dictionary<int, GameObject>();

        public Dictionary<int, GameObject> approachingPlanes = new Dictionary<int, GameObject>();
        public Dictionary<int, GameObject> planes = new Dictionary<int, GameObject>();

        public Transform trackContainer;

        public Transform unloadingPoint;
        public Transform[] stockPoints;
        public Transform[] packagingPoints;
        public Transform warmStockPoint;
        public Transform coldStockPoint;
        public Transform loadingPoint;

        // ВЕСА (КГ)
        public float totalStockWeight = 0f;

        private float lastTimeWagon = 0f;
        private float lastTimePlane = 0f;

        private ResourceManager resourceM;
        private MapManager mapM;

        [Range(0.1f, 50f)]
        public float speedAnimation = 1f;

        // Счётчики
        public int trackArrivalCount = 0;
        public int trackLeftCount = 0;

        public void Start()
        {
            resourceM = ResourceManager.GetInstance();
            mapM = MapManager.GetInstance();
        }

        public void Update()
        {
            Time.timeScale = speedAnimation;

            if (lastTimeWagon <= Time.time)
            {
                lastTimeWagon = Time.time + delayWagon;
                int indexWagon = GetIndexWagon();
                GameObject prefabTrack = resourceM.GetPrefab("Фура");
                Vector3 positionTrack = prefabTrack.transform.localPosition;
                Quaternion rotationTrack = prefabTrack.transform.localRotation;
                GameObject track = Instantiate(prefabTrack, new Vector3(positionTrack.x - indexWagon * 3f, positionTrack.y, positionTrack.z), rotationTrack, trackContainer);
                approachingTracks.Add(indexWagon, track);

                float liftWagonWeight = Random.Range(importedWeightMin, importedWeightMax);
                TrackManager trackM = track.GetComponent<TrackManager>();
                trackM.indexParking = indexWagon;
                trackM.weight = liftWagonWeight;
                StartCoroutine(TruckArrival(track));
            }

            if (lastTimePlane <= Time.time)
            {
                lastTimePlane = Time.time + delayPlane;

                int indexPlane = GetIndexPlane();

                GameObject prefabPlane = (Random.Range(0, 100) > 20) ? resourceM.GetPrefab("A320") : resourceM.GetPrefab("A310");
                Vector3 positionPlane = prefabPlane.transform.localPosition;
                Quaternion rotationPlane = prefabPlane.transform.localRotation;
                GameObject plane = Instantiate(prefabPlane, new Vector3(positionPlane.x - indexPlane * 51f, positionPlane.y, positionPlane.z), rotationPlane, trackContainer);
                approachingPlanes.Add(indexPlane, plane);

                PlaneManager planeM = plane.GetComponent<PlaneManager>();
                planeM.indexParking = indexPlane;
                StartCoroutine(PlaneArrival(plane));
            }

            // Если есть свободные погрузчики РАЗГРУЗКИ
            if (currentLoadersFreeUnloading.Count > 0 && tracks.Count > 0)
            {   
                for (int i = 0; i < currentLoadersFreeUnloading.Count; i++)
                {
                    StartCoroutine(GoToUnloading(currentLoadersFreeUnloading[i]));
                    currentLoadersFreeUnloading.RemoveAt(i);
                }
            }

            // Если есть свободные погрузчики ЗАГРУЗКИ
            if (currentLoadersFreeLoading.Count > 0 && planes.Count > 0 && totalStockWeight > 0)
            {
                for (int i = 0; i < currentLoadersFreeLoading.Count; i++)
                {
                    StartCoroutine(GoToStockUnloading(currentLoadersFreeLoading[i]));
                    currentLoadersFreeLoading.RemoveAt(i);
                }
            }
        }

        IEnumerator GoToStockUnloading(GameObject loaderFree)
        {
            Transform loader = loaderFree.transform;
            LoaderManager loaderM = loaderFree.GetComponent<LoaderManager>();

            int randomPointStock = Random.Range(0, stockPoints.Length);
            Transform stockPoint = stockPoints[randomPointStock];
            List<Transform> way = mapM.GetWay(loaderM.currentPlace, stockPoint);

            foreach (var point in way)
            {
                float toX = point.position.x;
                float toZ = point.position.z;

                bool next = true;
                while (next)
                {
                    var lookPos = point.position - loader.position;
                    lookPos.y = 0;
                    Vector3 newDir = Vector3.RotateTowards(loader.forward, lookPos, Time.deltaTime * 15f, 0.0f);
                    loader.rotation = Quaternion.LookRotation(newDir);

                    Vector3 oldPosition = loader.position;
                    loader.position = Vector3.MoveTowards(oldPosition, new Vector3(toX, oldPosition.y, toZ), Time.deltaTime * 15f);

                    if (loader.position.x == point.position.x && loader.position.z == point.position.z) next = false;

                    yield return null;
                }
                loaderM.currentPlace = point;
            }

            //Ждём время загрузки.
            yield return new WaitForSeconds(timeLoadUnloadLoader);

            if (totalStockWeight > maxLiftingWeight)
            {
                totalStockWeight -= maxLiftingWeight;
                loaderM.currentWeight = maxLiftingWeight;
            }
            else
            {
                loaderM.currentWeight = totalStockWeight;
                totalStockWeight = 0;
            }

            StartCoroutine(GoToPackaging(loaderFree));

            yield break;
        }

        IEnumerator GoToPackaging(GameObject loaderFree)
        {
            Transform loader = loaderFree.transform;
            LoaderManager loaderM = loaderFree.GetComponent<LoaderManager>();

            int randomPointPackaging = Random.Range(0, packagingPoints.Length);
            Transform packagingPoint = packagingPoints[randomPointPackaging];
            List<Transform> way = mapM.GetWay(loaderM.currentPlace, packagingPoint);

            foreach (var point in way)
            {
                float toX = point.position.x;
                float toZ = point.position.z;

                bool next = true;
                while (next)
                {
                    var lookPos = point.position - loader.position;
                    lookPos.y = 0;
                    Vector3 newDir = Vector3.RotateTowards(loader.forward, lookPos, Time.deltaTime * 15f, 0.0f);
                    loader.rotation = Quaternion.LookRotation(newDir);

                    Vector3 oldPosition = loader.position;
                    loader.position = Vector3.MoveTowards(oldPosition, new Vector3(toX, oldPosition.y, toZ), Time.deltaTime * 15f);

                    if (loader.position.x == point.position.x && loader.position.z == point.position.z) next = false;

                    yield return null;
                }
                loaderM.currentPlace = point;
            }

            float weight = loaderM.currentWeight;

            //Ждём время разгрузки.
            yield return new WaitForSeconds(timeLoadUnloadLoader);

            loaderM.currentWeight = 0;

            //Ждём время пакетирования.
            yield return new WaitForSeconds(timeLoadUnloadLoader);

            //Ждём время загрузки.
            yield return new WaitForSeconds(timeLoadUnloadLoader);

            loaderM.currentWeight = weight;

            StartCoroutine(GoToLoading(loaderFree));

            yield break;
        }

        IEnumerator GoToLoading(GameObject loaderFree)
        {
            Transform loader = loaderFree.transform;
            LoaderManager loaderM = loaderFree.GetComponent<LoaderManager>();

            List<Transform> way = mapM.GetWay(loaderM.currentPlace, loadingPoint);

            foreach (var point in way)
            {
                float toX = point.position.x;
                float toZ = point.position.z;

                bool next = true;
                while (next)
                {
                    var lookPos = point.position - loader.position;
                    lookPos.y = 0;
                    Vector3 newDir = Vector3.RotateTowards(loader.forward, lookPos, Time.deltaTime * 15f, 0.0f);
                    loader.rotation = Quaternion.LookRotation(newDir);

                    Vector3 oldPosition = loader.position;
                    loader.position = Vector3.MoveTowards(oldPosition, new Vector3(toX, oldPosition.y, toZ), Time.deltaTime * 15f);

                    if (loader.position.x == point.position.x && loader.position.z == point.position.z) next = false;

                    yield return null;
                }
                loaderM.currentPlace = point;
            }

            //Ждём время разгрузки.
            yield return new WaitForSeconds(timeLoadUnloadLoader);

            int firstKey = GetFirstKey(planes);
            if (firstKey == -1)
            {
                currentLoadersFreeLoading.Add(loaderFree);
            }
            else
            {
                PlaneManager plane = planes[firstKey].GetComponent<PlaneManager>();
                //float weightTaken = plane.weight;

                if (plane.weight + loaderM.currentWeight > plane.maxWeight)
                {
                    StartCoroutine(PlaneLeft(planes[firstKey]));
                    planes.Remove(firstKey);
                    approachingPlanes.Remove(firstKey);
                    //Debug.Log("Фургон №" + firstKey + ": разгрузили последние " + weightTaken + " кг и фургон уехал");
                    //trackLeftCount++;

                    totalStockWeight += loaderM.currentWeight;
                }
                else
                {
                    //Debug.Log("Фургон №" + firstKey + ": разгрузил 3000 кг из " + weightTaken + " кг");
                    plane.weight += loaderM.currentWeight;
                }

                loaderM.currentWeight = 0;
                StartCoroutine(GoToStockUnloading(loaderFree));
            }

            yield break;
        }

        IEnumerator GoToUnloading(GameObject loaderFree)
        {
            Transform loader = loaderFree.transform;
            LoaderManager loaderM = loaderFree.GetComponent<LoaderManager>();

            List<Transform> way = mapM.GetWay(loaderM.currentPlace, unloadingPoint);

            foreach (var point in way)
            {
                float toX = point.position.x;
                float toZ = point.position.z;

                bool next = true;
                while (next)
                {
                    var lookPos = point.position - loader.position;
                    lookPos.y = 0;
                    Vector3 newDir = Vector3.RotateTowards(loader.forward, lookPos, Time.deltaTime * 15f, 0.0f);
                    loader.rotation = Quaternion.LookRotation(newDir);

                    Vector3 oldPosition = loader.position;
                    loader.position = Vector3.MoveTowards(oldPosition, new Vector3(toX, oldPosition.y, toZ), Time.deltaTime * 15f);

                    if (loader.position.x == point.position.x && loader.position.z == point.position.z) next = false;

                    yield return null;
                }
                loaderM.currentPlace = point;
            }

            //Ждём время разгрузки.
            yield return new WaitForSeconds(timeLoadUnloadLoader);

            int firstKey = GetFirstKey(tracks);
            if (firstKey == -1)
            {
                currentLoadersFreeUnloading.Add(loaderFree);
            }
            else
            {
                TrackManager track = tracks[firstKey].GetComponent<TrackManager>();
                float weightTaken = track.weight;

                if (track.weight - maxLiftingWeight < 0)
                {
                    loaderM.currentWeight = track.weight;
                    StartCoroutine(TruckLeft(tracks[firstKey]));
                    tracks.Remove(firstKey);
                    approachingTracks.Remove(firstKey);
                    Debug.Log("Фургон №" + firstKey + ": разгрузили последние " + weightTaken + " кг и фургон уехал");
                    trackLeftCount++;

                    StartCoroutine(GoToStockLoading(loaderFree, false));
                }
                else
                {
                    Debug.Log("Фургон №" + firstKey + ": разгрузил 3000 кг из " + weightTaken + " кг");
                    loaderM.currentWeight = maxLiftingWeight;
                    track.weight -= maxLiftingWeight;

                    StartCoroutine(GoToStockLoading(loaderFree, true));
                }
            }

            // Финал, фура подъехала.
            yield break;
        }

        // Погрузчики разгружают груз из фур на склад.
        IEnumerator GoToStockLoading(GameObject loaderFree, bool roundtrip)
        {
            Transform loader = loaderFree.transform;
            LoaderManager loaderM = loaderFree.GetComponent<LoaderManager>();

            int randomPointStock = Random.Range(0, stockPoints.Length);
            Transform stockPoint = stockPoints[randomPointStock];
            List<Transform> way = mapM.GetWay(loaderM.currentPlace, stockPoint);

            foreach (var point in way)
            {
                float toX = point.position.x;
                float toZ = point.position.z;

                bool next = true;
                while (next)
                {
                    var lookPos = point.position - loader.position;
                    lookPos.y = 0;
                    Vector3 newDir = Vector3.RotateTowards(loader.forward, lookPos, Time.deltaTime * 15f, 0.0f);
                    loader.rotation = Quaternion.LookRotation(newDir);

                    Vector3 oldPosition = loader.position;
                    loader.position = Vector3.MoveTowards(oldPosition, new Vector3(toX, oldPosition.y, toZ), Time.deltaTime * 15f);

                    if (loader.position.x == point.position.x && loader.position.z == point.position.z) next = false;

                    yield return null;
                }
                loaderM.currentPlace = point;
            }

            //Ждём время разгрузки.
            yield return new WaitForSeconds(timeLoadUnloadLoader);

            totalStockWeight += loaderM.currentWeight;
            loaderM.currentWeight = 0;

            if (roundtrip)
            {
                StartCoroutine(GoToUnloading(loaderFree));
            }
            else
            {
                currentLoadersFreeUnloading.Add(loaderFree);
            }

            // Финал, фура подъехала.
            yield break;
        }

        IEnumerator TruckArrival(GameObject track)
        {
            TrackManager trackM = track.GetComponent<TrackManager>();
            float toZ = 31.5f;

            while (track.transform.localPosition.z > toZ)
            {
                Vector3 oldPosition = track.transform.localPosition;
                float newZPos = oldPosition.z - Time.deltaTime * 15f;
                if (newZPos < toZ) newZPos = toZ;
                Vector3 newPosition = new Vector3(oldPosition.x, oldPosition.y, newZPos);
                track.transform.localPosition = newPosition;

                yield return null;
            }

            tracks.Add(trackM.indexParking, track);

            Debug.Log("Фургон №" + trackM.indexParking + ": приехал (" + trackM.weight + " кг)");
            trackArrivalCount++;
            // Финал, фура подъехала.
            yield break;
        }

        IEnumerator PlaneArrival(GameObject plane)
        {
            PlaneManager planeM = plane.GetComponent<PlaneManager>();
            float toZ = -58;

            while (plane.transform.localPosition.z < toZ)
            {
                Vector3 oldPosition = plane.transform.localPosition;
                float newZPos = oldPosition.z + Time.deltaTime * 5f;
                if (newZPos > toZ) newZPos = toZ;
                Vector3 newPosition = new Vector3(oldPosition.x, oldPosition.y, newZPos);
                plane.transform.localPosition = newPosition;

                yield return null;
            }

            planes.Add(planeM.indexParking, plane);

            Debug.Log("Самолёт №" + planeM.indexParking + ": прилетел");
            //trackArrivalCount++;
            yield break;
        }

        IEnumerator TruckLeft(GameObject track)
        {
            float toZ = 80f;

            while (track.transform.localPosition.z < toZ)
            {
                Vector3 oldPosition = track.transform.localPosition;
                float newZPos = oldPosition.z + Time.deltaTime * 15f;
                if (newZPos > toZ) newZPos = toZ;
                Vector3 newPosition = new Vector3(oldPosition.x, oldPosition.y, newZPos);
                track.transform.localPosition = newPosition;

                yield return null;
            }

            // Финал, фура отъехала.
            Destroy(track);
            yield break;
        }

        IEnumerator PlaneLeft(GameObject plane)
        {
            float toZ = -100f;

            while (plane.transform.localPosition.z > toZ)
            {
                Vector3 oldPosition = plane.transform.localPosition;
                float newZPos = oldPosition.z - Time.deltaTime * 10f;
                if (newZPos < toZ) newZPos = toZ;
                Vector3 newPosition = new Vector3(oldPosition.x, oldPosition.y, newZPos);
                plane.transform.localPosition = newPosition;

                yield return null;
            }

            Destroy(plane);
            yield break;
        }

        // Даём самый минимальный индекс из возможных.
        public int GetIndexWagon()
        {
            int i = 0;
            while (approachingTracks.ContainsKey(i))
            {
                i++;
            }
            return i;
        }

        // Даём самый минимальный индекс из возможных.
        public int GetIndexPlane()
        {
            int i = 0;
            while (approachingPlanes.ContainsKey(i))
            {
                i++;
            }
            return i;
        }

        public int GetFirstKey(Dictionary<int, GameObject> dictionary)
        {
            int firstKey = -1;
            if (dictionary.Count > 0)
            {
                foreach (int key in dictionary.Keys)
                {
                    firstKey = key;
                    break;
                }
            }
            return firstKey;
        }

    }
}
