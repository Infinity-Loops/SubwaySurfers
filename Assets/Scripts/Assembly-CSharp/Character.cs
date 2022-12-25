using System;
using System.Collections;
using UnityEngine;

public class Character : MonoBehaviour
{
    public event Character.OnGroundedDelegate OnGrounded;

    public void Awake()
    {
        this.game = Game.Instance;
        Variable<bool> isInGame2 = this.game.isInGame;
        isInGame2.OnChange = (Variable<bool>.OnChangeDelegate)Delegate.Combine(isInGame2.OnChange, new Variable<bool>.OnChangeDelegate(delegate (bool isInGame)
        {
            if (!isInGame)
            {
                base.StopAllCoroutines();
                this.immuneToCriticalHit = false;
                this.characterController.enabled = true;
                this.stopColliding = false;
            }
        }));
        this.track = Track.Instance;
        this.characterController = Game.Charactercontroller;
        this.hoverboard = Hoverboard.Instance;
        this.running = Running.Instance;
        this.superSneakers = this.FindObject<SuperSneakers>();
        this.characterModel = base.GetComponentInChildren<CharacterModel>();
        this.characterCamera = CharacterCamera.Instance;
        this.guard = FollowingGuard.Instance;
        this.CharacterPickupParticleSystem = base.GetComponentInChildren<CharacterPickupParticles>();
        this.characterColliderTrigger = this.characterCollider.GetComponent<OnTriggerObject>();
        OnTriggerObject onTriggerObject = this.characterColliderTrigger;
        onTriggerObject.OnEnter = (OnTriggerObject.OnEnterDelegate)Delegate.Combine(onTriggerObject.OnEnter, new OnTriggerObject.OnEnterDelegate(this.OnCharacterColliderEnter));
        OnTriggerObject onTriggerObject2 = this.characterColliderTrigger;
        onTriggerObject2.OnExit = (OnTriggerObject.OnExitDelegate)Delegate.Combine(onTriggerObject2.OnExit, new OnTriggerObject.OnExitDelegate(this.OnCharacterColliderExit));
        this.characterAnimation["caught"].layer = 4;
        this.characterAnimation["caught"].enabled = false;
        this.characterAnimation["caught2"].layer = 4;
        this.characterAnimation["caught2"].enabled = false;
        this.characterControllerCenter = this.characterController.center;
        this.characterControllerHeight = this.characterController.height;
        this.characterColliderCenter = this.characterCollider.center;
        this.characterColliderHeight = this.characterCollider.height;
        this.stats = GameStats.Instance;
    }

    public void Restart()
    {
        this.trackIndex = this.initialTrackIndex;
        this.trackIndexTarget = this.initialTrackIndex;
        this.x = this.track.GetTrackX(this.trackIndex);
        this.trackIndexPosition = (float)this.trackIndex;
        this.characterModel.ResetBlink();
        this.z = 0f;
        this.trackMovement = 0;
        this.trackMovementNext = 0;
        this.characterController.transform.position = this.track.GetPosition(this.x, this.z) + Vector3.up * 5f;
        this.characterController.Move(-5f * Vector3.up);
        this.verticalSpeed = 0f;
        this.superSneakersJump = null;
        this.jumpHeight = this.jumpHeightNormal;
        this.inAirJump = false;
        this.lastGroundedY = 0f;
        this.guard.Restart(true);
        this.Stumble = true;
        this.startedJumpFromGround = false;
    }

    public void ChangeTrack(int movement, float duration)
    {
        this.stats.trackChanges++;
        if (this.trackMovement != movement)
        {
            this.ForceChangeTrack(movement, duration);
        }
        else
        {
            this.trackMovementNext = movement;
        }
    }

    public void ForceChangeTrack(int movement, float duration)
    {
        this.trackMovement = movement;
        this.trackMovementNext = 0;
        base.StopAllCoroutines();
        base.StartCoroutine(this.ChangeTrackCoroutine(movement, duration));
    }

