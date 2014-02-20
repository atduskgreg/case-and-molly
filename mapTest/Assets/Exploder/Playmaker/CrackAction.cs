// uncomment next line to work with Playmaker
//#define PLAYMAKER
#if PLAYMAKER

// (c) Copyright HutongGames, LLC 2010-2013. All rights reserved.

using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
    [ActionCategory(ActionCategory.Effects)]
    [Tooltip("Crack objects in the radius using Exploder")]
    public class Crack : FsmStateAction
    {
        [RequiredField]
        [CheckForComponent(typeof(ExploderObject))]
        [Tooltip("The GameObject with an Exploder component.")]
        public FsmOwnerDefault gameObject;

        [Tooltip("Position of the exploder")]
        public FsmVector3 Position;

        public override void Reset()
        {
            gameObject = null;
        }

        public override void OnEnter()
        {
            var go = Fsm.GetOwnerDefaultTarget(gameObject);
            if (go != null)
            {
                var exploder = go.GetComponent<ExploderObject>();

                if (exploder != null)
                {
                    if (!Position.IsNone)
                    {
                        exploder.transform.position = Position.Value;
                    }

                    exploder.Crack(OnCracked);
                }
            }
        }

        void OnCracked()
        {
            Finish();
        }
    }
}

#endif
