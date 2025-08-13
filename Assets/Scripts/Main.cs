using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using System.Linq;

public class Main : MonoBehaviour
{
	#region Property
	public VideoPlayer videoPlayer;
	public AudioSource audioSource;

	public AudioClip[] audioClip;
	public VideoClip[] videoClip;

	private int currentIndex = 0;

	public string[] listSong;
	public Text title;
	public Text nextTitle;
	public Text prevTitle;

	public RawImage[] button;
	public Texture[] buttonTexture;

	private bool isMainMenu = true;

	private int score = 0;

	public Text escapeText;
	private List<List<int>> dataGameList;
	public List<string> dataCombination;

	private int currDataGame = 0;
	private int[] combination = new int[7];
	private int currCombination = 0;

	private bool timeCombination = false;
	private int incScore = 0;
	private int firstScore = 0;

	public GameObject NewHighScore;

	public GameObject PauseObject;

	public Text LatestScore;

	private List<int> listScore;

	public Slider slider;

	#endregion

	#region Main
	void Start ()
	{
		ForceResolution ();
		ActivateMedia (currentIndex);

		// GenerateTimeStamps
		if (dataCombination.Count != listSong.Length) {
			Debug.Log ("Generate timestamp");
			foreach (var audio in audioClip) {
				
				int current = 0;
				string combination = "";
				while (true) {
					int step = Random.Range (4, 8);
					if (current + step + 5 > audio.length) {
						break;
					}
					current += step;
					combination += current + ";";
				}
				dataCombination.Add (combination);
			}
		}

		// ConvertStringToListInt
		dataGameList = dataCombination
			.Select (entry => entry
				.Split (';')
				.Where(x => !string.IsNullOrEmpty(x))
				.Select (x => int.Parse (x))
				.ToList ()
		).ToList ();

		//count totalscore and increment score

		var totalScore = dataGameList[currentIndex].Count * 7;
		incScore = (int)(100000f / totalScore);

		var remainder = incScore * totalScore;
		firstScore = 100000 - remainder;
	}


	void Update ()
	{
		if (Input.GetKeyDown (KeyCode.Space)) {
			PauseVideo ();
		}
		if (Input.GetKeyDown (KeyCode.Escape)) {
			if (!isMainMenu && !videoPlayer.isPlaying) {
				isMainMenu = true;
				ActivateMedia (currentIndex);
			} else if (isMainMenu && !videoPlayer.isPlaying) {
				ExitApp ();
			}
		}
		ChangeVideo ();
		Mode ();
		if (timeCombination) {
			PlayCombination ();	
		}
		if (isMainMenu) {
			slider.gameObject.SetActive(false);
		}
	}

	#endregion


	#region Utils

	void ExitApp ()
	{
		#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
		#else
		Application.Quit();
		#endif
	}

	KeyCode GetKeyCode (int code)
	{
		switch (code) {
		case 0:
			return KeyCode.UpArrow;
		case 1:
			return KeyCode.RightArrow;
		case 2:
			return KeyCode.DownArrow;
		case 3:
			return KeyCode.LeftArrow;
		default:
			return KeyCode.None;
		}
	}

	KeyCode[] directionKeys = {
		KeyCode.UpArrow,
		KeyCode.RightArrow,
		KeyCode.DownArrow,
		KeyCode.LeftArrow
	};

	void PlayCombination ()
	{
		KeyCode expectedKeyCode = GetKeyCode (combination [currCombination]);

		/* for testing automatically
		for (var i = 0; i < 7; i++) {
			if (score == 0) {
				score += firstScore;
			}
			score += incScore;
			if (i == 6) {
				RemoveButton ();
			}
		}*/
		
		foreach (KeyCode key in directionKeys) {
			if (Input.GetKeyDown (key)) {
				if (key == expectedKeyCode) {
					// first score for make it 100%
					if (score == 0) {
						score += firstScore;
					}
					score += incScore;
					button [currCombination].color = new Color (1f, 1f, 1f, .4f);
					currCombination++;

					if (currCombination >= 7)
					{
						RemoveButton();
						currCombination = 0;
					}
				} else {
					RemoveButton ();
					currCombination = 0;
				}
				return;
			}
		}
	}


	void ForceResolution ()
	{
		Cursor.visible = false;
		Screen.fullScreen = true;
		// Screen.SetResolution (1240, 720, false);
	}

	void PauseVideo ()
	{
		
		if (videoPlayer.isPlaying) {
			videoPlayer.targetCameraAlpha = .5f;
			videoPlayer.Pause ();
			audioSource.Pause ();
			RemoveButton ();
			escapeText.text = "Press Escape to Exit";

			if (isMainMenu) {
				PauseObject.SetActive (false);
			}
		} else {
			if (isMainMenu) {
				PauseObject.SetActive (true);
			}
			videoPlayer.targetCameraAlpha = 1;
			videoPlayer.Play ();
			audioSource.Play ();
			if (!isMainMenu) {
				escapeText.text = "";
			} else {
				escapeText.text = "Press Enter to Play";
			}
		}

	}

