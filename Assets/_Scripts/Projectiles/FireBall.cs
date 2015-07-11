﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FireBall : BaseProjectile {
    public GameObject Target;
    public GameObject Owner;

	void Awake() {
        
    }

	// Use this for initialization
	void Start () {
	
	}
	

	// Update is called once per frame
	void Update () {
	
	}

	void FixedUpdate() {
       
    }

	//LateUpdate is called after all Update functions have been called
	void LateUpdate() {
        
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Unit" && !other.gameObject.GetComponent<BaseCell>().isMine)
        {
            Target.GetComponent<BaseCell>().currentProtein = Target.GetComponent<BaseCell>().currentProtein - Owner.GetComponent<BaseCell>().attackDamage;
            Target.GetComponent<Animator>().SetTrigger("BeingAttackTrigger");
            Destroy(this.gameObject);
        }
       
    }
}