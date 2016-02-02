using UnityEngine;
using System.Collections;

public class HouseManager : MonoBehaviour {
	
	public static HouseManager singleton;
	
	public static int remainingHouses = 32;
	
	public GameObject house1Prefab;
	public GameObject house2Prefab;
	public GameObject house3Prefab;
	public GameObject house4Prefab;
	public GameObject hotelPrefab;
	
	void Awake () {
		singleton = this;
		remainingHouses = 32;
	}
	
	void OnDestroy () {
		singleton = null;
	}
	
}
