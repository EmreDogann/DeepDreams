using System;
using System.Collections.Generic;
using System.Reflection;
using DeepDreams.DataPersistence.Data;

namespace DeepDreams.UI
{
    [Serializable]
    public class DataPersistenceFieldReference<T> where T : PersistentData
    {
        public FieldInfo[] fieldInfoArray;
        public string[] fieldNames;
        public int selectedField;
        public string selectedName;

        public DataPersistenceFieldReference()
        {
            GetFields();
        }

        public void GetFields()
        {
            fieldInfoArray = typeof(T).GetFields(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance);

            var fieldsNamesList = new List<string>();

            for (int i = 0; i < fieldInfoArray.Length; i++)
            {
                fieldsNamesList.Add(fieldInfoArray[i].Name + " (" + ParseTypeToString(fieldInfoArray[i].FieldType) + ")");
            }

            fieldNames = fieldsNamesList.ToArray();
            selectedField = 0;
            selectedName = fieldInfoArray[selectedField].Name;
        }

        private string ParseTypeToString(Type type)
        {
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Int16 or TypeCode.Int32 or TypeCode.Int64:
                    return "Int";
                case TypeCode.Single:
                    return "Float";
                case TypeCode.Boolean:
                    return "Bool";
                case TypeCode.String:
                    return "String";
                default:
                    return "Type not recognized.";
            }
        }
    }
}