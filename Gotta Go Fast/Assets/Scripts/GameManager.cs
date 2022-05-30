using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.IO;
using UnityEngine.UI;

/// <summary>
/// Handles game flow, UI and save/load highscore to a json
/// </summary>
public class GameManager : MonoBehaviour
{
    //Gameplay fields
    public static bool gameStarted = false;
    public static bool gameOver = false;
    public static bool cityLevel = true;
    public static bool firstGame = true;
    private static bool gravityChanged = false;
    [SerializeField] private GameObject spawnManager;
    [SerializeField] private GameObject ground;
    [SerializeField] private GameObject playerCountry;
    [SerializeField] private AudioSource music;
    [SerializeField] private PlayerController playerController;
    private MoveLeft moveLeft;
    private GameObject playerCity;
    private IEnumerator speedBooster;
    private IEnumerator updateScore;
    private int score = 0;
    private int maxScore = 1000000;
    private int bestScore = 0;
    private float scoreUpdateDelay = 0.2f;
    private float controlsHintDelay = 7.0f;
    private float gravityModifier = 2.5f;

    //Level design fields
    [SerializeField] private Sprite[] backgroundSprites;
    [SerializeField] private Material[] groundMaterials;
    [SerializeField] private SpriteRenderer backgroundSpriteRenderer;
    private Vector2 textureScale = new Vector2(10.0f, 1.0f);

    //UI fields
    [SerializeField] private GameObject startMenuScreen;
    [SerializeField] private GameObject gameOverScreen;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI finalScoreText;
    [SerializeField] private TextMeshProUGUI finalBestScoreText;
    [SerializeField] private TextMeshProUGUI controlsHintText;
    [SerializeField] private Button countryButton;
    [SerializeField] private Button cityButton;

    void Awake()
    {
        playerCity = playerController.gameObject;
        moveLeft = ground.GetComponent<MoveLeft>();
        if (!gravityChanged)
        {
            Physics.gravity *= gravityModifier;
            gravityChanged = true;
        }
    }

    void Update()
    {
        //UI
        if (startMenuScreen.activeInHierarchy)
        {
            if (cityLevel)
            {
                countryButton.image.color = new Color32(0x9F, 0x9F, 0x9F, 0xFF);
                cityButton.image.color = new Color32(0xFF, 0xFF, 0xFF, 0xFF);
            }
            else
            {
                cityButton.image.color = new Color32(0x9F, 0x9F, 0x9F, 0xFF);
                countryButton.image.color = new Color32(0xFF, 0xFF, 0xFF, 0xFF);
            }
        }
    }

    public void OnGameStarted()
    {
        gameStarted = true;
        playerController.playerAnimator.SetFloat("Speed_f", 0.55f);
        playerController.dirtParticle.Play();
        MoveLeft.speed = 12f;
        speedBooster = moveLeft.SpeedBooster();
        StartCoroutine(speedBooster);
        music.Play();
        spawnManager.SetActive(true);
        //UI
        if (firstGame)
        {
            StartCoroutine(ShowControls());
        }
        updateScore = UpdateScore();
        StartCoroutine(updateScore);
        scoreText.gameObject.SetActive(true);
        startMenuScreen.SetActive(false);
    }

    public void OnGameOver()
    {
        gameOver = true;
        gameStarted = false;
        music.Stop();
        StopCoroutine(speedBooster);
        playerController.playerAnimator.SetBool("Death_b", true);
        playerController.playerAnimator.SetInteger("DeathType_int", 1);
        // If player jumps while crouching under obstacle there's a chance
        // that he won't collide with the Ground on death
        // (cause he might clip through the Ground),
        // that's why onGround and flying is set manually
        playerController.onGround = true;
        playerController.flying = false;
        playerController.StopCrouhing();
        playerController.dirtParticle.Stop();
        playerController.exploisionParticle.Play();
        playerController.playerAudio.PlayOneShot(playerController.crashSound, 1f);
        spawnManager.SetActive(false);
        //UI
        controlsHintText.gameObject.SetActive(false);
        StopCoroutine(updateScore);
        scoreText.gameObject.SetActive(false);
        finalScoreText.text = $"Your final score: {score}";
        LoadBestScore();
        if (score > bestScore)
        {
            SaveBestScore();
            finalBestScoreText.text = $"Your best score: {score}";
        }
        else
        {
            finalBestScoreText.text = $"Your best score: {bestScore}";
        }
        gameOverScreen.SetActive(true);
    }

    public void OnNewGame()
    {
        var obstacles = GameObject.FindGameObjectsWithTag("Obstacle");

        foreach (GameObject obstacle in obstacles)
        {
            Destroy(obstacle);
        }
        playerController.playerAnimator.Rebind();
        playerController.playerAnimator.Update(0f);
        gameOver = false;
        score = 0;
        //UI
        scoreText.text = $"Score: {score}";
        gameOverScreen.SetActive(false);
        OnGameStarted();
    }

    public void OnCountryLevel()
    {
        if (cityLevel)
        {
            cityLevel = false;
            moveLeft.groundMeshRenderer.material = groundMaterials[1];
            moveLeft.groundMeshRenderer.material.mainTextureScale = textureScale;
            moveLeft.groundMeshRenderer.material.SetTextureScale("_BumpMap", textureScale);
            playerCity.SetActive(false);
            playerCountry.SetActive(true);
            playerController = playerCountry.GetComponent<PlayerController>();
            backgroundSpriteRenderer.sprite = backgroundSprites[1];
        }
    }

    public void OnCityLevel()
    {
        if (!cityLevel)
        {
            cityLevel = true;
            moveLeft.groundMeshRenderer.material = groundMaterials[0];
            moveLeft.groundMeshRenderer.material.mainTextureScale = textureScale;
            moveLeft.groundMeshRenderer.material.SetTextureScale("_BumpMap", textureScale);
            playerCountry.SetActive(false);
            playerCity.SetActive(true);
            playerController = playerCity.GetComponent<PlayerController>();
            backgroundSpriteRenderer.sprite = backgroundSprites[0];
        }
    }

    public void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        gameOver = false;
        cityLevel = true;
        MoveLeft.speed = 5.0f;
    }

    private IEnumerator UpdateScore()
    {
        while (score < maxScore)
        {
            yield return new WaitForSeconds(scoreUpdateDelay);
            score += (int)MoveLeft.speed / 10;
            scoreText.text = $"Score: {score}";
        }
        scoreText.color = new Color32(0x74, 0xEE, 0x76, 0xFF);
    }

    private IEnumerator ShowControls()
    {
        firstGame = false;
        controlsHintText.gameObject.SetActive(true);
        yield return new WaitForSeconds(controlsHintDelay);
        controlsHintText.gameObject.SetActive(false);
    }

    [System.Serializable]
    private class SaveData
    {
        public int bestScore;
    }

    public void SaveBestScore()
    {
        SaveData data = new SaveData();
        data.bestScore = score;

        string json = JsonUtility.ToJson(data);

        File.WriteAllText(Application.persistentDataPath + "/ggf_savefile.json", json);
    }

    public void LoadBestScore()
    {
        string path = Application.persistentDataPath + "/ggf_savefile.json";
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            SaveData data = JsonUtility.FromJson<SaveData>(json);

            bestScore = data.bestScore;
        }
    }
}
