﻿using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Mono.Cecil;
using Mono.Collections.Generic;
using UnityEditor.Callbacks;
using UnityEngine.SceneManagement;

namespace Weaver
{
    [CreateAssetMenu(menuName = "Weaver/Settings", fileName = "Weaver Settings")]
    public class WeaverSettings : ScriptableObject
    {
        [SerializeField]
        private List<WeavedAssembly> m_WeavedAssemblies;

        [SerializeField]
        private ComponentController m_Components;

        // Resolver
        private WeaverAssemblyResolver m_Resolver;

        public WeaverAssemblyResolver resolver
        {
            get { return m_Resolver; }
        }

        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            // Find all settings
            string[] guids = AssetDatabase.FindAssets("t:WeaverSettings");
            // Load them all
            for (int i = 0; i < guids.Length; i++)
            {
                // Convert our path
                string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
                // Load it
                AssetDatabase.LoadAssetAtPath<WeaverSettings>(assetPath);
            }
        }

        [PostProcessScene()]
        public static void PostprocessScene()
        {
            // Only run this code if we are building the player 
            if (BuildPipeline.isBuildingPlayer)
            {
                // Get our current scene 
                Scene scene = SceneManager.GetActiveScene();
                // If we are the first scene (we only want to run once)
                if (scene.IsValid() && scene.buildIndex == 0)
                {
                    // Find all settings
                    string[] guids = AssetDatabase.FindAssets("t:WeaverSettings");
                    // Load them all
                    if (guids.Length > 0)
                    {
                        // Convert our path
                        string assetPath = AssetDatabase.GUIDToAssetPath(guids[0]);
                        // Load it
                        WeaverSettings settings = AssetDatabase.LoadAssetAtPath<WeaverSettings>(assetPath);
                        // Invoke
                        settings.WeaveBuild();
                    }
                }
            }
        }

        private void OnEnable()
        {
            // Create a collection for all the assemblies that changed. 
            Collection<ModuleDefinition> changedModules = new Collection<ModuleDefinition>();

            if (m_WeavedAssemblies == null)
            {
                m_WeavedAssemblies = new List<WeavedAssembly>();
            }

            List<WeavedAssembly> assembliesToWrite = new List<WeavedAssembly>();
            // Loop over them all
            for (int i = 0; i < m_WeavedAssemblies.Count; i++)
            {
                if (m_WeavedAssemblies[i].HasChanges())
                {

                    assembliesToWrite.Add(m_WeavedAssemblies[i]);
                }
            }

            WeaveAssemblies(assembliesToWrite);
        }

        private void WeaveBuild()
        {
            IList<WeavedAssembly> assemblyToWeave = new List<WeavedAssembly>();
            // loop over them all
            for (int i = 0; i < m_WeavedAssemblies.Count; i++)
            {
                if (m_WeavedAssemblies[i].IsUnityGenerated() && m_WeavedAssemblies[i].Exists())
                {
                    assemblyToWeave.Add(m_WeavedAssemblies[i]);
                }
            }
            WeaveAssemblies(assemblyToWeave);
        }

        private void WeaveAssemblies(IList<WeavedAssembly> assemblies)
        {
            AssemblyUtility.PopulateAssemblyCache();
            // Create new resolver
            m_Resolver = new WeaverAssemblyResolver();
            // Create a new reader
            ReaderParameters readerParameters = new ReaderParameters();
            // Pass the reader our resolver 
            readerParameters.AssemblyResolver = m_Resolver;
            // Create a list of definitions
            Collection<ModuleDefinition> editingModules = new Collection<ModuleDefinition>();
            for (int i = 0; i < assemblies.Count; i++)
            {
                // We have a changed assembly so we need to get the defintion to modify. 
                ModuleDefinition moduleDefinition = ModuleDefinition.ReadModule(assemblies[i].relativePath, readerParameters);
                // Add it to our list
                editingModules.Add(moduleDefinition);
            }
            // Initialize our component manager
            m_Components.Initialize(this);
            // Visit Modules
            m_Components.VisitModules(editingModules);
            // Save
            for (int i = 0; i < assemblies.Count; i++)
            {
                editingModules[i].Write(assemblies[i].GetSystemPath());
            }
            assemblies.Clear();
        }
    }
}