    private IEnumerator ChangeTrackCoroutine(int move, float duration)
    {
        this.trackMovement = move;
        this.trackMovementNext = 0;
        int newTrackIndex = this.trackIndexTarget + move;
        float trackChangeIndexDistance = Mathf.Abs((float)newTrackIndex - this.trackIndexPosition);
        float trackIndexPositionBegin = this.trackIndexPosition;
        float startX = this.x;
        float endX = this.track.GetTrackX(newTrackIndex);
        float dir = Mathf.Sign((float)(newTrackIndex - this.trackIndexTarget));
        float startRotation = this.characterRotation;
        if (this.characterController.isGrounded)
        {
            string dodgeAnimation = ((dir >= 0f) ? this.animations.dodgeRight : this.animations.dodgeLeft);
            this.characterAnimation["dodgeRight"].speed = Game.Instance.NormalizedGameSpeed;
            this.characterAnimation["dodgeLeft"].speed = Game.Instance.NormalizedGameSpeed;
            this.characterAnimation.CrossFade(dodgeAnimation, 0.02f);
        }
        if (!this.jumping)
        {
            this.characterAnimation.CrossFadeQueued(this.animations.run, (!this.game.Modifiers.IsActive(this.game.Modifiers.Hoverboard)) ? 0.02f : 0.4f);
        }
        if (newTrackIndex < 0 || newTrackIndex >= this.track.numberOfTracks)
        {
            this.NotifyStumble(this.StumbleSideSound, "side");
            if (!this.game.Modifiers.IsActive(this.game.Modifiers.Hoverboard) && !this.game.IsInJetpackMode)
            {
                this.characterAnimation.CrossFade((dir >= 0f) ? "stumbleOffRight" : "stumbleOffLeft", 0.2f);
            }
            if (!this.jumping)
            {
                this.characterAnimation.CrossFadeQueued(this.animations.run, (!this.game.Modifiers.IsActive(this.game.Modifiers.Hoverboard)) ? 0.02f : 0.4f);
            }
            yield break;
        }
        if (move < 0)
        {
            if (this.game.Modifiers.IsActive(this.game.Modifiers.Hoverboard))
            {
                So.Instance.playSound(this.H_Left);
            }
            else
            {
                So.Instance.playSound(this.DodgeLeft);
            }
        }
        else if (this.game.Modifiers.IsActive(this.game.Modifiers.Hoverboard))
        {
            So.Instance.playSound(this.H_Right);
        }
        else
        {
            So.Instance.playSound(this.DodgeRight);
        }
        this.trackIndexTarget = newTrackIndex;
        yield return base.StartCoroutine(pTween.To(trackChangeIndexDistance * duration, delegate (float t)
        {
            this.trackIndexPosition = Mathf.Lerp(trackIndexPositionBegin, (float)newTrackIndex, t);
            this.x = Mathf.Lerp(startX, endX, t);
            this.characterRotation = pMath.Bell(t) * dir * this.characterAngle + Mathf.Lerp(startRotation, 0f, t);
            this.characterRoot.localRotation = Quaternion.Euler(0f, this.characterRotation, 0f);
        }));
        this.trackIndex = newTrackIndex;
        this.trackMovement = 0;
        if (this.trackMovementNext != 0)
        {
            base.StartCoroutine(this.ChangeTrackCoroutine(this.trackMovementNext, duration));
        }
        yield break;
    }

    public void SetBackToCheckPoint(float zoomTime)
    {
        float lastCheckPoint = this.track.GetLastCheckPoint(this.z);
        this.trackIndex = this.initialTrackIndex;
        this.trackIndexTarget = this.initialTrackIndex;
        float trackX = this.track.GetTrackX(this.trackIndex);
        this.trackIndexPosition = (float)this.trackIndex;
        this.trackMovement = 0;
        this.trackMovementNext = 0;
        base.StartCoroutine(this.MoveCharacterToPosition(trackX, lastCheckPoint, zoomTime));
    }

    private IEnumerator MoveCharacterToPosition(float newX, float newZ, float time)
    {
        float oldX = this.x;
        float oldZ = this.z;
        this.game.ChangeState(null);
        this.immuneToCriticalHit = true;
        this.stopColliding = true;
        this.characterController.enabled = false;
        this.characterAnimation.CrossFade(this.animations.run, time);
        yield return base.StartCoroutine(pTween.To(time, delegate (float t)
        {
            this.x = Mathf.SmoothStep(oldX, newX, t);
            this.z = Mathf.SmoothStep(oldZ, newZ, t);
        }));
        this.immuneToCriticalHit = false;
        this.characterController.enabled = true;
        this.characterAnimation.Play(this.animations.run);
        this.stopColliding = false;
        this.game.ChangeState(this.game.Running);
        yield break;
    }

    private Character.ObstacleTypes ObstacleTagToType(string tag)
    {
        switch (tag)
        {
            case "JumpTrain":
                return Character.ObstacleTypes.jumpTrain;
            case "RollBarrier":
                return Character.ObstacleTypes.rollBarrier;
            case "JumpBarrier":
                return Character.ObstacleTypes.jumpBarrier;
            case "JumpHighBarrier":
                return Character.ObstacleTypes.jumpHighBarrier;
        }
        return Character.ObstacleTypes.none;
    }

    private void OnCharacterColliderExit(Collider collider)
    {
        if (collider.CompareTag("Subway"))
        {
            this.isInsideSubway = false;
            return;
        }
        Character.ObstacleTypes obstacleTypes = this.ObstacleTagToType(collider.tag);
        if (obstacleTypes == this.lastObstacleTriggerType && this.lastObstacleTriggerTrackInex == this.trackIndex)
        {
            switch (obstacleTypes)
            {
                case Character.ObstacleTypes.jumpHighBarrier:
                    this.stats.jumpBarrier++;
                    this.stats.jumpHighBarrier++;
                    break;
                case Character.ObstacleTypes.jumpTrain:
                    this.stats.jumpsOverTrains++;
                    break;
                case Character.ObstacleTypes.rollBarrier:
                    this.stats.dodgeBarrier++;
                    break;
                case Character.ObstacleTypes.jumpBarrier:
                    this.stats.jumpBarrier++;
                    break;
            }
        }
    }

