using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

//守卫，巡逻，追击，死亡
public enum EnemyStates { GUARD, PATROL, CHASE, DEAD };

//该特性会自动判断挂载当前脚本的物体是否有NavMeshAgent组件，如果没有就自动添加。
[RequireComponent(typeof(NavMeshAgent))]
public class EnemyController : MonoBehaviour,IEndGameObserver
{
    private NavMeshAgent agent;

    private EnemyStates enemyStates;

    private Animator anim;

    [Header("Basic Settings")]
    //视线范围
    public float sightRaidus;

    [Header("Patrol State")]
    //敌人移动范围
    public float patrolRange;

    //攻击目标
    private GameObject attackTarget;

    //初始状态
    public bool isGuard = true;

    //记录原有的速度
    private float speed;

    //bool配合动画
    bool isWalk;
    bool isChase;
    bool isFollow;
    bool isDead;
    //起始位置
    private Vector3 guardPos;
    //起始朝向
    private Quaternion guardRotation;

    //随机巡逻点
    private Vector3 wayPoint;

    //巡逻时间
    public float lookAtTime;
    private float remainLookAtTime;

    private float laseAttackTime;

    private CharacterStats characterStats;

    bool playerDead=false;

    private void Awake()
    {
        characterStats = GetComponent<CharacterStats>();
        anim = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        //记录原本的速度
        speed = agent.speed;
        guardPos = transform.position;
        guardRotation = transform.rotation;
        wayPoint = transform.position;

    }

    private void Start()
    {
        //Debug.Log("Enemy:" + GameManager.isInitialized);
        GameManager.Instance.AddObserver(this);
        //初始化状态，判断是否是站桩
        if (isGuard)
        { 
            enemyStates = EnemyStates.GUARD;
        }
        else//否则巡逻的敌人
        {
            enemyStates = EnemyStates.PATROL;
        }
        remainLookAtTime = lookAtTime;
    }

    //void OnEnable()
    //{
    //    //Debug.Log("Enemy:"+GameManager.isInitialized);
    //    //添加观察者
    //    GameManager.Instance.AddObserver(this);
    //}

    void OnDisable()
    {
        //移除观察者
        GameManager.Instance.RemoveObserver(this);
    }

    void Update()
    {
        isDead = characterStats.CurrentHealth == 0;

        if (!playerDead)
        {
            SwitchState();
            SwitchAnimation();
            laseAttackTime -= Time.deltaTime;
        }
        
    }

    void SwitchAnimation()
    {
        anim.SetBool("Walk", isWalk);
        anim.SetBool("Chase", isChase);
        anim.SetBool("Follow", isFollow);
        anim.SetBool("Critical", characterStats.isCritical);
        anim.SetBool("Die", isDead);
    }

    void SwitchState()
    {
        if (isDead)
        {
            enemyStates = EnemyStates.DEAD;
        }
        //查找范围内是否存在Player
        else if (FoundPlayer())
        {
            //修改自身状态
            enemyStates = EnemyStates.CHASE;
            //Debug.Log("找到了");
        }

        switch (enemyStates)
        {
            //站桩
            case EnemyStates.GUARD:
                //取消追击动画
                isChase = false;
                //回到守卫地点
                if (transform.position != guardPos)
                {
                    isWalk = true;
                    agent.isStopped = false;
                    agent.destination = guardPos;
                    //SqrMagnitude 计算两个坐标之间的距离 相较Distance而言 更节省系统开销
                    if (Vector3.SqrMagnitude(guardPos - transform.position) <= agent.stoppingDistance)
                    {
                        isWalk = false;
                        //需要将Enemy朝向设置为原本的朝向
                        transform.rotation = Quaternion.Lerp(transform.rotation, guardRotation, 0.01f);
                    }
                }
                break;
            //巡逻状态
            case EnemyStates.PATROL:
                //巡逻时速度减半，使用乘法效率更高
                agent.speed = speed * 0.5f;
                //取消追击
                isChase = false;
                //当目标距离和当前距离小于等于 停止的距离，就获取新的点
                if (Vector3.Distance(wayPoint, transform.position) <= agent.stoppingDistance)
                {
                    isWalk = false;
                    //倒计时
                    if (remainLookAtTime > 0)
                    {
                        remainLookAtTime -= Time.deltaTime;
                    }
                    else
                    {
                        //倒计时结束之后开始新的移动
                        GetNewWayPoint();
                    }
                }
                else
                {
                    isWalk = true;
                    agent.destination = wayPoint;
                }

                break;
            //追击状态
            case EnemyStates.CHASE:
                isWalk = false;
                isChase = true;

                //追击时变为原本的速度
                agent.speed = speed;
                //跟丢了攻击目标
                if (!FoundPlayer())
                {
                    //拉脱回到原本状态
                    isFollow = false;
                    //当Player脱离敌人追赶，敌人要在原地观望一会
                    if (remainLookAtTime > 0)
                    {
                        //停留在原地
                        agent.destination = transform.position;
                        remainLookAtTime -= Time.deltaTime;
                    }
                    else if (isGuard)//返回原本状态
                    {
                        enemyStates = EnemyStates.GUARD;
                    }
                    else//返回原本状态
                    {
                        enemyStates = EnemyStates.PATROL;
                    }
                }
                else
                {
                    //配合动画
                    isFollow = true;
                    //继续移动
                    agent.isStopped = false;
                    //追赶Player
                    agent.SetDestination(attackTarget.transform.position);
                }
                //在攻击范围内则攻击
                if (TargetInAttackRange() || TargetInSkillRange())
                {
                    //停止跟随动画
                    isFollow = false;
                    //停止跟随
                    agent.isStopped = true;
                    //cd倒计时
                    if (laseAttackTime < 0)
                    {
                        laseAttackTime = characterStats.attackData.collDown;
                        //暴击判断
                        characterStats.isCritical = Random.value < characterStats.attackData.criticalChance;
                        //执行攻击
                        Attack();
                    }

                }
                break;
            case EnemyStates.DEAD:
                //关闭NavMeshAgent组件
                agent.enabled = false;
                //关闭Collider，玩家就无法攻击死亡的敌人
                gameObject.GetComponent<Collider>().enabled = false;
                Destroy(gameObject, 3f);
                break;

        }
    }

