using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Base : NetworkBehaviour
{
    [SyncVar]
    private Team _baseTeam;




    public Team GetTeam()
    {
        return _baseTeam;
    }


    #region Server

    [Server]
    public void SetTeam(Team team)
    {
        _baseTeam = team;
    }

    #endregion

}