    private void OnCharacterColliderEnter(Collider collider)
    {
        if (collider.CompareTag("Subway"))
        {
            this.isInsideSubway = true;
            return;
        }
        if (this.stopColliding || collider.gameObject.layer == 16)
        {
            return;
        }
        Pickup componentInChildren = collider.GetComponentInChildren<Pickup>();
        if (componentInChildren != null)
        {
            this.NotifyPickup(componentInChildren);
            return;
        }
        if (collider.gameObject.layer == 0)
        {
            if (collider.isTrigger && this.characterController.isGrounded && this.OnGrounded != null)
            {
                this.OnGrounded();
            }
            if (collider.isTrigger)
            {
                Character.ObstacleTypes obstacleTypes = this.ObstacleTagToType(collider.tag);
                if (obstacleTypes != Character.ObstacleTypes.none)
                {
                    this.lastObstacleTriggerType = obstacleTypes;
                    this.lastObstacleTriggerTrackInex = this.trackIndex;
                }
            }
            return;
        }
        if (collider.isTrigger)
        {
            this.characterAnimation.CrossFade(this.animations.stumble, 0.05f);
            this.characterAnimation.CrossFadeQueued(this.animations.run, 0.5f);
            if (collider.name == "bush")
            {
                this.NotifyStumble(this.StumbleBushSound, collider.name);
            }
            else
            {
                this.NotifyStumble(this.StumbleSound, collider.name);
            }
            return;
        }
        this.lastHitTag = collider.tag;
        Character.ImpactX impactX = this.GetImpactX(collider);
        Character.ImpactY impactY = this.GetImpactY(collider);
        Character.ImpactZ impactZ = this.GetImpactZ(collider);
        int num = (((collider.bounds.min.x + collider.bounds.max.x) / 2f <= base.transform.position.x) ? (-1) : 1);
        bool flag = this.trackMovement == num;
        bool flag2 = this.characterCollider.bounds.center.z < collider.bounds.min.z;
        bool flag3 = impactZ == Character.ImpactZ.Before && !flag2 && flag;
        if (impactZ == Character.ImpactZ.Middle || flag3)
        {
            if (this.trackMovement != 0)
            {
                float num2 = 0.5f;
                if (this.track.IsRunningOnTutorialTrack)
                {
                    num2 = 0.2f;
                }
                this.ChangeTrack(-this.trackMovement, num2);
            }
            if (impactX == Character.ImpactX.Left)
            {
                this.characterAnimation.Play(this.animations.stumbleLeftSide);
                this.characterAnimation.PlayQueued(this.animations.run);
                this.NotifyStumble(this.StumbleSound, collider.name);
            }
            else if (impactX == Character.ImpactX.Right)
            {
                this.characterAnimation.Play(this.animations.stumbleRightSide);
                this.characterAnimation.PlayQueued(this.animations.run);
                this.NotifyStumble(this.StumbleSound, collider.name);
            }
        }
        else if (impactX == Character.ImpactX.Middle)
        {
            bool flag4 = true;
            if (!this.immuneToCriticalHit)
            {
                if (impactY == Character.ImpactY.Lower)
                {
                    this.characterAnimation.CrossFade(this.animations.stumble, 0.05f);
                    this.characterAnimation.CrossFadeQueued(this.animations.run, 0.5f);
                    flag4 = false;
                    this.verticalSpeed = this.CalculateJumpVerticalSpeed(8f);
                    this.NotifyStumble(this.StumbleSound, collider.name);
                }
                else if (collider.gameObject.CompareTag("HitMovingTrain"))
                {
                    this.HitByTrainSequence();
                }
                else if (impactY == Character.ImpactY.Middle)
                {
                    this.characterAnimation.CrossFade(this.animations.hitMid, 0.07f);
                }
                else
                {
                    this.characterAnimation.CrossFade(this.animations.hitUpper, 0.07f);
                }
            }
            if (flag4)
            {
                this.NotifyCriticalHit();
            }
        }
        else
        {
            if (impactZ == Character.ImpactZ.Before && flag)
            {
                if (collider.gameObject.CompareTag("HitMovingTrain"))
                {
                    this.HitByTrainSequence();
                    this.NotifyCriticalHit();
                }
                else if (collider.gameObject.layer == 13)
                {
                    this.characterAnimation.CrossFade(this.animations.stumble, 0.05f);
                    this.characterAnimation.CrossFadeQueued(this.animations.run, 0.5f);
                }
                else
                {
                    this.ForceChangeTrack(-this.trackMovement, 0.5f);
                }
            }
            else if (collider.gameObject.layer == 13)
            {
                this.ForceChangeTrack(-this.trackMovement, 0.5f);
            }
            if (impactX == Character.ImpactX.Left)
            {
                this.characterAnimation.Play(this.animations.stumbleLeftCorner);
                this.characterAnimation.PlayQueued(this.animations.run);
            }
            else if (impactX == Character.ImpactX.Right)
            {
                this.characterAnimation.Play(this.animations.stumbleRightCorner);
                this.characterAnimation.PlayQueued(this.animations.run);
            }
            this.NotifyStumble(this.StumbleSound, collider.name);
        }
    }

