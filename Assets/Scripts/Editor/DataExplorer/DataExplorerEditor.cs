using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditor.Scripting.Python;
using UnityEngine;

public class DataExplorerEditor : EditorWindow
{
	private const string miniMapRenderTexPath = "Assets/Art Assets/UI/MiniMap/MiniMapRenderTex.renderTexture";
	private const string gameplayScenePath = "Assets/Scenes/Gameplay.unity";
	private int selectedOption = 0;
	private Vector2 scrollPosition;
	private List<RemoteConfigEntry> remoteConfigEntries = new();
	private readonly string[] valueTypes = new string[] { "Boolean", "JSON", "Float", "String", "Integer", "Long" };

	private Dictionary<RemoteConfigEntry, bool> entryExpansionStates = new();

	private Dictionary<string, bool> jsonFoldouts = new();

	private string getBackupType()
	{
		return selectedOption switch
		{
			1 => "RemoteConfig",
			2 => "HeatMap",
			3 => "AbilityUsage",
			4 => "ToolsUsage",
			5 => "MatchDamage",
			_ => "Unknown",
		};
	}

	[MenuItem("Window/Data Explorer")]
	public static void ShowWindow()
	{
		GetWindow<DataExplorerEditor>("Data Explorer");
	}

	private void OnGUI()
	{
		GUILayout.Space(5);
		GUILayout.Label("Please select one of the following options:", EditorStyles.boldLabel);
		GUILayout.Space(10);

		GUILayout.BeginHorizontal();
		if (GUILayout.Button("Remote Config CSV file", GetButtonStyle(1)))
		{
			selectedOption = 1;
		}
		if (GUILayout.Button("Generate HeatMap", GetButtonStyle(2)))
		{
			selectedOption = 2;
		}
		if (GUILayout.Button("Ability Usage Statistics", GetButtonStyle(3)))
		{
			selectedOption = 3;
		}
		if (GUILayout.Button("Tool Usage Statistics", GetButtonStyle(4)))
		{
			selectedOption = 4;
		}
		if (GUILayout.Button("Damage Statistics", GetButtonStyle(5)))
		{
			selectedOption = 5;
		}
		GUILayout.EndHorizontal();

		createDropbox();

		if (selectedOption == 1 && remoteConfigEntries.Count > 0)
		{
			GUILayout.Space(10);
			GUILayout.Label("Remote Config Entries:", EditorStyles.boldLabel);

			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Sort Alphabetically"))
			{
				SortEntriesAlphabetically();
			}
			if (GUILayout.Button("Validate Fields"))
			{
				ValidateFields();
			}
			GUILayout.EndHorizontal();

			scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
			int _entryIndex = 0;
			foreach (var _entry in remoteConfigEntries.ToArray())
			{
				DrawEntryUI(_entry, _entryIndex);
				_entryIndex++;
			}
			EditorGUILayout.EndScrollView();

			if (GUILayout.Button("Add New Entry"))
			{
				var _newEntry = new RemoteConfigEntry();
				remoteConfigEntries.Add(_newEntry);
				entryExpansionStates[_newEntry] = true;
			}

			if (GUILayout.Button("Save Changes"))
			{
				if (ValidateFields())
				{
					SaveRemoteConfigEntries();
				}
				else
				{
					Debug.LogError("Validation failed. Please fix the errors before saving.");
				}
			}
		}
	}

	private void createDropbox()
	{
		GUILayout.Label("Drag and Drop File Here", EditorStyles.boldLabel);
		Rect _dropArea = GUILayoutUtility.GetRect(0.0f, 100.0f, GUILayout.ExpandWidth(true));
		GUI.Box(_dropArea, "Drag your file here");

		Event _evt = Event.current;
		switch (_evt.type)
		{
			case EventType.DragUpdated:
			case EventType.DragPerform:
				if (!_dropArea.Contains(_evt.mousePosition))
					return;

				DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

				if (_evt.type == EventType.DragPerform)
				{
					DragAndDrop.AcceptDrag();
					foreach (string _draggedObjectPath in DragAndDrop.paths)
					{
						ReadFile(_draggedObjectPath);
					}
				}
				break;
		}
	}

