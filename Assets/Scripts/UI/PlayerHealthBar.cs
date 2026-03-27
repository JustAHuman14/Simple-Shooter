using UnityEngine;
using UnityEngine.UI;
using Assets.Scripts.Character;

namespace Assets.Scripts.UI
{
    public class PlayerHealthBar : MonoBehaviour
    {
        // Serialized Fields
        [SerializeField] private Player _player;
        [SerializeField] private Image _healthBarImage;

        private void Start()
        {
            _player.OnDamage += UpdateEnemyHealth;
            UpdateEnemyHealth();
        }

        private void UpdateEnemyHealth()
        {
            _healthBarImage.fillAmount = _player.currentHealth / _player.maxHealth;
        }

        private void OnDestroy()
        {
            if (_player != null)
                _player.OnDamage -= UpdateEnemyHealth;
        }
    }
}
