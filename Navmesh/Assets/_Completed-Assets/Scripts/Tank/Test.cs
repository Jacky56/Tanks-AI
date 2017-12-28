using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Complete
{
	public class Test : MonoBehaviour
	{
		public bool m_NPC;
		public GameObject cube;
		// Use this for initialization
		void Start()
		{

		}

		// Update is called once per frame
		void Update()
		{
			if (Input.GetKeyDown("1") && m_NPC != true)
			{
				GameObject newCube = Instantiate(cube, this.transform.position,this.transform.rotation) as GameObject;
				newCube.transform.position += newCube.transform.forward * 10;
			}
		}
	}
}