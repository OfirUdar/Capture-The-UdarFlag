using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[System.Serializable]
public class Quest
{
    public string title;
    public string description;
    public QuestGoal questGoal;

    public Action OnQuestAccomplished;

    public void OneRequiredComplete(Goal goal)
    {
        if (goal != questGoal.goal) { return; }
        questGoal.AddCurrentAmount();

        if (questGoal.IsGoalAccomplished())
        {
            OnQuestAccomplished?.Invoke();
        }
    }
}
[System.Serializable]
public class QuestGoal
{
    public Goal goal;
    public int requiredAmount;
    public int currentAmount;

    public bool IsGoalAccomplished()
    {
        return (currentAmount >= requiredAmount);
    }

    public void AddCurrentAmount()
    {
        currentAmount++;
    }

}
public enum Goal
{
    Wait,
    Move,
    Run,
    Dash,
    CollectItem,
    UseItem,
    AimingOrThrowingItem,
    SwitchBetweenItems,
    RemoveItem
}

public class Tutorial : MonoBehaviour
{
    [SerializeField] private Animator _anim;
    [SerializeField] private TextMeshProUGUI _textTutorial;
    [SerializeField] private GameObject _tutorialCompletePanel;
    [SerializeField] private TMP_InputField _playerNameInputField;
    [Space]
    [SerializeField] private Quest[] _quests;


    private int _currentIndex = 0;


    private void Start()
    {
        Invoke(nameof(StartQuest),0.3f);
    }

    private void StartQuest()
    {
        //Display
        _anim.Rebind();
        _textTutorial.text = _quests[_currentIndex].description;
        PlayPopupAudio();
        //Start Quest
        SubscribeActions(true);
        _quests[_currentIndex].OnQuestAccomplished = QuestAccomplished;
        if (_quests[_currentIndex].questGoal.goal == Goal.Wait)
        {
            int waitSec = _quests[_currentIndex].questGoal.requiredAmount;
            Invoke(nameof(QuestAccomplished), waitSec);
        }
    }

    private void QuestAccomplished()
    {
        SubscribeActions(false);
        NextQuest();
    }
    private void NextQuest()
    {
        _currentIndex++;
        if (_currentIndex < _quests.Length)
            StartQuest();
        else
        {
            //Tutorial End
            PlayerPrefs.SetInt("IsTutorialMade", 1);
            _tutorialCompletePanel.SetActive(true);
            FindObjectOfType<PlayerMovement>().DisableMovement();
        }
    }

    private void PlayPopupAudio()
    {
        AudioManager.Instance.PlayOneShot("Popup");
    }


    public void SetPlayerName() //called from editor
    {
        PlayerPrefs.SetString("PlayerName", _playerNameInputField.text);
    }
    public void BackToMenu()//called from editor
    {
        if (NetworkServer.active)
            NetworkManager.singleton.StopHost();
        else
            NetworkManager.singleton.StopClient();
    }


    //EVENTS
    private void SubscribeActions(bool isSubscribe)
    {
        if (isSubscribe)
        {
            PlayerMovement.AuthortiyOnMoved += AuthorityHandleMoved;
            PlayerMovement.AuthortiyOnRan += AuthorityHandleRan;
            PlayerMovement.AuthortiyOnDashed += AuthorityHandleDashed;
            Item.AuthorityOnItemUsed += AuthorityHandleItemUsed;
            Item.AuthorityOnItemRemoved += AuthorityHandleItemRemoved;
            PlayerAiming.AuthorityOnAiming += AuthorityHandleAiming;
            BagManager.AuthorityOnAddedItem += AuthorityHandleAddedItem;
            BagManager.AuthorityOnItemSwitched += AuthorityHandleItemSwitched;
        }
        else
        {
            PlayerMovement.AuthortiyOnMoved -=AuthorityHandleMoved;
            PlayerMovement.AuthortiyOnRan -= AuthorityHandleRan;
            PlayerMovement.AuthortiyOnDashed -= AuthorityHandleDashed;
            Item.AuthorityOnItemUsed -= AuthorityHandleItemUsed;
            Item.AuthorityOnItemRemoved -= AuthorityHandleItemRemoved;
            PlayerAiming.AuthorityOnAiming -= AuthorityHandleAiming;
            BagManager.AuthorityOnAddedItem -= AuthorityHandleAddedItem;
            BagManager.AuthorityOnItemSwitched -= AuthorityHandleItemSwitched;
        }
    }

    private void AuthorityHandleMoved()
    {
        _quests[_currentIndex].OneRequiredComplete(Goal.Move);
    }
    private void AuthorityHandleRan()
    {
        _quests[_currentIndex].OneRequiredComplete(Goal.Run);
    }
    private void AuthorityHandleDashed()
    {
        _quests[_currentIndex].OneRequiredComplete(Goal.Dash);
    }


    private void AuthorityHandleItemUsed()
    {
        _quests[_currentIndex].OneRequiredComplete(Goal.UseItem);
    }
    private void AuthorityHandleAiming()
    {
        _quests[_currentIndex].OneRequiredComplete(Goal.AimingOrThrowingItem);
    }
    private void AuthorityHandleItemRemoved()
    {
        _quests[_currentIndex].OneRequiredComplete(Goal.RemoveItem);
    }


    private void AuthorityHandleAddedItem(List<Item> items)
    {
        _quests[_currentIndex].OneRequiredComplete(Goal.CollectItem);
    }
    private void AuthorityHandleItemSwitched(List<Item> items,int currentIndexItem)
    {
        _quests[_currentIndex].OneRequiredComplete(Goal.SwitchBetweenItems);
    }
}
