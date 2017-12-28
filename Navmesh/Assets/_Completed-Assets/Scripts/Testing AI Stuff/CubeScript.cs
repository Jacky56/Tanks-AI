using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeScript : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	private void OnTriggerEnter(Collider col)
	{
		Complete.TankHealth targetHealth = col.gameObject.GetComponent<Complete.TankHealth>();
		if (targetHealth != null)
		{
			targetHealth.TakeDamage(-100f);
			print("eat cube " + targetHealth);
			Destroy(gameObject);
		}
	}
}
