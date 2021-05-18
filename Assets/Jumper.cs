using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bomber.Jumper
{
    public class Jumper : MonoBehaviour
    {
        [SerializeField] float jumpForce = 2900f;
        [SerializeField] GameObject jumpFX = null;
        [Range(0.1f, 5f)]
        [SerializeField] float jumpOnRange = 1.2f;
        [Range(0.1f, 3)]
        [SerializeField] float spherecastRadius = 0.7f;

        Rigidbody rb;
        bool isJumping = false;

        private enum JumpState
        {
            Base,
            DoubleJump,
            TripleJump
        }

        JumpState jumpState = JumpState.Base;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
        }

        private void FixedUpdate()
        {
            if (isJumping)
            {
                rb.AddForce(Vector3.up * jumpForce);
                isJumping = false;
                // TODO disable ability to jump again
            }
        }

        // called by the Player Input Component
        public void OnJump()
        {
            // set up fx before jumping
            GameObject fx = Instantiate(jumpFX, transform.position, Quaternion.identity);

            // if (IsJumpingOnEnemy())
            // {
            //     fx.GetComponent<JumpResponder>().DoubleJump();
            // }
            // else
            // {
            //     fx.GetComponent<JumpResponder>().BaseJump();

            // }

            fx.GetComponent<JumpResponder>().TripleJump();


            isJumping = true;
        }

        private bool IsJumpingOnEnemy()
        {
            RaycastHit hit;
            bool hasHit = Physics.SphereCast(transform.position, spherecastRadius, 
                                                    Vector3.down, out hit, jumpOnRange);//transform.position, Vector3.down, out hit, jumpOnRange);
            if (hasHit)
            {
                if(hit.transform.tag == "Slime")
                {
                    print("jumping on enemy!");

                    // TODO start here
                    MoveToTargetHead(hit.transform);

                    if (jumpState == JumpState.Base)
                    {
                        jumpState = JumpState.DoubleJump;
                        return true;
                    }
                    
                }
            }
            return false;
        }

        void MoveToTargetHead(Transform target)
        {
            // turn on kinematic on rigidbody
            // lerp to the top of the head
            // squash enemy
        }

    }

}