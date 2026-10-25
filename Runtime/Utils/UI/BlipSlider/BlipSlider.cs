using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace SHUU.Utils.UI
{
    public class BlipSlider : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        #region Variables
        
        #region Inspector
        [Header("References")]
        [SerializeField] private RectTransform sliderRegion;
        [SerializeField] private RectTransform[] blips;


        [Header("Events")]
        public UnityEvent<int> onBlipChanged;


        [Header("Drag Settings")]
        [SerializeField] private bool allowDragOutsideRegion = true;
        #endregion



        #region Internal
        public int activeBlip { get; private set; } = -1;

        private bool isHolding;
        #endregion

        #endregion




        #region Main
        private void Update()
        {
            if (isHolding && Input.GetMouseButtonUp(0)) StopInteraction();
        }
        #endregion



        #region Override points
        public void OnPointerDown(PointerEventData eventData)
        {
            int index = GetBlipUnderPointer(eventData);

            if (index == -1) return;

            isHolding = true;
            SetActiveBlip(index);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!isHolding) return;
            if (!allowDragOutsideRegion && !RectTransformUtility.RectangleContainsScreenPoint(sliderRegion, eventData.position, eventData.pressEventCamera)) return;

            int index = GetBlipUnderPointer(eventData);
            if (index != -1) SetActiveBlip(index);
        }

        public void OnPointerUp(PointerEventData eventData) => StopInteraction();
        #endregion



        #region Logic
        private int GetBlipUnderPointer(PointerEventData eventData)
        {
            for (int i = 0; i < blips.Length; i++)
                if (RectTransformUtility.RectangleContainsScreenPoint(blips[i], eventData.position, eventData.pressEventCamera)) return i;

            return -1;
        }

        private void SetActiveBlip(int index)
        {
            if (activeBlip == index) return;

            activeBlip = index;
            onBlipChanged?.Invoke(index);
        }


        private void StopInteraction() => isHolding = false;
        #endregion
    }
}
