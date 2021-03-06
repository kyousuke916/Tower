﻿using UnityEngine;
using System.Collections;

public abstract class MonsterAI : MonoBehaviour
{
    public static readonly int IDLE_HASH = Animator.StringToHash("idle");
    public static readonly int SPAWN_HASH = Animator.StringToHash("spawn");
    public static readonly int DEATH_HASH = Animator.StringToHash("death");
    public static readonly int ATTACK_HASH = Animator.StringToHash("attack");
    public static readonly int DANCE_HASH = Animator.StringToHash("dance");
    public static readonly int WALK_HASH = Animator.StringToHash("walk");

    [SerializeField]
    protected float m_DetectInterval = 0.5f;

    [SerializeField]
    protected string m_MonsterID = "Unknow";

    [SerializeField]
    protected MonsterParam m_OriginParam = null;

    [SerializeField]
    protected MonsterParam m_UseParam = null;

    public string MonsterID { get { return m_MonsterID; } }

    protected Transform mTs = null;

    protected Transform mTargetTs = null;

    protected Animator mAni = null;

    protected NavMeshAgent mAgent = null;

    private Coroutine mDetectCoroutine = null;

    protected bool IsAlive { get { return m_UseParam.HP > 0; } }
    protected bool IsDeath { get { return !IsAlive; } }

    protected virtual void Awake()
    {
        mTs = transform;

        mAgent = GetComponent<NavMeshAgent>();
        mAni = GetComponentInChildren<Animator>();
    }
    
    protected virtual void OnDestroy()
    {
        
    }

    protected virtual void Update()
    {
        WalkUpdate();
    }

    protected void StartDetect()
    {
        StopDetect();

        mDetectCoroutine = StartCoroutine(LoopDetect());
    }

    protected IEnumerator LoopDetect()
    {
        while (true)
        {
            Follow();

            yield return new WaitForSeconds(m_DetectInterval);
        }
    }

    public void SetEnable()
    {
        gameObject.SetActive(true);

        IninParam();
    }

    public void SetDisable()
    {
        gameObject.SetActive(false);
    }

    private void IninParam()
    {
        m_UseParam.SetTo(m_OriginParam);

        mAgent.enabled = true;
        mAgent.speed = m_UseParam.MoveSpeed;

        SetSpawn();
    }

    protected void Follow()
    {
        if (mTargetTs != null)
            mAgent.SetDestination(mTargetTs.position);
    }

    protected void StopDetect()
    {
        if (mDetectCoroutine != null)
        {
            StopCoroutine(mDetectCoroutine);
            mDetectCoroutine = null;

            mAgent.Stop();
        }
    }

    public void SetPosition(Vector3 position)
    {
        mTs.position = position;
    }

    public void SetRotation(Quaternion rotation)
    {
        mTs.rotation = rotation;
    }

    public void SetTarget(Transform targetTs)
    {
        mTargetTs = targetTs;
    }

    public int getHp()
	{
        return m_UseParam.HP;
	}

    public void Damage(int damage)
    {
        m_UseParam.HP = Mathf.Max(0, m_UseParam.HP - damage);

        if (IsDeath)
            SetDeath();
    }

    protected void SetSpawn()
    {
        StartCoroutine(SpawnHandle());
    }

    protected IEnumerator SpawnHandle()
    {
        StopDetect();
        SetAnimTrigger(MonsterAI.SPAWN_HASH);

        yield return new WaitForSeconds(1f);

        if (IsAlive)
            SetIdle();
    }

    protected void SetIdle()
    {
        StartCoroutine(IdleHandle());
    }

    protected IEnumerator IdleHandle()
    {
        StopDetect();
        SetAnimTrigger(MonsterAI.IDLE_HASH);

        yield return new WaitForSeconds(0.1f);

        if (IsAlive)
            StartDetect();
    }

    protected void SetDeath()
    {
        StartCoroutine(DeathHandle());
    }

    protected IEnumerator DeathHandle()
    {
        StopDetect();
        SetAnimTrigger(MonsterAI.DEATH_HASH);

        mAgent.enabled = false;

        yield return new WaitForSeconds(5f);

        MonsterPools.Retrieve(this);
    }

    protected void SetAttack()
    {
        StopDetect();
        SetAnimTrigger(MonsterAI.ATTACK_HASH);
    }

    protected void SetDance()
    {
        StopDetect();
        SetAnimTrigger(MonsterAI.DANCE_HASH);
    }

    protected void WalkUpdate()
    {
        bool isWalk = !mAgent.velocity.normalized.Equals(Vector3.zero);

        SetAnimBool(MonsterAI.WALK_HASH, isWalk);
    }

    protected void SetAnimTrigger(int hash)
    {
        mAni.SetTrigger(hash);
    }

    protected void SetAnimBool(int hash, bool value)
    {
        mAni.SetBool(hash, value);
    }

    [System.Serializable]
    public class MonsterParam
    {
        public int HP = 0;
        public float MoveSpeed = 2f;

        public void SetTo(MonsterParam param)
        {
            HP = param.HP;
            MoveSpeed = param.MoveSpeed;
        }
    }

}
