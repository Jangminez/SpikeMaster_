#if UNITY_EDITOR
using JangLib;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

public partial class GoogleSheetParser : EditorWindow
{
    private Vector2 scrollPos = Vector2.zero;

    string sheetAPIurl = "";
    string sheeturl = "";
    string jsonPath = "";
    string classPath = "";

    private List<SheetData> sheets = new List<SheetData>();
    private int selectedSheetIndex = 0;
    private bool isFetching = false;
    private bool isFetched = false;

    private bool createClass = false;

    #region <<  EDITORPREFS KEYS  >>
    private const string API_URL_PREF_KEY = "SheetParsing_SheetAPIUrl";
    private const string SHEET_URL_PREF_KEY = "SheetParsing_SheetUrl";
    private const string JSON_PATH_PREF_KEY = "SheetParsing_JSONPath";
    private const string CLASS_PATH_PREF_KEY = "SheetParsing_ClassPath";
    private const string CREATE_CLASS_PREF_KEY = "SheetParsing_CreateClass";
    #endregion

    private void OnEnable()
    {
        sheetAPIurl = EditorPrefs.GetString(API_URL_PREF_KEY, "** 배포 받은 url 링크 **");
        sheeturl = EditorPrefs.GetString(SHEET_URL_PREF_KEY, "** 구글스프레드시트 링크 **");
        jsonPath = EditorPrefs.GetString(JSON_PATH_PREF_KEY, "** JSON 파일 저장 경로 **");
        classPath = EditorPrefs.GetString(CLASS_PATH_PREF_KEY, "** 클래스 파일 저장 경로 **");
        createClass = EditorPrefs.GetBool(CREATE_CLASS_PREF_KEY, false);
    }

    #region <<  GUI LAYOUT  >>
    [MenuItem("Tools/Google Sheet Parsing Tool")]
    public static void ShowWindow()
    {
        EditorWindow window = GetWindow(typeof(GoogleSheetParser));
        window.titleContent = new GUIContent("Google Sheet Parser");
        window.maxSize = new Vector2(600, 400);
        window.minSize = new Vector2(600, 400);
    }

    private void OnGUI()
    {
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        GUILayout.Space(15);

        GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel);
        titleStyle.fontSize = 18;
        titleStyle.alignment = TextAnchor.MiddleCenter;
        EditorGUILayout.LabelField("GoogleSheet To JSON Parser", titleStyle, GUILayout.Height(30));

        GUILayout.Space(20);

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        GUIStyle headerStyle = new GUIStyle(EditorStyles.boldLabel);
        headerStyle.fontSize = 14;
        headerStyle.normal.textColor = new Color(0.2f, 0.4f, 0.8f);

        EditorGUILayout.LabelField("Configuration Settings", headerStyle);

        GUILayout.Space(10);

        EditorGUI.BeginChangeCheck();

        EditorGUILayout.LabelField("API URL (Sheet Names/IDs)", EditorStyles.boldLabel);
        sheetAPIurl = EditorGUILayout.TextField(sheetAPIurl);

        GUILayout.Space(5);

        EditorGUILayout.LabelField("Google Spreadsheet Base URL", EditorStyles.boldLabel);
        sheeturl = EditorGUILayout.TextField(sheeturl);

        if (EditorGUI.EndChangeCheck())
        {
            EditorPrefs.SetString(API_URL_PREF_KEY, sheetAPIurl);
            EditorPrefs.SetString(SHEET_URL_PREF_KEY, sheeturl);
        }

        if (isFetching)
        {
            GUIStyle fetchingStyle = new GUIStyle(EditorStyles.label);
            fetchingStyle.normal.textColor = Color.red;
            EditorGUILayout.LabelField("Fetching data... Please wait.", fetchingStyle);

            isFetched = true;
        }
        else
        {
            GUI.backgroundColor = Color.cyan;

            GUILayout.Space(10);
            if (GUILayout.Button("Fetch Sheets Data", GUILayout.Height(40)))
            {
                EditorCoroutineUtility.StartCoroutine(FetchSheetsData(), this);
            }

            GUI.backgroundColor = Color.white;
        }

        EditorGUILayout.EndVertical();

        EditorGUILayout.Separator();
        GUILayout.Space(30);

