using Platformer.Gameplay;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Platformer.Core.Simulation;

namespace Platformer.Mechanics {
    /// <summary>
    /// Marks a trigger as a VictoryZone, usually used to end the current game level.
    /// </summary>
    public class VictoryZone : MonoBehaviour {
        public int Next =-1;
        void OnTriggerEnter2D(Collider2D collider) {
            var p = collider.gameObject.GetComponent<PlayerController>();
            if (p != null) {
                var ev = Schedule<PlayerEnteredVictoryZone>();
                ev.victoryZone = this;
            }
            if (Next != -1) {
                SceneManager.LoadScene(Next);
            }
        }
    }
}