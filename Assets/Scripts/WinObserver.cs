namespace BGTest.Game
{
    using UnityEngine;

    /// <summary>
    /// Наблюдатель для запуска победного ивента
    /// </summary>
    public class WinObserver : MonoBehaviour
    {
        [SerializeField]
        private Transform _winTransform;

        [SerializeField]
        private GuysGroupField _groupField;

        private void Awake()
        {
            _winTransform.gameObject.SetActive(false);
            _groupField.OnPathEnd += OnWin;
        }
        private void OnWin()
        {
            _winTransform.gameObject.SetActive(true);
        }

        private void OnDestroy()
        {
            _groupField.OnPathEnd -= OnWin;
        }
    }
}