        if (isFetched && !isFetching)
        {

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            GUIStyle parseHeaderStyle = new GUIStyle(EditorStyles.boldLabel);
            parseHeaderStyle.fontSize = 16;
            parseHeaderStyle.normal.textColor = new Color(0.1f, 0.5f, 0.1f); // 녹색 계열
            EditorGUILayout.LabelField("Data Parsing Options", parseHeaderStyle, GUILayout.Height(20));

            GUILayout.Space(10);
            EditorGUI.BeginChangeCheck();

            createClass = EditorGUILayout.Toggle("Create Class File", createClass);
            GUILayout.Space(5);

            EditorGUILayout.LabelField("Json File Save Path", EditorStyles.boldLabel);
            jsonPath = EditorGUILayout.TextField(jsonPath);

            if (createClass)
            {
                EditorGUILayout.LabelField("Class File Save Path", EditorStyles.boldLabel);
                classPath = EditorGUILayout.TextField(classPath);
            }

            if (EditorGUI.EndChangeCheck())
            {
                EditorPrefs.SetString(JSON_PATH_PREF_KEY, jsonPath);
                EditorPrefs.SetString(CLASS_PATH_PREF_KEY, classPath);
                EditorPrefs.SetBool(CREATE_CLASS_PREF_KEY, createClass);
            }

            GUILayout.Space(10);
            GUILayout.Label("Parse Selected Sheet Data", EditorStyles.boldLabel);

            if (sheets.Count > 0)
            {
                string[] sheetNames = sheets.Select(s => s.sheetName).ToArray();

                GUILayout.Space(5);
                selectedSheetIndex = EditorGUILayout.Popup("Select Sheet", selectedSheetIndex, sheetNames);
                GUILayout.Space(10);

                GUI.backgroundColor = new Color(0.7f, 1.0f, 0.7f); // 연한 녹색
                if (GUILayout.Button("Parse Selected Sheet", GUILayout.Height(30)))
                {
                    ParseSelectedSheet();
                }
                GUI.backgroundColor = Color.white;
            }
            else
            {
                GUIStyle fetchingStyle = new GUIStyle(EditorStyles.label);
                fetchingStyle.normal.textColor = Color.red;
                EditorGUILayout.LabelField("No sheets found. Please Fetch data first.", fetchingStyle);
            }

            GUILayout.Space(20);

            GUILayout.Label("Parse All Sheets Data", EditorStyles.boldLabel);

            GUI.backgroundColor = new Color(1.0f, 0.6f, 0.2f);

            if (GUILayout.Button("Parse All Sheets", GUILayout.Height(40)))
            {
                if (sheets.Count > 0)
                {
                    ParseAllSheets();
                }
                else
                {
                    EditorUtility.DisplayDialog("Error", "Please fetch sheet names.", "OK");
                }
            }

            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.EndScrollView();
    }
    #endregion

    #region <<  FETCH GOOGLE SHEET DATA  >>
    private IEnumerator FetchSheetsData()
    {
        isFetching = true;
        UnityWebRequest request = UnityWebRequest.Get(sheetAPIurl);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            ProcessSheetsData(request.downloadHandler.text);
        }
        else
        {
            EditorLog.LogError("Error fetching data: " + request.error);
        }

