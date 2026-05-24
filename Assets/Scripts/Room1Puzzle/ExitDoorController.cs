using System.Collections;
using UnityEngine;

namespace Room1Puzzle
{
    /// <summary>Slides the door open when the puzzle is solved.</summary>
    public class ExitDoorController : MonoBehaviour
    {
        public Transform doorLeaf;
        public Vector3 slideDirection = Vector3.up;
        public float slideDistance = 2.2f;
        public float openSpeed = 1.1f;
        public AudioClip unlockClip;

        bool done;
        Vector3 closedPos, openPos;
        AudioSource src;

        void Awake()
        {
            src = GetComponent<AudioSource>();
            if (src == null)
                src = gameObject.AddComponent<AudioSource>();
            src.spatialBlend = 1f;

            if (doorLeaf == null)
                doorLeaf = transform;

            closedPos = doorLeaf.position;
            openPos = closedPos + slideDirection.normalized * slideDistance;
        }

        public void UnlockAndOpen()
        {
            if (done) return;
            done = true;
            if (unlockClip != null)
                src.PlayOneShot(unlockClip);
            StartCoroutine(Slide());
        }

        IEnumerator Slide()
        {
            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime * openSpeed;
                doorLeaf.position = Vector3.Lerp(closedPos, openPos, t);
                yield return null;
            }

            doorLeaf.position = openPos;
        }
    }
}
