using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;
public class PlayerMovement : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerLinks _playerLinks;
    [SerializeField] private LayerMask _playerCollisionLayerMask;
    [Space]
    [SerializeField] private float _speedJogging = 3.5f;
    [SerializeField] private float _speedRunning = 4.5f;
    [SerializeField] private float _speedAiming = 2.5f;
    [Space]
    [SerializeField] private float _maxSprintStamina = 500;
    [Space]
    [SerializeField] private float _dashDistance = 1.5f;
    [Header("Audio")]
    [SerializeField] private Audio _joggingAudio;
    [SerializeField] private Audio _runningAudio;
    [SerializeField] private Audio _walkingAimingAudio;
    [SerializeField] private Audio _dashingAudio;


    //Movement
    private Vector3 _serverMovement;
    private bool _isCameraOnReverseMode = false;
    //Anim
    private bool _isSentMovementToServer = false; // it means that when the player stop inputing send once to server that the movement is 0    
    private float _velocityZ;
    private float _velocityX;

    [SyncVar(hook = nameof(AuthorityHandleSprintStaminaChanged))]
    private float _currentSprintStamina;
    private float _energySprintStamina;
    private bool _isRunningButtonDown;
    public static event Action<float> AuthorityOnSprintStaminaChanged;


    private enum MovementState
    {
        Idle,
        Jogging,
        Running,
        IdleAiming,
        WalkingAiming
    }
    [SyncVar(hook = nameof(ClientHandleMovementStateChanged))]
    private MovementState _movementState;
    private Dictionary<MovementState, float> _speedMovementDictionary = new Dictionary<MovementState, float>();


    //Tutorial
    public static event Action AuthortiyOnMoved;
    public static event Action AuthortiyOnRan;
    public static event Action AuthortiyOnDashed;

    private void Start()
    {
        _energySprintStamina = (_maxSprintStamina / 3f);
    }

    private void Update()
    {
        if (hasAuthority)
            InputHandler();

        if (isServer)
        {
            if (_movementState != MovementState.Running)
            {
                if (!_isRunningButtonDown || _movementState != MovementState.Jogging)
                {
                    SprintStaminaOnNotRunning();
                }
            }
            MovementAnimate();
        }
    }

    #region Server

    public override void OnStartServer()
    {
        _currentSprintStamina = _maxSprintStamina;

        _speedMovementDictionary.Add(MovementState.Idle, 0);
        _speedMovementDictionary.Add(MovementState.Jogging, _speedJogging);
        _speedMovementDictionary.Add(MovementState.Running, _speedRunning);
        _speedMovementDictionary.Add(MovementState.WalkingAiming, _speedAiming);
    }

    [Server]
    private void MovementAnimate()
    {
        _playerLinks.animManager.SetVelocites(_velocityZ, _velocityX);
    }

    [Server]
    public void RotateToDirection(Vector3 direction)
    {
        transform.forward = direction.normalized;
    }

    [Server]
    public void SetMovementAiming(bool isAiming)
    {
        if (isAiming)
        {
            if (_movementState == MovementState.Jogging || _movementState == MovementState.Running)
                _movementState = MovementState.WalkingAiming;
            else
                _movementState = MovementState.IdleAiming;
        }
        else
        {
            _movementState = MovementState.Idle;
        }

    }

    [Server]
    private void SprintStaminaOnRunning()
    {
        if (_currentSprintStamina > 0)
            _currentSprintStamina = Math.Max(0, _currentSprintStamina - 1.5f);
    }

    [Server]
    private void SprintStaminaOnNotRunning()
    {
        if (_currentSprintStamina < _maxSprintStamina)
            _currentSprintStamina = Math.Min(_maxSprintStamina, _currentSprintStamina + 1);
    }

    [Server]
    private IEnumerator Dash(Vector3 destinationPosition, float startDelay) //Dashing
    {
        yield return new WaitForSeconds(startDelay);
        while ((transform.position - destinationPosition).sqrMagnitude > 0.01)
        {
            transform.position = Vector3.Lerp(transform.position, destinationPosition, 0.3f);
            yield return null;
        }
    }

    [Server]
    private void UpdateMovementState(bool isMove, bool isAiming, bool isRunning)//Update the movementState
    {
        if (isMove)
        {
            if (isAiming) _movementState = MovementState.WalkingAiming;
            else if (isRunning) _movementState = MovementState.Running;
            else if (!isRunning && !isAiming) _movementState = MovementState.Jogging;
        }
        else
        {
            if (_movementState == MovementState.WalkingAiming)
                _movementState = MovementState.IdleAiming;
            else
                _movementState = MovementState.Idle;
        }
    }

    [Command]
    private void CmdMove(Vector3 movement, float deltaTime, bool isRunningButtonDown)
    {
        _serverMovement = movement;
        _isRunningButtonDown = isRunningButtonDown;

        bool isAiming = _movementState == MovementState.WalkingAiming || _movementState == MovementState.IdleAiming;
        bool isRunning = isRunningButtonDown && _currentSprintStamina > 0 && !isAiming;// If player is running and no aiming   
        bool isMove = _serverMovement != Vector3.zero;


        UpdateMovementState(isMove, isAiming, isRunning); //Update the movementState
        if (isMove) // If there is movement
        {
            if (deltaTime > 0.1f) //prevent speed hacks
                deltaTime = Time.deltaTime;

            //Move:
            _serverMovement.Normalize();
            _serverMovement *= _speedMovementDictionary[_movementState] * deltaTime;
            transform.Translate(_serverMovement, Space.World);

            if (!isAiming)
            {
                //Rotate to direction of moving:
                _playerLinks.playerAiming.SlerpRotation(_serverMovement);
            }
        }

        //Animate
        _velocityZ = Vector3.Dot(_serverMovement.normalized, transform.forward);
        _velocityZ *= isRunning ? 2f : 1f;

        _velocityX = Vector3.Dot(_serverMovement.normalized, transform.right);

        //Handle sprint stamina
        if (isRunning)
            SprintStaminaOnRunning();
    }

    [Command]
    private void CmdDash(Vector3 dashDirection)
    {
        if (_currentSprintStamina - _energySprintStamina < 0) { return; }

        _playerLinks.animManager.Dash();
        StopAllCoroutines();
        if (Physics.Raycast(transform.position, transform.forward,
        out RaycastHit hitInfo, _dashDistance, _playerCollisionLayerMask))
        {
            float minDistanceDash = 0.5f;
            if ((hitInfo.point - transform.position).
                sqrMagnitude < minDistanceDash * minDistanceDash) { return; }
            Vector3 destinationPosition = hitInfo.point;
            StartCoroutine(Dash(destinationPosition, 0.1f));
        }
        else
        {
            Vector3 destinationPosition = transform.position + dashDirection.normalized * _dashDistance;
            StartCoroutine(Dash(destinationPosition, 0.1f));
        }

        _currentSprintStamina = Math.Max(0, _currentSprintStamina - _energySprintStamina);
    }

    #endregion

    #region Client

    public override void OnStartAuthority()
    {
        CameraFollow[] camerasFollow = FindObjectsOfType<CameraFollow>();
        foreach(CameraFollow cameraFollow in camerasFollow)
        {
            cameraFollow.SetupCamera(this.transform);
            //_isCameraOnReverseMode = cameraFollow.SetupCamera(this.transform);
        }      
    }

    [Client]
    private void InputHandler()
    {
        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        bool isDashing = Input.GetKeyDown(KeyCode.Space);

        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Movement(horizontal, vertical, isRunning, isDashing);
    }
    [Client]
    private void Movement(float horizontal, float vertical, bool isRunning, bool isDashing)
    {
        //Vector3 movement = new Vector3(horizontal, 0f, vertical);
        Vector3 movement = new Vector3(vertical, 0f, -horizontal);
        //movement *= _isCameraOnReverseMode ? -1 : 1;
        if (movement.x != 0 || movement.z != 0)
        {
            if (isDashing)
            {
                if (_currentSprintStamina - _energySprintStamina >= 0)
                {
                    CmdDash(movement);
                    _playerLinks.playerAudio.PlayOneShot(_dashingAudio.audioClip, _dashingAudio.volume);
                    AuthortiyOnDashed?.Invoke();
                }
            }
            else
            {
                CmdMove(movement, Time.deltaTime, isRunning);
                _isSentMovementToServer = false;
                if (isRunning)
                    AuthortiyOnRan?.Invoke();
                else
                    AuthortiyOnMoved?.Invoke();
            }
        }
        else
        {
            if (!_isSentMovementToServer)
            {
                CmdMove(Vector3.zero, Time.deltaTime, isRunning);
                _isSentMovementToServer = true;
            }
        }
    }

    [Client]
    private void AuthorityHandleSprintStaminaChanged(float oldSprintStamina, float newSprintStamina)
    {
        if (hasAuthority)
            AuthorityOnSprintStaminaChanged?.Invoke(newSprintStamina / _maxSprintStamina);
    }

    [Client]
    private void ClientHandleMovementStateChanged(MovementState oldMovementState, MovementState newMovementState)
    {

        switch (_movementState)
        {
            case MovementState.Jogging:
                {
                    _playerLinks.playerAudio.Play(_joggingAudio);
                    break;
                }
            case MovementState.Running:
                {
                    _playerLinks.playerAudio.Play(_runningAudio);
                    break;
                }
            case MovementState.WalkingAiming:
                {
                    _playerLinks.playerAudio.Play(_walkingAimingAudio);
                    break;
                }
            default:
                {
                    _playerLinks.playerAudio.Stop();
                    break;
                }
        }
    }


    public void DisableMovement()
    {
        this.enabled = false;
        _playerLinks.playerAudio.Stop();
    }
    #endregion

}
