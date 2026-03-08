using UnityEngine;
using Assets.Scripts.Interfaces;

namespace Assets.Scripts.Character
{
    public class NPC : MonoBehaviour, IInteractable
    {
        public void Interact()
        {
            print($"Interact {gameObject.name}!");
        }
    }
}
