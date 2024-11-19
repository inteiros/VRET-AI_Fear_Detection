using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class Swim : MonoBehaviour
{
    [SerializeField] private CharacterController characterController;
    [SerializeField] private float swimSpeed = 30f;
    [SerializeField] private float verticalSwimSpeed = 20f;
    [SerializeField] private WaterSurface targetSurface;

    private WaterSearchParameters searchParameters = new WaterSearchParameters();
    private WaterSearchResult searchResult = new WaterSearchResult();
    private float waterSurfaceHeight;
    private float maxDiveDepth = 10f;
    private float maxAboveWaterHeight = 0.5f;

    void Update()
    {
        UpdateWaterHeight();
        HandleMovement();
        ApplyBuoyancy();
    }

    private void UpdateWaterHeight()
    {
        if (targetSurface != null)
        {
            searchParameters.startPositionWS = searchResult.candidateLocationWS;
            searchParameters.targetPositionWS = transform.position;
            searchParameters.error = 0.01f;
            searchParameters.maxIterations = 8;

            if (targetSurface.ProjectPointOnWaterSurface(searchParameters, out searchResult))
            {
                waterSurfaceHeight = searchResult.projectedPositionWS.y;
            }
        }
    }

    private void HandleMovement()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 movement = (transform.right * horizontal + transform.forward * vertical) * swimSpeed;

        if (Input.GetKey(KeyCode.LeftControl) && transform.position.y > waterSurfaceHeight - maxDiveDepth)
        {
            movement.y = -verticalSwimSpeed;
        }
        else if (Input.GetKey(KeyCode.Space) && transform.position.y < waterSurfaceHeight + maxAboveWaterHeight)
        {
            movement.y = verticalSwimSpeed;
        }
        else
        {
            movement.y = 0;
        }

        characterController.Move(movement * Time.deltaTime);
    }

    private void ApplyBuoyancy()
    {
        if (transform.position.y < waterSurfaceHeight && !Input.GetKey(KeyCode.LeftControl))
        {
            Vector3 buoyancyMovement = new Vector3(0, waterSurfaceHeight - transform.position.y, 0);
            characterController.Move(buoyancyMovement * Time.deltaTime);
        }
        else if (transform.position.y > waterSurfaceHeight + maxAboveWaterHeight)
        {
            Vector3 adjustment = new Vector3(0, (waterSurfaceHeight + maxAboveWaterHeight) - transform.position.y, 0);
            characterController.Move(adjustment * Time.deltaTime);
        }
    }
}
