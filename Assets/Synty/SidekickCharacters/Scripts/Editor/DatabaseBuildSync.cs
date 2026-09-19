#if UNITY_EDITOR

using System.IO;
using UnityEditor;

namespace Synty.SidekickCharacters
{
    internal static class DatabaseBuildSync
    {
        private const string SourcePath = "Assets/Synty/SidekickCharacters/Database/Side_Kick_Data.db";
        private const string RuntimePath = "Assets/Synty/SidekickCharacters/Resources/Database/Side_Kick_Data.bytes";

        public static void SyncRuntimeDatabase()
        {
            if (!File.Exists(SourcePath))
            {
                UnityEngine.Debug.LogError($"Sidekick database not found: {SourcePath}");
                return;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(RuntimePath));
            File.Copy(SourcePath, RuntimePath, true);
            AssetDatabase.ImportAsset(RuntimePath, ImportAssetOptions.ForceUpdate);
            UnityEngine.Debug.Log($"Sidekick runtime database synced to {RuntimePath}.");
        }
    }
}

#endif
