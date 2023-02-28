using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace DeepDreams.DataPersistence
{
    public class FileDataHandler
    {
        private struct FileInfo
        {
            public readonly string dataDirPath;
            public readonly string dataFileName;
            public readonly string fullPath;
            public readonly bool useEncryption;

            public FileInfo(string dataDirPath, string dataFileName, string fullPath, bool useEncryption)
            {
                this.dataDirPath = dataDirPath;
                this.dataFileName = dataFileName;
                this.fullPath = fullPath;
                this.useEncryption = useEncryption;
            }
        }

        private readonly Dictionary<string, FileInfo> _fileDictionary;

        private readonly string _encryptionCodeword;

        public FileDataHandler()
        {
            _fileDictionary = new Dictionary<string, FileInfo>();
            _encryptionCodeword = "d*5Q4Qc@G^w5";
        }

        public void AddFile(string dataDirPath, string dataFileName, bool useEncryption, string dataObjectId)
        {
            if (_fileDictionary.ContainsKey(dataObjectId))
            {
                Debug.LogWarning($"Persistent Data File Handler: File {dataFileName} is already being used.");
                return;
            }

            FileInfo fileInfo = new FileInfo(dataDirPath, dataFileName, Path.Combine(dataDirPath, dataFileName), useEncryption);
            _fileDictionary.Add(dataObjectId, fileInfo);
        }

        public T Load<T>(string dataObjectId)
        {
            FileInfo fileInfo = _fileDictionary[dataObjectId];
            T loadedData = default;

            if (File.Exists(fileInfo.fullPath))
            {
                try
                {
                    string dataToLoad = "";

                    using (FileStream stream = new FileStream(fileInfo.fullPath, FileMode.Open))
                    {
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            dataToLoad = reader.ReadToEnd();
                        }
                    }

                    if (fileInfo.useEncryption) dataToLoad = EncryptDecrypt(dataToLoad);

                    loadedData = JsonConvert.DeserializeObject<T>(dataToLoad);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error occured when trying to save data to file: {fileInfo.fullPath}\n{e}");
                }
            }

            return loadedData;
        }

        public void Save(PersistentDataObject data)
        {
            FileInfo fileInfo = _fileDictionary[data.id];

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(fileInfo.fullPath));

                string dataToStore = JsonConvert.SerializeObject(data.persistentData, Formatting.Indented);

                if (fileInfo.useEncryption) dataToStore = EncryptDecrypt(dataToStore);

                using (FileStream stream = new FileStream(fileInfo.fullPath, FileMode.Create))
                {
                    using (StreamWriter writer = new StreamWriter(stream))
                    {
                        writer.Write(dataToStore);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error occured when trying to save data to file: {fileInfo.fullPath}\n{e}");
            }
        }

        // Xor encryption
        private string EncryptDecrypt(string data)
        {
            string modifiedData = "";

            for (int i = 0; i < data.Length; i++)
            {
                modifiedData += (char)(data[i] ^ _encryptionCodeword[i % _encryptionCodeword.Length]);
            }

            return modifiedData;
        }
    }
}