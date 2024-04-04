namespace BGTest.Game
{
    using UniRx;
    using UnityEngine;

    /// <summary>
    /// Вьюшка-контроллер бегущего игрока
    /// </summary>
    public class PlayerGuyViewController : MonoBehaviour
    {
        private const string DANCE_STATE = "IsDancing";

        private const string RUN_STATE = "RunState";

        public Bounds Bounds => _meshRenderer.bounds;

        public Collider Collider => _collider;

        [SerializeField]
        private Collider _collider;
        
        [SerializeField]
        private SkinnedMeshRenderer _meshRenderer;

        [SerializeField]
        private Animator _animator;

        [SerializeField]
        private ParticleSystem _deadParticles;

        private CompositeDisposable _disposable = new CompositeDisposable();

        public void SetRunState()
        {
            ResetDanceState();
            _disposable.Clear();

            if (_animator.GetFloat(RUN_STATE) == 1)
            {
                return;
            }
      
            float iterator = 0;

            Observable.EveryUpdate().Subscribe(_ =>
            {
                if (iterator >= 1)
                {
                    _animator.SetFloat(RUN_STATE, 1);
                    _disposable.Clear();
                    return;
                }
                
                _animator.SetFloat(RUN_STATE, iterator);
                iterator += Time.deltaTime;

            }).AddTo(_disposable);
        }

        public void SetIdleState()
        {
            ResetDanceState();
            _disposable.Clear();

            if (_animator.GetFloat(RUN_STATE) == 0)
            {
                return;
            }
            
            float iterator = 1;

            Observable.EveryUpdate().Subscribe(_ =>
            {
                if (iterator <= 0)
                {
                    _animator.SetFloat(RUN_STATE, 0);
                    _disposable.Clear();
                    return;
                }

                _animator.SetFloat(RUN_STATE, iterator);
                iterator -= Time.deltaTime;

            }).AddTo(_disposable);
        }

        public void SetDead()
        {
            _meshRenderer.enabled = false;
            _deadParticles.Play();
        }

        public void SetDanceState()
        {
            float iteratorAngle = 0;
            Observable.EveryUpdate()
                .Subscribe(_ =>
            {
                if (iteratorAngle >= 180)
                {
                    transform.rotation = Quaternion.Euler(0, 180,0);
                    return;
                }
                transform.rotation = Quaternion.Euler(0, iteratorAngle,0);
                iteratorAngle += Time.deltaTime * 60;
            }).AddTo(this);
           
            _animator.SetBool(DANCE_STATE, true);
        }

        private void OnDisable()
        {
            _disposable.Clear();
        }

        private void ResetDanceState() => _animator.SetBool(DANCE_STATE, false);
    }
}
