﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class StemCell : BaseCell
{
    public bool isInAcidic;
    public bool isInAlkali;
    public GameObject stemtoHeat;
    public GameObject stemtoCold;
    public GameObject stemtoAlkali;
    public GameObject stemtoAcidic;
    public override void Mutation(CellType _newType)
    {
        if (currentProtein <= 50.0f)
        {
            return;
        }
        GameObject newCell;
        switch (_newType)
        {
            case CellType.HEAT_CELL:
                newCell = GameObject.Instantiate(stemtoHeat, transform.position, Quaternion.Euler(0.0f, 0.0f, 0.0f)) as GameObject;
                newCell.GetComponent<CellSplitAnimation>().currentProtein = currentProtein * 0.5f;
                newCell.GetComponent<CellSplitAnimation>().isAIPossessed = isAIPossessed;
                currentState = CellState.DEAD;
                break;
            case CellType.COLD_CELL:
                newCell = GameObject.Instantiate(stemtoCold, transform.position, Quaternion.Euler(0.0f, 0.0f, 0.0f)) as GameObject;
                newCell.GetComponent<CellSplitAnimation>().currentProtein = currentProtein * 0.5f;
                newCell.GetComponent<CellSplitAnimation>().isAIPossessed = isAIPossessed;
                currentState = CellState.DEAD;
                break;
            case CellType.ACIDIC_CELL:
                newCell = GameObject.Instantiate(stemtoAcidic, transform.position, Quaternion.Euler(0.0f, 0.0f, 0.0f)) as GameObject;
                newCell.GetComponent<CellSplitAnimation>().currentProtein = currentProtein * 0.5f;
                newCell.GetComponent<CellSplitAnimation>().isAIPossessed = isAIPossessed;
                currentState = CellState.DEAD;
                break;
            case CellType.ALKALI_CELL:
                newCell = GameObject.Instantiate(stemtoAlkali, transform.position, Quaternion.Euler(0.0f, 0.0f, 0.0f)) as GameObject;
                newCell.GetComponent<CellSplitAnimation>().currentProtein = currentProtein * 0.5f;
                newCell.GetComponent<CellSplitAnimation>().isAIPossessed = isAIPossessed;
                currentState = CellState.DEAD;
                break;
            default:
                break;
        }
    }

    void DamagePerSecond()
    {
        primaryTarget.GetComponent<BaseCell>().currentProtein -= attackDamage;
    }

    public override void Attack(GameObject _target)
    {
        if (_target && _target != this.gameObject)
        {
            SetPrimaryTarget(_target);
            currentState = CellState.ATTACK;
        }
    }

   


    void Awake()
    {
        base.bAwake();
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
                SetPrimaryTarget(null);
                if (IsInvoking("DamagePerSecond"))
                {
                    if (GetComponent<ParticleSystem>().isPlaying)
                    {

                        GetComponent<ParticleSystem>().Stop();
                    }
                    CancelInvoke("DamagePerSecond");
                }

                //guard mode auto attack enemy in range
                //base.Guarding();
                break;
            case CellState.ATTACK:

                float distance = Vector3.Distance(primaryTarget.transform.position, transform.position);

                if (distance > attackRange && distance <= fovRadius)
                {
                    if (IsInvoking("DamagePerSecond"))
                    {
                        if (GetComponent<ParticleSystem>().isPlaying)
                        {

                            GetComponent<ParticleSystem>().Stop();
                        }
                        CancelInvoke("DamagePerSecond");
                    }
                    base.ChaseTarget();
                }
                else if (distance <= attackRange)
                {
                    if (!IsInvoking("DamagePerSecond"))
                    {
                        if (GetComponent<ParticleSystem>().isStopped || GetComponent<ParticleSystem>().isPaused)
                        {
                            GetComponent<ParticleSystem>().Play();
                        }
                        InvokeRepeating("DamagePerSecond", 1.0f, 1.0f);
                    }

                }
                else
                {
                    if (IsInvoking("DamagePerSecond"))
                    {
                        if (GetComponent<ParticleSystem>().isPlaying)
                        {

                            GetComponent<ParticleSystem>().Stop();
                        }
                        CancelInvoke("DamagePerSecond");
                    }
                    currentState = CellState.IDLE;
                }
                break;
            case CellState.CONSUMING:
                base.bUpdate();

                break;
            case CellState.MOVING:

                base.bUpdate();
                if (primaryTarget && base.isStopped())
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
                else if (!primaryTarget || base.isStopped())
                {
                    currentState = CellState.IDLE;
                }
              

                break;
            case CellState.ATTACK_MOVING:
                base.bUpdate();

                break;
            case CellState.DEAD:
                base.Die();
                break;
            default:
                break;
        }


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
