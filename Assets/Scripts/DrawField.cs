using System.Linq;
namespace BGTest.Game
{
    using System.Collections.Generic;
    using System;
    using UnityEngine;
    using UnityEngine.EventSystems;

    /// <summary>
    /// Поле для рисования. Чувствителен к родительскому скейлу!!!
    /// </summary>
    public class DrawField : MonoBehaviour, IPointerMoveHandler, IPointerDownHandler, IPointerUpHandler
    {
        /// <summary>
        /// Окончание рисования
        /// </summary>
        public event Action OnDrawEnd = delegate {  };
        
        [Header("Draw settings")]

        [SerializeField]
        private Color _drawColor;
        
        [SerializeField]
        private LineRenderer _lineRenderer;

        private bool _isDraw;

        private void Awake()
        {
            _lineRenderer.startColor = _drawColor;
            _lineRenderer.endColor = _drawColor;
        }

        public void OnPointerMove(PointerEventData eventData)
        {
            if (!_isDraw)
            {
                return;
            }

            var position = eventData.pointerCurrentRaycast.worldPosition;
            if (position == Vector3.zero)
            {
                return;
            }

            _lineRenderer.positionCount += 1;
            _lineRenderer.SetPosition(_lineRenderer.positionCount - 1, _lineRenderer.transform.InverseTransformPoint(position));
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _isDraw = true;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            OnDrawEnd();
            _isDraw = false;
            _lineRenderer.positionCount = 0;
        }

        public List<Vector3> GetPositions()
        {
            Vector3[] returnedPositions = new Vector3[_lineRenderer.positionCount];
            _lineRenderer.GetPositions(returnedPositions);
            for (int i = 0; i < returnedPositions.Length; i++)
            {
               returnedPositions[i] = returnedPositions[i].normalized;
            }
            return returnedPositions.ToList();
        }
    }
}
