using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class VoxRotator : MonoBehaviour {

	Transform[] layers = null;

	
	public float rotationDegrees {
		get{return _rotationDegrees;}
		set{
			_rotationDegrees = value;
			UpdateRotationDeg();
		}
	}

	[SerializeField]
	float _rotationDegrees = 0f;
	void Awake () {
		SpriteRenderer[] layerRenderers = GetComponentsInChildren<SpriteRenderer>();
		layers = new Transform[layerRenderers.Length];
		for(int i = 0; i< layers.Length; i++){
			layers[i] = layerRenderers[i].transform;
		}
	}

	void UpdateRotationDeg(){
		foreach(Transform layer in layers){			
			layer.localRotation = Quaternion.Euler(0f, 0f, _rotationDegrees - 90);
		}
	}

	void OnValidate(){
		if(layers != null){
			UpdateRotationDeg();
		}		
	}
}