    private void HitByTrainSequence()
    {
        if (this.hoverboard.isActive)
        {
            return;
        }
        this.characterAnimation.Play(this.animations.hitMoving);
        Vector3 currentPos = base.transform.position;
        Vector3 camPos = this.characterCamera.transform.position;
        base.StartCoroutine(pTween.To(0.5f, delegate (float t)
        {
            this.transform.position = Vector3.Lerp(currentPos, new Vector3(camPos.x, camPos.y - 33f, currentPos.z), t);
        }));
    }

    private Character.ImpactX GetImpactX(Collider collider)
    {
        Bounds bounds = this.characterCollider.bounds;
        Bounds bounds2 = collider.bounds;
        float num = Mathf.Max(bounds.min.x, bounds2.min.x);
        float num2 = Mathf.Min(bounds.max.x, bounds2.max.x);
        float num3 = (num + num2) * 0.5f;
        float num4 = (num3 - bounds2.min.x) / bounds2.size.x;
        float num5 = num3 - bounds2.min.x;
        Character.ImpactX impactX;
        if ((double)num5 > (double)bounds2.size.x - (double)this.ColliderTrackWidth * 0.33)
        {
            impactX = Character.ImpactX.Right;
        }
        else if ((double)num5 < (double)this.ColliderTrackWidth * 0.33)
        {
            impactX = Character.ImpactX.Left;
        }
        else
        {
            impactX = Character.ImpactX.Middle;
        }
        return impactX;
    }

    private Character.ImpactZ GetImpactZ(Collider collider)
    {
        Vector3 position = base.transform.position;
        Bounds bounds = collider.bounds;
        Character.ImpactZ impactZ;
        if (position.z > bounds.max.z - ((bounds.max.z - bounds.min.z <= 30f) ? ((bounds.max.z - bounds.min.z) * 0.5f) : this.stumbleCornerTolerance))
        {
            impactZ = Character.ImpactZ.After;
        }
        else if (position.z < bounds.min.z + this.stumbleCornerTolerance)
        {
            impactZ = Character.ImpactZ.Before;
        }
        else
        {
            impactZ = Character.ImpactZ.Middle;
        }
        return impactZ;
    }

    private Character.ImpactY GetImpactY(Collider collider)
    {
        Bounds bounds = this.characterCollider.bounds;
        Bounds bounds2 = collider.bounds;
        float num = Mathf.Max(bounds.min.y, bounds2.min.y);
        float num2 = Mathf.Min(bounds.max.y, bounds2.max.y);
        float num3 = (num + num2) * 0.5f;
        float num4 = (num3 - bounds.min.y) / bounds.size.y;
        Character.ImpactY impactY;
        if (num4 < 0.33f)
        {
            impactY = Character.ImpactY.Lower;
        }
        else if (num4 < 0.66f)
        {
            impactY = Character.ImpactY.Middle;
        }
        else
        {
            impactY = Character.ImpactY.Upper;
        }
        return impactY;
    }

    public void Update()
    {
        if (this.roll != null)
        {
            this.roll.MoveNext();
        }
        Vector3 position = base.transform.position;
        if (position.y < 0f)
        {
            position.y = 1f;
            base.transform.position = position;
            Debug.Log("Character y-position has been clamped to avoid fallthrough.");
        }
    }

    public float GetTrackX()
    {
        return this.track.GetPosition(this.track.GetTrackX(this.trackIndex), 0f).x;
    }

    public void Jump()
    {
        this.fallAnim = true;
        if (this.hoverboard.isActive)
        {
            this.animations.SetRandomHoverJump();
        }
        else
        {
            this.animations.SetRandomJump();
        }
        bool flag = this.falling && !this.jumping && this.verticalSpeed < 0f && this.verticalSpeed > this.verticalSpeed_jumpTolerance;
        if (this.characterController.isGrounded || flag)
        {
            this.jumping = true;
            this.falling = false;
            this.shadow.active = false;
            this.characterAnimation.CrossFade(this.animations.jump, 0.05f);
            if (this.superSneakers.isActive)
            {
                Vector3 position = base.transform.position;
                Character.SuperSneakersJump superSneakersJump = default(Character.SuperSneakersJump);
                superSneakersJump.z_start = position.z;
                superSneakersJump.z_length = this.JumpLength(this.game.currentSpeed, this.jumpHeight) * this.superSneakersJumpApexRatio;
                superSneakersJump.z_end = superSneakersJump.z_start + superSneakersJump.z_length;
                superSneakersJump.y_start = position.y;
                this.superSneakersJump = new Character.SuperSneakersJump?(superSneakersJump);
                this.verticalSpeed = 0f;
            }
            else
            {
                this.verticalSpeed = this.CalculateJumpVerticalSpeed(this.jumpHeight);
            }
            if (this.IsRunningOnGround())
            {
                this.startedJumpFromGround = true;
                this.trainJump = false;
                this.trainJumpSampleZ = this.z + this.trainJumpSampleLength;
            }
            if (this.OnJump != null)
            {
                this.OnJump();
            }
            this.stats.jumps++;
        }
        else if (this.verticalSpeed < 0f)
        {
            this.inAirJump = true;
        }
    }

    private bool IsRunningOnGround()
    {
        return this.running.currentRunPosition == Running.RunPositions.ground;
    }

