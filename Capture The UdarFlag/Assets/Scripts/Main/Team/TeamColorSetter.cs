using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class TeamColorSetter : MonoBehaviour
{


    //Object Type
    public ObjectType objectType;
    public enum ObjectType
    {
        Flag,
        Base,
        Player
    }

    public Flag flag;
    public Base flagBase;
    public PlayerManager player;

    //Renderer Type
    public RendererType rendererType;
    public enum RendererType
    {
        Renderer,
        Sprite,
        Image,
        Text,
        ParticleSystem
    }

    public Renderer[] renderers;
    public SpriteRenderer[] spriteRenderers;
    public Image[] images;
    public TextMeshProUGUI[] texts;
    public ParticleSystem[] particleSystems;


    private Color _teamAuthorityColor;
    private Color _teamOpponentColor;



    private void Start()
    {
        GamePlayer connPlayer = NetworkClient.connection.identity.GetComponent<GamePlayer>();
        _teamAuthorityColor = TeamsManager.Instance.teamAuthorityColor;
        _teamOpponentColor = TeamsManager.Instance.teamOpponentColor;
        SetupColor(connPlayer);
    }

    private void SetupColor(GamePlayer connPlayer)
    {
        bool isTeammate = true;
        Color teamColor;

        switch (objectType)
        {
            case ObjectType.Flag:
                {
                    isTeammate = connPlayer.IsTeammate(flag.GetTeam());
                    break;
                }
            case ObjectType.Base:
                {
                    isTeammate = connPlayer.IsTeammate(flagBase.GetTeam());
                    break;
                }
            case ObjectType.Player:
                {
                    isTeammate = connPlayer.IsTeammate(player.playerLinks.gamePlayer);
                    break;
                }
        }

        teamColor = isTeammate ? _teamAuthorityColor : _teamOpponentColor;

        switch (rendererType)
        {
            case RendererType.Renderer:
                {
                    foreach (Renderer renderer in renderers)
                        renderer.material.SetColor("_Color", teamColor);
                    break;
                }
            case RendererType.Sprite:
                {
                    foreach (SpriteRenderer sprite in spriteRenderers)
                        sprite.color = teamColor;
                    break;
                }
            case RendererType.Image:
                {
                    foreach (Image image in images)
                        image.color = teamColor;
                    break;
                }
            case RendererType.Text:
                {
                    foreach (TextMeshProUGUI text in texts)
                        text.color = teamColor;
                    break;
                }
            case RendererType.ParticleSystem:
                {
                    foreach (ParticleSystem particleSystem in particleSystems)
                    {
                        var main = particleSystem.main;
                        main.startColor = teamColor;
                    }
                    break;
                }
        }


        this.enabled = false;

    }
}

