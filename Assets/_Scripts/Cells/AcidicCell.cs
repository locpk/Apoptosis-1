﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AcidicCell : BaseCell
{

    public delegate void TakeDamage();
    public TakeDamage multidamagesources;
    public GameObject stun;
    public GameObject Acid;
    int instanonce = 0;
    public AlkaliCell mergePartner;
    public bool haveMergePartner = false;
    public GameObject nerveCell;

    void Awake()
    {
        sound_manager = GameObject.FindGameObjectWithTag("Sound_Manager").GetComponent<Sound_Manager>();
        base.bAwake();
        navAgent.speed = 7.0f;
        if (currentProtein > MAX_PROTEIN) // avoid overfeeding
        {
            currentProtein = MAX_PROTEIN;
        } 
       
    }
    public override void Attack(GameObject _target)
    {
        if (_target && _target != this.gameObject)
        {
            SetPrimaryTarget(_target);
            currentState = CellState.ATTACK;
        }
    }


    public override void Move(Vector3 _destination)
    {
        base.Move(_destination);
        navAgent.SetAreaCost(3, 3);
        navAgent.SetAreaCost(4, 1);
        navAgent.SetAreaCost(5, 3);
        navAgent.SetAreaCost(6, 5);
    }
    public void Merge()
    {
        List<BaseCell> alkaliCellMerge;
        List<BaseCell> possibleMergers = pcontroller.selectedUnits;

        alkaliCellMerge = possibleMergers.FindAll(item => item.celltype == CellType.ALKALI_CELL && item.GetComponent<AlkaliCell>());

        if (alkaliCellMerge.Count >= 1)
        {
            for (int i = 0; i < alkaliCellMerge.Count; i++)
            {
                if (mergePartner == null || Vector3.Distance(this.transform.position, alkaliCellMerge[i].transform.position)
                          < Vector3.Distance(this.transform.position, mergePartner.transform.position) ||
                     (haveMergePartner == false && mergePartner.haveMergePartner == false))
                {
                    if (mergePartner != null)
                    {
                        break;
                    }
                    mergePartner = alkaliCellMerge[i].GetComponent<AlkaliCell>();
                    mergePartner.mergePartner = this;
                    haveMergePartner = true;
                    mergePartner.haveMergePartner = true;
                }
            }
        }
    }

    void MergingTheCells(AlkaliCell other)
    {

        float distance = Vector3.Distance(this.transform.position, other.transform.position);
        if (distance < GetComponent<SphereCollider>().radius *1.3f)
        {
            Vector3 trackingPos = this.transform.position;
            Quaternion trackingRot = this.transform.rotation;



            GameObject knerveCell = Instantiate(nerveCell, trackingPos, trackingRot) as GameObject;
            knerveCell.GetComponent<CellSplitAnimation>().currentLevel = currentLevel;
            knerveCell.GetComponent<CellSplitAnimation>().currentProtein = currentProtein;
            knerveCell.GetComponent<CellSplitAnimation>().isAIPossessed = isAIPossessed;
            knerveCell.GetComponent<CellSplitAnimation>().originCell = this;
            knerveCell.GetComponent<CellSplitAnimation>().originCell1 = other;
            Deactive();
            other.Deactive();

            if (!sound_manager.sounds_evolution[5].isPlaying)
            {
                sound_manager.sounds_evolution[5].Play();
            }

        }
        else
        {

            Move(other.transform.position);

        }

    }
    void DamagePerSecond()
    {
        if (primaryTarget != null)
        {
            GameObject kAcid = PhotonNetwork.connected ? PhotonNetwork.Instantiate("AcidStart", transform.position, transform.rotation, 0)
                : Instantiate(Acid, transform.position, transform.rotation) as GameObject;
            kAcid.GetComponent<Acidd>().Target = primaryTarget;
            kAcid.GetComponent<Acidd>().Owner = this.gameObject;
            Vector3 them2me = kAcid.GetComponent<Acidd>().Target.transform.position - transform.position;
            kAcid.GetComponent<Rigidbody>().velocity += them2me.normalized * kAcid.GetComponent<Acidd>().speed;
            currentState = CellState.ATTACK;
            if (!sound_manager.sounds_attacks[2].isPlaying)
            {
                sound_manager.sounds_attacks[2].Play();

            }
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
                Destroy(stun.gameObject);
                this.stunTimer = 3;
                this.stunned = false;
                this.hitCounter = 0;
                return;
            }
        }

        
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
                            InvokeRepeating("DamagePerSecond", 1.0f, 3.0f);
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
        base.bUpdate();
            if (mergePartner != null)
                MergingTheCells(mergePartner);
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