    public void CheckInAirJump()
    {
        if (this.characterController.isGrounded && this.inAirJump)
        {
            this.Jump();
            this.inAirJump = false;
        }
    }

    public void Roll()
    {
        if (this.roll != null)
        {
            return;
        }
        Character.SuperSneakersJump? superSneakersJump = this.superSneakersJump;
        if (superSneakersJump != null)
        {
            this.superSneakersJump = null;
        }
        this.roll = this.BeginRoll();
        this.stats.rolls++;
        if (this.trackIndex == 0)
        {
            this.stats.rollsLeftTrack++;
        }
        if (this.trackIndex == 1)
        {
            this.stats.rollsCenterTrack++;
        }
        if (this.trackIndex == 2)
        {
            this.stats.rollsRightTrack++;
        }
    }

    public void ApplyGravity()
    {
        if (this.verticalSpeed < 0f && this.characterController.isGrounded)
        {
            if (this.startedJumpFromGround && this.trainJump && this.IsRunningOnGround())
            {
                this.stats.jumpsOverTrains++;
            }
            if (this.running.currentRunPosition != Running.RunPositions.air)
            {
                this.startedJumpFromGround = false;
            }
            this.verticalSpeed = 0f;
            this.shadow.active = true;
            if (this.jumping || this.falling)
            {
                this.jumping = false;
                this.falling = false;
                if (this.OnGrounded != null)
                {
                    this.OnGrounded();
                }
                if (this.roll == null)
                {
                    this.SetRunAnim();
                    if (this.fallAnim)
                    {
                        this.characterAnimation.CrossFade(this.animations.landing, 0.05f);
                        this.characterAnimation.CrossFadeQueued(this.animations.run, 0.1f);
                    }
                    else
                    {
                        this.fallAnim = true;
                        this.characterAnimation.CrossFade(this.animations.run, 0.1f);
                    }
                }
            }
        }
        else if (this.startedJumpFromGround && this.trainJumpSampleZ < this.z)
        {
            RaycastHit raycastHit;
            if (Physics.Raycast(new Ray(base.transform.position, -Vector3.up), out raycastHit))
            {
                Debug.DrawRay(base.transform.position, -Vector3.up * raycastHit.distance, Color.red, 1000f);
                if (raycastHit.collider.CompareTag("HitMovingTrain") || raycastHit.collider.CompareTag("HitTrain"))
                {
                    this.trainJump = true;
                }
            }
            this.trainJumpSampleZ += this.trainJumpSampleLength;
        }
        this.verticalSpeed -= this.gravity * Time.deltaTime;
        if (!this.characterController.isGrounded && !this.falling && this.verticalSpeed < this.verticalFallSpeedLimit && this.roll == null)
        {
            this.falling = true;
            if (this.fallAnim)
            {
                this.characterAnimation.CrossFade(this.animations.hangtime, 0.2f);
                this.shadow.active = false;
            }
        }
    }

    public void MoveWithGravity()
    {
        if (this.characterController.enabled)
        {
            this.verticalSpeed -= this.gravity * Time.deltaTime;
            if (this.verticalSpeed > 0f)
            {
                this.verticalSpeed = 0f;
            }
            Vector3 vector = this.verticalSpeed * Time.deltaTime * Vector3.up;
            this.characterController.Move(vector);
        }
    }

    public void MoveForward()
    {
        Vector3 position = base.transform.position;
        float num = this.z + this.game.currentSpeed * Time.deltaTime;
        Vector3 vector = this.verticalSpeed * Time.deltaTime * Vector3.up;
        Vector3 position2 = this.track.GetPosition(this.x, num);
        Vector3 vector2 = new Vector3(position.x, 0f, position.z);
        if (this.superSneakersJump != null)
        {
            Character.SuperSneakersJump value = this.superSneakersJump.Value;
            if (this.z < value.z_end)
            {
                float num2 = this.superSneakersJumpCurve.Evaluate((num - value.z_start) / value.z_length) * this.jumpHeightSuperSneakers + value.y_start;
                float num3 = num2 - position.y;
                vector = Vector3.up * num3;
            }
            else
            {
                this.superSneakersJump = null;
                this.verticalSpeed = 0f;
                vector = Vector3.zero;
            }
        }
        Vector3 vector3 = position2 - vector2;
        if (this.characterController.enabled)
        {
            this.characterController.Move(vector + vector3);
        }
        else
        {
            this.characterController.transform.position = this.characterController.transform.position + vector3;
        }
        Debug.DrawLine(this.last, base.transform.position, Color.magenta, 1000f);
        this.last = base.transform.position;
        this.z = base.transform.position.z;
        if (this.characterController.isGrounded)
        {
            this.lastGroundedY = position.y;
        }
    }

