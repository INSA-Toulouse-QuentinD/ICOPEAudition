using UnityEngine;

namespace Managers{
    public class PartyManager : MonoBehaviour{
        public static PartyManager Instance;
        public AudioSource audioSource;
        public ParticleSystem[] particles;

        void Awake(){
            Instance = this;
        }
        
        public void Play(){
            foreach (ParticleSystem particle in particles) {
                particle.Play();
            }
            audioSource.Play();
        }
    }
}