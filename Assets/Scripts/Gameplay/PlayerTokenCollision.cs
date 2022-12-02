using Platformer.Core;
using Platformer.Mechanics;
using Platformer.Model;
using UnityEngine;

namespace Platformer.Gameplay
{
    /// <summary>
    /// Fired when a player collides with a token.
    /// </summary>
    /// <typeparam name="PlayerCollision"></typeparam>
    public class PlayerTokenCollision : Simulation.Event<PlayerTokenCollision>
    {
        public PlayerController player;
        public TokenInstance token;
        float lastToken = 0, pitch = 1, buffer = 2f;

        PlatformerModel model = Simulation.GetModel<PlatformerModel>();

        public override void Execute()
        {
            AudioSource tmp = PlayClipAt(token.tokenCollectAudio, token.transform.position);
            if(Mathf.Abs(lastToken - Time.realtimeSinceStartup) <= buffer){
                if(pitch < 2) pitch += .05f;
                tmp.pitch =pitch;
            }
            else{
                pitch = 1;
            }
            lastToken = Time.realtimeSinceStartup;
        }

        AudioSource PlayClipAt(AudioClip clip, Vector3 pos){
            GameObject tempGO = new GameObject("TempAudio"); // create the temp object
            tempGO.transform.position = pos; // set its position
            AudioSource aSource = tempGO.AddComponent<AudioSource>(); // add an audio source
            aSource.clip = clip; // define the clip
            // set other aSource properties here, if desired
            aSource.Play(); // start the sound
            UnityEngine.Object.Destroy(tempGO, clip.length); // destroy object after clip duration
            return aSource; // return the AudioSource reference
        }
    }
}