using System;
using UnityEngine;
using GameFramework.Extensions;

public partial class CharacterMovement
{
    public abstract class Module
    {
        protected const float COLLISION_OFFSET = .001f;
        protected const float RECALCULATE_NORMAL_FALLBACK = .01f;
        protected const float RECALCULATE_NORMAL_ADDON = .001f;

        public virtual void OnLoaded(CharacterMovement charMovement)
        {
            if (mCharMovement is not null)
            {
                throw new UnityException($"{this} module is already loaded by {mCharMovement}");
            }

            mCharMovement = charMovement;
        }

        public virtual void OnUnloaded(CharacterMovement charMovement)
        {
            if (mCharMovement is null)
            {
                throw new UnityException($"{this} module is not loaded by any CharacterMovementBehaviour");
            }

            if (charMovement != mCharMovement)
            {
                throw new UnityException($"{this} module is not loaded by {charMovement}");
            }
        }

        public virtual void Update()
        {
            if (mCharMovement is null)
            {
                throw new NullReferenceException(@"mCharMovement is null, 
                    this module is not loaded by CharacterMovementBehaviour");
            }

            mCapsule = mCharMovement.mCapsule;
            mCollider = mCharMovement.mCollider;
            mSkinWidth = mCharMovement.mSkinWidth;

            mCharUp = mCharacter.up;
            mCharRight = mCharacter.right;
            mCharForward = mCharacter.forward;
        }

        public virtual void SetMoveVector(Vector2 move)
        {
            mInputMove = move;
            mInputMoveAngle = Vector2.SignedAngle(mInputMove, Vector2.up);
        }

        protected bool CapsuleCast(Vector3 move, out RaycastHit hit)
        {
            bool result = CapsuleCast(move, out RaycastHit smallHit, out RaycastHit bigHit);
            hit = smallHit.collider ? smallHit : bigHit;

            return result;
        }

        protected bool CapsuleCast(Vector3 move, out RaycastHit smallHit, out RaycastHit bigHit)
        {
            if (mSkinWidth > 0f)
            {
                mCapsule.radius += mSkinWidth;
                mCapsule.CapsuleCast(move, out bigHit);
                mCapsule.radius -= mSkinWidth;

                if (bigHit.collider)
                {
                    move = move.normalized * bigHit.distance;
                }

                mCapsule.CapsuleCast(move, out smallHit);
            }
            else
            {
                mCapsule.CapsuleCast(move, out bigHit);
                smallHit = bigHit;
            }

            return smallHit.collider || bigHit.collider;
        }

        protected bool BaseSphereCast(Vector3 move, out RaycastHit hit)
        {
            bool result = BaseSphereCast(move, out RaycastHit smallHit, out RaycastHit bigHit);
            hit = smallHit.collider ? smallHit : bigHit;

            return result;
        }

        protected bool BaseSphereCast(Vector3 move, out RaycastHit hit, out Vector3 hitNormal)
        {
            bool didHit = BaseSphereCast(move, out hit);
            RecalculateNormal(hit, move.normalized, out hitNormal);

            return didHit;
        }

        protected bool BaseSphereCast(Vector3 move, out RaycastHit smallHit, out RaycastHit bigHit)
        {
            if (mSkinWidth > 0f)
            {
                mCapsule.radius += mSkinWidth;
                mCapsule.BaseSphereCast(move, out bigHit);
                mCapsule.radius -= mSkinWidth;

                if (bigHit.collider)
                {
                    move = move.normalized * bigHit.distance;
                }

                mCapsule.BaseSphereCast(move, out smallHit);
            }
            else
            {
                mCapsule.BaseSphereCast(move, out bigHit);
                smallHit = bigHit;
            }

            return smallHit.collider || bigHit.collider;
        }

        protected Vector3 CapsuleMove(Vector3 move)
        {
            return CapsuleMove(move, out RaycastHit smallHit, out RaycastHit bigHit);
        }

        protected Vector3 CapsuleMove(Vector3 move, out RaycastHit hit)
        {
            Vector3 moved = CapsuleMove(move, out RaycastHit smallHit, out RaycastHit bigHit);
            hit = smallHit.collider ? smallHit : bigHit;

            return moved;
        }

        protected Vector3 CapsuleMove(Vector3 move, out RaycastHit hit, out Vector3 hitNormal)
        {
            var moved = CapsuleMove(move, out RaycastHit smallHit, out RaycastHit bigHit, out hitNormal);
            hit = smallHit.collider ? smallHit : bigHit;

            return moved;
        }

        protected Vector3 CapsuleMove(Vector3 move, out RaycastHit smallHit, out RaycastHit bigHit)
        {
            if (move.magnitude == 0f)
            {
                smallHit = new RaycastHit();
                bigHit = new RaycastHit();

                CapsuleResolvePenetration();

                return Vector3.zero;
            }

            CapsuleCast(move, out smallHit, out bigHit);

            Vector3 direction = move.normalized;
            float distance = move.magnitude;

            if (smallHit.collider)
            {
                float skinWidth = Mathf.Max(0, mSkinWidth);
                distance = smallHit.distance - skinWidth - COLLISION_OFFSET;
            }
            else if (bigHit.collider)
            {
                distance = bigHit.distance - COLLISION_OFFSET;
            }

            mCapsule.position += direction * distance;

            CapsuleResolvePenetration();

            return direction * Math.Max(0f, distance);
        }

        protected Vector3 CapsuleMove(Vector3 move, out RaycastHit smallHit, out RaycastHit bigHit, out Vector3 hitNormal)
        {
            Vector3 moved = CapsuleMove(move, out smallHit, out bigHit);
            RaycastHit hit = smallHit.collider ? smallHit : bigHit;

            RecalculateNormal(hit, move.normalized, out hitNormal);

            return moved;
        }

        protected Vector3 CapsuleResolvePenetration()
        {
            Vector3 resolve = Vector3.zero;

            if (mSkinWidth > 0f)
            {
                mCapsule.radius += mSkinWidth;
                resolve = mCapsule.ResolvePenetration(mCollider, COLLISION_OFFSET);
                mCapsule.radius -= mSkinWidth;
            }
            else
            {
                resolve = mCapsule.ResolvePenetration(mCollider, COLLISION_OFFSET);
            }

            return resolve;
        }

        protected void TeleportTo(Vector3 pos, Quaternion rot)
        {
            mCapsule.position = pos;
            mCapsule.rotation = rot;
        }

        protected bool RecalculateNormal(RaycastHit hit, out Vector3 normal)
        {
            return hit.RecalculateNormalUsingRaycast(out normal,
                mCapsule.layerMask, mCapsule.queryTrigger);
        }

        protected bool RecalculateNormal(RaycastHit hit, Vector3 direction, out Vector3 normal)
        {
            if (hit.collider && direction != Vector3.zero)
            {
                Vector3 origin = hit.point + (-direction * RECALCULATE_NORMAL_FALLBACK);
                const float rayDistance = RECALCULATE_NORMAL_FALLBACK + RECALCULATE_NORMAL_ADDON;

                Physics.Raycast(origin, direction, out RaycastHit rayHit,
                    rayDistance, mCapsule.layerMask, mCapsule.queryTrigger);

                if (rayHit.collider && rayHit.collider == hit.collider)
                {
                    normal = rayHit.normal;
                    return true;
                }
            }

            if (RecalculateNormal(hit, out normal))
            {
                return true;
            }

            normal = Vector3.zero;
            return false;
        }

        protected bool RecalculateNormalIfZero(RaycastHit hit, ref Vector3 normal)
        {
            if (normal == Vector3.zero)
            {
                return RecalculateNormal(hit, out normal);
            }

            return true;
        }

        protected void FlushCapsuleMove()
        {
            mCapsule.WriteValuesTo(mCollider);
        }

        protected virtual void SetVelocityByMove(Vector3 moved)
        {
            moved = moved / mDeltaTime;
            mVelocity = moved;
        }

        protected CharacterMovement mCharMovement;
        protected Character mCharacter;
        protected CapsuleCollider mCollider;
        protected VirtualCapsule mCapsule;
        protected float mSkinWidth;

        protected float mDeltaTime;
        protected Vector3 mCharUp;
        protected Vector3 mCharRight;
        protected Vector3 mCharForward;
        protected Vector3 mVelocity;

        protected Vector3 mInputMove;
        protected float mInputMoveAngle;
    }
}

public abstract class CharacterMovementModule : CharacterMovement.Module { }

public abstract class CharacterMovementModuleSource : ScriptableObject
{
    public const string MENU_PATH = "CharacterMovement/";

    public abstract CharacterMovementModule GetModule();
}