namespace BGTest.Game
{
    using UnityEngine;

    /// <summary>
    /// Вьюшка для кристалов\валюты
    /// </summary>
    public class CrystalView : MonoBehaviour
    {
        [SerializeField]
        private Transform _crystalTransform;

        [SerializeField]
        private ParticleSystem _takenParticles;

        public void Take()
        {
            _crystalTransform.gameObject.SetActive(false);
            _takenParticles.Play();
        }
    }
}
