using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;

using Cinemachine;

[RequireComponent(typeof(ParticleSystem))]
public class EmitParticlesOnLand : MonoBehaviour {

    public bool emitOnLand = true;
    public bool emitOnEnemyDeath = true;

//#if UNITY_TEMPLATE_PLATFORMER

    public CinemachineVirtualCamera vcam;
    public float _time = .5f;
    public ParticleSystem p;

    void Awake() {
        p = GetComponent<ParticleSystem>();

        if (emitOnLand) {
            Platformer.Gameplay.PlayerLanded.OnExecute += PlayerLanded_OnExecute;
        }

        if (emitOnEnemyDeath) {
            Platformer.Gameplay.EnemyDeath.OnExecute += EnemyDeath_OnExecute;
        }
    }
            
    void PlayerLanded_OnExecute(Platformer.Gameplay.PlayerLanded obj) {
        p.Play();
    }

    void EnemyDeath_OnExecute(Platformer.Gameplay.EnemyDeath obj) {
        p.Play();
        StartCoroutine(_ProcessShake());
    }

    void OnDestroy(){
        if (emitOnLand) {
            Platformer.Gameplay.PlayerLanded.OnExecute -= PlayerLanded_OnExecute;
        }

        if (emitOnEnemyDeath) {
            Platformer.Gameplay.EnemyDeath.OnExecute -= EnemyDeath_OnExecute;
        }
    }



    private IEnumerator _ProcessShake(float shakeIntensity = 5f, float shakeTiming = 0.5f) {
        vcam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_AmplitudeGain = 3;
        Time.timeScale = Mathf.Lerp(Time.timeScale, 0.5f, Time.time);
        yield return new WaitForSeconds(_time);
        vcam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_AmplitudeGain = 0;
        Time.timeScale = Mathf.Lerp(Time.timeScale, 1f, Time.time);
    }



//#endif

}
