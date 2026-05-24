using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Filtering;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class CorrectSocketOnly : MonoBehaviour
{
    public string acceptedObjectID;

    private XRSocketInteractor socket;

    void Awake()
    {
        socket = GetComponent<XRSocketInteractor>();
        socket.selectFilters.Add(new CorrectObjectFilter(this));
    }

    private class CorrectObjectFilter : IXRSelectFilter
    {
        private CorrectSocketOnly rule;

        public CorrectObjectFilter(CorrectSocketOnly rule)
        {
            this.rule = rule;
        }

        public bool canProcess => true;

        public bool Process(IXRSelectInteractor interactor, IXRSelectInteractable interactable)
        {
            SequenceObject obj = interactable.transform.GetComponent<SequenceObject>();

            if (obj == null)
                return false;

            return obj.objectID == rule.acceptedObjectID;
        }
    }
}