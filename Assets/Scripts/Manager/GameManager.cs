using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    private CharacterStats playerStats;
    //保存所有观察者
    List<IEndGameObserver> endGameObservers = new List<IEndGameObserver>();
    public void RegisterPlayer(CharacterStats player)
    {
        playerStats = player;
    }

    //添加观察者
    public void AddObserver(IEndGameObserver observer)
    {
        endGameObservers.Add(observer);
    }

    //移除观察者
    public void RemoveObserver(IEndGameObserver observer)
    {
        endGameObservers.Remove(observer);
    }

    //广播，通知所有观察者
    public void NotifyObservers()
    {
        foreach (var observer in endGameObservers)
        {
            observer.EndNotify();
        }
    }
}
