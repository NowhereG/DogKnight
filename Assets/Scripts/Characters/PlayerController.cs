using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PlayerController : MonoBehaviour,IEndGameObserver
{
    private NavMeshAgent agent;

    private Animator anim;

    private GameObject attackTarget;

    private float lastAttackTime;
    //获取玩家数值
    private CharacterStats characterStats;

    private bool isDead;
    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        //anim = GetComponent<Animator>();
        anim = GetComponent<Animator>();
        characterStats = GetComponent<CharacterStats>();
    }

    private void Start()
    {
        //添加事件
        MouseManager.Instance.OnMouseClicked += MoveToTarget;
        MouseManager.Instance.OnEnemyClicked += EventAttack;

        //Debug.Log(GameManager.isInitialized);
        //访问单例
        GameManager.Instance.RegisterPlayer(characterStats);
    }

    private void Update()
    {
        isDead = characterStats.CurrentHealth == 0;

        if (isDead)
        {
            //广播
            GameManager.Instance.NotifyObservers();
        }

        SwitchAnimation();
        lastAttackTime -= Time.deltaTime;
    }

    private void SwitchAnimation()
    {
        //通过agent.velocity值来决定移动速度
        //sqrMagnitude：将vector3类型转换成float
        anim.SetFloat("Speed", agent.velocity.sqrMagnitude);
        anim.SetBool("Die", isDead);
    }

    //当调用事件的时候，这个函数会被执行
    private void MoveToTarget(Vector3 target)
    {
        //当玩家正在攻击敌人时点击其它地方，立即停止攻击，移动位置
        agent.isStopped = false;

        if (isDead) return;

        //关闭所有携程
        StopAllCoroutines();
        //移动位置
        agent.SetDestination(target);
    }

    private void EventAttack(GameObject target)
    {
        if (isDead) return;

        //需要先判断攻击目标存在
        if (target != null)
        {
            //记录攻击目标
            attackTarget = target;
            //执行攻击之前，先判断是否发生暴击
            characterStats.isCritical = UnityEngine.Random.value < characterStats.attackData.criticalChance;
            //启动协程
            StartCoroutine(MoveToAttackTarget());
        } 
    }

    IEnumerator MoveToAttackTarget()
    {
        agent.isStopped = false;

        transform.LookAt(attackTarget.transform);
        
        //当攻击目标和玩家之间的距离大于攻击距离，就让玩家移动到攻击目标面前
        while (Vector3.Distance(attackTarget.transform.position, transform.position) > characterStats.attackData.attackRange)
        {
            agent.SetDestination(attackTarget.transform.position);
            //执行循环
            yield return null;
        }

        //当玩家到达攻击目标面前，攻击
        //停下来攻击敌人
        agent.isStopped = true;

        if (lastAttackTime < 0)
        {
            anim.SetBool("Critical", characterStats.isCritical);
            anim.SetTrigger("Attack");
            //重置攻击时间
            lastAttackTime = characterStats.attackData.collDown;
        }
    }

    void Hit()
    {
        var targetStats = attackTarget.GetComponent<CharacterStats>();
        characterStats.TakeDemage(characterStats, targetStats);
    }

    public void EndNotify()
    {
        
    }
}
