using UnityEngine;
using System.Collections;

public class Unity2DSortingLayer : MonoBehaviour {
	public string sortingLayerName = "Front";
	public int sortingOrder = 0;
	
	void Awake () {
		GetComponent<Renderer>().sortingLayerName = sortingLayerName;
		GetComponent<Renderer>().sortingOrder = sortingOrder;
    }
}
