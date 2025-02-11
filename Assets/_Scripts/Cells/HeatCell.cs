﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HeatCell : BaseCell
{
    public GameObject fireball;
    public GameObject Tier2Heat;
    HeatCell mergePartner;
    bool haveMergePartner = false;
    public delegate void TakeDamage();
    public TakeDamage multidamagesources;
    // float splitCD = 0;
    float fireballSpeed = 10;
    public bool Inheat;
    public PlayerController controller;
    //GameObject previousTarget;
    public GameObject stun;
    int instanonce = 0;


    public void Merge()
    {



        List<BaseCell> heatCellsMerge;
        List<BaseCell> possibleMergers = controller.selectedUnits;
        heatCellsMerge = possibleMergers.FindAll(item => item.celltype == CellType.HEAT_CELL && item.GetComponent<HeatCell>().Inheat == true &&
               item != this);

        if (heatCellsMerge.Count >= 1)
        {
            for (int o = 0; o < heatCellsMerge.Count; o++)
            {
                if (mergePartner == null || Vector3.Distance(this.transform.position, heatCellsMerge[o].transform.position)
                    < Vector3.Distance(this.transform.position, mergePartner.transform.position) ||
                    (haveMergePartner == false && mergePartner.haveMergePartner == false))
                {
                    mergePartner = heatCellsMerge[o].GetComponent<HeatCell>();
                    mergePartner.mergePartner = this;
                    haveMergePartner = true;
                    mergePartner.haveMergePartner = true;

                }
            }

        }


    }


    void MergingTheCells(HeatCell other)
    {

        float distance = Vector3.Distance(this.transform.position, other.transform.position);
        if (distance < GetComponent<SphereCollider>().radius * 2.0f)
        {
            Vector3 trackingPos = this.transform.position;
            Quaternion trackingRot = this.transform.rotation;



            GameObject kTier2Heat = Instantiate(Tier2Heat, trackingPos, trackingRot) as GameObject;
            kTier2Heat.GetComponent<CellSplitAnimation>().currentLevel = currentLevel;
            kTier2Heat.GetComponent<CellSplitAnimation>().currentProtein = currentProtein;
            kTier2Heat.GetComponent<CellSplitAnimation>().isAIPossessed = isAIPossessed;
            kTier2Heat.GetComponent<CellSplitAnimation>().originCell = this;
            kTier2Heat.GetComponent<CellSplitAnimation>().originCell1 = other;


            if (!sound_manager.sounds_evolution[5].isPlaying)
            {
                sound_manager.sounds_evolution[5].Play();
            }
            Deactive();
            other.Deactive();

        }
        else
        {

            Move(other.transform.position);

        }

    }
    void Awake()
    {
        base.bAwake();
        InvokeRepeating("MUltiDMg", 1.0f, 1.0f);
        controller = GameObject.Find("PlayerControl").GetComponent<PlayerController>();

        sound_manager = GameObject.FindGameObjectWithTag("Sound_Manager").GetComponent<Sound_Manager>();
        navAgent.speed = 9.0f;
        if (currentProtein > MAX_PROTEIN) // avoid overfeeding
        {
            currentProtein = MAX_PROTEIN;
        } 
    }

    void MUltiDMg()
    {
        if (multidamagesources != null)
            multidamagesources();
    }

    public override void Move(Vector3 _destination)
    {
        base.Move(_destination);
        navAgent.SetAreaCost(3, 1);
        navAgent.SetAreaCost(4, 5);
        navAgent.SetAreaCost(5, 5);
        navAgent.SetAreaCost(6, 5);
    }
    public void AreaDamage()
    {
        currentProtein -= 10;
    }

    public void DamagePerSecond()
    {
        if (primaryTarget != null)
        {
            //previousTarget = primaryTarget;
            Vector3 them2me = primaryTarget.transform.position - transform.position;
            GameObject thefireball = PhotonNetwork.connected ? PhotonNetwork.Instantiate("Fireball", transform.position, transform.rotation, 0) as GameObject
                : Instantiate(fireball, transform.position, transform.rotation) as GameObject;
            thefireball.GetComponent<Rigidbody>().velocity += them2me.normalized * fireballSpeed;
            thefireball.GetComponent<FireBall>().Target = primaryTarget;

            thefireball.GetComponent<FireBall>().Owner = this.gameObject;

            if (!sound_manager.sounds_attacks[0].isPlaying)
            {
                sound_manager.sounds_attacks[0].Play();
            }
        }

    }



    // Use this for initialization
    void Start()
    {
        base.bStart();
    }

    // Update is called once per frame
    void Update()
    {
        if (stunned == true)
        {
            if (instanonce < 1)
            {
                Vector3 trackingPos = new Vector3(transform.position.x, transform.position.y + 1, transform.position.z);
                GameObject.Instantiate(stun, trackingPos, transform.rotation);
            }
            instanonce++;

            stunTimer -= 1 * Time.fixedDeltaTime;

            if (stunTimer > 0)
            {
                navAgent.enabled = false;
                navObstacle.enabled = true;
                primaryTarget = null;

                return;
            }
            if (this.stunTimer <= 0)
            {
                instanonce = 0;
                // Destroy(stun.gameObject);
                this.stunTimer = 3;
                this.stunned = false;
                this.hitCounter = 0;

            }
        }
        else
        {
            switch (currentState)
            {
                case CellState.IDLE:
                    if (IsInvoking("DamagePerSecond"))
                    {
                        CancelInvoke("DamagePerSecond");
                    }
                   
                    base.bUpdate();

                    break;
                case CellState.ATTACK:
                    if (primaryTarget != null)
                    {
                        if (Vector3.Distance(primaryTarget.transform.position, transform.position) <= attackRange)
                        {
                            if (!IsInvoking("DamagePerSecond"))
                            {
                                InvokeRepeating("DamagePerSecond", 1.0f, 1.0f);
                               // navAgent.Stop();
                                navAgent.enabled = false;
                                navObstacle.enabled = true;
                            }
                        }

                        else// if (Vector3.Distance(primaryTarget.transform.position, transform.position) <= fovRadius)
                        {
                            if (IsInvoking("DamagePerSecond"))
                            {
                                CancelInvoke("DamagePerSecond");
                            }
                            if (Vector3.Distance(primaryTarget.transform.position, transform.position) > attackRange)
                            {
                                base.ChaseTarget();
                            }

                        }

                    }
                    else
                    {
                        currentState = CellState.IDLE;
                    }
                    break;
                case CellState.CONSUMING:
                    base.bUpdate();
                    break;
                case CellState.MOVING:
                    if (IsInvoking("DamagePerSecond"))
                    {
                        CancelInvoke("DamagePerSecond");
                    }
                    base.bUpdate();
                    break;
                case CellState.ATTACK_MOVING:
                    if (!navAgent.isActiveAndEnabled && !primaryTarget && targets.Count == 0)
                    {
                        currentState = CellState.IDLE;
                    }
                    break;
                case CellState.DEAD:
                    base.Die();
                    if (PhotonNetwork.connected)
                    {
                        photonView.RPC("Die", PhotonTargets.Others, null);
                    }
                    break;

                default:
                    break;
            }
            if (mergePartner != null)
                MergingTheCells(mergePartner);

        }
    }

    void FixedUpdate()
    {
        base.bFixedUpdate();
    }
    public override void Attack(GameObject _target)
    {
        if (_target)
        {
            SetPrimaryTarget(_target);
            currentState = CellState.ATTACK;
        }
    }


    //LateUpdate is called after all Update functions have been called
    void LateUpdate()
    {

        base.bLateUpdate();
    }

    void Evolve(GameObject other)
    {

    }
}