    private IEnumerator BeginRoll()
    {
        this.characterAnimation.CrossFade(this.animations.roll, 0.1f);
        this.SetRunAnim();
        this.fallAnim = false;
        this.characterAnimation.CrossFadeQueued(this.animations.run, (!this.game.Modifiers.IsActive(this.game.Modifiers.Hoverboard)) ? 0f : 0.2f);
        this.characterController.height = 4f;
        this.characterController.center = new Vector3(0f, 2f, this.characterControllerCenter.z);
        this.characterCollider.height = 4f;
        this.characterCollider.center = new Vector3(0f, 4f, this.characterColliderCenter.z);
        this.verticalSpeed = -this.CalculateJumpVerticalSpeed(this.jumpHeight);
        float endTime = Time.time + this.characterAnimation[this.animations.roll].length;
        while (Time.time < endTime)
        {
            yield return 0;
            if (!this.characterAnimation[this.animations.roll].enabled)
            {
                break;
            }
        }
        if (this.characterController.enabled)
        {
            this.characterController.Move(Vector3.up * 2f);
        }
        this.characterController.center = this.characterControllerCenter;
        this.characterController.height = this.characterControllerHeight;
        this.characterCollider.center = this.characterColliderCenter;
        this.characterCollider.height = this.characterColliderHeight;
        if (this.characterController.enabled)
        {
            this.characterController.Move(Vector3.down * 2f);
        }
        this.roll = null;
        this.fallAnim = true;
        yield break;
    }

    public float CalculateJumpVerticalSpeed(float jumpHeight)
    {
        return Mathf.Sqrt(2f * jumpHeight * this.gravity);
    }

    public float CalculateJumpVerticalSpeed()
    {
        return this.CalculateJumpVerticalSpeed(this.jumpHeight);
    }

    public float JumpLength(float speed, float jumpHeight)
    {
        return speed * 2f * this.CalculateJumpVerticalSpeed(jumpHeight) / this.gravity;
    }

    public bool Stumble
    {
        get
        {
            return this.stumble;
        }
        set
        {
            if (value)
            {
                this.StartStumble();
            }
            else
            {
                this.StopStumble();
            }
            this.stumble = value;
        }
    }

    private void StartStumble()
    {
        this.guard.CatchUp();
        this.guard.StartCoroutine(this.StumbleDecay());
    }

    private void StopStumble()
    {
        this.guard.ResetCatchUp();
    }

    private IEnumerator StumbleDecay()
    {
        yield return new WaitForSeconds(this.stumbleDecayTime);
        this.stumble = false;
        this.StopStumble();
        yield break;
    }

    private void NotifyStumble(AudioClipInfo sound, string nameOfCollider)
    {
        if (this.game.IsInJetpackMode)
        {
            return;
        }
        So.Instance.playSound(sound);
        if (this.track.IsRunningOnTutorialTrack)
        {
            return;
        }
        if (this.OnStumble != null)
        {
            this.OnStumble();
            switch (nameOfCollider)
            {
                case "lightSignal":
                    Missions.Instance.PlayerDidThis(Missions.MissionTarget.BumpLightSignal, 1);
                    goto IL_195;
                case "bush":
                case "powerbox":
                    Missions.Instance.PlayerDidThis(Missions.MissionTarget.BumpBush, 1);
                    goto IL_195;
                case "side":
                    goto IL_195;
                case "collider stumble":
                    Missions.Instance.PlayerDidThis(Missions.MissionTarget.BumpTrain, 1);
                    goto IL_195;
                case "blocker_jump":
                    Missions.Instance.PlayerDidThis(Missions.MissionTarget.BumpBarrier, 1);
                    goto IL_195;
                case "blocker_roll":
                    Missions.Instance.PlayerDidThis(Missions.MissionTarget.BumpBarrier, 1);
                    goto IL_195;
                case "blocker_standard":
                    Missions.Instance.PlayerDidThis(Missions.MissionTarget.BumpBarrier, 1);
                    goto IL_195;
                case "collider":
                    goto IL_195;
            }
            Missions.Instance.PlayerDidThis(Missions.MissionTarget.BumpTrain, 1);
        }
    IL_195:
        this.Stumble = true;
    }

    private void NotifyCriticalHit()
    {
        if (this.OnCriticalHit != null)
        {
            this.OnCriticalHit();
            string text = this.lastHitTag;
            switch (text)
            {
                case "HitTrain":
                    this.stats.trainHit++;
                    break;
                case "HitBarrier":
                    this.stats.barrierHit++;
                    break;
                case "HitMovingTrain":
                    this.stats.movingTrainHit++;
                    break;
            }
        }
    }

    public void NotifyPickup(Pickup pickup)
    {
        pickup.NotifyPickup(this.CharacterPickupParticleSystem);
    }

    public void ChangeAnimations()
    {
        if (this.game.isDead)
        {
            return;
        }
        if (this.hoverboard.isActive)
        {
            this.animations.run = "h_run";
            this.animations.roll = "h_roll";
            this.animations.dodgeLeft = "h_left";
            this.animations.dodgeRight = "h_right";
        }
        else
        {
            if (this.superSneakers.isActive)
            {
                this.animations.run = "superRun";
                this.animations.landing = "landing";
            }
            else
            {
                this.animations.SetRandomRun();
            }
            this.animations.roll = "roll";
            this.animations.dodgeLeft = "dodgeLeft";
            this.animations.dodgeRight = "dodgeRight";
            this.animations.SetRandomJump();
        }
        if (this.characterController.isGrounded)
        {
            this.characterAnimation.CrossFade(this.animations.run);
        }
    }

