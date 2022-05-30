using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Moves obstacles, background and changes offset of Ground's material at the
/// same speed. Destroys obstacles that went beyond leftBound.
/// Also increases speed over time using coroutine
/// </summary>
public class MoveLeft : MonoBehaviour
{
    public static float speed = 5.0f;
    public MeshRenderer groundMeshRenderer;
    private float speedLimit = 30.0f;
    private float timeDelay = 3.0f;
    private float speedBoost = 0.9f;
    private float leftBound = -10.0f;

    void Awake()
    {
        if (gameObject.CompareTag("Ground"))
        {
            groundMeshRenderer = GetComponent<MeshRenderer>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!GameManager.gameOver)
        {
            if (!gameObject.CompareTag("Ground"))
            {
                transform.Translate(Vector3.left * Time.deltaTime * speed);
            }
            else
            {
                ChangeTextureOffset();
            }
        }
        if (gameObject.transform.position.x < leftBound && gameObject.CompareTag("Obstacle"))
        {
            Destroy(gameObject);
        }
    }

    private void ChangeTextureOffset()
    {
        var changePace = Time.deltaTime * speed / 10.0f;
        var newMainOffsetX = groundMeshRenderer.material.mainTextureOffset.x - changePace;
        var newMainOffset = new Vector2(newMainOffsetX, 0f);
        var newNormalMapOffsetX = groundMeshRenderer.material.GetTextureOffset("_BumpMap").x - changePace;
        var newNormalMapOffset = new Vector2(newNormalMapOffsetX, 0f);

        groundMeshRenderer.material.mainTextureOffset = newMainOffset;
        groundMeshRenderer.material.SetTextureOffset("_BumpMap", newNormalMapOffset);
    }

    public IEnumerator SpeedBooster()
    {
        while (speed < speedLimit)
        {
            yield return new WaitForSeconds(timeDelay);
            speed += speedBoost;
        }
    }
}