	private GUIStyle GetButtonStyle(int _buttonName)
	{
		GUIStyle _style = new GUIStyle(GUI.skin.button);

		if (selectedOption == _buttonName)//check if button is selected
		{
			_style.normal.textColor = Color.white;
			_style.normal.background = MakeTex(2, 2, Color.blue);
		}
		else
		{
			_style.normal.textColor = Color.black;
		}

		return _style;
	}

	private Texture2D MakeTex(int _width, int _height, Color _colour)
	{
		Color[] _pix = new Color[_width * _height];
		for (int i = 0; i < _pix.Length; i++)
			_pix[i] = _colour;

		Texture2D _result = new Texture2D(_width, _height);
		_result.SetPixels(_pix);
		_result.Apply();

		return _result;
	}

	private void ReadFile(string _path)
	{
		stopPlaying();
		var _fileExtension = Path.GetExtension(_path).ToLower();
		if (_fileExtension != ".csv")
		{
			Debug.LogError("Invalid file type. Please drop a CSV file");
			return;
		}
		string _fileContent = File.ReadAllText(_path);
		Debug.Log("File Content: " + _fileContent);
		saveBackupFile(_fileContent);

		switch (selectedOption)
		{
			case 1:
				readCSV(_fileContent);
				break;
			case 2:
				generateHeatMap(ref _fileContent);
				break;
			case 3:
				PythonRunner.EnsureInitialized();
				generateTempFile(_fileContent);
				PythonRunner.RunFile($"{Application.dataPath}/Scripts/Editor/DataExplorer/Python/AbilityUsageGraph.py");
				break;
			case 4:
				PythonRunner.EnsureInitialized();
				generateTempFile(_fileContent);
				PythonRunner.RunFile($"{Application.dataPath}/Scripts/Editor/DataExplorer/Python/ToolsUsageGraph.py");
				break;
			case 5:
				PythonRunner.EnsureInitialized();
				generateTempFile(_fileContent);
				PythonRunner.RunFile($"{Application.dataPath}/Scripts/Editor/DataExplorer/Python/DamageStatistics.py");
				break;
			default:
				Debug.LogError("Invalid option selected");
				break;
		}
	}

	private void saveBackupFile(string _fileContent)
	{
		string _projectRootPath = Directory.GetParent(Application.dataPath).FullName;
		string _backupFolderPath = Path.Combine(_projectRootPath, "DataExplorer", "Backups");
		Directory.CreateDirectory(_backupFolderPath);

		string _fileName = getBackupType() + "_backup_" + System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".csv";
		string _filePath = Path.Combine(_backupFolderPath, _fileName);

		File.WriteAllText(_filePath, _fileContent);
		Debug.Log("Backup file saved successfully");
	}

	private void generateTempFile(string _fileContent)
	{
		string _projectRootPath = Directory.GetParent(Application.dataPath).FullName;
		string _tempFolderPath = Path.Combine(_projectRootPath, "DataExplorer", "Temp");
		Directory.CreateDirectory(_tempFolderPath);

		string _fileName = getBackupType() + "TempData.txt";
		string _filePath = Path.Combine(_tempFolderPath, _fileName);

		File.WriteAllText(_filePath, _fileContent);
		Debug.Log("Temp file saved successfully at " + _filePath);
	}

	private void stopPlaying()
	{
		if (!EditorApplication.isPlaying) { return; }
		EditorApplication.isPlaying = false;
	}

	private void readCSV(string _fileContent)
	{
		remoteConfigEntries.Clear();
		entryExpansionStates.Clear();
		var _lines = _fileContent.Split('\n');
		if (_lines.Length < 2)
		{
			Debug.LogError("Invalid CSV file. Please ensure the file has at least 2 lines");
			return;
		}
		for (int i = 1; i < _lines.Length; i++)//skip the header
		{
			var _line = _lines[i];
			if (string.IsNullOrWhiteSpace(_line))
			{
				continue;
			}
			//parse
			List<string> _fields = ParseCsvLine(_line);
			if (_fields.Count < 4)
			{
				Debug.LogError("Invalid CSV format on line " + (i + 1));
				continue;
			}

			string _key = _fields[0];
			string _type = _fields[1];
			string _schemaId = _fields[2];
			string _value = _fields[3];

			Debug.Log($"Key: {_key}, Type: {_type}, Schema ID: {_schemaId}, Value: {_value}");

			//unescape values
			string _unescapedValue = UnescapeCsvValue(_value);

			RemoteConfigEntry _entry = new RemoteConfigEntry
			{
				Key = _key,
				Type = _type,
				SchemaId = _schemaId,
				Value = _unescapedValue
			};

			if (_type.Equals("json", StringComparison.OrdinalIgnoreCase))
			{
				try
				{
					_entry.JsonData = JObject.Parse(_unescapedValue);
				}
				catch (Exception e)
				{
					Debug.LogError("Error parsing JSON on line " + (i + 1) + ": " + e.Message);
					_entry.JsonData = new JObject();
				}
			}

			remoteConfigEntries.Add(_entry);
			entryExpansionStates[_entry] = true;
		}
	}

