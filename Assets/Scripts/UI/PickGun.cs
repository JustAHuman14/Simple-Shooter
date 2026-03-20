using UnityEngine;
using TMPro;
using Assets.Scripts.Character;

namespace Assets.Scripts.UI
{
    public class PickGun : MonoBehaviour
    {
        private Player _player;
        [SerializeField] private TextMeshProUGUI _text;

        private void Awake()
        {
            _player = GameObject.Find(nameof(Player)).GetComponent<Player>();
        }

        private void Start()
        {
            _player.OnGunInPickingRange += HandleInteract;
        }

        private void HandleInteract(GameObject weapon)
        {
            _text.text = $"Press 'E' to Pickup {weapon.name}";
        }
    }
}
