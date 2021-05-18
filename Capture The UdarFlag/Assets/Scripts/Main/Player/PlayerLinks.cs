using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Smooth;
using Mirror;
public class PlayerLinks : NetworkBehaviour
{
    public PlayerManager playerManager;
    public SmoothSyncMirror smoothSync;
    public Rigidbody rigidBody;
    public Collider coll;
    public Health health;
    public PlayerMovement movement;
    public PlayerAiming playerAiming;
    public BagManager bagManager;
    public ActiveItem activeItem;
    public ItemCollector itemCollector;
    public PrisonerDetector prisonerDetector;
    public CircleFiller prisonerCirceFiller;
    public PlayerAnimationManager animManager;
    public AudioOB playerAudio;
    public AudioListener audioListener;
    [HideInInspector]
    public GamePlayer gamePlayer;



}