	/// <summary>
	/// Parse a CSV line into fields, handling commas within quotes
	/// </summary>
	/// <param name="_line"></param>
	/// <returns></returns>
	private List<string> ParseCsvLine(string _line)
	{
		List<string> _fields = new();
		bool _inQuotes = false;
		string _field = "";

		for (int i = 0; i < _line.Length; i++)
		{
			char _character = _line[i];

			if (_character == '"')
			{
				if (_inQuotes && i + 1 < _line.Length && _line[i + 1] == '"')
				{
					//double quote inside quoted field
					_field += '"';
					i++; //skip the next quote
				}
				else
				{
					_inQuotes = !_inQuotes;
				}
			}
			else if (_character == ',' && !_inQuotes)
			{
				_fields.Add(_field);
				_field = "";
			}
			else
			{
				_field += _character;
			}
		}
		_fields.Add(_field);
		return _fields;
	}

	/// <summary>
	/// Unescape a CSV value, removing quotes and unescaping double quotes
	/// </summary>
	/// <param name="_value"></param>
	/// <returns></returns>
	private string UnescapeCsvValue(string _value)
	{
		string _unescapedValue = _value;
		if (_unescapedValue.StartsWith("\"") && _unescapedValue.EndsWith("\""))
		{
			_unescapedValue = _unescapedValue.Substring(1, _unescapedValue.Length - 2);
			_unescapedValue = _unescapedValue.Replace("\"\"", "\"");
		}
		return _unescapedValue;
	}