    private void Attack()
    {
        //让敌人面朝玩家
        transform.LookAt(attackTarget.transform);
        //判断是那种攻击
        if (TargetInAttackRange())
        {
            //近身攻击动画
            anim.SetTrigger("Attack");
        }
        if (TargetInSkillRange())
        {
            //技能攻击动画
            anim.SetTrigger("Skill");
        }
    }

    bool FoundPlayer()
    {
        //查找sightRaidus范围内的Player
        Collider[] colliders = Physics.OverlapSphere(transform.position, sightRaidus);
        foreach (Collider collider in colliders)
        {
            if (collider.CompareTag("Player"))
            {
                attackTarget = collider.gameObject;
                return true;

            }
        }
        return false;
    }

    //判断攻击目标是否在攻击范围内
    bool TargetInAttackRange()
    {
        if (attackTarget != null)
        {
            //当玩家和Enemy之间的距离小于 Enemy的攻击距离 返回true否则返回false
            return Vector3.Distance(attackTarget.transform.position, transform.position) <= characterStats.attackData.attackRange;
        }
        else
        {
            return false;
        }
    }
    bool TargetInSkillRange()
    {
        if (attackTarget != null)
        {
            return Vector3.Distance(attackTarget.transform.position, transform.position) <= characterStats.attackData.skillRange;
        }
        else
        {
            return false;
        }
    }

    void GetNewWayPoint()
    {
        //重置巡逻时间
        remainLookAtTime = lookAtTime;

        //x，z轴随机生成偏移量
        float randomX = Random.Range(-patrolRange, patrolRange);
        float randomZ = Random.Range(-patrolRange, patrolRange);
        //随机生成的点
        Vector3 randomPos = new Vector3(guardPos.x + randomX, transform.position.y, guardPos.z + randomZ);
        NavMeshHit hit;
        //判断该点是否在NavMesh的NavMesh的1号Area中，序号从1开始，1代表Walkable,如果是可以到达的位置就返回这个点的坐标，如果不可到达就返回自身坐标，再重新随机一个坐标
        wayPoint = NavMesh.SamplePosition(randomPos, out hit, patrolRange, 1) ? randomPos : transform.position;

    }

    //Scene面板中，选择一个物体，会显示画的范围
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        //敌人的视线范围
        Gizmos.DrawWireSphere(transform.position, sightRaidus);
    }

    void Hit()
    {
        if (attackTarget != null)
        {
            var targetStats = attackTarget.GetComponent<CharacterStats>();
            characterStats.TakeDemage(characterStats, targetStats);
        }
        
    }

    public void EndNotify()
    {
        attackTarget = null;
        //玩家死亡
        playerDead = true;
        //胜利动画
        isChase = false;
        isWalk = false;
        anim.SetBool("Win", true);
        //停止移动
        //停止Agent
        agent.isStopped = true;

    }
}
