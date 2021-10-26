using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//作用：在create菜单当中创建子集：就是Character Stats子集中的Data文件，默认文件名为New Data。创建的Data文件包含该类中的属性
[CreateAssetMenu(fileName = "New Data", menuName = "Character Stats/Data")]
public class CharacterData_SO : ScriptableObject
{
    [Header("Stats Info")]
    //最大血量
    public int maxHealth;
    //当前血量
    public int currentHealth;
    //基本防御
    public int baseDefence;
    //当前防御
    public int currentDefence;


}