	private void DrawEntryUI(RemoteConfigEntry _entry, int _entryIndex)
	{
		EditorGUILayout.BeginVertical("box");
		EditorGUI.indentLevel = 0;

		if (!entryExpansionStates.ContainsKey(_entry))
		{
			entryExpansionStates[_entry] = true;
		}

		entryExpansionStates[_entry] = EditorGUILayout.Foldout(entryExpansionStates[_entry], _entry.Key ?? "New Entry", true);

		if (entryExpansionStates[_entry])
		{
			EditorGUI.indentLevel++;
			EditorGUILayout.BeginHorizontal();
			string _oldKey = _entry.Key;
			_entry.Key = EditorGUILayout.TextField("Key", _entry.Key);
			if (GUILayout.Button("Delete", GUILayout.Width(60)))
			{
				remoteConfigEntries.Remove(_entry);
				entryExpansionStates.Remove(_entry);
				return; 
			}
			EditorGUILayout.EndHorizontal();

			if (_oldKey != _entry.Key)
			{
				if (Regex.IsMatch(_entry.Key ?? "", @"^Ability\d*$", RegexOptions.IgnoreCase))
				{
					//set schemaId, Type, and initialize JSON data
					_entry.SchemaId = "ability-template";
					_entry.Type = "JSON";

					_entry.JsonData ??= new JObject();

					//add required fields if they don't exist
					AddRequiredFieldsToAbility(_entry.JsonData);
				}
			}

			int _typeIndex = Array.FindIndex(valueTypes, s => s.Equals(_entry.Type, StringComparison.OrdinalIgnoreCase));
			if (_typeIndex < 0)
			{
				_typeIndex = 0;
			}
			_typeIndex = EditorGUILayout.Popup("Type", _typeIndex, valueTypes);
			_entry.Type = valueTypes[_typeIndex];

			_entry.SchemaId = EditorGUILayout.TextField("SchemaId", _entry.SchemaId);

			switch (_entry.Type.ToLower())
			{
				case "boolean":
					{
						bool _boolValue = false;
						bool _parseSuccess = bool.TryParse(_entry.Value, out _boolValue);
						if (!_parseSuccess)
						{
							EditorGUILayout.HelpBox("Invalid boolean value, defaulting to false.", MessageType.Warning);
						}
						_boolValue = EditorGUILayout.Toggle("Value", _boolValue);
						_entry.Value = _boolValue.ToString();
						break;
					}
				case "float":
					{
						float _floatValue = 0f;
						bool _parseSuccess = float.TryParse(_entry.Value, out _floatValue);
						if (!_parseSuccess)
						{
							EditorGUILayout.HelpBox("Invalid float value, defaulting to 0.", MessageType.Warning);
						}
						_floatValue = EditorGUILayout.FloatField("Value", _floatValue);
						_entry.Value = _floatValue.ToString();
						break;
					}
				case "integer":
					{
						int _intValue = 0;
						bool _parseSuccess = int.TryParse(_entry.Value, out _intValue);
						if (!_parseSuccess)
						{
							EditorGUILayout.HelpBox("Invalid integer value, defaulting to 0.", MessageType.Warning);
						}
						_intValue = EditorGUILayout.IntField("Value", _intValue);
						_entry.Value = _intValue.ToString();
						break;
					}
				case "long":
					{
						long _longValue = 0;
						bool _parseSuccess = long.TryParse(_entry.Value, out _longValue);
						if (!_parseSuccess)
						{
							EditorGUILayout.HelpBox("Invalid long value, defaulting to 0.", MessageType.Warning);
						}
						_longValue = EditorGUILayout.LongField("Value", _longValue);
						_entry.Value = _longValue.ToString();
						break;
					}
				case "string":
					{
						_entry.Value = EditorGUILayout.TextField("Value", _entry.Value);
						break;
					}
				case "json":
					{
						//initialize JsonData if null
						if (_entry.JsonData == null)
						{
							if (!string.IsNullOrEmpty(_entry.Value))
							{
								try
								{
									_entry.JsonData = JObject.Parse(_entry.Value);
								}
								catch
								{
									_entry.JsonData = new JObject();
								}
							}
							else
							{
								_entry.JsonData = new JObject();
							}
						}

						string _jsonPath = $"Entry_{_entryIndex}";
						DrawJsonEditor(_entry.JsonData, _jsonPath);
						_entry.Value = _entry.JsonData.ToString(Newtonsoft.Json.Formatting.None);

						break;
					}
				default:
					{
						EditorGUILayout.LabelField("Unknown type");
						break;
					}
			}

			EditorGUI.indentLevel--;
		}

		EditorGUILayout.EndVertical();
	}