        isFetching = false;
        Repaint();
    }

    private void ProcessSheetsData(string json)
    {
        try
        {
            var sheetsData = JsonConvert.DeserializeObject<SheetDataList>(json);

            sheets.Clear();
            sheets.AddRange(sheetsData.sheetData);

            if (sheets.Count > 0)
            {
                selectedSheetIndex = 0;
            }
        }
        catch (Exception ex)
        {
            EditorLog.LogError("Newtonsoft.Json Deserialize Error: " + ex.Message);
            EditorLog.LogError("Failed JSON: " + json);
        }
    }
    #endregion

    #region <<  PARSE LOGIC  >>
    private void ParseSelectedSheet()
    {
        var selectedSheet = sheets[selectedSheetIndex];
        string jsonFileName = selectedSheet.sheetName;
        EditorLog.Log($"Selected Sheet: {selectedSheet.sheetName}, Sheet ID: {selectedSheet.sheetId}");

        EditorCoroutineUtility.StartCoroutine(ParseGoogleSheet(jsonFileName, selectedSheet.sheetId), this);
    }

    private void ParseAllSheets()
    {
        foreach (var sheet in sheets)
        {
            string jsonFileName = sheet.sheetName;
            EditorCoroutineUtility.StartCoroutine(ParseGoogleSheet(jsonFileName, sheet.sheetId, false), this);
        }
    }

    private IEnumerator ParseGoogleSheet(string jsonFileName, string gid, bool notice = true)
    {
        string sheetUrl = $"{sheeturl}/export?format=tsv&gid={gid}";

        UnityWebRequest request = UnityWebRequest.Get(sheetUrl);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            EditorUtility.DisplayDialog("Fail", "GoogleConnect Fail!", "OK");
            yield break;
        }

        string data = request.downloadHandler.text;
        List<string> rows = ParseTSVData(data);

        if (rows == null || rows.Count < 4)
        {
            EditorLog.LogError("Not enough data rows to parse.");
            yield break;
        }

        HashSet<int> dbIgnoreColumns = new HashSet<int>();
        var keys = rows[0].Split('\t').ToList();
        var types = rows[1].Split('\t').ToList();

        JArray jArray = new JArray();
        for (int i = 3; i < rows.Count; i++)
        {
            var rowData = rows[i].Split('\t').ToList();

            // 첫 열이 DB_IGNORE라면 행 제외
            if (rowData[0].Equals("DB_IGNORE", StringComparison.OrdinalIgnoreCase))
            {
                EditorLog.Log($"Row {i + 1} ignored due to DB_IGNORE");
                continue;
            }

            var rowObject = ParseRow(keys, types, rowData, dbIgnoreColumns);
            if (rowObject != null)
            {
                jArray.Add(rowObject);
            }
        }

        SaveJsonToFile(jsonFileName, jArray);

        // C# 클래스 생성
        if (createClass)
        {
            string className = CreateDataClass(jsonFileName, keys, types, dbIgnoreColumns);
        }

        if (notice)
        {
            EditorUtility.DisplayDialog("Success", "Sheet parsed and saved as JSON successfully!", "OK");
            AssetDatabase.Refresh();
        }
    }

    // TSV 데이터 파싱
    private List<string> ParseTSVData(string data)
    {
        return data.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).ToList();
    }

    // DB_IGNORE 열 필터링
    private HashSet<int> GetDBIgnoreColumns(string headerRow)
    {
        var dbIgnoreColumns = new HashSet<int>();
        var firstRow = headerRow.Split('\t').ToList();

        for (int i = 0; i < firstRow.Count; i++)
        {
            if (firstRow[i].Equals("DB_IGNORE", StringComparison.OrdinalIgnoreCase))
            {
                dbIgnoreColumns.Add(i);
                EditorLog.Log($"Column {i + 1} ignored due to DB_IGNORE");
            }
        }

        return dbIgnoreColumns;
    }

    // 개별 행 파싱
    private JObject ParseRow(List<string> keys, List<string> types, List<string> rowData, HashSet<int> dbIgnoreColumns)
    {
        var rowObject = new JObject();

        for (int j = 0; j < keys.Count && j < rowData.Count; j++)
        {
            if (dbIgnoreColumns.Contains(j)) continue;

            string key = keys[j].Trim();
            string type = types[j].Trim();
            string value = rowData[j].Trim();

            if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(value)) continue;

            if (type.StartsWith("Enum<", StringComparison.OrdinalIgnoreCase) && type.EndsWith(">"))
            {
                rowObject[key] = ConvertEnumToInt(type, value);
            }
            else
            {
                rowObject[key] = ConvertValue(value, type);
            }
        }

        return rowObject.HasValues ? rowObject : null;
    }

    // 값을 적절한 형식으로 변환하는 메서드
    private JToken ConvertValue(string value, string type)
    {
        switch (type.Trim()) // 불필요한 공백 제거
        {
            case "int": return int.TryParse(value, out int intValue) ? intValue : 0;
            case "long": return long.TryParse(value, out long longValue) ? longValue : 0L;
            case "float": return float.TryParse(value, out float floatValue) ? floatValue : 0.0f;
            case "double": return double.TryParse(value, out double doubleValue) ? doubleValue : 0.0d;
            case "bool": return bool.TryParse(value, out bool boolValue) ? boolValue : false;
            case "byte": return byte.TryParse(value, out byte byteValue) ? byteValue : (byte)0;
            case "int[]": return JArray.FromObject(value.Split(',').Select(v => int.TryParse(v.Trim(), out int tempInt) ? tempInt : 0));
            case "float[]": return JArray.FromObject(value.Split(',').Select(v => float.TryParse(v.Trim(), out float tempFloat) ? tempFloat : 0.0f));
            case "string[]": return JArray.FromObject(value.Split(',').Select(v => v.Trim()));
            case "DateTime": return DateTime.TryParse(value, out DateTime dateTimeValue) ? dateTimeValue : DateTime.MinValue; // DateTime 변환
            case "TimeSpan": return TimeSpan.TryParse(value, out TimeSpan timeSpanValue) ? timeSpanValue : TimeSpan.Zero;
            case "Guid": return Guid.TryParse(value, out Guid guidValue) ? guidValue.ToString() : Guid.Empty.ToString();
            case "List<int>": return JArray.FromObject(value.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(v => int.TryParse(v.Trim(), out int tempInt) ? tempInt : 0));
            case "List<float>": return JArray.FromObject(value.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(v => float.TryParse(v.Trim(), out float tempFloat) ? tempFloat : 0.0f));
            case "List<string>": return JArray.FromObject(value.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(v => v.Trim()));
            default: return value; // 기본적으로 문자열로 반환
        }
    }

    //문자열 Enum 이름으로 실제 Type 객체를 탐색
    private Type FindEnumType(string enumTypeName)
    {
        return AppDomain.CurrentDomain.GetAssemblies()
                        .SelectMany(assembly => assembly.GetTypes())
                        .FirstOrDefault(t => t.IsEnum && t.Name.Equals(enumTypeName, StringComparison.OrdinalIgnoreCase));
    }

    // Enum<T> 형식의 문자열 값을 해당 Enum의 정수 값으로 변환
    private JToken ConvertEnumToInt(string type, string value)
    {
        try
        {
            string enumTypeName = type.Substring(5, type.Length - 6).Trim();

            Type enumType = FindEnumType(enumTypeName);

            if (enumType != null)
            {
                object enumValue = Enum.Parse(enumType, value, true);

                return new JValue(Convert.ToInt32(enumValue));
            }
            else
            {
                EditorLog.LogWarning($"Enum Type '{enumTypeName}' not found. Saving value '{value}' as string.");
                return new JValue(value);
            }
        }
        catch (Exception ex)
        {
            EditorLog.LogError($"Failed to parse Enum value '{value}' for type '{type}'. Error: {ex.Message}");
            return new JValue(value);
        }
    }
    #endregion

    #region <<  SAVE JSON FILE  >>
    private void SaveJsonToFile(string jsonFileName, JArray jArray)
    {
        string directoryPath = Path.Combine(Application.dataPath, jsonPath);

        // 폴더가 존재하지 않으면 생성
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        string jsonFilePath = Path.Combine(directoryPath, $"{jsonFileName}.json");

        JObject wrapper = new JObject
        {
            { "Items", jArray }
        };

        File.WriteAllText(jsonFilePath, wrapper.ToString(Formatting.Indented));
        EditorLog.Log($"Saved JSON to: {jsonFilePath}");
    }
    #endregion

    #region <<  GENERATE C# CLASS  >>
    private string CreateDataClass(string fileName, List<string> keys, List<string> types, HashSet<int> dbIgnoreColumns)
    {
        string className = ConvertToPascalCase(fileName); // 파일 이름을 클래스 이름으로 사용
        string directoryPath = Path.Combine(Application.dataPath, classPath);

        string firstKey = keys.Count > 0 ? keys[0] : "key";

        // 폴더가 존재하지 않으면 생성
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        string dataClassPath = Path.Combine(directoryPath, $"{className}.cs");

        using (StreamWriter writer = new StreamWriter(dataClassPath))
        {
            writer.WriteLine("using System.Collections.Generic;");
            writer.WriteLine("using UnityEngine;");
            writer.WriteLine("using JangLib;");
            writer.WriteLine("");

            // 실제 데이터를 담는 클래스
            writer.WriteLine("[System.Serializable]");
            writer.WriteLine($"public class {className}");
            writer.WriteLine("{");
            for (int i = 0; i < keys.Count; i++)
            {
                if (dbIgnoreColumns.Contains(i)) continue; // DB_IGNORE가 설정된 컬럼 건너뜀

                string fieldType = ConvertTypeToCSharp(types[i]);
                string fieldName = keys[i];

                // 필드명이 비어있지 않은지 확인
                if (!string.IsNullOrEmpty(fieldName))
                {
                    writer.WriteLine($"\tpublic {fieldType} {fieldName};");
                }
            }
            writer.WriteLine("}");
            writer.WriteLine("");

            // 관리 및 로드 클래스
            string firstKeyType = ConvertTypeToCSharp(types[0]);

            writer.WriteLine($"public class {className}Loader");
            writer.WriteLine("{");
            writer.WriteLine($"\tpublic List<{className}> ItemList {{ get; private set; }}");
            writer.WriteLine($"\tpublic Dictionary<{firstKeyType}, {className}> ItemDict {{ get; private set; }}");
            writer.WriteLine("");

            // 생성자 (기본 경로는 Resources 폴더 기준 데이터 파일명)
            writer.WriteLine($"\tpublic {className}Loader(string path = \"JsonData/{fileName}\")");
            writer.WriteLine("\t{");
            writer.WriteLine("\t\tTextAsset json = Resources.Load<TextAsset>(path);");
            writer.WriteLine("\t\tif (json == null)");
            writer.WriteLine("\t\t{");
            writer.WriteLine($"\t\t\tEditorLog.LogWarning($\"{{path}} 데이터가 존재하지 않습니다.\");");
            writer.WriteLine("\t\t\treturn;");
            writer.WriteLine("\t\t}");
            writer.WriteLine("");
            writer.WriteLine($"\t\tItemList = JsonUtility.FromJson<{className}Wrapper>(json.text).Items;");
            writer.WriteLine($"\t\tItemDict = new Dictionary<{firstKeyType}, {className}>();");
            writer.WriteLine("\t\tforeach (var item in ItemList)");
            writer.WriteLine("\t\t{");

            // 'key'라는 컬럼이 반드시 존재한다는 가정하에 딕셔너리 생성
            writer.WriteLine($"\t\t\tItemDict[item.{firstKey}] = item;");
            writer.WriteLine("\t\t}");
            writer.WriteLine("\t}");
            writer.WriteLine("");

            // 데이터 획득 메서드
            writer.WriteLine($"\tpublic {className} GetData({firstKeyType} key)");
            writer.WriteLine("\t{");
            writer.WriteLine($"\t\tItemDict.TryGetValue(key, out {className} data);");
            writer.WriteLine("\t\treturn data;");
            writer.WriteLine("\t}");
            writer.WriteLine("}");
            writer.WriteLine("");

            // JSON 파싱용 Wrapper 클래스 (예: PopupWrapper)
            writer.WriteLine("/// <summary>");
            writer.WriteLine($"/// {className} 리스트 Wrapper 클래스");
            writer.WriteLine("/// </summary>");
            writer.WriteLine("[System.Serializable]");
            writer.WriteLine($"public class {className}Wrapper");
            writer.WriteLine("{");
            writer.WriteLine($"\tpublic List<{className}> Items;");
            writer.WriteLine("}");
        }

        EditorLog.Log($"Saved C# class to: {dataClassPath}");
        AssetDatabase.Refresh();

        return className;
    }

    private string ConvertTypeToCSharp(string type)
    {
        string trimmedType = type.Trim();

        // Enum<T> 형식에서 T만 추출
        if (trimmedType.StartsWith("Enum<", StringComparison.OrdinalIgnoreCase) && trimmedType.EndsWith(">"))
        {
            string enumName = trimmedType.Substring(5, trimmedType.Length - 6).Trim();
            return enumName;
        }

        switch (trimmedType) // 불필요한 공백 제거
        {
            case "int": return "int";
            case "long": return "long";
            case "float": return "float";
            case "double": return "double";
            case "bool": return "bool";
            case "byte": return "byte";
            case "int[]": return "int[]";
            case "float[]": return "float[]";
            case "string[]": return "string[]";
            case "DateTime": return "System.DateTime"; // DateTime에 대한 올바른 반환값
            case "TimeSpan": return "System.TimeSpan";
            case "Guid": return "System.Guid";
            case "List<int>": return "List<int>";
            case "List<float>": return "List<float>";
            case "List<string>": return "List<string>";
            default: return "string"; // 기본적으로 string으로 처리
        }
    }

    private string ConvertToPascalCase(string sheetName)
    {
        string[] words = sheetName.Trim().Split(new char[] { '_', ' ' }, StringSplitOptions.RemoveEmptyEntries);

        if (words.Length == 0)
        {
            return string.Empty;
        }

        StringBuilder pascalCaseString = new StringBuilder();

        foreach (string word in words)
        {
            if (string.IsNullOrWhiteSpace(word))
            {
                continue;
            }

            char firstCharUpper = char.ToUpper(word[0]);
            string restOfWord = word.Length > 1 ? word.Substring(1).ToLower() : string.Empty;

            pascalCaseString.Append(firstCharUpper);
            pascalCaseString.Append(restOfWord);
        }

        return pascalCaseString.ToString();
    }
    #endregion
}

// SheetData 클래스
[System.Serializable]
public class SheetData
{
    public string sheetName;
    public string sheetId;
}

// SheetDataList 클래스
[System.Serializable]
public class SheetDataList
{
    public SheetData[] sheetData;
}
#endif