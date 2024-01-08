using System;
using System.Reflection.Emit;
using ReactiveTag.Utils;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

namespace ReactiveTag.Editor
{
    struct FormValueState
    {
        public string OriginalId;
        public string Name;
        public string IconPath;    
    }
    public class ReactiveTagDetailView
    {
        public delegate void OnTagUpdatedAction(YamlTagDefinition tag);
        public OnTagUpdatedAction OnTagUpdated;
        
        private FormValueState _formValueState;
        private string LabeledTextField(string label, string text, bool grayout = false)
        {
            
            EditorGUILayout.BeginHorizontal();
            if (grayout)
            {
                GUI.enabled = false;
            }
            EditorGUILayout.LabelField(label);
            var newText = EditorGUILayout.TextField(text);
            if (grayout)
            {
                GUI.enabled = true;
            }
            EditorGUILayout.EndHorizontal();
            return newText;
        }
        
        private string TagIconForm(string path)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Tag Icon");
            var icon = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            var newAssetObject = (Texture2D)EditorGUILayout.ObjectField(icon, typeof(Texture2D), false);
            EditorGUILayout.EndHorizontal();
            return AssetDatabase.GetAssetPath(newAssetObject);
        }

        private void TagValidationInfo(TagValidationResult result)
        {
            switch (result)
            {
                case TagValidationResult.EmptyName:
                    EditorGUILayout.HelpBox("Tag name is empty", MessageType.Error);
                    break;
                case TagValidationResult.DuplicatedName:
                    EditorGUILayout.HelpBox("Tag name is duplicated", MessageType.Error);
                    break;
                case TagValidationResult.NameHasInvalidCharacter:
                    EditorGUILayout.HelpBox(@"Tag name has invalid character(s).
First character must be alphabet.
Other characters must be alphabet or number.", MessageType.Error);
                    break;
                case TagValidationResult.NameIsReservedWord:
                    EditorGUILayout.HelpBox("Tag name is reserved word.", MessageType.Error);
                    break;
                case TagValidationResult.OK:
                    break;
                default:
                    EditorGUILayout.HelpBox("Some problem has occured.", MessageType.Error);
                    break;
            }
        }
        public void OnGUI(YamlTagDefinition yaml)
        {
            if (yaml is null) return;
            if (yaml.Id != _formValueState.OriginalId)
            {
                _formValueState.Name = yaml.Name;
                _formValueState.IconPath = yaml.IconPath;
                _formValueState.OriginalId = yaml.Id;
            }
            
            // tag name form
            EditorGUILayout.BeginVertical();
            
            _formValueState.Name = LabeledTextField("Tag Name", _formValueState.Name);
            LabeledTextField("Tag ID", _formValueState.OriginalId, true);
            _formValueState.IconPath = TagIconForm(_formValueState.IconPath);
            
            // validation info
            var validationResult = TagValidator.ValidateTagName(_formValueState.Name, yaml);
            TagValidationInfo(validationResult);
            
            EditorGUILayout.EndVertical();
            
            if (_formValueState.Name != yaml.Name && validationResult == TagValidationResult.OK)
            {
                yaml.Name = _formValueState.Name;
                OnTagUpdated?.Invoke(yaml);
            }
            if (_formValueState.IconPath != yaml.IconPath)
            {
                yaml.IconPath = _formValueState.IconPath;
                OnTagUpdated?.Invoke(yaml);
            }
        }
    }
}