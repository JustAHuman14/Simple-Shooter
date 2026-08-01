using UnityEngine;
using UnityEngine.UI;
using Assets.Scripts.Character;
using JetBrains.Annotations;

namespace Assets.Scripts.UI
{
    [UsedImplicitly]
    public class PlayerHealthBar : MonoBehaviour
    {
        // Serialized Fields
#pragma warning disable CS0649
        [SerializeField] private Player _player;
        [SerializeField] private Image _healthBarImage;
#pragma warning restore CS0649

        [UsedImplicitly]
        private void Start()
        {
            _player.OnDamage += UpdateEnemyHealth;
            UpdateEnemyHealth();
        }

        private void UpdateEnemyHealth()
        {
            _healthBarImage.fillAmount = _player.CurrentHealth / _player.MaxHealth;
        }

        [UsedImplicitly]
        private void OnDestroy()
        {
            if (_player != null)
                _player.OnDamage -= UpdateEnemyHealth;
        }
    }
}
