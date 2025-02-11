﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ColdCell : BaseCell
{
    public delegate void TakeDamage();
    public TakeDamage multidamagesources;
    ColdCell mergingPartner;
    bool haveMergingPartner = false;
    public bool InCold;
    public PlayerController controller;
    public GameObject Tier2Cold;
    public GameObject stun;
    int instanonce = 0;
    public GameObject particle;



    void Awake()
    {
        base.bAwake();
        InvokeRepeating("MUltiDMg", 1.0f, 1.0f);
        controller = GameObject.Find("PlayerControl").GetComponent<PlayerController>();

        navAgent.speed = 6.0f;
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
    public void AreaDamage()
    {
        currentProtein -= 10;
    }

    public override void Move(Vector3 _destination)
    {
        base.Move(_destination);
        navAgent.SetAreaCost(3, 5);
        navAgent.SetAreaCost(4, 5);
        navAgent.SetAreaCost(5, 1);
        navAgent.SetAreaCost(6, 5);
    }
    public void Merge()
    {
        List<ColdCell> coldCellsMerge = new List<ColdCell>();
        List<BaseCell> possibleMergers = controller.selectedUnits;
        for (int i = 0; i < possibleMergers.Count; i++)
        {

            if (possibleMergers[i].celltype == CellType.COLD_CELL &&
                possibleMergers[i].GetComponent<ColdCell>().InCold == true &&
               possibleMergers[i] != this)
            {
                coldCellsMerge.Add(possibleMergers[i].GetComponent<ColdCell>());
            }

        }

        if (coldCellsMerge.Count >= 1)
        {
            for (int o = 0; o < coldCellsMerge.Count; o++)
            {
                if (mergingPartner == null || Vector3.Distance(this.transform.position, coldCellsMerge[o].transform.position)
                    < Vector3.Distance(this.transform.position, mergingPartner.transform.position) ||
                    (haveMergingPartner == false && mergingPartner.haveMergingPartner == false))
                {
                    mergingPartner = coldCellsMerge[o];
                    mergingPartner.haveMergingPartner = this;
                    haveMergingPartner = true;
                    mergingPartner.haveMergingPartner = true;

                }
            }

        }


    }

    void MergingTheCells(ColdCell other)
    {

        float distance = Vector3.Distance(this.transform.position, other.transform.position);
        if (distance <= GetComponent<SphereCollider>().radius * 2.0f)
        {
            Vector3 trackingPos = this.transform.position;
            Quaternion trackingRot = this.transform.rotation;
            GameObject cTier2Cold = Instantiate(Tier2Cold, trackingPos, trackingRot) as GameObject;
            cTier2Cold.GetComponent<CellSplitAnimation>().currentLevel = currentLevel;
            cTier2Cold.GetComponent<CellSplitAnimation>().currentProtein = currentProtein;
            cTier2Cold.GetComponent<CellSplitAnimation>().isAIPossessed = isAIPossessed;
            cTier2Cold.GetComponent<CellSplitAnimation>().originCell = this;
            cTier2Cold.GetComponent<CellSplitAnimation>().originCell1 = other;


            if (!sound_manager.sounds_evolution[6].isPlaying)
            {
                sound_manager.sounds_evolution[6].Play();
            }

            Deactive();
            other.Deactive();



        }
        else
        {

            Move(other.transform.position);

        }
    }

    void Start()
    {
        base.bStart();
    }
    void DamagePerSecond()
    {
        if (primaryTarget != null)
        {
            AoeDmg(transform.position, attackRange);
            if (PhotonNetwork.connected)
            {
                primaryTarget.gameObject.GetPhotonView().RPC("ApplyDamage", PhotonTargets.Others, attackDamage);
            }

            if (!sound_manager.sounds_attacks[1].isPlaying)
            {
                sound_manager.sounds_attacks[1].Play();
            }
        }
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
                                navAgent.enabled = false;
                                navObstacle.enabled = true;
                            }
                        }
                        else //if (Vector3.Distance(primaryTarget.transform.position, transform.position) <= fovRadius)
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
                case CellState.MOVING:
                    base.bUpdate();
                    break;
                case CellState.ATTACK_MOVING:
                    //  if (!navAgent.isActiveAndEnabled && !primaryTarget && targets.Count == 0)
                    //  {
                    //      currentState = CellState.IDLE;
                    //  }
                    break;
                case CellState.CONSUMING:
                    base.bUpdate();
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
            if (mergingPartner != null)
                MergingTheCells(mergingPartner);
        }
    }


    //LateUpdate is called after all Update functions have been called

    public override void Attack(GameObject _target)
    {
        if (_target)
        {
            SetPrimaryTarget(_target);
            currentState = CellState.ATTACK;
        }
    }

    void FixedUpdate()
    {
        base.bFixedUpdate();
    }

    void LateUpdate()
    {
        base.bLateUpdate();
    }

    void AoeDmg(Vector3 center, float radius)
    {
        if (this.isAIPossessed == false)
        {
            Collider[] hitColliders = Physics.OverlapSphere(center, radius);
            for (int i = 0; i < hitColliders.Length; i++)
            {
                BaseCell basecellerino = hitColliders[i].GetComponent<BaseCell>();
                if (basecellerino != null)
                {
                    if (basecellerino.isMine == false)
                    {
                        basecellerino.currentProtein -= attackDamage;
                        basecellerino.gameObject.GetComponent<Animator>().SetTrigger("BeingAttackTrigger");
                        Vector3 tracking = new Vector3(basecellerino.transform.position.x, basecellerino.transform.position.y + 2, basecellerino.transform.position.z);
                        // Vector3
                        Instantiate(particle, tracking, basecellerino.transform.rotation);
                        if (PhotonNetwork.connected)
                        {
                            basecellerino.gameObject.GetPhotonView().RPC("ApplyDamage", PhotonTargets.Others, attackDamage);
                        }
                    }
                    if (!sound_manager.sounds_attacks[1].isPlaying)
                    {
                        sound_manager.sounds_attacks[1].Play();

                    }

                }
            }
        }
        else
        {
            Collider[] hitCollider4AI = Physics.OverlapSphere(center, radius);
            for (int i = 0; i < hitCollider4AI.Length; i++)
            {
                if (hitCollider4AI[i].GetComponent<BaseCell>())
                {
                    BaseCell baseSenpaiCell = hitCollider4AI[i].GetComponent<BaseCell>();
                    if (baseSenpaiCell != null)
                    {
                        if (baseSenpaiCell.isMine == true)
                        {
                            baseSenpaiCell.currentProtein -= attackDamage;
                            baseSenpaiCell.gameObject.GetComponent<Animator>().SetTrigger("BeingAttackTrigger");
                            Vector3 tracking = new Vector3(baseSenpaiCell.transform.position.x, baseSenpaiCell.transform.position.y + 4, baseSenpaiCell.transform.position.z);
                            // Vector3
                            Instantiate(particle, tracking, baseSenpaiCell.transform.rotation);
                            if (PhotonNetwork.connected)
                            {
                                baseSenpaiCell.gameObject.GetPhotonView().RPC("ApplyDamage", PhotonTargets.Others, attackDamage);
                            }
                        }
                        if (!sound_manager.sounds_attacks[1].isPlaying)
                        {
                            sound_manager.sounds_attacks[1].Play();

                        }
                    }
                }
            }
        }
    }
}

