using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MuteBGM : MonoBehaviour
{
    // Start is called before the first frame update
    public AudioSource BGM;
    public GameObject cross;
    void Start()
    {
        BGM = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    public void mute()
    {
      if (BGM.mute == false) {
        BGM.mute = true;
        cross.SetActive(true);
        return;
      }
        BGM.mute = false;
        cross.SetActive(false);
      }
      
    }

