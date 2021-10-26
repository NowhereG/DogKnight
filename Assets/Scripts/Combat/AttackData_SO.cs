using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="New AttackData",menuName="Attack/Attack Data")]
//攻击数值的SO文件
public class AttackData_SO : ScriptableObject
{
    //攻击范围
    public float attackRange;
    //远程技能攻击范围
    public float skillRange;
    //技能冷却时间
    public float collDown;
    //最低伤害
    public int minDamage;
    //最高伤害
    public int maxDamage;
    //暴击伤害 加成百分比
    public float criticalMultiplier;
    //暴击几率
    public float criticalChance;
}
