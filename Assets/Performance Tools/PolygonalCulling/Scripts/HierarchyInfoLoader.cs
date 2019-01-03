using UnityEngine;
using System.Collections;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace NGS.PolygonalCulling
{
    public static class HierarchyInfoLoader
    {
        private static string _directoryPath = "Assets/PCulling_Data/Resources/";
        private static string _extension = ".bytes";

        public static void SaveHierarchy(string name, GraphicCallsHierarchy hierarchy)
        {
            if (!Directory.Exists(_directoryPath))
                Directory.CreateDirectory(_directoryPath);

            using (FileStream fileStream = new FileStream(_directoryPath + name + _extension, FileMode.Create))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(fileStream, hierarchy);
            }
        }

        public static GraphicCallsHierarchy LoadHierarchy(string name)
        {
            GraphicCallsHierarchy hierarchy = null;

            TextAsset textAsset = Resources.Load(name) as TextAsset;

            if (textAsset == null)
                return null;

            using (MemoryStream stream = new MemoryStream(textAsset.bytes))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                hierarchy = formatter.Deserialize(stream) as GraphicCallsHierarchy;
            }

            return hierarchy;
        }

        public static void DeleteHierarchyInfo(string name)
        {
            if (File.Exists(_directoryPath + name + _extension))
                File.Delete(_directoryPath + name + _extension);
        }
    }
}
