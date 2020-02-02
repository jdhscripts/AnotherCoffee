﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class music_system : MonoBehaviour
{
    public AudioMixer MasterMixer;
    public AudioClip[] stingers;

    AudioSource[] audiosources;
    string[] snapshots = {"Dreamer", "Realist"};

    List<float[]> parameters = new List<float[]>();

    float[] phraseLen = {6.99F, 4.66F}; //this determines how long the tracks run for each phrase
    float phraseNum = 0;
    int prevSource = 1;
    // 0 = dreamer
    // 1 = realist
    float prevTime = 0;
    int prevClip = 0;

    bool interrupt = false;
    /*
    Each parameter is represented by these variables.

    To indicate the current player speaking:
        character
        0 = Realist (player character)
        1 = Dreamer

    To indicate the current topic:
        topic
        0 = introduction
        1 = careers
        2 = childhood memories
        3 = relationships

    For the conversation's tension:
        tension
        0 = low tension
        1 = high tension

    For the conversation's awkwardness:
        awkwardness
        0 = low awkwardness
        1 = high awkwardness

    For the resolution between the two characters:
        resolution
        0 = low resolution
        1 = high resolution

    To indicate if there was an interruption:
        interrupt
        false = no interruption (duh)
        true = interruption
    */

    public void updateMusic(int character, int topic,
                            float tension, float awkwardness,
                            float resolution, bool newInterrupt){

        parameters.Add(new float[] {
                //safety first
                Mathf.Clamp(character, 0, 3),
                Mathf.Clamp(topic, 0, 1),
                Mathf.Clamp(tension, 0, 100),
                Mathf.Clamp(awkwardness, 0, 100),
                Mathf.Clamp(resolution, 0, 100),
                (float)(newInterrupt ? 1 : 0)
            });

        interrupt = newInterrupt;
    }

    int fetchCue(float[] newParams) {
        int index = 0;
        index += (int)newParams[0]*16;
        float biggest = 0;
        int tempIndex = 0;
        for (int i = 0; i < 3; i++){
            if (newParams[i + 2] > biggest){
                biggest = newParams[i + 2];
                tempIndex = i;
            }
        }

        if (newParams[tempIndex + 2] >= 1){
            index += (int)(tempIndex + 1)*4;
        }

        index += (int)newParams[1];
        print("playing index " + index.ToString());
        return index;
    }

    void Start() {
        audiosources = GetComponents<AudioSource>();
        // updateMusic(0, 0, 0, 0, 0, false);
        prevTime = 0;
    }

    void Update() {
        if (parameters.Count > 0){
            if (interrupt){
                int currentSource = (int)parameters[parameters.Count - 1][0];
                int tempIndex = fetchCue(parameters[parameters.Count - 1]);
                audiosources[currentSource].clip = stingers[tempIndex];
                audiosources[currentSource].PlayDelayed(3);
                audiosources[prevSource].Stop();
                prevSource = currentSource;
                parameters.Clear();
                interrupt = false;
                prevTime = Time.time + 3;
                MasterMixer.FindSnapshot(snapshots[currentSource]).TransitionTo(0.5F);
            }else{
                //if the previous cue is at the end of its phrase
                if (Time.time - prevTime >= phraseLen[prevSource] - 0.5){
                    int currentSource = (int)parameters[0][0];
                    int currentClip = fetchCue(parameters[0]);
                    if (currentSource == prevSource && currentClip == prevClip){
                        prevTime = Time.time;
                        parameters.RemoveAt(0);
                    }else{
                        audiosources[currentSource].clip = stingers[fetchCue(parameters[0])];
                        audiosources[currentSource].time = Mathf.Clamp((phraseLen[currentSource]*phraseNum) - 0.5F, 0, 100);
                        phraseNum = (phraseNum + 1) % 4;
                        audiosources[currentSource].Play();
                        prevSource = currentSource;
                        parameters.RemoveAt(0);
                        prevTime = Time.time;
                        MasterMixer.FindSnapshot(snapshots[currentSource]).TransitionTo(0.5F);
                    }
                }
            }
        }else{
            if (Time.time - prevTime >= phraseLen[prevSource] - 0.5){
                MasterMixer.FindSnapshot("noMusic").TransitionTo(0.5F);
            }
        }
    }

}