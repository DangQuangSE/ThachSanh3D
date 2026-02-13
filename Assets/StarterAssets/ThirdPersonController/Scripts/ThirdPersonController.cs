using UnityEngine;
#if ENABLE_INPUT_SYSTEM 
using UnityEngine.InputSystem;
#endif

/* Note: animations are called via the controller for both the character and capsule using animator null checks
 */

namespace StarterAssets
{
    [RequireComponent(typeof(CharacterController))]
#if ENABLE_INPUT_SYSTEM 
    [RequireComponent(typeof(PlayerInput))]
#endif
    public class ThirdPersonController : MonoBehaviour
    {
        [Header("Player")]
        [Tooltip("Move speed of the character in m/s")]
        public float MoveSpeed = 2.0f;

        [Tooltip("Sprint speed of the character in m/s")]
        public float SprintSpeed = 5.335f;

        [Tooltip("How fast the character turns to face movement direction")]
        [Range(0.0f, 0.3f)]
        public float RotationSmoothTime = 0.12f;

        [Tooltip("Acceleration and deceleration")]
        public float SpeedChangeRate = 10.0f;

        public AudioClip LandingAudioClip;
        public AudioClip[] FootstepAudioClips;
        [Range(0, 1)] public float FootstepAudioVolume = 0.5f;

        [Space(10)]
        [Tooltip("The height the player can jump")]
        public float JumpHeight = 1.2f;

