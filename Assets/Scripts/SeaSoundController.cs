using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class SeaSoundController : MonoBehaviour
{
    AudioPlayer audioPlayer;
    public WaterSurface targetSurface = null;
    public Transform player;
    public float heightOffset = 0f;

    WaterSearchParameters searchParameters = new WaterSearchParameters();
    WaterSearchResult searchResult = new WaterSearchResult();

    private bool isUnderwater = false;

    void Start()
    {
        audioPlayer = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioPlayer>();
        audioPlayer.PlaySea();
        isUnderwater = false;
    }

    void Update()
    {
        if (targetSurface != null && player != null)
        {
            searchParameters.startPositionWS = searchResult.candidateLocationWS;
            searchParameters.targetPositionWS = player.position;
            searchParameters.error = 0.01f;
            searchParameters.maxIterations = 8;

            if (targetSurface.ProjectPointOnWaterSurface(searchParameters, out searchResult))
            {
                float playerHeight = player.position.y + heightOffset;
                float waterHeight = searchResult.projectedPositionWS.y;

                if (playerHeight > waterHeight && isUnderwater)
                {
                    audioPlayer.PlaySea();
                    isUnderwater = false;
                }
                else if (playerHeight <= waterHeight && !isUnderwater)
                {
                    audioPlayer.PlayUnderwater();
                    isUnderwater = true;
                }
            }
        }
    }
}