	private void DrawJsonEditor(JToken _token, string _path)
	{
		if (_token is JObject _obj)
		{
			bool _isExpanded = true;
			jsonFoldouts.TryGetValue(_path, out _isExpanded);
			_isExpanded = EditorGUILayout.Foldout(_isExpanded, "Object", true);
			jsonFoldouts[_path] = _isExpanded;

			if (_isExpanded)
			{
				EditorGUI.indentLevel++;

				List<JProperty> _propertiesToRemove = new List<JProperty>();
				List<Tuple<JProperty, string>> _propertiesToRename = new List<Tuple<JProperty, string>>();

				foreach (var _property in _obj.Properties())
				{
					EditorGUILayout.BeginHorizontal();
					string _propertyName = _property.Name;

					string _uniquePropertyKey = $"{_path}_{_propertyName}_name";
					string _newPropertyName = EditorGUILayout.DelayedTextField(_propertyName);
					if (GUILayout.Button("Delete", GUILayout.Width(60)))
					{
						_propertiesToRemove.Add(_property);
						EditorGUILayout.EndHorizontal();
						continue;
					}
					EditorGUILayout.EndHorizontal();

					if (_newPropertyName != _propertyName)
					{
						_propertiesToRename.Add(Tuple.Create(_property, _newPropertyName));
					}

					EditorGUI.indentLevel++;
					string _childPath = $"{_path}_{_newPropertyName}";
					DrawJsonEditor(_property.Value, _childPath);
					EditorGUI.indentLevel--;
				}

				//remove deleted properties
				foreach (var _prop in _propertiesToRemove)
				{
					_prop.Remove();
				}

				//rename properties
				foreach (var _tuple in _propertiesToRename)
				{
					var _property = _tuple.Item1;
					string _newPropertyName = _tuple.Item2;

					//remove the old property and add a new one with the new name
					JToken _value = _property.Value;
					_property.Remove();
					_obj.Add(_newPropertyName, _value);
				}

				EditorGUILayout.Space();
				if (GUILayout.Button("Add Property"))
				{
					AddJsonProperty(_obj);
				}

				EditorGUI.indentLevel--;
			}
		}
		else if (_token is JArray _array)
		{
			bool _isExpanded = true;
			jsonFoldouts.TryGetValue(_path, out _isExpanded);
			_isExpanded = EditorGUILayout.Foldout(_isExpanded, "Array", true);
			jsonFoldouts[_path] = _isExpanded;

			if (_isExpanded)
			{
				EditorGUI.indentLevel++;

				List<int> _indicesToRemove = new List<int>();

				for (int i = 0; i < _array.Count; i++)
				{
					EditorGUILayout.BeginHorizontal();
					EditorGUILayout.LabelField("Element " + i);
					if (GUILayout.Button("Delete", GUILayout.Width(60)))
					{
						_indicesToRemove.Add(i);
						EditorGUILayout.EndHorizontal();
						continue;
					}
					EditorGUILayout.EndHorizontal();

					EditorGUI.indentLevel++;
					string _elementPath = $"{_path}_Element_{i}";
					DrawJsonEditor(_array[i], _elementPath);
					EditorGUI.indentLevel--;
				}

				//remove elements in reverse order to prevent index shifting
				_indicesToRemove.Sort((a, b) => b.CompareTo(a));
				foreach (var _index in _indicesToRemove)
				{
					_array.RemoveAt(_index);
				}

				EditorGUILayout.Space();
				if (GUILayout.Button("Add Element"))
				{
					AddJsonArrayElement(_array);
				}

				EditorGUI.indentLevel--;
			}
		}
		else if (_token is JValue _value)
		{
			object _val = _value.Value;
			Type _valueType = _val?.GetType();

			EditorGUI.BeginChangeCheck();

			if (_valueType == typeof(string) || _val == null)
			{
				string _strVal = _val != null ? (string)_val : "";
				_strVal = EditorGUILayout.TextField(_strVal);
				if (EditorGUI.EndChangeCheck())
				{
					_value.Value = _strVal;
				}
			}
			else if (_valueType == typeof(bool))
			{
				bool _boolVal = _val != null ? (bool)_val : false;
				_boolVal = EditorGUILayout.Toggle(_boolVal);
				if (EditorGUI.EndChangeCheck())
				{
					_value.Value = _boolVal;
				}
			}
			else if (_valueType == typeof(int))
			{
				int _intVal = _val != null ? (int)_val : 0;
				_intVal = EditorGUILayout.IntField(_intVal);
				if (EditorGUI.EndChangeCheck())
				{
					_value.Value = _intVal;
				}
			}
			else if (_valueType == typeof(long))
			{
				long _longVal = _val != null ? (long)_val : 0;
				_longVal = EditorGUILayout.LongField(_longVal);
				if (EditorGUI.EndChangeCheck())
				{
					_value.Value = _longVal;
				}
			}
			else if (_valueType == typeof(float))
			{
				float _floatVal = _val != null ? Convert.ToSingle(_val) : 0f;
				_floatVal = EditorGUILayout.FloatField(_floatVal);
				if (EditorGUI.EndChangeCheck())
				{
					_value.Value = _floatVal;
				}
			}
			else if (_valueType == typeof(double))
			{
				double _doubleVal = _val != null ? Convert.ToDouble(_val) : 0.0;
				_doubleVal = EditorGUILayout.DoubleField(_doubleVal);
				if (EditorGUI.EndChangeCheck())
				{
					_value.Value = _doubleVal;
				}
			}
			else
			{
				EditorGUILayout.LabelField("Unsupported type: " + _valueType);
			}
		}
	}

	private void AddJsonProperty(JObject _obj)
	{
		//prompt the user for a property name
		string _newPropName = "NewProperty";
		if (_obj.Property(_newPropName) != null)
		{
			int _counter = 1;
			while (_obj.Property(_newPropName + _counter) != null)
			{
				_counter++;
			}
			_newPropName += _counter;
		}
		_obj.Add(new JProperty(_newPropName, ""));
	}

	private void AddJsonArrayElement(JArray _array)
	{
		_array.Add("");
	}

