using UnityEngine;
using TMPro;
using Assets.Scripts.Character;
using JetBrains.Annotations;

namespace Assets.Scripts.UI
{
    [UsedImplicitly]
    public class PickGun : MonoBehaviour
    {
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value
        [SerializeField] private TextMeshProUGUI _text;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value

        private Player _player;

        [UsedImplicitly]
        private void Start()
        {
            _player = GameObject.Find(nameof(Player)).GetComponent<Player>();
            _player.OnGunInPickingRange += HandleInteract;
        }

        private void HandleInteract(GameObject weapon)
        {
            _text.text = $"Press 'E' to Pickup {weapon.name}";
        }
    }
}
