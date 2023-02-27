using System;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace DeepDreams.DataPersistence
{
    public class FileDataHandler
    {
        private readonly string _dataDirPath;
        private readonly string _dataFileName;
        private readonly string _fullPath;

        private readonly bool _useEncryption;
        private readonly string _encryptionCodeword;

        public FileDataHandler(string dataDirPath, string dataFileName, bool useEncryption)
        {
            _dataDirPath = dataDirPath;
            _dataFileName = dataFileName;
            _useEncryption = useEncryption;
            _encryptionCodeword = "d*5Q4Qc@G^w5";

            _fullPath = Path.Combine(_dataDirPath, _dataFileName);
            ;
        }

        public T Load<T>()
        {
            T loadedData = default;

            if (File.Exists(_fullPath))
            {
                try
                {
                    string dataToLoad = "";

                    using (FileStream stream = new FileStream(_fullPath, FileMode.Open))
                    {
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            dataToLoad = reader.ReadToEnd();
                        }
                    }

                    if (_useEncryption) dataToLoad = EncryptDecrypt(dataToLoad);

                    loadedData = JsonConvert.DeserializeObject<T>(dataToLoad);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error occured when trying to save data to file: {_fullPath}\n{e}");
                }
            }

            return loadedData;
        }

        public void Save<T>(T data)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_fullPath));

                string dataToStore = JsonConvert.SerializeObject(data, Formatting.Indented);

                if (_useEncryption) dataToStore = EncryptDecrypt(dataToStore);

                using (FileStream stream = new FileStream(_fullPath, FileMode.Create))
                {
                    using (StreamWriter writer = new StreamWriter(stream))
                    {
                        writer.Write(dataToStore);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error occured when trying to save data to file: {_fullPath}\n{e}");
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