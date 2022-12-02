using Platformer.Gameplay;
using UnityEngine;
using UnityEngine.SceneManagement;

using System.Collections;
using static Platformer.Core.Simulation;

namespace Platformer.Mechanics {
    /// <summary>
    /// Marks a trigger as a VictoryZone, usually used to end the current game level.
    /// </summary>
    public class VictoryZone : MonoBehaviour {
        public GameObject anim;
        public string Next;
        public GameObject pc;
        void OnTriggerEnter2D(Collider2D collider) {
            var p = collider.gameObject.GetComponent<PlayerController>();
            if (p != null) {
                var ev = Schedule<PlayerEnteredVictoryZone>();
                ev.victoryZone = this;
            }
            if (Next != null) {
                if(pc) pc.active = false;
                if(anim) anim.active = true;
                
                StartCoroutine(LoadThaLevel());
                //}
            }
        }

        IEnumerator LoadThaLevel() {
            yield return new WaitForSeconds(1);
            SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());
            SceneManager.LoadSceneAsync(Next);
        }
    }
}