    public void SetAnimations()
    {
        this.animations.run = "run";
        this.animations.runAnimations = new string[] { "run", "run2", "run3", "run4_long" };
        this.animations.landAnimations = new string[] { "landing", "landing", "landing", "landing3" };
        this.animations.jumpAnimations = new string[] { "jump", "jump", "jump_salto", "jump2", "jump3" };
        this.animations.hangtimeAnimations = new string[] { "hangtime", "hangtime", "hangtime2", "hangtime3" };
        this.animations.grindAnimations = new string[] { "h_Grind1", "h_Grind2", "h_Grind3" };
        this.animations.grindLandAnimations = new string[] { "landing_grind1", "landing_grind2", "landing_grind3" };
        this.animations.hoverAnimations = new string[] { "h_run" };
        this.animations.hoverLandAnimations = new string[] { "h_landing" };
        this.animations.hoverJumpAnimations = new string[]
        {
            "h_jump", "h_jump2_kickflip", "h_jump3_180", "h_jump4_360flip", "h_jump5_Impossible", "h_jump6_nollie", "h_jump7_heelflip", "h_jump8_pop shuvit", "h_jump9_fs360", "h_jump10_heel360",
            "h_jump11_fs salto"
        };
        this.animations.hoverHangtimeAnimations = new string[]
        {
            "h_hangtime", "h_jump2_kickflip", "h_jump3_180", "h_jump4_360flip", "h_jump5_Impossible", "h_jump6_nollie", "h_jump7_heelflip", "h_jump8_pop shuvit", "h_jump9_fs360", "h_jump10_heel360",
            "h_jump11_fs salto"
        };
        this.animations.run = "run";
        this.animations.jump = "jump";
        this.animations.hangtime = "hangtime";
        this.animations.landing = "landing";
        this.animations.roll = "roll";
        this.animations.dodgeLeft = "dodgeLeft";
        this.animations.dodgeRight = "dodgeRight";
        this.animations.hitMid = "death_bounce";
        this.animations.hitUpper = "death_upper";
        this.animations.hitLower = "death_lower";
        this.animations.hitMoving = "death_movingTrain";
        this.animations.stumble = "stumble_low";
        this.animations.stumbleDeath = "caught";
        this.animations.stumbleLeftSide = "stumbleSideLeft";
        this.animations.stumbleRightSide = "stumbleSideRight";
        this.animations.stumbleLeftCorner = "stumbleCornerLeft";
        this.animations.stumbleRightCorner = "stumbleCornerRight";
    }

    private void SetRunAnim()
    {
        if (this.hoverboard.isActive)
        {
            if (base.transform.position.y > 20f)
            {
                this.animations.SetRandomGrind();
            }
            else
            {
                this.animations.SetRandomHover();
            }
        }
        else if (!this.superSneakers.isActive)
        {
            this.animations.SetRandomRun();
        }
    }

    public static Character Instance
    {
        get
        {
            Character character;
            if ((character = Character.instance) == null)
            {
                character = (Character.instance = UnityEngine.Object.FindObjectOfType(typeof(Character)) as Character);
            }
            return character;
        }
    }

    public int initialTrackIndex = 1;

    public CapsuleCollider characterCollider;

    public OnTriggerObject coinMagnetCollider;

    public Character.OnStumbleDelegate OnStumble;

    public Character.OnCriticalHitDelegate OnCriticalHit;

    public Character.OnJumpDelegate OnJump;

    public Transform characterRoot;

    public float characterAngle = 45f;

    public Animation characterAnimation;

    public GameObject shadow;

    public ParticleSystem hoverboardCrashParticleSystem;

    public Transform superJumpEFX;

    public bool fallAnim;

    private Vector3 characterControllerCenter;

    private float characterControllerHeight;

    private Vector3 characterColliderCenter;

    private float characterColliderHeight;

    public CharacterPickupParticles CharacterPickupParticleSystem;

    public float ColliderTrackWidth = 17f;

    public Animation guardAnimation;

    [HideInInspector]
    public CharacterController characterController;

    [HideInInspector]
    public OnTriggerObject characterColliderTrigger;

    [HideInInspector]
    public CharacterModel characterModel;

    [HideInInspector]
    public CharacterCamera characterCamera;

    [HideInInspector]
    public Hoverboard hoverboard;

    [HideInInspector]
    public SuperSneakers superSneakers;

    [HideInInspector]
    public Running running;

    public GameObject sprayCanModel;

    [HideInInspector]
    public bool immuneToCriticalHit;

    [HideInInspector]
    public int trackIndex;

    [HideInInspector]
    public float x;

    public float z;

    public float verticalSpeed;

    [HideInInspector]
    public float lastGroundedY;

    [HideInInspector]
    public bool isInsideSubway;

    private int trackMovement;

    private int trackMovementNext;

    private float characterRotation;

    private int trackIndexTarget;

    private float trackIndexPosition;

    private Game game;

    private Track track;

    [HideInInspector]
    public Character.Animations animations;

    [HideInInspector]
    public float jumpHeight;

    public float gravity = 200f;

    public float jumpHeightNormal = 20f;

    public float jumpHeightSuperSneakers = 40f;

    public float verticalFallSpeedLimit = -1f;

