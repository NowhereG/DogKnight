using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//用来对CharacterData_SO中的数值进行操作
public class CharacterStats : MonoBehaviour
{
    //声明CharacterData_SO变量
    public CharacterData_SO characterData;

    public AttackData_SO attackData;
    //在Inspector面板中隐藏
    [HideInInspector]
    public bool isCritical;

    #region Read from Data_SO
    public int MaxHealth
    {
        get { if (characterData != null) return characterData.maxHealth; else return 0; }
        set { characterData.maxHealth = value; }
    }   
    public int CurrentHealth
    {
        get { if (characterData != null) return characterData.currentHealth; else return 0; }
        set { characterData.currentHealth = value; }
    }

    public int BaseDefence
    {
        get { if (characterData != null) return characterData.baseDefence; else return 0; }
        set { characterData.baseDefence = value; }
    }

    public int CurrentDefence
    {
        get { if (characterData != null) return characterData.currentDefence; else return 0; }
        set { characterData.currentDefence = value; }
    }
    #endregion

    #region character combat

    public void TakeDemage(CharacterStats attacker,CharacterStats defender)
    {
        //所以当造成的伤害小于目标防御力时，就将伤害值改为0
        int demage = Mathf.Max(attacker.CurrentDemage() - defender.CurrentDefence,0);
        //防止生命值小于0
        defender.CurrentHealth = Mathf.Max(defender.CurrentHealth - demage, 0);
        //受到暴击伤害，就播放defender的GetHit动画
        if (isCritical)
        {
            //受到暴击伤害，就播放defender的GetHit动画
            defender.GetComponent<Animator>().SetTrigger("Hit");
        }
        
    }

    private int CurrentDemage()
    {
        //随机伤害
        float coreDemage = UnityEngine.Random.Range(attackData.minDamage, attackData.maxDamage);
        //判断是否暴击
        if (isCritical)
        {
            //伤害乘上暴击倍率
            coreDemage *= attackData.criticalMultiplier;
            Debug.Log("暴击！" + coreDemage);
        }
        else{
            Debug.Log("普通攻击！" + coreDemage);
        }
        //返回伤害值
        return (int)coreDemage;
    }
    #endregion
}
