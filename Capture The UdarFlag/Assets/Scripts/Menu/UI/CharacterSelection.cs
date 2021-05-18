using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSelection : MonoBehaviour
{
    public static CharacterSelection Instance { get; private set; }

    [SerializeField] private CharacterSO[] _charactersSO;
    [SerializeField] private Transform _charactersContainer;

    private List<GameObject> _charactersModels = new List<GameObject>();
    private int _currentIndexCharacter;


    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        foreach(CharacterSO character in _charactersSO)
        {
            GameObject characterInstance=
                Instantiate(character.model, _charactersContainer);
            characterInstance.SetActive(false);
            _charactersModels.Add(characterInstance);
        }
        _charactersModels[_currentIndexCharacter].SetActive(true);
    }



    public void NextCharacter()
    {
        _charactersModels[_currentIndexCharacter].SetActive(false);
        if (_currentIndexCharacter + 1 >= _charactersSO.Length)
            _currentIndexCharacter = 0;
        else
            _currentIndexCharacter++;
        _charactersModels[_currentIndexCharacter].SetActive(true);
    }
    public void PrevCharacter()
    {
        _charactersModels[_currentIndexCharacter].SetActive(false);
        if (_currentIndexCharacter - 1 < 0)
            _currentIndexCharacter = _charactersSO.Length-1;
        else
            _currentIndexCharacter--;
        _charactersModels[_currentIndexCharacter].SetActive(true);
    }

    public CharacterSO GetCurrentCharacter()
    {
        return _charactersSO[_currentIndexCharacter];
    }
    public int GetCurrentIndexCharacter()
    {
        return _currentIndexCharacter;
    }
    public CharacterSO GetCharacter(int index)
    {
        return _charactersSO[index];
    }


}
