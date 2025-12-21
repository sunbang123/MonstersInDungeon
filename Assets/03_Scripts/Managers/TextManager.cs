using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using System;

enum StoryState
{
    STORY,
    SELECT
}

enum SelectState
{
    FIRE,
    WATER,
    PLANT
}

public class TextManager : MonoBehaviour
{
    [SerializeField] private AssetReference nextSceneReference;

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI dialog;
    [SerializeField] private GameObject obFade;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private GameObject selectPanel;
    [SerializeField] private GameObject dialogPanel;

    [Header("Story Settings")]
    [SerializeField] private List<string> storyFiles = new List<string> { "story001", "story002" };
    [SerializeField] private List<Sprite> backgroundSprites;

    [Header("Select Settings")]
    [SerializeField] private TextMeshProUGUI itemInfoTxt;
    [SerializeField] private List<GameObject> selectCursorPrefab;
    [SerializeField] private List<string> infoFiles = new List<string> { "info_fire", "info_water", "info_plant" };
    [SerializeField] private List<string> selectFiles = new List<string> { "story_fire", "story_water", "story_plant" };
    [SerializeField] private List<Sprite> selectBgSprites;

    private List<Dictionary<string, object>> currentData;
    private int dialogIndex = 0;
    private int storyIndex = 0;
    private bool isTransitioning = false;
    private StoryState currentState = StoryState.STORY;
    private bool isPostSelectionStory = false;
    private int currentSelectionIndex = 0;

    void Start()
    {
        StartCoroutine(FadeIn());
        LoadCurrentStory();
        selectPanel.SetActive(false);
        itemInfoTxt.text = "";

        foreach (var cursor in selectCursorPrefab)
        {
            if (cursor != null) cursor.SetActive(false);
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && !isTransitioning && currentState == StoryState.STORY)
        {
            HandleDialogueProgression();
        }
    }

    private void HandleDialogueProgression()
    {
        if (HasMoreDialogue())
        {
            ShowNextDialogue();
            return;
        }

        if (!isPostSelectionStory && storyIndex == storyFiles.Count - 1)
        {
            ActivateSelection();
            return;
        }

        if (!isPostSelectionStory && HasMoreStories())
        {
            StartCoroutine(TransitionToNextStory());
            return;
        }

        StartCoroutine(EndStory());
    }

    private bool HasMoreDialogue() => currentData != null && dialogIndex < currentData.Count;
    private bool HasMoreStories() => storyIndex + 1 < storyFiles.Count;

    private void SetState(StoryState newState)
    {
        currentState = newState;

        dialogPanel.SetActive(currentState == StoryState.STORY);
        selectPanel.SetActive(currentState == StoryState.SELECT);

        if (currentState == StoryState.STORY && itemInfoTxt != null)
        {
            itemInfoTxt.text = "";
            foreach (var cursor in selectCursorPrefab)
            {
                if (cursor != null) cursor.SetActive(false);
            }
        }
    }

    private void ShowNextDialogue()
    {
        var currentLine = currentData[dialogIndex];
        dialog.text = $"{currentLine["이름"]} : {currentLine["대사"]}";
        dialogIndex++;
    }

    private void LoadCurrentStory(string fileName = null, Sprite newBackground = null)
    {
        string fileToLoad = fileName ?? storyFiles[storyIndex];
        dialogIndex = 0;
        currentData = CSVReader.Read(fileToLoad);
        UpdateBackground(newBackground);

        if (currentData != null && currentData.Count > 0)
        {
            ShowNextDialogue();
        }
        SetState(StoryState.STORY);
    }

    private void UpdateBackground(Sprite newSprite = null)
    {
        if (backgroundImage == null) return;
        if (newSprite != null)
        {
            backgroundImage.sprite = newSprite;
        }
        else if (backgroundSprites != null && storyIndex < backgroundSprites.Count)
        {
            backgroundImage.sprite = backgroundSprites[storyIndex];
        }
    }

    private void ActivateSelection()
    {
        SetState(StoryState.SELECT);
        currentSelectionIndex = 0;
        SetSelectionIndex(currentSelectionIndex);
    }

