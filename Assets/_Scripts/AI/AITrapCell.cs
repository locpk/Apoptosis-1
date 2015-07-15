﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AITrapCell : MonoBehaviour{

    private float m_visionRange;
    private RaycastHit[] m_cellsInSight;
    private BaseCell m_baseCell;


    void Awake () {
        switch (GetComponent<BaseCell>().celltype) {
            case CellType.HEAT_CELL:
                m_baseCell = GetComponent<HeatCell>();
                break;
            case CellType.COLD_CELL:
                m_baseCell = GetComponent<ColdCell>();
                break;
            case CellType.HEAT_CELL_TIRE2:
                m_baseCell = GetComponent<Tier2HeatCell>();
                break;
            case CellType.COLD_CELL_TIRE2:
                //m_baseCell = GetComponent<Tier2HeatCell>();
                break;
            case CellType.ACIDIC_CELL:
                m_baseCell = GetComponent<AcidicCell>();
                break;
            case CellType.ALKALI_CELL:
                m_baseCell = GetComponent<AlkaliCell>();
                break;
            case CellType.CANCER_CELL:
                m_baseCell = GetComponent<CancerCell>();
                break;
            case CellType.NERVE_CELL:
                m_baseCell = GetComponent<NerveCell>();
                break;
            default:
                break;
        }

        m_visionRange = GetComponent<BaseCell>().fovRadius;
    }

    void Start() {
        m_baseCell.isMine = false;
        m_baseCell.isAIPossessed = false;
        m_baseCell.tag = "EnemyCell";
        m_baseCell.SetSpeed(m_baseCell.navAgent.speed * .5f);
        m_baseCell.currentState = CellState.IDLE;
        if (GetComponent<FogOfWarHider>() == null) {
            gameObject.AddComponent<FogOfWarHider>();
        }
    }

    void FixedUpdate() {
        m_cellsInSight = Physics.SphereCastAll(transform.position, m_visionRange, transform.forward);
        bool targetFound = false;
        foreach (RaycastHit hitInfo in m_cellsInSight) {
            if (hitInfo.collider.gameObject.tag == "Unit") {
                if (IsInvoking("RandomMove")) {
                    CancelInvoke("RandomMove");
                }
                targetFound = true;
                if (Vector3.Distance(transform.position, hitInfo.transform.position) > m_baseCell.attackRange) {
                    m_baseCell.primaryTarget = null;
                    m_baseCell.Move(hitInfo.transform.position);

                } else {
                    if (m_baseCell.primaryTarget == null) {
                        m_baseCell.primaryTarget = hitInfo.transform.gameObject;
                        m_baseCell.currentState = CellState.ATTACK;
                    }
                }
                break;
            }
        }
        if (!targetFound) {
            m_baseCell.currentState = CellState.IDLE;
            if (!IsInvoking("RandomMove")) {
                InvokeRepeating("RandomMove", 1.0f, 5.0f);
            }
        }

    }

    void LateUpdate() {
        
    }

    void OnDrawGizmosSelected() {
        if (m_visionRange > 0.0f) {
			Gizmos.color = Color.red;
			Gizmos.DrawWireSphere(transform.position, m_visionRange);
		}
    }

    void RandomMove() {
        Vector3 _des = new Vector3(Random.Range(-5, 5), 0, Random.Range(-5, 5));
        m_baseCell.Move(_des + transform.position);
    }
}
