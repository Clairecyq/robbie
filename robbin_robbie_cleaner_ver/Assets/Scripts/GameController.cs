﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour {
    public static GameController instance;
    public static bool isPaused;

    private GameObject robbie;
    public GameObject finishLevelText;
    private GameObject finishLevelText2;
    private GameObject gameOverText;
    private GameObject tutorialText1;
    private GameObject tutorialText2;
    private GameObject trashcan;
    private GameObject boot;
    private GameObject scoreText;
    private GameObject collectText;
    private GameObject levelNavButtonBar;
    public GameObject leftRightTutorial;
    public GameObject upTutorial;

    private GameObject targetTimeText;
    private GameObject energyBarOne;

    public RectTransform.Axis anchor;

    public AudioClip robbieVictorySound1;
    public AudioClip robbieVictorySound2;
    public AudioClip robbieVictorySound3;
    public AudioClip robbieGameOverSound1;

    RobbieMovement robbieMovement;

    public bool gameOver = false;
    public bool levelFinish = false;
    
    private int snapshot = 0;
    private int robbieScore = 0;

    public int levelId;
    public int totalNumLevels = 26; // this should not
    public int targetTime = 30; 
    private float timer = 0;
    private int minutesElapsed;

    public int totalPossibleTimeScore = 1000; //this is the amount of time bonus available 
    public int oneStarScore;
    public int twoStarScore;
    public int threeStarScore;
    private int starsObtained;

    public GameObject levelCompleteBadge;
    public GameObject candyBadge;

    public GameObject timeBadge;

    public string levelDescription;
    private GameObject CandyText;
    private GameObject CandyCaneUI;
    private int canesCollected = 0;

    void Awake ()
    {

        GameObject[] objects = (GameObject[]) Resources.FindObjectsOfTypeAll( typeof(GameObject) );

        foreach (GameObject o in objects )
        {
            
            switch (o.name)
            {
                // case "VictoryText":
                //     finishLevelText = o;
                //     break;
                case "ContinueText":
                    finishLevelText2 = o;
                    break;
                case "GameOverImage":
                    gameOverText = o;
                    break;
                case "scoretext":
                    scoreText = o;
                    break;
                case "CollectionText":
                    collectText = o;
                    break;
                case "TargetTimeText":
                    targetTimeText = o;
                    break;
                case "EnergyBar":
                    energyBarOne = o;
                    break;
                // case "LevelCompleteBadge":
                //      levelCompleteBadge = o;
                //      break;
                // case "AllCandyBadge":
                //     candyBadge = o;
                //      break;
                // case "FastTimeBadge":
                //      timeBadge = o;
                //      break;
            }
        }
        robbie      = GameObject.FindWithTag("Player");
        CandyText   = GameObject.Find("CandyText");
        CandyCaneUI = GameObject.Find("CandyCaneUI");


        if (instance == null) {
            instance = this;
        } else if (instance != this) {
            Destroy(gameObject);
        }
        anchor = UnityEngine.RectTransform.Axis.Horizontal;
        if (LoggingManager.instance != null) LoggingManager.instance.RecordLevelStart(levelId, levelDescription);
        robbieMovement = robbie.GetComponent<RobbieMovement>();

        GameObject[] fires = GameObject.FindGameObjectsWithTag("Collectable");
        if (LoggingManager.instance != null && LoggingManager.instance.playerABValue == 2 && false) {
            int ctr = 0;
            for (int idx = 0; idx < fires.Length; idx++) {
                if (idx % 2 == 0 && fires[idx].name.Contains("fire")) {
                    GameObject fire = fires[idx];
                    fire.SetActive(false);
                    ctr += 1;
                }
            }
            if (CandyText != null) CandyText.GetComponent<Text>().text = "0 / " + ctr.ToString();
        } else {
            if (CandyText != null) CandyText.GetComponent<Text>().text = "0 / " + fires.Length.ToString();
        }

        /*
         * TODO: create testing instance 
        if (GameStateManager.instance == null || !GameStateManager.instance.isInitialized)
        {
            Debug.Log("I am intializing");
            totalNumLevels = 12;
            GameStateManager.instance.Initialize(totalNumLevels);
            GameStateManager.instance.isInitialized = true;
        }
        */
	}

    public void ChangeToScene(string targetScene)
    {
        SceneManager.LoadScene(targetScene);
    }

    public void ChangeToHome()
    {
        SceneManager.LoadScene("LevelSelector");
        AudioListener.pause = true;
    }

    public void MuteMusic()
    {
        AudioListener.pause = !AudioListener.pause;
    }

    // Update is called once per frame
    void Update () {
        // if (Input.anyKeyDown) {
        //     if (upTutorial != null)
        //     {
        //         upTutorial.SetActive(false);
        //         leftRightTutorial.SetActive(false);
        //     }
        // }
        if (Input.GetKeyDown("p"))
        {
            PauseOrResume();
        }

        if (Input.GetKeyDown("r")) {
            Restart();
        }
        CharacterController2D char_component = robbie.GetComponent<CharacterController2D>();
        energyBarOne.GetComponent<Image>().fillAmount = Mathf.Min(1.0f, (float) char_component.currentHidingPower / char_component.getMaxHidingEnergy());

        //scoreText.GetComponent<Text>().text = "Score: " + robbieScore.ToString();

        //TODO: this needs to be modified
        if (levelFinish && !gameOver) {            

            robbieMovement.canMove = false;

            if (Input.GetKeyDown("c"))
            {
                if (LoggingManager.instance != null) {
                    LoggingManager.instance.RecordLevelEnd();
                }

                int currentLevelBuildIndex = SceneManager.GetActiveScene().buildIndex;
                int levelStartingIndex = SceneManager.GetSceneByName("T1").buildIndex;

                if (currentLevelBuildIndex < GameStateManager.instance.totalNumLevels + 2)
                {
                    Debug.Log(currentLevelBuildIndex + 1);
                    if (levelCompleteBadge != null) {
                        // levelCompleteBadge.SetActive(false);
                        // timeBadge.GetComponent<Image>().color = Color.white;
                        // timeBadge.SetActive(false);
                        // candyBadge.GetComponent<Image>().color = Color.white;
                        // candyBadge.SetActive(false);
                    }

                    GameStateManager.instance.levelsUnlocked[currentLevelBuildIndex - 1] = true; //levels are offset by 1 because of load scene and level selector
                    SceneManager.LoadScene(currentLevelBuildIndex + 1);
                }
            }
        }
	}

    void FixedUpdate() {
        snapshot += 1;
        timer += Time.deltaTime;

        minutesElapsed = (int) timer/60;
        string extraZero = "";
        if ((int) timer % 60 < 10) {
            extraZero = "0";
        }

        scoreText.GetComponent<Text>().text = minutesElapsed.ToString() + " : " + extraZero + (((int) timer) % 60).ToString();

        if (snapshot % 300 == 0) {
            packageInfo(18, "Snapshot - level:");
            snapshot = 0;
        }
    }

    public void PauseOrResume() {
        if (isPaused) {
            Resume();
        } else {
            packageInfo(19, "Pause");
            Pause();
        }
    }

    void Resume() {
        Time.timeScale = 1f;
        isPaused = false;
    }

    void Pause() {
        Time.timeScale = 0f;
        isPaused = true;
    }

    public void Restart() {
        if (true) {
            packageInfo(17, "Level Reset");
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            levelFinish = false;
            gameOver = false;
            isPaused = false;
            Vector3 energyScale = energyBarOne.transform.localScale;
            energyScale.x = 1.0f;
            energyBarOne.transform.localScale = energyScale;
            if (levelCompleteBadge != null) {
                levelCompleteBadge.SetActive(false);
                timeBadge.GetComponent<Image>().color = Color.white;
                timeBadge.SetActive(false);
                candyBadge.GetComponent<Image>().color = Color.white;
                candyBadge.SetActive(false);
            }
            robbie.GetComponent<RobbieMovement>().health = robbie.GetComponent<RobbieMovement>().maxHealth;
        }
    }

    public void RobbieDied() {
        if (!levelFinish)
        {
            robbie.gameObject.GetComponent<Animator>().SetBool("died", true);
            robbie.gameObject.GetComponent<RobbieMovement>().killHeart();
            robbieMovement.canMove = false;
            Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Enemy"), true);
            packageInfo(11, "Robbie Died - Generic");
            gameOverText.SetActive(true);
            gameOver = true;
            if (trashcan != null) trashcan.SetActive(false);
            if (boot != null) boot.SetActive(false);
            SoundManager.instance.PlaySingle(robbieGameOverSound1);
            levelFinish = true;

        }
    }

    public void displayVictoryStats() {
        // step 1: display all fires collected
        Camera cam = GameObject.FindWithTag("MainCamera").GetComponent<Camera>();
        GameObject[] fires = GameObject.FindGameObjectsWithTag("Collectable");

        int ctr = 0;
        float xpos = robbie.GetComponent<CharacterController2D>().transform.position.x;
        float ypos = robbie.GetComponent<CharacterController2D>().transform.position.y;
        Vector2 fpos = new Vector2(xpos-fires.Length+1, ypos+2);

        for (int idx = 0; idx < fires.Length; idx++) {
            GameObject fire = fires[idx];
            if (fire.activeSelf && fire.name.Contains("fire")) {
                if (fire.GetComponent<SpriteRenderer>().enabled) {
                    fire.GetComponent<SpriteRenderer>().color = Color.grey;
                }
                else {
                    ctr += 1;
                }
                fire.GetComponent<SpriteRenderer>().enabled = true;
                Vector2 fviewpos = fpos;
                fviewpos.x += (idx);
                fire.transform.position = fviewpos;
            }
        }
        // levelCompleteBadge = GameObject.Find("LevelCompleteBadge");
        // candyBadge = GameObject.Find("AllCandyBadge");
        // timeBadge = GameObject.Find("FastTimeBadge");

        // if (ctr == fires.Length) {
        //     collectText.GetComponent<Text>().text = "All Fire Collected!";
        // }
        // else {
        //     collectText.GetComponent<Text>().text = "Fires Collected: " + ctr.ToString();
        // }
        //Vector2 ctextLoc = fpos + new Vector2(-20,5);
        //collectText.transform.position = ctextLoc;
        //collectText.SetActive(true);

        // step 2: display time

        string finishTime = scoreText.GetComponent<Text>().text;
        string finText = finishTime + "!  ";
        int lossTime = targetTime - (int) timer;
        int winTime = (int) timer - targetTime;
        //Debug.Log("Timer: " + timer.ToString());
        if ((int) timer > targetTime) {
            timeBadge.GetComponent<Image>().color = Color.grey;
        }
        //targetTimeText.transform.position = fpos + new Vector2(-20,8);
        //targetTimeText.SetActive(true);
        scoreText.SetActive(false);
        
        Debug.Log("Level Complete Badgee BEFORE: " + levelCompleteBadge.activeSelf);
        levelCompleteBadge.SetActive(true);
        Debug.Log("Level Complete Badgee AFTER: " + levelCompleteBadge.activeSelf);
        if (fires.Length > canesCollected) {candyBadge.GetComponent<Image>().color = Color.grey;}
        timeBadge.SetActive(true);
        candyBadge.SetActive(true);
    }

    public void PickedDonut() {
        if (!gameOver) {
            packageInfo(10, "Robbie Victory");
            finishLevelText.SetActive(true);
            //finishLevelText2.SetActive(true);
            displayVictoryStats();
            if (tutorialText1!=null) tutorialText1.SetActive(false);
            if (tutorialText2!=null) tutorialText2.SetActive(false);
            SoundManager.instance.RandomizeSfx(robbieVictorySound1, robbieVictorySound2, robbieVictorySound3);
            levelFinish = true;
            Debug.Log("Level Complete Badgee DEUS: " + levelCompleteBadge.activeSelf);
        }
    }

    public void obtainCoin() {
        robbieScore += 100;
        canesCollected += 1;
        GameObject[] fires = GameObject.FindGameObjectsWithTag("Collectable");
        if (canesCollected == fires.Length) {
            if (CandyCaneUI != null) CandyCaneUI.GetComponent<Image>().color = Random.ColorHSV();
        }
        if (CandyText != null) CandyText.GetComponent<Text>().text = canesCollected.ToString() + " / " + fires.Length.ToString();
        //packageInfo(20, "Collect Fire");
    }

    public int getNumStarsObtained() {
        if (robbieScore > threeStarScore) {
            return 3;
        }

        else if (robbieScore > twoStarScore) {
            return 2;
        }

        else if (robbieScore > oneStarScore) {
            return 1;
        }

        return 0;
    }

    public void packageInfo(int actionID, string action) {
        int level = levelId;
        float stamina = (float)robbie.GetComponent<CharacterController2D>().currentHidingPower / robbie.GetComponent<CharacterController2D>().getMaxHidingEnergy();
        float xpos = robbie.GetComponent<CharacterController2D>().transform.position.x;
        float ypos = robbie.GetComponent<CharacterController2D>().transform.position.y;
        int totalTime = (int) timer;
        if (LoggingManager.instance != null ) LoggingManager.instance.RecordEvent(
            actionID, 
            action + " " + level.ToString() + "  stamina: " + stamina.ToString() + "  Xpos: " + xpos.ToString() + "  Ypos: " + ypos.ToString() + "  playtime: " + totalTime.ToString() + " fires: " + robbieScore.ToString()
        );
    }
}
