﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AcidicCell : BaseCell
{

    public delegate void TakeDamage();
    public TakeDamage multidamagesources;
   
    void Awake()
    {
        base.bAwake();
        multidamagesources += nothing;
          InvokeRepeating("MUltiDMg", 1.0f, 1.0f);
    }

    void DamagePreSecond()
    {
        if (primaryTarget != null)
        {
            primaryTarget.GetComponent<BaseCell>().currentProtein -= attackDamage;
            primaryTarget.GetComponent<Animator>().SetTrigger("BeingAttackTrigger");
        }
    }
    void MUltiDMg()
    {
        multidamagesources();

    }
    public void AreaDamage()
    {
        currentProtein -= 10;
    }
    void nothing()
    {

    }
    // Use this for initialization
    void Start()
    {
        base.bStart();
    }

    // Update is called once per frame
    void Update()
    {
        switch (currentState)
        {
            case CellState.IDLE:
              
                break;
            case CellState.ATTACK:
                if (primaryTarget != null)
                {
                    if (Vector3.Distance(primaryTarget.transform.position, transform.position) <= attackRange)
                    {
                        if (!IsInvoking("DamagePreSecond"))
                        {
                            InvokeRepeating("DamagePreSecond", 1.0f, 1.0f);

                        }
                    }
                    else if (Vector3.Distance(primaryTarget.transform.position, transform.position) <= fovRadius)
                    {
                        if (IsInvoking("DamagePreSecond"))
                        {
                            CancelInvoke("DamagePreSecond");
                        }
                        if (Vector3.Distance(primaryTarget.transform.position, transform.position) > attackRange)
                        {
                            base.ChaseTarget();
                        }
                    }
                    else
                    {
                        SetPrimaryTarget(null);
                        navAgent.Stop();
                    }

                }
                else
                {
                    currentState = CellState.IDLE;
                }
                break;
            case CellState.MOVING:
                base.bUpdate();
                if (primaryTarget != null)
                {
                    if (primaryTarget.GetComponent<BaseCell>())
                    {
                        currentState = CellState.ATTACK;
                    }
                    else if (primaryTarget.GetComponent<Protein>())
                    {
                        currentState = CellState.CONSUMING;
                    }

                }
                break;
            case CellState.ATTACK_MOVING:
                break;
            case CellState.CONSUMING:
                base.bUpdate();
                break;
            case CellState.DEAD:
                base.Die();
                break;


            default:
                break;
        }
        base.bUpdate();
    }

    void FixedUpdate()
    {
        base.bFixedUpdate();
    }

    //LateUpdate is called after all Update functions have been called
    void LateUpdate()
    {
        base.bLateUpdate();
    }
}
