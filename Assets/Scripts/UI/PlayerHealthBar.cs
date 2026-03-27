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

        // Non-Serialized Fields
        private GameObject[] _playerPOVArray;

        private void Awake()
        {
            _playerPOVArray = GameObject.FindGameObjectsWithTag("PlayerPOV");
        }

        private void Start()
        {
            _player.OnDamage += UpdateEnemyHealth;
            UpdateEnemyHealth();
        }

        private void UpdateEnemyHealth()
        {
            _healthBarImage.fillAmount = _player.currentHealth / _player.maxHealth;
        }

        private void LateUpdate()
        {
            foreach (GameObject playerPOV in _playerPOVArray)
            {
                if (playerPOV.transform.parent != _player.transform && _player != null)
                    transform.LookAt(playerPOV.transform);
            }
        }

        private void OnDestroy()
        {
            if (_player != null)
                _player.OnDamage -= UpdateEnemyHealth;
        }
    }
}
