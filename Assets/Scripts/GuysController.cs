using Unity.VisualScripting;
namespace BGTest.Game
{
    using UnityEngine;
    using System.Collections.Generic;
    using UniRx;
    using UniRx.Triggers;
    using System;

    /// <summary>
    /// Контроллер бегущих игроков
    /// </summary>
    public class GuysController : MonoBehaviour
    {
        /// <summary>
        /// Игрок попал на кристал
        /// </summary>
        public event Action OnCrystalHit = delegate { };
        
        [SerializeField]
        private PlayerGuyViewController _playerGuyViewController;

        [SerializeField]
        private GuysGroupField _groupField;

        [SerializeField]
        private DrawField _drawField;
        
        [Header("Spawn settings")]
        
        [SerializeField, Min(1)]
        private int _defaultGuysCount = 15;

        [SerializeField, Min(0.01f)]
        private float _distanceBetweenGuys = 0.2f;

        [Header("Position settings")]
        
        [SerializeField, Min(0.1f)]
        private float _lenghtPos = 1;

        [SerializeField, Min(0.01f)]
        private float _moveSpeed = 2;
        
        private LayerMask _trapLayer;
        
        private LayerMask _rewardLayer;

        private LayerMask _lonelyPlayerLayer;

        private float _gameCont;

        private List<PlayerGuyViewController> _guysList = new List<PlayerGuyViewController>();

        private CompositeDisposable _disposable = new CompositeDisposable();

        private void Awake()
        {
            _trapLayer = LayerMask.NameToLayer("Trap");
            _rewardLayer = LayerMask.NameToLayer("Reward");
            _lonelyPlayerLayer = LayerMask.NameToLayer("LonelyPlayer");
            
            SpawnGuys();
            _drawField.OnDrawEnd += SetGuysPosition;
            _groupField.OnPathEnd += OnWinCallback;
        }
        private void OnWinCallback()
        {
            foreach (PlayerGuyViewController guy in _guysList)
            {
                guy.SetDanceState();
            }
        }

        /// <summary>
        /// Выставляем позицию игроков по рисунку
        /// </summary>
        private void SetGuysPosition()
        {
            if (_guysList.Count == 0)
            {
                return;
            }
            
            if (_disposable.Count == 0)
            {
                foreach (var guy in _guysList)
                {
                    guy.SetRunState();
                }
            }
            
            _disposable.Clear();
            
            float lenght = 0;
            
            var positions = _drawField.GetPositions();
            for (int i = positions.Count - 1; i > 1; i--)
            {
               lenght += (positions[i] - positions[i - 1]).magnitude;
            }

            lenght /= _guysList.Count;

            for (int i = 0; i < _guysList.Count; i++)
            {
                float iterator = 0;
                var pos = _guysList[i].transform.localPosition;
                var finalPos = FindPos(lenght * i);
                finalPos = new Vector3(finalPos.x, pos.y, finalPos.z);
                int guy = i;
                Observable.EveryUpdate().Subscribe(_ =>
                {
                    if (iterator >= 1)
                    {
                        return;
                    }
                    _guysList[guy].transform.localPosition = Vector3.Lerp(pos, finalPos, iterator);
                    iterator += Time.deltaTime * _moveSpeed;
                }).AddTo(_disposable);
            }

            Vector3 FindPos(float posOn)
            {
                float lenght = 0;
                for (int i = positions.Count - 1; i > 1; i--)
                {
                    lenght += (positions[i] - positions[i - 1]).magnitude;
                    if (lenght >= posOn)
                    {
                        var pos=  Vector3.MoveTowards(positions[i], positions[i - 1], lenght - posOn);
                        return pos * _lenghtPos;
                    }
                }
                return Vector3.zero;
            }
            
           
        }

        private void OnDestroy()
        {
            _drawField.OnDrawEnd -= SetGuysPosition;
            _groupField.OnPathEnd -= OnWinCallback;
        }

        /// <summary>
        /// Спавним игроков в рамке коллайдера
        /// </summary>
        private void SpawnGuys()
        { 
            var guySize = _playerGuyViewController.Bounds.size;

            var groupBounds = _groupField.Bounds;

            int groupLineCapacityX = (int)(groupBounds.size.x / guySize.x);

            int groupLineCapacityZ = (int)(groupBounds.size.z / guySize.z);
            
            int spawnedGuys = 0;
            
            for (int i = 0; i < groupLineCapacityZ; i++)
            {
                for (int j = 0; j < groupLineCapacityX; j++)
                {
                    var pos = new Vector3(
                        groupBounds.max.x - (guySize.x + _distanceBetweenGuys) * j - guySize.x / 2,
                        groupBounds.center.y - groupBounds.extents.y,
                        groupBounds.max.z - (guySize.z + _distanceBetweenGuys) * i - guySize.z / 2);
                    var guy = Instantiate(_playerGuyViewController, pos, Quaternion.identity, _groupField.transform);
                    guy.SetIdleState();
                    SubGuyHit(guy);
                    _guysList.Add(guy);
                    spawnedGuys++;
                    if (spawnedGuys == _defaultGuysCount)
                    {
                        return;
                    }
                }
                if (spawnedGuys == _defaultGuysCount)
                {
                    return;
                }
            }
            
            Debug.LogWarning($"Не хватило места для {_defaultGuysCount - spawnedGuys} игроков");
        }

        /// <summary>
        /// Подписываемся на коллбек попадания на игровые обьекты
        /// </summary>
        /// <param name="guy">Игрок</param>
        private void SubGuyHit(PlayerGuyViewController guy)
        {
            CompositeDisposable disposable = new CompositeDisposable();

            if (guy.Collider == null)
            {
                Debug.LogError($"Коллайдер не выставлен {guy.gameObject.name}");
            }
            
            guy.Collider.OnTriggerEnterAsObservable().Subscribe(collider =>
            {
                if (collider.gameObject.layer == _trapLayer)
                {
                    guy.SetDead();
                    guy.transform.parent = null;
                    _guysList.Remove(guy);
                    disposable.Clear();
                    return;
                }

                if (collider.gameObject.layer == _rewardLayer && collider.gameObject.TryGetComponent(out CrystalView crystal))
                {
                    crystal.Take();
                    OnCrystalHit();
                    return;
                }

                if (collider.gameObject.layer == _lonelyPlayerLayer && collider.gameObject.TryGetComponent(out PlayerGuyViewController newGuy))
                {
                    newGuy.SetRunState();
                    newGuy.transform.parent = _groupField.transform;
                    newGuy.gameObject.layer = guy.gameObject.layer;
                    newGuy.Collider.isTrigger = false;
                    _guysList.Add(newGuy);
                    SubGuyHit(newGuy);
                    return;
                }
            }).AddTo(disposable);
        }
    }
}
