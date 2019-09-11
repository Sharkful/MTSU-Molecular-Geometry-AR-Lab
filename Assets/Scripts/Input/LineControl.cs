using UnityEngine;

namespace MtsuMLAR
{
    [RequireComponent(typeof(LineRenderer))]
    public class LineControl : MonoBehaviour
    {
        #region Private Variables
        //Line Drawing Variables
        [SerializeField]
        private float cursorExtent = 10.0f;
        [SerializeField]
        private int numSegments = 30;
        private Vector3[] pointPositions;

        //Component Variables
        //private Raycaster rayScript;
        private MLEventSystem eventSystem;
        private MLInputModuleV2 inputModule;
        private LineRenderer lRend;
        #endregion

        private void Awake()
        {
            //rayScript = GetComponent<Raycaster>();
            lRend = GetComponent<LineRenderer>();
            lRend.positionCount = numSegments + 1;
            pointPositions = new Vector3[numSegments + 1];
        }

        private void Start()
        {
            inputModule = GameObject.FindGameObjectWithTag("MLEventSystem").GetComponent<MLInputModuleV2>();
            eventSystem = GameObject.FindGameObjectWithTag("MLEventSystem").GetComponent<MLEventSystem>();
        }

        /*This Code checks the raycaster state, and draws the cursor depending on
         * whether it is dragging, interacting with an object, or a UI element*/
        private void LateUpdate()
        {
            if (eventSystem.IsDragging)
                DrawSelectLine(eventSystem.DraggedObject.transform);
            else if (inputModule.CurrentHitState == MLInputModuleV2.HitState.ObjectHit && inputModule.PrimaryHitObject.tag == "ARUI")
                DrawStraightLine(inputModule.PrimaryHitObjectDistance);
            else if (inputModule.CurrentHitState == MLInputModuleV2.HitState.ObjectHit)
                DrawSelectLine(inputModule.PrimaryHitObject.transform);
            else
                DrawStraightLine(cursorExtent);

            //switch (rayScript.CurrentRaycasterState)
            //{
            //    case RaycasterState.IsDrag:
            //    case RaycasterState.RayHit:
            //        DrawSelectLine(rayScript.PrevHitObject.transform); //draw the cursor following the object
            //        break;

            //    case RaycasterState.UIHit:
            //        DrawStraightLine(rayScript.Hit.distance);                //Draw a straight line that ends at the UI
            //        break;

            //    case RaycasterState.NoHit:                             //Draw a line of predetermined length
            //        DrawStraightLine(cursorExtent);
            //        break;
            //}
        }

        /*This function takes an input length and draws a straight line that long*/
        private void DrawStraightLine(float length)
        {
            for (int j = 0; j <= numSegments; j++)
            {
                lRend.sortingOrder = 0;
                float fracComplete = (float)j / numSegments;
                pointPositions[j] = Vector3.Lerp(transform.position, transform.position + (transform.forward * length), fracComplete);
            }
            lRend.SetPositions(pointPositions);
        }

        /*This function draws a curved line to the target transform with a straight
         * beginning, giving it a flexible feeling*/
        private void DrawSelectLine(Transform target)
        {
            lRend.sortingOrder = 4;
            pointPositions[0] = transform.position;
            float firstSegmentScale = (target.position - transform.position).magnitude * 4 / numSegments;
            for (int i = 1; i <= numSegments; i++)
            {
                float fracComplete = (float)i / numSegments;
                pointPositions[i] = Vector3.Slerp(transform.forward * firstSegmentScale, target.position - transform.position, fracComplete) + transform.position;
            }
            lRend.SetPositions(pointPositions);
        }
    }
}