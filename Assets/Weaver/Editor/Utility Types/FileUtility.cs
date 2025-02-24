﻿using UnityEditor;
using UnityEngine;

namespace Weaver
{
    [InitializeOnLoad]
    public static class FileUtility
    {
        private static readonly string m_ProjectPath;
        private static readonly int m_ProjectPathLength;

        static FileUtility()
        {
            // Get our data path
            m_ProjectPath = Application.dataPath;
            // Remove 'Assets'
            m_ProjectPath = m_ProjectPath.Substring(0, m_ProjectPath.Length - /* Assets */ 6);
            // Store our length
            m_ProjectPathLength = m_ProjectPath.Length;
        }

        /// <summary>
        /// Gets the folder at the root of the project below 'Assets'
        /// </summary>
        public static string projectPath => m_ProjectPath;

        /// <summary>
        /// Converts a full System Path to a Unity project relative path.
        /// </summary>
        public static string SystemToProjectPath(string systemPath)
        {
            var systemPathLength = systemPath.Length;
            var assetPathLength = systemPathLength - m_ProjectPathLength;
            if (assetPathLength <= 0)
            {
                throw new System.InvalidOperationException("Unable to convert system path to asset path");
            }
            return systemPath.Substring(m_ProjectPathLength, assetPathLength);
        }
    }
}