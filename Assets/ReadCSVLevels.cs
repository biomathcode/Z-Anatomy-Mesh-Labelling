﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Text;
using System;
using CsvHelper;
using UnityEditor;

public class ReadCSVLevels : MonoBehaviour
{
    public static ReadCSVLevels Instance;
    public TextAsset skeletonCSV;
    public TextAsset levelOverviewCSV;
    public TextAsset difficultyLevelsCSV;
    public Text difficultyLevelText;

    public GameObject contentGameObject;

    [SerializeField]
    private GameObject buttonPrefab;
    [SerializeField]
    private GameObject buttonParent;
    [Range(5, 15)]
    [SerializeField]
    private float titleTextSize = 10;
    [Range(5, 15)]
    [SerializeField]
    private float textSize = 10;
    [Range(0, 10)]
    [SerializeField]
    private float horizontalSpacing = 3;
    [Range(0, 10)]
    [SerializeField]
    private float verticalSpacing = 0;
    [Range(0, 15)]
    [SerializeField]
    private float spaceBetweenSections = 0;

    [SerializeField]
    private SingleSelectionScrollView scrollView;

    private Dictionary<string, string> levelOverviews = new Dictionary<string, string>();
    private Dictionary<string, string> levelDifficulties = new Dictionary<string, string>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        buttonParent.GetComponent<VerticalLayoutGroup>().spacing = verticalSpacing;
    }

    void Start()
    {
        LoadLevelOverviews();
        LoadLevelDifficulties();
        CreateButtons();
    }

    private void LoadLevelOverviews()
    {
        List<List<string>> rows = ParseCSV(levelOverviewCSV.text);

        foreach (List<string> row in rows)
        {
            if (row.Count >= 2)
            {
                string levelName = row[0];
                string levelOverview = row[1];
                levelOverviews[levelName] = levelOverview;
            }
        }
    }

    private void LoadLevelDifficulties()
    {
        List<List<string>> rows = ParseCSV(difficultyLevelsCSV.text);

        foreach (List<string> row in rows)
        {
            if (row.Count >= 2)
            {
                string levelName = row[0];
                string levelDifficulty = row[1];
                levelDifficulties[levelName] = levelDifficulty;
            }
        }
    }

    private void CreateButtons()
    {
        List<List<string>> rows = GetRows();

        // For each row
        foreach (List<string> row in rows)
        {
            // For each column, create 
            for (int i = 0; i < row.Count; i++)
            {
                // The first column is the title
                bool isTitle = i == 0;
                CreateButton(isTitle, row[i]);
            }

            // Add space between sections
            AddSpacing();
        }
    }

    private List<List<string>> GetRows()
    {
        TextAsset csv = skeletonCSV;
        string csvText = csv.text;
        return ParseCSV(csvText);
    }

    private List<List<string>> ParseCSV(string csvText)
    {
        List<List<string>> rows = new List<List<string>>();
        StringReader reader = new StringReader(csvText);

        string line;
        while ((line = reader.ReadLine()) != null)
        {
            List<string> row = new List<string>(line.Split(','));
            rows.Add(row);
        }

        return rows;
    }

    private void CreateButton(bool isTitle, string text)
    {
        GameObject newButton = Instantiate(buttonPrefab, buttonParent.transform);
        var textScript = newButton.GetComponentInChildren<TextMeshProUGUI>();
        textScript.text = text;


        if (isTitle)
        {
            textScript.fontStyle = FontStyles.Bold | FontStyles.Underline;
            textScript.fontSize = titleTextSize;
            textScript.color = scrollView.normalColor;
        }
        else
        {
            textScript.GetComponent<RectTransform>().offsetMin += new Vector2(horizontalSpacing, 0);
            textScript.fontSize = textSize;
            textScript.color = scrollView.normalColor;
            string prefabPath = "Prefabs/Label Prefabs/" + text;
            GameObject go = Resources.Load<GameObject>(prefabPath);
            bool exists = go != null;
            if (!exists)
                textScript.color = scrollView.disabledColor;

        }

        RectTransform buttonRectTransform = newButton.GetComponent<RectTransform>();
        Vector2 txtSize = textScript.GetPreferredValues();
        buttonRectTransform.sizeDelta = new Vector2(buttonRectTransform.sizeDelta.x, txtSize.y);

        Button buttonComponent = newButton.GetComponent<Button>();
        if (buttonComponent != null)
        {
            buttonComponent.onClick.AddListener(() => OnButtonClick(buttonComponent, text));
        }
    }
    public List<string> GetSelectedParts(string title)
    {
        List<List<string>> rows = GetRows();
        List<string> selectedParts = new List<string>();

        // Iterate over the rows
        foreach (List<string> row in rows)
        {
            // Check if the title column matches the given title
            if (row.Count > 0 && row[0] == title)
            {
                // Add the remaining columns to the selectedParts list
                for (int j = 1; j < row.Count; j++)
                {
                    selectedParts.Add(row[j]);
                }

                break; // Exit the loop since we found the matching title
            }
        }

        return selectedParts;
    }

    private void AddSpacing()
    {
        GameObject spacing = new GameObject();
        spacing.transform.parent = buttonParent.transform;
        spacing.AddComponent<RectTransform>().SetHeight(spaceBetweenSections);
    }

    private void OnButtonClick(Button btn, string levelName)
    {
        if (btn.GetComponentInChildren<TextMeshProUGUI>().color == scrollView.disabledColor)
            return;

        if (levelDifficulties.TryGetValue(levelName, out string levelDifficulty))
        {
            if (difficultyLevelText != null)
            {
                difficultyLevelText.text = levelDifficulty; // Display the difficulty level
            }
            else
            {
                Debug.LogWarning("Difficulty Level Text component not assigned.");
            }
        }
        else
        {
            Debug.LogWarning("Difficulty level not found for the selected level.");
        }

        if (levelOverviews.TryGetValue(levelName, out string levelOverview))
        {
            TextMeshProUGUI contentText = contentGameObject.GetComponentInChildren<TextMeshProUGUI>();
            if (contentText != null)
            {
                levelOverview = levelOverview.Replace("⋅", ",");

                string[] lines = levelOverview.Split('.');
                string formattedContent = "\n• " + string.Join("\n• ", lines).Trim();
                contentText.text = formattedContent;
            }
            else
            {
                Debug.LogWarning("TextMeshProUGUI component not found in the children of the assigned GameObject.");
            }
        }
    }
}
