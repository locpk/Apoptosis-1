﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Protein : MonoBehaviour {

    public float value;
    public bool beingConsumed = false;
    ParticleSystem emitter;
    public List<BaseCell> consumers;
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.isWriting)
        {
            // We own this player: send the others our data
            stream.SendNext(value);
        }
        else
        {
            // Network player, receive data
            this.value = (float)stream.ReceiveNext();
        }
    }

    public float Harvest()
    {
        value -= 5.0f;
        return value >= 0 ? 5.0f : 5.0f + value;
    }
	// Use this for initialization
	void Start () {
        value = Random.Range(70, 110);
        emitter = GetComponent<ParticleSystem>();
	}
	
	// Update is called once per frame
	void Update () {
        
        
	}
    void FixedUpdate()
    {
        if (consumers.FindAll(item => item.GetComponent<BaseCell>().currentState == CellState.CONSUMING).Count == 0)
	    {
            beingConsumed = false;
            consumers.Clear();
	    }
        else
        {
            consumers.RemoveAll(item => item.GetComponent<BaseCell>().currentState != CellState.CONSUMING);
            beingConsumed = true;
        }
        
        if (beingConsumed)
        {
            if (emitter.isStopped)
            {
                emitter.Play();
            }
        }
        else
        {
            if (emitter.isPlaying)
            {
                emitter.Stop();
            }
        }
    }
    void LateUpdate()
    {
        float scale = 0.3f * value / 110 + .2f;
        transform.localScale = new Vector3(scale, scale, 1.0f);
        if (value <= 0.0f)
        {
            Destroy(gameObject);
        }
    }


   
}
