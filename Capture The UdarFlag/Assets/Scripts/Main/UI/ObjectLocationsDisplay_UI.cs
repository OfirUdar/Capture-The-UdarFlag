using Mirror;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ObjectLocationsDisplay_UI : MonoBehaviour
{


    [Header("Flag")]
    [SerializeField] private RectTransform _teammateSignFlag;
    [SerializeField] private RectTransform _teammateSignFlagArrow;
    [SerializeField] private RectTransform _opponentSignFlag;
    [SerializeField] private RectTransform _opponentSignFlagArrow;
    private Transform _teammateFlagTarget;
    private Transform _opponentFlagTarget;

    [Header("Teammate")]
    [SerializeField] private RectTransform[] _teammateSigns;
    [SerializeField] private RectTransform[] _teammateSignArrows;
    [SerializeField] private TextMeshProUGUI[] _teammateNameTexts;

    private List<Transform> _teammateTargets = new List<Transform>();



    private Camera _mainCamera;

    private Vector2 _screenSize;
    private Vector2 _flagImageHalfSize;
    private Vector2 _teammateImageHalfSize;

    private void Start()
    {
        _mainCamera = Camera.main;
        _screenSize = new Vector2(Screen.width, Screen.height);
        _flagImageHalfSize = transform.localScale.x * (_teammateSignFlag.sizeDelta) / 2;
        _teammateImageHalfSize = transform.localScale.x * (_teammateSigns[0].sizeDelta) / 2;
    }

    private void OnEnable()
    {
        PlayerManager.ClientOnTeammateConnect += ClientHandleTeammateConnect;
        PlayerManager.ClientOnTeammateDisconnect += ClientHandleTeammateDisconnect;

        Flag.ClientOnFlagSetTeam += ClientHandleFlagSetTeam;
    }

    private void OnDisable()
    {
        PlayerManager.ClientOnTeammateConnect -= ClientHandleTeammateConnect;
        PlayerManager.ClientOnTeammateDisconnect -= ClientHandleTeammateDisconnect;

        Flag.ClientOnFlagSetTeam -= ClientHandleFlagSetTeam;
    }


    private void LateUpdate()
    {
        //Follow Flags

        FlagsLocationHandler();

        //Follow Teammates
        TeammatesLocationHandler();
    }


    private void FlagsLocationHandler()
    {
        if (_teammateFlagTarget != null)
        {
            bool isVisible = IsVisible(_teammateFlagTarget);
            _teammateSignFlag.gameObject.SetActive(!isVisible);
            if (!isVisible)
                Follow(_teammateFlagTarget, _teammateSignFlag, _teammateSignFlagArrow, _flagImageHalfSize);
        }
        if (_opponentFlagTarget != null)
        {
            bool isVisible = IsVisible(_opponentFlagTarget);
            _opponentSignFlag.gameObject.SetActive(!isVisible);
            if (!isVisible)
                Follow(_opponentFlagTarget, _opponentSignFlag, _opponentSignFlagArrow, _flagImageHalfSize);
        }
    }

    private void TeammatesLocationHandler()
    {
        for (int i = 0; i < _teammateTargets.Count; i++)
        {
            if (_teammateTargets[i] == null) { continue; }
            bool isVisible = IsVisible(_teammateTargets[i]);
            _teammateSigns[i].gameObject.SetActive(!isVisible);
            if (!isVisible)
                Follow(_teammateTargets[i], _teammateSigns[i], _teammateSignArrows[i], _teammateImageHalfSize);
        }
    }

    private void Follow(Transform target, RectTransform signRectTransform, RectTransform arrowRectTransform, Vector2 imageSize)
    {
        Vector3 targetScreenPoint = _mainCamera.WorldToScreenPoint(target.position);

        Vector2 limitsX = new Vector2(imageSize.x, _screenSize.x - imageSize.x);
        Vector2 limitsY = new Vector2(imageSize.y, _screenSize.y - imageSize.y);

        targetScreenPoint *= Mathf.Sign(targetScreenPoint.z); // fix bug 

        Vector2 newPos;
        newPos.x = Mathf.Clamp(targetScreenPoint.x, limitsX.x, limitsX.y);
        newPos.y = Mathf.Clamp(targetScreenPoint.y, limitsY.x, limitsY.y);
        signRectTransform.position = newPos;


        Vector2 dir = -(signRectTransform.position - targetScreenPoint).normalized;
        arrowRectTransform.transform.up = dir;

    }
    private bool IsVisible(Transform target)
    {
        Vector3 targetScreenPoint = _mainCamera.WorldToViewportPoint(target.position);
        if (targetScreenPoint.x > 0 && targetScreenPoint.x < 1
           && targetScreenPoint.y > 0 && targetScreenPoint.y < 1)
        {
            return true;
        }
        return false;
    }

    private void ClientHandleFlagSetTeam(bool isTeammateFlag, Flag flag)
    {
        bool isFlagNull = flag == null;
        if (isTeammateFlag)
        {
            if (!isFlagNull)
            {
                _teammateFlagTarget = flag.transform;
                _teammateSignFlag.GetComponentInChildren<Image>().color = TeamsManager.Instance.teamAuthorityColor;
            }
            else
            {
                _teammateFlagTarget = null;
                _teammateSignFlag.gameObject.SetActive(false);
            }
        }
        else
        {
            if (!isFlagNull)
            {
                _opponentFlagTarget = flag.transform;
                _opponentSignFlag.GetComponentInChildren<Image>().color = TeamsManager.Instance.teamOpponentColor;
            }
            else
            {
                _opponentFlagTarget = null;
                _opponentSignFlag.gameObject.SetActive(false);
            }
        }
    }

    private void ClientHandleTeammateConnect(PlayerManager player)
    {
        _teammateTargets.Add(player.transform);

        int index = _teammateTargets.IndexOf(player.transform);
        _teammateNameTexts[index].text = player.playerLinks.gamePlayer.stats.GetPlayerName();
    }

    private void ClientHandleTeammateDisconnect(PlayerManager player)
    {
        int index = _teammateTargets.IndexOf(player.transform);
        _teammateSigns[index].gameObject.SetActive(false);

        _teammateTargets.Remove(player.transform);

    }
}
