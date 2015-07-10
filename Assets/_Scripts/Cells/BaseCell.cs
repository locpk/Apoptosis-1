﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Cancer Chance
/// </summary>
public static class CancerChance
{
    public const float LEVEL_1 = 0.0f;
    public const float LEVEL_2 = 0.25f;
    public const float LEVEL_3 = 0.35f;
    public const float LEVEL_4 = 0.45f;
    public const float LEVEL_5 = 1.0f;
}

/// <summary>
/// Different Cell types
/// </summary>
public enum CellType
{
    STEM_CELL, HEAT_CELL, COLD_CELL, HEAT_CELL_TIRE2, COLD_CELL_TIRE2, ACIDIC_CELL, ALKALI_CELL, CANCER_CELL,NERVE_CELL
}


/// <summary>
/// Different cells states
/// </summary>
public enum CellState
{
    IDLE, ATTACK, MOVING, CONSUMING, ATTACK_MOVING, DEAD, CANCEROUS_SPLITTING, PERFECT_SPLITTING, EVOLVING, INCUBATING, MERGING,
}


/// <summary>
/// Define the base cell class
/// </summary>
public class BaseCell : MonoBehaviour
{
    public GameObject gCancerCellPrefab;
    public GameObject gStemCellPrefab;
    public GameObject gHeatCellPrefab;
    public GameObject gColdCellPrefab;
    public GameObject gColdCancerPrefab;
    public GameObject gAcidicCellPrefab;
    public GameObject gAlkaliCellPrefab;

    public Sprite health_10;
    public Sprite health_50;
    public Sprite health_100;

    public const float MAX_PROTEIN = 500.0f;
    public const float DEPLETE_TIME = 5.0f;
    public const float ATTACK_COOLDOWN = 1.0f;
    public const float moveSpeed = 3.0f;
    /// <summary>
    /// Fields
    /// </summary>
    //
    public int currentLevel = 0;
    public bool isSinglePlayer = true;
    public bool isMine;
    public bool isAlive = true;
    public bool isAIPossessed;
    public bool isDepleting;
    public bool isSelected;
    public CellType celltype;
    public CellState currentState;
    public NavMeshAgent navAgent;
    public NavMeshObstacle navObstacle;
    public Vector3 destination;
    public List<GameObject> targets;
    public GameObject primaryTarget;
    // public PhotonView photonView;
    public float currentProtein;
    public float fovRadius;
    public float attackDamage;
    public float attackRange;
    public float defense;
    public float depleteTimer;
    public float depleteAmount = 3.0f; // per second
    public float attackCooldown;
    public float splitCooldown;

    #region RPC Methods


