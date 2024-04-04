namespace BGTest.Game
{
    using UnityEngine;
    using Dreamteck.Splines;
    using System;

    /// <summary>
    /// Поле для бегунов
    /// </summary>
    public class GuysGroupField : MonoBehaviour
    {
        public event Action OnPathEnd = delegate { };

        public Bounds Bounds => _collider.bounds;
        
        [SerializeField]
        private Collider _collider;

        [SerializeField]
        private SplineFollower _splineFollower;

        [SerializeField]
        private DrawField _drawField;

        private void Awake()
        {
            _splineFollower.follow = false;
            _splineFollower.onEndReached += OnEnd;
            _drawField.OnDrawEnd += OnDrawEnd;
        }
        private void OnEnd(double obj)
        {
            OnPathEnd();
        }
        private void OnDrawEnd()
        {
            _splineFollower.follow = true;
            _drawField.OnDrawEnd -= OnDrawEnd;
        }

        private void OnDestroy()
        {
            _splineFollower.onEndReached -= OnEnd;
            _drawField.OnDrawEnd -= OnDrawEnd;
        }
    }
}
