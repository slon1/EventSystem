using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;

public class TaskLoader {
	private readonly string json;

	public TaskLoader(string json) {
		this.json = json;
	}
	
	public List<IQuestCondition> LoadAllQuests() {
		try {
			var settings = new JsonSerializerSettings {
				TypeNameHandling = TypeNameHandling.Auto,
				Error = (sender, args) => {
					Debug.LogError($"JSON parsing error: {args.ErrorContext.Error.Message} at path {args.ErrorContext.Path}");
					args.ErrorContext.Handled = true; 
				}
			};

			var quests = JsonConvert.DeserializeObject<List<IQuestCondition>>(json, settings);

			
			if (quests == null) {
				Debug.LogError("Failed to deserialize quests: JSON is empty or invalid.");
				return new List<IQuestCondition>();
			}

			
			for (int i = quests.Count - 1; i >= 0; i--) {
				var quest = quests[i];
				if (quest == null || string.IsNullOrEmpty(quest.TaskID)) {
					Debug.LogError($"Invalid quest at index {i}: null or missing TaskID.");
					quests.RemoveAt(i);
				}
			}

			return quests;
		}
		catch (JsonException ex) {
			Debug.LogError($"Failed to load quests: {ex.Message}");
			return new List<IQuestCondition>();
		}
	}
}