        [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
        public float Gravity = -15.0f;

        [Space(10)]
        [Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
        public float JumpTimeout = 0.50f;

        [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
        public float FallTimeout = 0.15f;

        [Header("Player Grounded")]
        [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
        public bool Grounded = true;

        [Tooltip("Useful for rough ground")]
        public float GroundedOffset = -0.14f;

        [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
        public float GroundedRadius = 0.28f;

        [Tooltip("What layers the character uses as ground")]
        public LayerMask GroundLayers;

        [Header("Combat")]
        [Tooltip("Time window to chain next attack in combo")]
        public float ComboWindow = 1.0f;

        [Tooltip("Cooldown time after completing or missing a combo")]
        public float AttackCooldown = 0.5f;

        [Header("Ultimate Skill")]
        [Tooltip("Cooldown time for ultimate skill in seconds")]
        public float UltimateCooldown = 15.0f;

        [Tooltip("Enable/disable ultimate skill")]
        public bool UltimateEnabled = true;

        [Header("E Skill (Attack360)")]
        [Tooltip("Cooldown time for E skill in seconds")]
        public float ESkillCooldown = 8.0f;

        [Tooltip("Enable/disable E skill")]
        public bool ESkillEnabled = true;

        [Header("Cinemachine")]
        [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
        public GameObject CinemachineCameraTarget;

        [Tooltip("How far in degrees can you move the camera up")]
        public float TopClamp = 70.0f;

        [Tooltip("How far in degrees can you move the camera down")]
        public float BottomClamp = -30.0f;

        [Tooltip("Additional degress to override the camera. Useful for fine tuning camera position when locked")]
        public float CameraAngleOverride = 0.0f;

        [Tooltip("For locking the camera position on all axis")]
        public bool LockCameraPosition = false;

        // cinemachine
        private float _cinemachineTargetYaw;
        private float _cinemachineTargetPitch;

        // player
        private float _speed;
        private float _animationBlend;
        private float _targetRotation = 0.0f;
        private float _rotationVelocity;
        private float _verticalVelocity;
        private float _terminalVelocity = 53.0f;

        // timeout deltatime
        private float _jumpTimeoutDelta;
        private float _fallTimeoutDelta;

        // combat
        private int _attackCount = 0;
        private float _lastAttackTime = 0f;
        private float _attackCooldownTimer = 0f;
        private bool _attackQueued = false;
        private int _lastProcessedAttackCount = 0;

        // ultimate
        private float _ultimateCooldownTimer = 0f;
        private bool _isUltimateReady = true;
        private bool _isPerformingUltimate = false;

        // protect
        private bool _isProtecting = false;

        // eskill (Attack360)
        private float _eskillCooldownTimer = 0f;
        private bool _isESkillReady = true;
        private bool _isPerformingESkill = false;

        // animation IDs
        private int _animIDSpeed;
        private int _animIDGrounded;
        private int _animIDJump;
        private int _animIDFreeFall;
        private int _animIDMotionSpeed;
        private int _animIDAttack1;
        private int _animIDAttack2;
        private int _animIDAttack3;
        private int _animIDUltimate;
        private int _animIDProtect;
        private int _animIDESkill;

#if ENABLE_INPUT_SYSTEM 
        private PlayerInput _playerInput;
#endif
        private Animator _animator;
        private CharacterController _controller;
        private StarterAssetsInputs _input;
        private GameObject _mainCamera;

        private const float _threshold = 0.01f;

        private bool _hasAnimator;

        private bool IsCurrentDeviceMouse
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                return _playerInput.currentControlScheme == "KeyboardMouse";
#else
				return false;
#endif
            }
        }


        private void Awake()
        {
            // get a reference to our main camera
            if (_mainCamera == null)
            {
                _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
            }
        }

        private void Start()
        {
            _cinemachineTargetYaw = CinemachineCameraTarget.transform.rotation.eulerAngles.y;
            
            _hasAnimator = TryGetComponent(out _animator);
            _controller = GetComponent<CharacterController>();
            _input = GetComponent<StarterAssetsInputs>();
#if ENABLE_INPUT_SYSTEM 
            _playerInput = GetComponent<PlayerInput>();
#else
			Debug.LogError( "Starter Assets package is missing dependencies. Please use Tools/Starter Assets/Reinstall Dependencies to fix it");
#endif

            AssignAnimationIDs();

            // reset our timeouts on start
            _jumpTimeoutDelta = JumpTimeout;
            _fallTimeoutDelta = FallTimeout;
        }

        private void Update()
        {
            _hasAnimator = TryGetComponent(out _animator);

            JumpAndGravity();
            GroundedCheck();
            Move();
            HandleAttack();
            HandleUltimate();
            HandleProtect();
            HandleESkill();
            
            // Manual root motion handling when "Apply Root Motion = Handled by Script"
            if (_hasAnimator && !_animator.applyRootMotion)
            {
                ApplyRootMotionManually();
            }
        }

        private void LateUpdate()
        {
            CameraRotation();
        }

        private void AssignAnimationIDs()
        {
            _animIDSpeed = Animator.StringToHash("Speed");
            _animIDGrounded = Animator.StringToHash("Grounded");
            _animIDJump = Animator.StringToHash("Jump");
            _animIDFreeFall = Animator.StringToHash("FreeFall");
            _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
            _animIDAttack1 = Animator.StringToHash("Attack1");
            _animIDAttack2 = Animator.StringToHash("Attack2");
            _animIDAttack3 = Animator.StringToHash("Attack3");
            _animIDUltimate = Animator.StringToHash("Ultimate");
            _animIDProtect = Animator.StringToHash("Protect");
            _animIDESkill = Animator.StringToHash("ESkill");
        }

        private void GroundedCheck()
        {
            // set sphere position, with offset
            Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset,
                transform.position.z);
            Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers,
                QueryTriggerInteraction.Ignore);

            // update animator if using character
            if (_hasAnimator)
            {
                _animator.SetBool(_animIDGrounded, Grounded);
            }
        }

        private void CameraRotation()
        {
            // if there is an input and camera position is not fixed
            if (_input.look.sqrMagnitude >= _threshold && !LockCameraPosition)
            {
                //Don't multiply mouse input by Time.deltaTime;
                float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;

                _cinemachineTargetYaw += _input.look.x * deltaTimeMultiplier;
                _cinemachineTargetPitch += _input.look.y * deltaTimeMultiplier;
            }

            // clamp our rotations so our values are limited 360 degrees
            _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
            _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

            // Cinemachine will follow this target
            CinemachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + CameraAngleOverride,
                _cinemachineTargetYaw, 0.0f);
        }

        private void Move()
        {
            // Block movement during attack animations or ultimate
            bool isInAttackState = false;
            bool isInUltimateState = false;
            bool isInProtectState = false;
            bool isInESkillState = false;
            if (_hasAnimator)
            {
                AnimatorStateInfo currentState = _animator.GetCurrentAnimatorStateInfo(0);
                isInAttackState = currentState.IsName("Attack_1") || 
                                 currentState.IsName("Attack_2") || 
                                 currentState.IsName("Attack_3");
                isInUltimateState = currentState.IsName("UntimateAttack_1");
                isInProtectState = currentState.IsName("ProtectAxe");
                isInESkillState = currentState.IsName("Attack360");
            }
            
            // Don't allow HORIZONTAL movement during attack, ultimate, protect or eskill, but ALLOW vertical (gravity)
            if (isInAttackState || isInUltimateState || isInProtectState || isInESkillState)
            {
                // Apply ONLY vertical movement (gravity)
                Vector3 verticalMove = new Vector3(0.0f, _verticalVelocity, 0.0f);
                _controller.Move(verticalMove * Time.deltaTime);
                
                // Update animator to show no horizontal speed
                if (_hasAnimator)
                {
                    _animator.SetFloat(_animIDSpeed, 0f);
                    _animator.SetFloat(_animIDMotionSpeed, 0f);
                }
                
                return; // Exit early - no horizontal movement during attack/ultimate/protect/eskill
            }
            
            // set target speed based on move speed, sprint speed and if sprint is pressed
            float targetSpeed = _input.sprint ? SprintSpeed : MoveSpeed;

            // note: Vector2's == operator uses approximation so is not floating point error prone, and is cheaper than magnitude
            // if there is no input, set the target speed to 0
            if (_input.move == Vector2.zero) targetSpeed = 0.0f;

            // a reference to the players current horizontal velocity
            float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

            float speedOffset = 0.1f;
            float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;

            // accelerate or decelerate to target speed
            if (currentHorizontalSpeed < targetSpeed - speedOffset ||
                currentHorizontalSpeed > targetSpeed + speedOffset)
            {
                // creates curved result rather than a linear one giving a more organic speed change
                // note T in Lerp is clamped, so we don't need to clamp our speed
                _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude,
                    Time.deltaTime * SpeedChangeRate);

                // round speed to 3 decimal places
                _speed = Mathf.Round(_speed * 1000f) / 1000f;
            }
            else
            {
                _speed = targetSpeed;
            }

            _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);
            if (_animationBlend < 0.01f) _animationBlend = 0f;