	private void SaveRemoteConfigEntries()
	{
		string _path = EditorUtility.SaveFilePanel("Save Remote Config CSV", "", "RemoteConfig.csv", "csv");
		if (string.IsNullOrEmpty(_path))
		{
			return;
		}
		using (StreamWriter _writer = new StreamWriter(_path))
		{
			_writer.WriteLine("key,type,schemaId,value");
			foreach (var _entry in remoteConfigEntries)
			{
				string _value = _entry.Value;
				//escape value if necessary
				if (_value.Contains(",") || _value.Contains("\""))
				{
					_value = _value.Replace("\"", "\"\"");
					_value = "\"" + _value + "\"";
				}
				string _typeLowercase = _entry.Type.ToLower();
				_writer.WriteLine($"{_entry.Key},{_typeLowercase},{_entry.SchemaId},{_value}");
			}
		}
		Debug.Log("Remote Config entries saved to " + _path);
	}

	private void SortEntriesAlphabetically()
	{
		remoteConfigEntries.Sort((a, b) => string.Compare(a.Key, b.Key, StringComparison.OrdinalIgnoreCase));
	}

	private void AddRequiredFieldsToAbility(JObject _jsonData)
	{
		var _requiredFields = new List<string> { "Name", "Description", "Damage", "Element", "Cooldown" };
		foreach (var _field in _requiredFields)
		{
			if (_jsonData.Property(_field) == null)
			{
				_jsonData.Add(new JProperty(_field, ""));
			}
		}
	}

	private bool ValidateFields()
	{
		bool _isValid = true;
		foreach (var _entry in remoteConfigEntries)
		{
			if (!string.IsNullOrEmpty(_entry.SchemaId))
			{
				if (_entry.SchemaId == "ability-template")
				{
					if (_entry.JsonData == null)
					{
						Debug.LogError($"Entry '{_entry.Key}' has 'ability-template' schemaId but no JSON data.");
						_isValid = false;
						continue;
					}

					var _jsonObject = _entry.JsonData as JObject;
					var _requiredFields = new List<string> { "Name", "Description", "Damage", "Element", "Cooldown" };
					foreach (var _field in _requiredFields)
					{
						if (_jsonObject.Property(_field) == null)
						{
							Debug.LogError($"Entry '{_entry.Key}' is missing required field '{_field}' in JSON data.");
							_isValid = false;
						}
					}
				}
				//more schemaId checks can be added here
			}
		}
		return _isValid;
	}
	private void generateHeatMap(ref string _fileContent)
	{
		EditorApplication.isPlaying = true;
		encodeMiniMapTexture();
		EditorApplication.isPlaying = false;
		PythonRunner.EnsureInitialized();
		generateTempFile(_fileContent);
		PythonRunner.RunFile($"{Application.dataPath}/Scripts/Editor/DataExplorer/Python/HeatMap.py");
	}
	private void encodeMiniMapTexture()
	{
		RenderTexture _renderTexture = AssetDatabase.LoadAssetAtPath<RenderTexture>(miniMapRenderTexPath);
		if (_renderTexture == null)
		{
			Debug.LogError("MiniMapRenderTex not found at path: " + miniMapRenderTexPath);
			return;
		}
		Texture2D _texture = new(_renderTexture.width, _renderTexture.height, TextureFormat.ARGB32, false);

		var _oldRt = RenderTexture.active;
		RenderTexture.active = _renderTexture;

		_texture.ReadPixels(new Rect(0, 0, _renderTexture.width, _renderTexture.height), 0, 0);
		_texture.Apply();

		RenderTexture.active = _oldRt;

		string _projectRootPath = Directory.GetParent(Application.dataPath).FullName;
		string _tempFolderPath = Path.Combine(_projectRootPath, "DataExplorer", "Temp");
		Directory.CreateDirectory(_tempFolderPath);
		string _filePath = Path.Combine(_tempFolderPath, "MiniMapRenderTex.png");
		File.WriteAllBytes(_filePath, _texture.EncodeToPNG());

		Debug.Log("Minimap texture saved to " + _filePath);
	}
}
[Serializable]
public class RemoteConfigEntry
{
	public string Key;
	public string Type;
	public string SchemaId;
	public string Value;
	public JObject JsonData;
	public bool IsExpanded;
}