	void ChangeVideo ()
	{
		if (!isMainMenu) {
			return;
		}
		if (Input.GetKeyDown (KeyCode.RightArrow)) {
			currentIndex = (currentIndex + 1) % audioClip.Length;
			ActivateMedia (currentIndex);
		} else if (Input.GetKeyDown (KeyCode.LeftArrow)) {
			currentIndex = (currentIndex - 1 + audioClip.Length) % audioClip.Length;
			ActivateMedia (currentIndex);
		}
	}

	void ActivateMedia (int index)
	{
		videoPlayer.targetCameraAlpha = 1;

		ChangedTitle ();
		NewHighScore.SetActive (false);
		escapeText.text = "Press Enter to Play";
		PauseObject.SetActive (true);
		RemoveButton ();
		score = 0;
		currDataGame = 0;

		// Stop current playback
		audioSource.Stop ();
		videoPlayer.Stop ();

		// Assign new clips
		audioSource.clip = audioClip [index];
		videoPlayer.clip = videoClip [index];

		audioSource.time = audioClip[index].length / 2;
		videoPlayer.time = audioClip[index].length / 2;

		// Play new media
		audioSource.Play ();
		videoPlayer.Play ();

		// Get LatestScore
		int val = PlayerPrefs.GetInt (listSong[index]);
		Debug.Log (listSong [index] + " " + val);
		LatestScore.text = val.ToString("D6");
	}

	void Mode ()
	{
		if (Input.GetKeyDown (KeyCode.Return)) {
			if (isMainMenu) {
				isMainMenu = false;
				prevTitle.text = title.text;
				escapeText.text = "";
				title.text = "";
				nextTitle.text = "";
				audioSource.time = 0;
				videoPlayer.time = 0;
				PauseObject.SetActive (false);
			} else {
				PauseVideo ();
			}
		}

		if (isMainMenu) {
			if (audioSource.time >= (audioClip[currentIndex].length / 2)+20) {
				audioSource.time = audioClip[currentIndex].length / 2;
				videoPlayer.time = audioClip[currentIndex].length / 2;
			}
		} else {
			MainGame ();
		}
	}

	void MainGame ()
	{

		float progress = ((audioSource.time / audioSource.clip.length) * 100);
		if (progress >= 99.5) {
			progress = 100f;
		}
		nextTitle.text = (int)progress + "%";

		if (!isMainMenu) {
			if (progress == 100) {
				audioSource.Pause ();
				videoPlayer.Pause ();

				int oldScore = PlayerPrefs.GetInt (listSong[currentIndex]);

				if (score > oldScore) {
					NewHighScore.SetActive (true);
					PlayerPrefs.SetInt (listSong [currentIndex], score);
				}
				currDataGame = 0;
				escapeText.text = "Press Escape to Exit";
			}


			try{
				if (progress <= 99.5 && dataGameList [currentIndex] [currDataGame] == (int)audioSource.time) {
					ShowButton ();
					currDataGame++;
					Invoke ("RemoveButton", 3f);
				}
			}catch{
				Debug.LogWarning("Something Wrong here");
			}
		}


		title.text = score.ToString ("D6");
	}


	void RemoveButton ()
	{
		foreach (var btn in button) {
			btn.color = new Color (1f, 1f, 1f, 0f);
		}
		slider.gameObject.SetActive(false);
		timeCombination = false;
	}

	void ShowButton ()
	{
		for (int i = 0; i < button.Length; i++) {
			int rand = Random.Range (0, buttonTexture.Length);
			combination [i] = rand;
			button [i].texture = buttonTexture [rand];
			button [i].color = new Color (1f, 1f, 1f, 1f);
		}
		slider.value = 1;
		slider.gameObject.SetActive(true);
		StartCoroutine (AnimateSlider());
		timeCombination = true;
	}

	IEnumerator AnimateSlider()
	{
		float elapsed = 0f;

		while (elapsed < 3)
		{
			elapsed += Time.deltaTime;
			float t = Mathf.Clamp01(elapsed / 3);
			slider.value = Mathf.Lerp(1, 0, t);
			yield return null;
		}

		slider.value = 0;
	}

	void ChangedTitle ()
	{
		int length = listSong.Length;

		int prevIndex = (currentIndex - 1 + length) % length;
		int nextIndex = (currentIndex + 1) % length;

		title.text = listSong [currentIndex];
		prevTitle.text = listSong [prevIndex];
		nextTitle.text = listSong [nextIndex];
	}

	#endregion
}