    private void UpdateSelectionUI(int index)
    {
        if (itemInfoTxt == null || index < 0 || index >= selectCursorPrefab.Count) return;

        for (int i = 0; i < selectCursorPrefab.Count; i++)
        {
            if (selectCursorPrefab[i] != null)
            {
                selectCursorPrefab[i].SetActive(i == index);
            }
        }

        if (index < infoFiles.Count)
        {
            string infoFileName = infoFiles[index];
            List<Dictionary<string, object>> infoData = CSVReader.Read(infoFileName);

            if (infoData != null && infoData.Count > 0)
            {
                string infoKey = "설명";
                string displayText = "";

                foreach (var row in infoData)
                {
                    if (row.ContainsKey(infoKey) && row[infoKey] is string content)
                    {
                        displayText += content + "\n";
                    }
                }

                if (!string.IsNullOrEmpty(displayText))
                {
                    itemInfoTxt.text = displayText.TrimEnd('\n');
                }
                else
                {
                    itemInfoTxt.text = $"[경고] {infoFileName} 파일에서 '{infoKey}' 키의 내용을 찾을 수 없습니다.";
                }
            }
            else
            {
                itemInfoTxt.text = $"[오류] {infoFileName} 파일을 불러오는 데 실패했습니다.";
            }
        }
        else
        {
            itemInfoTxt.text = "원소를 선택하여 정보를 확인하세요.";
        }
    }

    private void SetSelectionIndex(int index)
    {
        if (currentState != StoryState.SELECT || isTransitioning) return;
        if (index < 0 || index >= selectFiles.Count) return;

        currentSelectionIndex = index;
        UpdateSelectionUI(currentSelectionIndex);
    }

    public void SelectNextElement()
    {
        int totalElements = selectFiles.Count;
        if (totalElements == 0) return;

        int nextIndex = (currentSelectionIndex + 1) % totalElements;
        SetSelectionIndex(nextIndex);
    }

    // 최종 선택 확정 함수: 매개변수를 받지 않고 현재 포커스 인덱스를 사용
    public void OnSelectionMade()
    {
        int index = currentSelectionIndex; // 현재 포커스된 인덱스 사용

        if (currentState != StoryState.SELECT || isTransitioning) return;
        if (index < 0 || index >= selectFiles.Count) return;

        isPostSelectionStory = true;

        string selectedStoryFile = selectFiles[index];
        Sprite selectedBg = selectBgSprites[index];

        StartCoroutine(TransitionToNewStory(selectedStoryFile, selectedBg));
    }

    private IEnumerator TransitionToNextStory()
    {
        isTransitioning = true;
        yield return StartCoroutine(FadeOut());

        storyIndex++;
        LoadCurrentStory();

        yield return new WaitForSeconds(0.3f);
        yield return StartCoroutine(FadeIn());

        isTransitioning = false;
    }

    private IEnumerator TransitionToNewStory(string fileName, Sprite newBackground)
    {
        isTransitioning = true;
        SetState(StoryState.STORY);

        yield return StartCoroutine(FadeOut());

        LoadCurrentStory(fileName, newBackground);

        yield return new WaitForSeconds(0.3f);
        yield return StartCoroutine(FadeIn());

        isTransitioning = false;
    }

    private IEnumerator EndStory()
    {
        int index = currentSelectionIndex;

        isTransitioning = true;
        yield return StartCoroutine(FadeOut());

        // UserPlayerStatusData 가져오기
        var status = UserDataManager.Instance.Get<UserPlayerStatusData>();
        if (status != null)
        {
            // 선택한 원소 저장
            switch (index)
            {
                case 0: status.SelectedElement = "FIRE"; break;
                case 1: status.SelectedElement = "WATER"; break;
                case 2: status.SelectedElement = "PLANT"; break;
            }

            // 튜토리얼 종료 저장
            status.TutorialEnd = true;

            // 한 번만 저장
            UserDataManager.Instance.SaveUserData();

            Debug.Log($"튜토리얼 종료! 선택: {status.SelectedElement}, TutorialEnd: {status.TutorialEnd}");
        }
        else
        {
            Debug.LogError("UserPlayerStatusData is NULL");
        }

        // 다음 씬 로드
        if (nextSceneReference != null)
        {
            nextSceneReference.LoadSceneAsync(UnityEngine.SceneManagement.LoadSceneMode.Single);
        }
    }



    private IEnumerator FadeIn()
    {
        yield return StartCoroutine(Fade(1f, 0f));
        obFade.SetActive(false);
    }

    private IEnumerator FadeOut()
    {
        obFade.SetActive(true);
        yield return StartCoroutine(Fade(0f, 1f));
    }

    private IEnumerator Fade(float from, float to)
    {
        Image fadeImage = obFade.GetComponent<Image>();
        Color color = fadeImage.color;

        for (float t = 0; t < 1f; t += Time.deltaTime * 2f)
        {
            color.a = Mathf.Lerp(from, to, t);
            fadeImage.color = color;
            yield return null;
        }

        color.a = to;
        fadeImage.color = color;
    }
}