    ////[RPC] Methods, which called via network
    //[PunRPC]
    //public void ApplyDamage(float _received_damage)
    //{
    //    currentProtein -= _received_damage - defense;
    //}
    #endregion

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.isWriting)
        {
            // We own this player: send the others our data
            stream.SendNext(transform.position);
            stream.SendNext(currentProtein);
        }
        else
        {
            // Network player, receive data
            this.transform.position = (Vector3)stream.ReceiveNext();
            this.currentProtein = (float)stream.ReceiveNext();
        }
    }


    #region Standard Actions
    // Public Methods

    public virtual void Move(Vector3 _destination)
    {

        currentState = CellState.MOVING;
        navObstacle.enabled = false;
        navAgent.enabled = true;
        destination = _destination;
        navAgent.SetDestination(_destination);

    }

    public void SetSpeed(float _speed)
    {
        navAgent.speed = _speed;
    }

    public virtual void AttackMove(Vector3 _destination)
    {
        currentState = CellState.ATTACK_MOVING;
        navObstacle.enabled = false;
        navAgent.enabled = true;
        destination = _destination;
        navAgent.SetDestination(_destination);
    }

    public void SetPrimaryTarget(GameObject _target)
    {
        if (IsInvoking("ConsumePerSecond"))
        {
            CancelInvoke("ConsumePerSecond");
        }

        primaryTarget = _target;

    }

    public void SetTargets(List<GameObject> _targets)
    {
        if (_targets.Count > 0)
        {
            targets = _targets;
        }

    }
    public void Die()
    {
        isMine = false;
        GameObject.Find("PlayerControl").GetComponent<PlayerController>().RemoveDeadCell(this);
        if (celltype != CellType.CANCER_CELL)
        {
            PlayerController.cap--;
            if (PlayerController.cap < 0)
            {
                PlayerController.cap = 0;
            }
        }

       
        //transform.GetChild(0).GetComponent<SpriteRenderer>().enabled = false;
        //GetComponent<SpriteRenderer>().enabled = false;
        Destroy(gameObject);
    }
    public void Deactive()
    {
        GameObject.Find("PlayerControl").GetComponent<PlayerController>().DeselectCell(this);
        gameObject.SetActive(false);
        transform.position = new Vector3(2500.0f, 2500.0f, 2500.0f);
       
        GetComponent<SpriteRenderer>().enabled = false;

    }

    public virtual void Attack(GameObject _target)
    {
    }

    void ConsumePerSecond()
    {
        if (primaryTarget)
        {
            currentProtein += primaryTarget.GetComponent<Protein>().Harvest();
            if (currentProtein > MAX_PROTEIN)
            {
                currentProtein = MAX_PROTEIN;
            }
        }

    }

    public void Consume(GameObject _target)
    {
        Protein targetProtein = _target.GetComponent<Protein>();
        if (targetProtein)
        {
            SetPrimaryTarget(_target);
            currentState = CellState.CONSUMING;
        }
    }

    public void Guarding()
    {
        //List<GameObject> aiUnits = GameObjectManager.FindAIUnits();
        //if (aiUnits.Count > 0)
        //{
        //    foreach (var enemy in aiUnits)
        //    {
        //        if (Vector3.Distance(enemy.transform.position, transform.position) <= fovRadius)
        //        {
        //            if (enemy != this)
        //            {
        //                Attack(enemy);
        //            }
        //            break;
        //        }
        //    }
        //}

    }

    public void AIAttacking()
    {
        List<GameObject> playerUnits = GameObjectManager.FindPlayerUnits();
        if (playerUnits.Count > 0)
        {
            foreach (var enemy in playerUnits)
            {
                if (Vector3.Distance(enemy.transform.position, transform.position) <= fovRadius)
                {
                    if (enemy != this)
                    {
                        Attack(enemy);
                    }
                    break;
                }
            }
        }
    }

    #endregion

    #region Special abilities
    public void PerfectSplit()
    {
        if (currentLevel >= 5 || currentProtein <= 1.0f || PlayerController.cap + 1 > PlayerController.MAX_CAP)
        {
            return;
        }

        Vector3 newposition = this.transform.position;

        newposition += Quaternion.Euler(0, 0, Random.Range(0, 360)) * new Vector3(GetComponent<SphereCollider>().radius * 0.1f, 0, 0);
        GameObject cellSplitAnimation;
        switch (celltype)
        {
            case CellType.STEM_CELL:
                cellSplitAnimation = GameObject.Instantiate(gStemCellPrefab, transform.position, Quaternion.identity) as GameObject;
                cellSplitAnimation.GetComponent<CellSplitAnimation>().currentLevel = currentLevel + 1;
                cellSplitAnimation.GetComponent<CellSplitAnimation>().currentProtein = currentProtein * 0.5f;
                cellSplitAnimation.GetComponent<CellSplitAnimation>().isAIPossessed = isAIPossessed;
                //GameObject.Find("PlayerControl").GetComponent<PlayerController>().RemoveDeadCell(this);
                cellSplitAnimation.GetComponent<CellSplitAnimation>().originCell = this;
                Deactive();
                break;
            case CellType.CANCER_CELL:
                cellSplitAnimation = GameObject.Instantiate(gCancerCellPrefab, transform.position, Quaternion.identity) as GameObject;
                cellSplitAnimation.GetComponent<BaseCell>().currentLevel = currentLevel + 1;
                cellSplitAnimation.GetComponent<BaseCell>().currentProtein = currentProtein * 0.5f;
                cellSplitAnimation.GetComponent<BaseCell>().isAIPossessed = isAIPossessed;
                // GameObject.Find("PlayerControl").GetComponent<PlayerController>().RemoveDeadCell(this);
                cellSplitAnimation.GetComponent<CellSplitAnimation>().originCell = this;
                Deactive();
                break;
            default:
                break;
        }




    }

    public void CancerousSplit()
    {
        if (currentLevel >= 5 || currentProtein <= 1.0f || PlayerController.cap + 1 > PlayerController.MAX_CAP)
        {
            return;
        }

        float cancerousChance = 0.0f;
        switch (currentLevel)
        {
            case 1:
                cancerousChance = CancerChance.LEVEL_1;
                break;
            case 2:
                cancerousChance = CancerChance.LEVEL_2;
                break;
            case 3:
                cancerousChance = CancerChance.LEVEL_3;
                break;
            case 4:
                cancerousChance = CancerChance.LEVEL_4;
                break;
            case 5:
                cancerousChance = CancerChance.LEVEL_5;
                break;
            default:
                break;
        }

        //Get a new position around myself
        Vector3 newposition = this.transform.position;
       

        //half my protein
        this.currentProtein *= 0.5f;
        currentLevel++;

        GameObject newCell;


        if (Random.Range(0.0f, 1.0f) > cancerousChance)
        {

            switch (this.celltype)
            {
                case CellType.HEAT_CELL:
                    newCell = GameObject.Instantiate(gHeatCellPrefab, newposition, Quaternion.Euler(0.0f, 0.0f, 0.0f)) as GameObject;
                    newCell.GetComponent<CellSplitAnimation>().currentLevel = currentLevel;
                    newCell.GetComponent<CellSplitAnimation>().currentProtein = currentProtein;
                    newCell.GetComponent<CellSplitAnimation>().isAIPossessed = isAIPossessed;
                    newCell.GetComponent<CellSplitAnimation>().originCell = this;
                    Deactive();
                    break;
                case CellType.COLD_CELL:
                    newCell = GameObject.Instantiate(gColdCellPrefab, newposition, Quaternion.Euler(0.0f, 0.0f, 0.0f)) as GameObject;
                    newCell.GetComponent<CellSplitAnimation>().currentLevel = currentLevel;
                    newCell.GetComponent<CellSplitAnimation>().currentProtein = currentProtein;
                    newCell.GetComponent<CellSplitAnimation>().isAIPossessed = isAIPossessed;
                    newCell.GetComponent<CellSplitAnimation>().originCell = this;
                    Deactive();
                    break;
                default:
                    break;
            }
        }
        else
        {
            //newCell = GameObject.Instantiate(gCancerCellPrefab, newposition, Quaternion.Euler(90.0f, 0.0f, 0.0f)) as GameObject;
            //newCell.GetComponent<BaseCell>().currentProtein = currentProtein;
            //newCell.GetComponent<BaseCell>().currentLevel = currentLevel;
            //newCell.GetComponent<BaseCell>().isAIPossessed = false;
            //newCell.GetComponent<BaseCell>().navAgent.updateRotation = false;

            switch (this.celltype)
            {
                case CellType.HEAT_CELL:
                
                    break;
                case CellType.COLD_CELL:
                    newCell = GameObject.Instantiate(gColdCancerPrefab, newposition, Quaternion.Euler(0.0f, 0.0f, 0.0f)) as GameObject;
                    newCell.GetComponent<CellSplitAnimation>().currentLevel = currentLevel;
                    newCell.GetComponent<CellSplitAnimation>().currentProtein = currentProtein;
                    newCell.GetComponent<CellSplitAnimation>().isAIPossessed = isAIPossessed;
                    newCell.GetComponent<CellSplitAnimation>().originCell = this;
                    Deactive();
                    break;
                default:
                    break;
            }
        }





    }


    public virtual void Mutation(CellType _newType)
    {

    }

    public virtual void Evolve(CellType _newType)
    {

    }


    public void ChaseTarget()
    {
        if (primaryTarget)
        {
            Move(primaryTarget.transform.position);
        }
    }
    public void Deplete(float _deltaTime)
    {
        depleteTimer -= _deltaTime;
        if (depleteTimer <= 0.0f)
        {
            depleteTimer = DEPLETE_TIME;
            currentProtein -= depleteAmount;
        }
    }
    #endregion

    protected void bAwake()
    {
        depleteTimer = DEPLETE_TIME;
        //if (isSinglePlayer)
        //{
        //    GetComponent<PhotonView>().enabled = false;
        //}
        navAgent = GetComponent<NavMeshAgent>();
        navObstacle = GetComponent<NavMeshObstacle>();
        navAgent.speed = moveSpeed;

       
        // photonView = GetComponent<PhotonView>();
        //  isMine = photonView.isMine;

    }

    // Use this for initialization
    protected void bStart()
    {
        navAgent.enabled = false;
        navAgent.updateRotation = false;
        navObstacle.enabled = true;

    }

    protected void bUpdate()
    {
        if (currentState == CellState.IDLE)
        {
           
            
            if (IsInvoking("ConsumePerSecond"))
            {
                CancelInvoke("ConsumePerSecond");
            }
        }
        if (currentState == CellState.MOVING)
        {
            if (isStopped())
            {

                navAgent.enabled = false;
                navObstacle.enabled = true;
                if (primaryTarget != null)
                {
                    if (primaryTarget.tag == "Protein")
                    {
                        
                        currentState = CellState.CONSUMING;
                        return;
                    }
                    else if (primaryTarget.tag == "Unit")
                    {
                        currentState = CellState.ATTACK;
                        return;
                    }
                }
                else
                {
                    currentState = CellState.IDLE;
                    return;
                }
            }
        }
        else if (currentState == CellState.CONSUMING)
        {
            if (primaryTarget)
            {
                if (!primaryTarget.GetComponent<Protein>().consumers.Contains(this))
                {
                    primaryTarget.GetComponent<Protein>().consumers.Add(this);
                }
                float distance = Vector3.Distance(primaryTarget.transform.position, transform.position);

                if (distance > attackRange && distance <= fovRadius)
                {
                    if (IsInvoking("ConsumePerSecond"))
                    {
                    
                        CancelInvoke("ConsumePerSecond");
                    }
                    ChaseTarget();
                }
                else if (distance <= attackRange)
                {
                    if (!IsInvoking("ConsumePerSecond"))
                    {
                        
                        InvokeRepeating("ConsumePerSecond", 1.0f, 1.0f);
                    }

                }
                else
                {
       
                    ChaseTarget();
                }
            }
            else
            {
              
                currentState = CellState.IDLE;
                return;
            }
        }
        else if (currentState == CellState.ATTACK_MOVING)
        {

            List<GameObject> theirUnits = GameObjectManager.FindTheirUnits();
            if (theirUnits.Count > 0)
            {
                foreach (var enemy in theirUnits)
                {
                    if (Vector3.Distance(enemy.transform.position, transform.position) <= attackRange)
                    {
                        if (enemy != this.gameObject)
                        {
                            Attack(enemy);
                            return;
                        }

                    }
                }
            }
            if (isStopped())
            {

                navAgent.enabled = false;
                navObstacle.enabled = true;
                currentState = CellState.IDLE;
                return;
            }
        }
        if (!isMine && transform.tag != "Animation" && this.celltype != CellType.CANCER_CELL)
        {

            this.gameObject.transform.GetChild(1).GetComponent<MeshRenderer>().material.color = Color.red;
        }
    }

    public bool isStopped()
    {
        if (navAgent.isActiveAndEnabled)
        {
            if (!navAgent.pathPending)
            {
                if (navAgent.remainingDistance <= navAgent.stoppingDistance)
                {
                    if (!navAgent.hasPath || navAgent.velocity.sqrMagnitude == 0f)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }
    protected void bFixedUpdate()
    {
        Deplete(Time.fixedDeltaTime);
        if (currentProtein <= 0.0f)
        {
            currentState = CellState.DEAD;
        }

    }

    protected void bLateUpdate()
    {
        float healthRatio = currentProtein / MAX_PROTEIN;
        if (healthRatio <= 0.5f && healthRatio > 0.1f)
        {
            transform.FindChild("Nucleus").GetComponent<SpriteRenderer>().sprite = health_50;
        }
        else if (healthRatio <= 0.1f)
        {
            transform.FindChild("Nucleus").GetComponent<SpriteRenderer>().sprite = health_10;
        }
        else
        {
            transform.FindChild("Nucleus").GetComponent<SpriteRenderer>().sprite = health_100;
        }
    }
}
