using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera)), ExecuteInEditMode]
[AddComponentMenu("Effects/Crepuscular Rays", -1)]
public class Crepuscular : MonoBehaviour
{

	public Material material;
	public GameObject lightObj;
    
	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		material.SetVector("_LightPos", GetComponent<Camera>().WorldToViewportPoint(transform.position - lightObj.transform.forward));
		Graphics.Blit(source, destination, material);
	}
}