    public float stumbleCornerTolerance = 15f;

    [HideInInspector]
    public bool stumble;

    public float stumbleDecayTime = 5f;

    private IEnumerator roll;

    [HideInInspector]
    public bool jumping;

    private Character.SuperSneakersJump? superSneakersJump;

    public AnimationCurve superSneakersJumpCurve;

    public float superSneakersJumpApexRatio = 0.5f;

    [HideInInspector]
    public bool falling;

    private bool inAirJump;

    private string lastHitTag;

    [HideInInspector]
    public bool stopColliding;

    private GameStats stats;

    private FollowingGuard guard;

    private AnimationState runState;

    private int grindSwitch = 1;

    private static Character instance;

    private bool startedJumpFromGround;

    private float trainJumpSampleZ;

    private float trainJumpSampleLength = 10f;

    private bool trainJump;

    private float verticalSpeed_jumpTolerance = -30f;

    private Character.ObstacleTypes lastObstacleTriggerType;

    private int lastObstacleTriggerTrackInex;

    public AudioClipInfo DodgeLeft;

    public AudioClipInfo DodgeRight;

    public AudioClipInfo H_Left;

    public AudioClipInfo H_Right;

    private Vector3 last;

    public AudioClipInfo StumbleSound;

    public AudioClipInfo StumbleBushSound;

    public AudioClipInfo StumbleSideSound;

    private struct SuperSneakersJump
    {
        public float z_start;

        public float z_length;

        public float z_end;

        public float y_start;
    }

    public struct Animations
    {
        public void SetRandomGrind()
        {
            if (this.grindAnimations.Length == 0 || this.grindLandAnimations.Length != this.grindAnimations.Length)
            {
                Debug.Log("animation arrays should be same length if paired; also not null");
                return;
            }
            int num = UnityEngine.Random.Range(0, this.grindAnimations.Length);
            this.run = this.grindAnimations[num];
            this.landing = this.grindLandAnimations[num];
        }

        public void SetRandomRun()
        {
            if (this.runAnimations.Length == 0 || this.runAnimations.Length != this.landAnimations.Length)
            {
                Debug.Log("animation arrays should be same length if paired; also not null");
                return;
            }
            int num = UnityEngine.Random.Range(0, this.runAnimations.Length);
            this.run = this.runAnimations[num];
            this.landing = this.landAnimations[num];
        }

        public void SetRandomHover()
        {
            if (this.hoverAnimations.Length == 0 || this.hoverAnimations.Length != this.hoverLandAnimations.Length)
            {
                Debug.Log("animation arrays should be same length if paired; also not null");
                return;
            }
            int num = UnityEngine.Random.Range(0, this.hoverAnimations.Length);
            this.run = this.hoverAnimations[num];
            this.landing = this.hoverLandAnimations[num];
        }

        public void SetRandomJump()
        {
            if (this.jumpAnimations.Length == 0 || this.hangtimeAnimations.Length == 0)
            {
                Debug.Log("animation array is null");
                return;
            }
            int num = UnityEngine.Random.Range(0, this.jumpAnimations.Length);
            int num2 = UnityEngine.Random.Range(0, this.hangtimeAnimations.Length);
            this.jump = this.jumpAnimations[num];
            this.hangtime = this.hangtimeAnimations[num2];
        }

        public void SetRandomHoverJump()
        {
            if (this.hoverJumpAnimations.Length == 0 || this.hoverJumpAnimations.Length != this.hoverHangtimeAnimations.Length)
            {
                Debug.Log("animation arrays should be same length if paired; also not null");
                return;
            }
            int num = UnityEngine.Random.Range(0, this.hoverJumpAnimations.Length);
            this.jump = this.hoverJumpAnimations[num];
            this.hangtime = this.hoverHangtimeAnimations[num];
        }

        public string[] runAnimations;

        public string[] landAnimations;

        public string[] jumpAnimations;

        public string[] hangtimeAnimations;

        public string[] grindAnimations;

        public string[] grindLandAnimations;

        public string[] hoverAnimations;

        public string[] hoverLandAnimations;

        public string[] hoverJumpAnimations;

        public string[] hoverHangtimeAnimations;

        public string jump;

        public string run;

        public string landing;

        public string hangtime;

        public string roll;

        public string dodgeLeft;

        public string dodgeRight;

        public string hitMid;

        public string hitUpper;

        public string hitLower;

        public string hitMoving;

        public string stumble;

        public string stumbleDeath;

        public string stumbleLeftSide;

        public string stumbleRightSide;

        public string stumbleLeftCorner;

        public string stumbleRightCorner;
    }

    private enum ObstacleTypes
    {
        jumpHighBarrier,
        jumpTrain,
        rollBarrier,
        jumpBarrier,
        none
    }

    private enum ImpactX
    {
        Left,
        Middle,
        Right
    }

    private enum ImpactY
    {
        Upper,
        Middle,
        Lower
    }

    private enum ImpactZ
    {
        Before,
        Middle,
        After
    }

    public delegate void OnStumbleDelegate();

    public delegate void OnCriticalHitDelegate();

    public delegate void OnJumpDelegate();

    public delegate void OnGroundedDelegate();
}