            // normalise input direction
            Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;

            // note: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
            // if there is a move input rotate player when the player is moving
            if (_input.move != Vector2.zero)
            {
                _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg +
                                  _mainCamera.transform.eulerAngles.y;
                float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity,
                    RotationSmoothTime);

                // rotate to face input direction relative to camera position
                transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
            }


            Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

            // move the player
            _controller.Move(targetDirection.normalized * (_speed * Time.deltaTime) +
                             new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);

            // update animator if using character
            if (_hasAnimator)
            {
                _animator.SetFloat(_animIDSpeed, _animationBlend);
                _animator.SetFloat(_animIDMotionSpeed, inputMagnitude);
            }
        }

        private void HandleAttack()
        {
            // Update cooldown timer
            if (_attackCooldownTimer > 0)
            {
                _attackCooldownTimer -= Time.deltaTime;
                return;
            }

            // Check if we're currently in an attack state
            bool isInAttackState = false;
            float normalizedTime = 0f;
            string currentStateName = "";
            
            if (_hasAnimator)
            {
                AnimatorStateInfo currentState = _animator.GetCurrentAnimatorStateInfo(0);
                isInAttackState = currentState.IsName("Attack_1") || 
                                 currentState.IsName("Attack_2") || 
                                 currentState.IsName("Attack_3");
                normalizedTime = currentState.normalizedTime % 1f;
                
                if (currentState.IsName("Attack_1")) currentStateName = "Attack_1";
                else if (currentState.IsName("Attack_2")) currentStateName = "Attack_2";
                else if (currentState.IsName("Attack_3")) currentStateName = "Attack_3";
            }

            // Reset combo if timeout and not attacking
            if (!isInAttackState && Time.time - _lastAttackTime > ComboWindow && _attackCount > 0)
            {
                _attackCount = 0;
                _attackQueued = false;
                _lastProcessedAttackCount = 0;
                _attackCooldownTimer = AttackCooldown;
            }

            // Handle attack input
            if (_input.attack)
            {
                _input.attack = false;
                
                if (Grounded && _attackCooldownTimer <= 0)
                {
                    if (!isInAttackState)
                    {
                        // Start new combo - only when NOT attacking
                        _attackCount = 1;
                        _lastAttackTime = Time.time;
                        _attackQueued = false;
                        _lastProcessedAttackCount = 0;
                        
                        if (_hasAnimator)
                        {
                            // Clear all triggers to prevent auto-replay
                            _animator.ResetTrigger(_animIDAttack1);
                            _animator.ResetTrigger(_animIDAttack2);
                            _animator.SetTrigger(_animIDAttack1);
                        }
                    }
                    else if (_attackCount > 0 && _attackCount < 3)
                    {
                        // Queue next attack - but DON'T set Attack1 trigger
                        _attackQueued = true;
                    }
                }
            }
            else
            {
                // Clear queue if no input
                if (isInAttackState && _attackQueued)
                {
                    _attackQueued = false;
                }
            }

            // Process queued attack ONLY when we're in the correct state
            if (_attackQueued && isInAttackState && _attackCount > 0)
            {
                // Verify we're in the expected state for current attack count
                bool canProcess = false;
                
                if (_attackCount == 1 && currentStateName == "Attack_1" && _lastProcessedAttackCount != 1)
                {
                    canProcess = true;
                }
                else if (_attackCount == 2 && currentStateName == "Attack_2" && _lastProcessedAttackCount != 2)
                {
                    canProcess = true;
                }
                
                // Only process if in correct state and at proper timing
                if (canProcess && normalizedTime >= 0.4f && normalizedTime < 0.95f)
                {
                    _attackQueued = false;
                    _lastProcessedAttackCount = _attackCount;
                    _attackCount++;
                    _lastAttackTime = Time.time;
                    
                    if (_hasAnimator)
                    {
                        // Clear all triggers before setting new one
                        _animator.ResetTrigger(_animIDAttack1);
                        _animator.ResetTrigger(_animIDAttack2);
                        _animator.ResetTrigger(_animIDAttack3);
                        
                        switch (_attackCount)
                        {
                            case 2:
                                _animator.SetTrigger(_animIDAttack2);
                                break;
                            case 3:
                                _animator.SetTrigger(_animIDAttack3);
                                break;
                        }
                        
                        if (_attackCount >= 3)
                        {
                            _attackCount = 0;
                            _lastProcessedAttackCount = 0;
                            _attackCooldownTimer = AttackCooldown;
                        }
                    }
                }
            }
            
            // Clear any leftover triggers when not in attack state
            if (!isInAttackState && _hasAnimator && _attackCount == 0)
            {
                _animator.ResetTrigger(_animIDAttack1);
                _animator.ResetTrigger(_animIDAttack2);
                _animator.ResetTrigger(_animIDAttack3);
            }
        }

        private void HandleUltimate()
        {
            // Update cooldown timer
            if (!_isUltimateReady && _ultimateCooldownTimer > 0)
            {
                _ultimateCooldownTimer -= Time.deltaTime;
                if (_ultimateCooldownTimer <= 0)
                {
                    _isUltimateReady = true;
                    Debug.Log("Ultimate skill is ready!");
                }
            }

            // Check if currently performing ultimate
            bool isInUltimateState = false;
            if (_hasAnimator)
            {
                AnimatorStateInfo currentState = _animator.GetCurrentAnimatorStateInfo(0);
                isInUltimateState = currentState.IsName("UntimateAttack");
                
                if (isInUltimateState)
                {
                    _isPerformingUltimate = true;
                    
                    // IMPORTANT: Don't modify _verticalVelocity here!
                    // Let ApplyRootMotionManually() handle it by setting to 0
                    // This allows FULL root motion (XYZ) to control the character
                }
                else if (_isPerformingUltimate)
                {
                    // Just finished ultimate - gravity will automatically resume
                    _isPerformingUltimate = false;
                }
            }

            // Handle ultimate input
            if (_input.ultimate)
            {
                _input.ultimate = false;

                if (!UltimateEnabled)
                {
                    Debug.LogWarning("Ultimate skill is disabled!");
                    return;
                }

                if (!_isUltimateReady)
                {
                    Debug.Log($"Ultimate on cooldown! {_ultimateCooldownTimer:F1}s remaining");
                    return;
                }

                if (!Grounded)
                {
                    Debug.Log("Cannot use ultimate in air!");
                    return;
                }

                if (_isPerformingUltimate || isInUltimateState)
                {
                    Debug.Log("Already performing ultimate!");
                    return;
                }

                // Start ultimate
                if (_hasAnimator)
                {
                    // Reset attack combo
                    _attackCount = 0;
                    _attackQueued = false;
                    _lastProcessedAttackCount = 0;
                    _attackCooldownTimer = 0f;

                    // Clear all attack triggers
                    _animator.ResetTrigger(_animIDAttack1);
                    _animator.ResetTrigger(_animIDAttack2);
                    _animator.ResetTrigger(_animIDAttack3);
                    _animator.ResetTrigger(_animIDUltimate);

                    // FULL ROOT MOTION APPROACH:
                    // Don't apply initial jump force - let animation control everything
                    // Just reset vertical velocity to let root motion take over
                    _verticalVelocity = 0f;
                    
                    // Trigger ultimate animation
                    _animator.SetTrigger(_animIDUltimate);
                    
                    _isUltimateReady = false;
                    _ultimateCooldownTimer = UltimateCooldown;
                    _isPerformingUltimate = true;

                    Debug.Log("Ultimate skill activated!");
                }
            }
        }

        private void HandleProtect()
        {
            // Check if currently performing protect
            bool isInProtectState = false;
            if (_hasAnimator)
            {
                AnimatorStateInfo currentState = _animator.GetCurrentAnimatorStateInfo(0);
                isInProtectState = currentState.IsName("ProtectAxe");
                
                if (isInProtectState)
                {
                    _isProtecting = true;
                }
                else if (_isProtecting)
                {
                    _isProtecting = false;
                }
            }

            // Handle protect input
            if (_input.protect)
            {
                _input.protect = false;

                if (!Grounded)
                {
                    Debug.Log("Cannot use protect in air!");
                    return;
                }

                if (_isProtecting || isInProtectState)
                {
                    Debug.Log("Already protecting!");
                    return;
                }

                // Start protect
                if (_hasAnimator)
                {
                    // Reset attack combo
                    _attackCount = 0;
                    _attackQueued = false;
                    _lastProcessedAttackCount = 0;

                    // Clear all triggers
                    _animator.ResetTrigger(_animIDAttack1);
                    _animator.ResetTrigger(_animIDAttack2);
                    _animator.ResetTrigger(_animIDAttack3);
                    _animator.ResetTrigger(_animIDUltimate);
                    _animator.ResetTrigger(_animIDProtect);
                    
                    // Trigger protect animation
                    _animator.SetTrigger(_animIDProtect);
                    
                    _isProtecting = true;

                    Debug.Log("Protect activated!");
                }
            }
        }

        private void HandleESkill()
        {
            // Update cooldown timer
            if (!_isESkillReady && _eskillCooldownTimer > 0)
            {
                _eskillCooldownTimer -= Time.deltaTime;
                if (_eskillCooldownTimer <= 0)
                {
                    _isESkillReady = true;
                    Debug.Log("E Skill (Attack360) is ready!");
                }
            }

            // Check if currently performing E skill
            bool isInESkillState = false;
            if (_hasAnimator)
            {
                AnimatorStateInfo currentState = _animator.GetCurrentAnimatorStateInfo(0);
                isInESkillState = currentState.IsName("Attack360");
                
                if (isInESkillState)
                {
                    _isPerformingESkill = true;
                }
                else if (_isPerformingESkill)
                {
                    _isPerformingESkill = false;
                }
            }

            // Handle E skill input
            if (_input.eskill)
            {
                _input.eskill = false;

                if (!ESkillEnabled)
                {
                    Debug.LogWarning("E Skill is disabled!");
                    return;
                }

                if (!_isESkillReady)
                {
                    Debug.Log($"E Skill on cooldown! {_eskillCooldownTimer:F1}s remaining");
                    return;
                }

                if (!Grounded)
                {
                    Debug.Log("Cannot use E Skill in air!");
                    return;
                }

                if (_isPerformingESkill || isInESkillState)
                {
                    Debug.Log("Already performing E Skill!");
                    return;
                }

                // Start E skill (Attack360)
                if (_hasAnimator)
                {
                    // Reset attack combo
                    _attackCount = 0;
                    _attackQueued = false;
                    _lastProcessedAttackCount = 0;
                    _attackCooldownTimer = 0f;

                    // Clear all triggers
                    _animator.ResetTrigger(_animIDAttack1);
                    _animator.ResetTrigger(_animIDAttack2);
                    _animator.ResetTrigger(_animIDAttack3);
                    _animator.ResetTrigger(_animIDUltimate);
                    _animator.ResetTrigger(_animIDProtect);
                    _animator.ResetTrigger(_animIDESkill);
                    
                    // Trigger E skill animation
                    _animator.SetTrigger(_animIDESkill);
                    
                    _isESkillReady = false;
                    _eskillCooldownTimer = ESkillCooldown;
                    _isPerformingESkill = true;

                    Debug.Log("E Skill (Attack360) activated!");
                }
            }
        }

        private void OnAnimatorMove()
        {
            // This callback is only used when Animator "Apply Root Motion" is TRUE
            // For "Handled by Script" mode, use ApplyRootMotionManually() instead
            if (_hasAnimator && _controller != null)
            {
                AnimatorStateInfo currentState = _animator.GetCurrentAnimatorStateInfo(0);
                bool isInAttackState = currentState.IsName("Attack_1") || 
                                     currentState.IsName("Attack_2") || 
                                     currentState.IsName("Attack_3");
                bool isInUltimateState = currentState.IsName("UntimateAttack_1");
                
                if (isInAttackState)
                {
                    // For attacks: Only apply horizontal movement (XZ), keep Y from gravity
                    Vector3 rootMotionDelta = _animator.deltaPosition;
                    Vector3 horizontalMotion = new Vector3(rootMotionDelta.x, 0f, rootMotionDelta.z);
                    _controller.Move(horizontalMotion);
                }
                else if (isInUltimateState)
                {
                    Vector3 rootMotionDelta = _animator.deltaPosition;
                    float normalizedTime = currentState.normalizedTime % 1f;
                    
                    // Phase 1 (0-35%): Jump up - FULL root motion (XYZ) + disable gravity
                    if (normalizedTime < 0.35f)
                    {
                        _controller.Move(rootMotionDelta);
                        _verticalVelocity = 0f;
                    }
                    // Phase 2 (35-100%): Attack and fall - XZ from animation + enable gravity
                    else
                    {
                        // Only horizontal movement from animation
                        Vector3 horizontalMotion = new Vector3(rootMotionDelta.x, 0f, rootMotionDelta.z);
                        _controller.Move(horizontalMotion);
                        // DON'T touch _verticalVelocity - let gravity work!
                    }
                }
            }
        }

        private void ApplyRootMotionManually()
        {
            // This method handles root motion when Animator is set to "Handled by Script"
            // It's called every frame from Update() if applyRootMotion is false
            
            if (!_animator || !_hasAnimator || !_controller) return;
    
            AnimatorStateInfo currentState = _animator.GetCurrentAnimatorStateInfo(0);
            bool isInAttackState = currentState.IsName("Attack_1") || 
                                 currentState.IsName("Attack_2") || 
                                 currentState.IsName("Attack_3");
            bool isInUltimateState = currentState.IsName("UntimateAttack_1");
            bool isInESkillState = currentState.IsName("Attack360");
            
            if (isInAttackState)
            {
                // For attacks: Only apply horizontal movement (XZ), keep Y from gravity
                Vector3 rootMotionDelta = _animator.deltaPosition;
                Vector3 horizontalMotion = new Vector3(rootMotionDelta.x, 0f, rootMotionDelta.z);
                _controller.Move(horizontalMotion);
            }
            else if (isInUltimateState)
            {
                Vector3 rootMotionDelta = _animator.deltaPosition;
                float normalizedTime = currentState.normalizedTime % 1f;
                
                // Phase 1 (0-35%): Jump up - FULL root motion (XYZ) + disable gravity
                if (normalizedTime < 0.35f)
                {
                    _controller.Move(rootMotionDelta);
                    _verticalVelocity = 0f;
                }
                // Phase 2 (35-100%): Attack and fall - XZ from animation + enable gravity
                else
                {
                    // Only horizontal movement from animation
                    Vector3 horizontalMotion = new Vector3(rootMotionDelta.x, 0f, rootMotionDelta.z);
                    _controller.Move(horizontalMotion);
                    // DON'T touch _verticalVelocity - let gravity work!
                }
            }
            else if (isInESkillState)
            {
                // For Attack360: Apply horizontal movement for spinning effect
                Vector3 rootMotionDelta = _animator.deltaPosition;
                Vector3 horizontalMotion = new Vector3(rootMotionDelta.x, 0f, rootMotionDelta.z);
                _controller.Move(horizontalMotion);
            }
        }

        private void JumpAndGravity()
        {
            if (Grounded)
            {
                // reset the fall timeout timer
                _fallTimeoutDelta = FallTimeout;

                // update animator if using character
                if (_hasAnimator)
                {
                    _animator.SetBool(_animIDJump, false);
                    _animator.SetBool(_animIDFreeFall, false);
                }

                // stop our velocity dropping infinitely when grounded
                if (_verticalVelocity < 0.0f)
                {
                    _verticalVelocity = -2f;
                }

                // Jump
                if (_input.jump && _jumpTimeoutDelta <= 0.0f)
                {
                    // the square root of H * -2 * G = how much velocity needed to reach desired height
                    _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);

                    // update animator if using character
                    if (_hasAnimator)
                    {
                        _animator.SetBool(_animIDJump, true);
                    }
                }

                // jump timeout
                if (_jumpTimeoutDelta >= 0.0f)
                {
                    _jumpTimeoutDelta -= Time.deltaTime;
                }
            }
            else
            {
                // reset the jump timeout timer
                _jumpTimeoutDelta = JumpTimeout;

                // fall timeout
                if (_fallTimeoutDelta >= 0.0f)
                {
                    _fallTimeoutDelta -= Time.deltaTime;
                }
                else
                {
                    // update animator if using character
                    if (_hasAnimator)
                    {
                        _animator.SetBool(_animIDFreeFall, true);
                    }
                }

                // if we are not grounded, do not jump
                _input.jump = false;
            }

            // apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
            if (_verticalVelocity < _terminalVelocity)
            {
                _verticalVelocity += Gravity * Time.deltaTime;
            }
        }

        private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
        {
            if (lfAngle < -360f) lfAngle += 360f;
            if (lfAngle > 360f) lfAngle -= 360f;
            return Mathf.Clamp(lfAngle, lfMin, lfMax);
        }

        private void OnDrawGizmosSelected()
        {
            Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
            Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

            if (Grounded) Gizmos.color = transparentGreen;
            else Gizmos.color = transparentRed;

            // when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
            Gizmos.DrawSphere(
                new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z),
                GroundedRadius);
        }

        private void OnFootstep(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                if (FootstepAudioClips.Length > 0)
                {
                    var index = Random.Range(0, FootstepAudioClips.Length);
                    AudioSource.PlayClipAtPoint(FootstepAudioClips[index], transform.TransformPoint(_controller.center), FootstepAudioVolume);
                }
            }
        }

        private void OnLand(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                AudioSource.PlayClipAtPoint(LandingAudioClip, transform.TransformPoint(_controller.center), FootstepAudioVolume);
            }
        }

        // Public methods for UI/External access
        public float GetUltimateCooldownProgress()
        {
            if (_isUltimateReady) return 1f;
            return 1f - (_ultimateCooldownTimer / UltimateCooldown);
        }

        public bool IsUltimateReady()
        {
            return _isUltimateReady;
        }

        public float GetUltimateRemainingCooldown()
        {
            return _ultimateCooldownTimer;
        }

        public float GetESkillCooldownProgress()
        {
            if (_isESkillReady) return 1f;
            return 1f - (_eskillCooldownTimer / ESkillCooldown);
        }

        public bool IsESkillReady()
        {
            return _isESkillReady;
        }

        public float GetESkillRemainingCooldown()
        {
            return _eskillCooldownTimer;
        }
    }
}