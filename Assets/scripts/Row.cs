using System.Collections;
using UnityEngine;

public class Row : MonoBehaviour
{
    [Header("Sequential Setup")]
    [Tooltip("Set 0 for Left Reel, 1 for Middle Reel, 2 for Right Reel")]
    public int reelIndex = 0;

    private float timeInterval;

    [HideInInspector]
    public bool rowStopped = true;

    [HideInInspector]
    public string stoppedSlot;

    // MEASURED Y-SNAP POSITIONS (Adjust these 7 values to match your scene's sprite Y positions)
    private readonly float[] snapPositions = new float[] { -2.25f, -1.5f, -0.75f, 0f, 0.75f, 1.5f, 2.25f };

    private void Start()
    {
        rowStopped = true;
        GameControl.HandlePulled += StartRotating;
    }

    private void StartRotating()
    {
        stoppedSlot = "";
        StartCoroutine(Rotate());
    }

    private IEnumerator Rotate()
    {
        rowStopped = false;
        timeInterval = 0.025f;

        // Base iterations + sequential stagger based on reelIndex (Reel 0 stops 1st, Reel 1 2nd, Reel 2 3rd)
        int totalSpins = 30 + (reelIndex * 20);

        for (int i = 0; i < totalSpins; i++)
        {
            transform.Translate(new Vector3(0, -0.25f, 0));

            // Wrap around bottom to top
            if (transform.position.y <= -2.25f)
            {
                transform.position = new Vector3(transform.position.x, 2.25f, transform.position.z);
            }

            // Decelerate near the end of the loop
            if (i > totalSpins - 15)
            {
                timeInterval += 0.01f;
            }

            yield return new WaitForSeconds(timeInterval);
        }

        // Smooth movement to nearest snap grid position for perfect parallel row alignment
        float targetY = GetClosestSnapPosition(transform.position.y);
        Vector3 targetPos = new Vector3(transform.position.x, targetY, transform.position.z);

        while (Vector3.Distance(transform.position, targetPos) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, 5f * Time.deltaTime);
            yield return null;
        }

        transform.position = targetPos; // Final exact snap

        DetermineStoppedSymbol();
        rowStopped = true;
    }

    private float GetClosestSnapPosition(float currentY)
    {
        float closest = snapPositions[0];
        float minDistance = Mathf.Abs(currentY - closest);

        foreach (float pos in snapPositions)
        {
            float dist = Mathf.Abs(currentY - pos);
            if (dist < minDistance)
            {
                minDistance = dist;
                closest = pos;
            }
        }
        return closest;
    }

    private void DetermineStoppedSymbol()
    {
        float yPos = transform.position.y;

        // Tolerance check for floating point precision
        if (Mathf.Abs(yPos - (-2.25f)) < 0.1f) stoppedSlot = "Diamond";
        else if (Mathf.Abs(yPos - (-1.5f)) < 0.1f) stoppedSlot = "Crown";
        else if (Mathf.Abs(yPos - (-0.75f)) < 0.1f) stoppedSlot = "Melon";
        else if (Mathf.Abs(yPos - 0f) < 0.1f) stoppedSlot = "Bar";
        else if (Mathf.Abs(yPos - 0.75f) < 0.1f) stoppedSlot = "Seven";
        else if (Mathf.Abs(yPos - 1.5f) < 0.1f) stoppedSlot = "Cherry";
        else if (Mathf.Abs(yPos - 2.25f) < 0.1f) stoppedSlot = "Lemon";
    }

    private void OnDestroy()
    {
        GameControl.HandlePulled -= StartRotating;
    }
}