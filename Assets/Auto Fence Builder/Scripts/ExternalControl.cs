using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace AFWB
{
    /// <summary>
    /// Represents a record of a parameter change, storing both the parameter name and its new value.
    /// </summary>
    public class ParameterChangeRecord
    {
        /// <summary>
        /// Gets the name of the parameter that was changed.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the new value of the parameter after the change.
        /// </summary>
        public dynamic Value { get; }

        /// <summary>
        /// Initializes a new gizmoSingletonInstance of the <see cref="ParameterChangeRecord"/> class.
        /// </summary>
        /// <param name="name">The name of the parameter that is changed.</param>
        /// <param name="value">The new value of the parameter.</param>
        public ParameterChangeRecord(string name, dynamic value)
        {
            Name = name;
            Value = value;
        }
    }

    /// <summary>
    /// A utility class for dynamically setting and getting values of fields in an gizmoSingletonInstance,
    /// and tracking each set operation as a change record.
    /// </summary>
    public partial class AutoFenceCreator
    {
        private List<ParameterChangeRecord> parameterChanges = new List<ParameterChangeRecord>();

        /// <summary>
        /// Sets the value of a field by name and records this change.
        /// </summary>
        /// <param name="variableName">The name of the variable to set.</param>
        /// <param name="newValue">The new value to set the field to.</param>
        /// <remarks>
        /// This method uses reflection to set the field value. If the field does not exist,
        /// an error is logged. Ensure that the field name is correct and accessible.
        /// </remarks>
        /// <example>
        /// Here is how you can use the SetParameterValue method:
        /// <code>
        /// AutoFenceCreator creator = new AutoFenceCreator();
        /// creator.SetParameterValue("exampleField", 42);
        /// </code>
        /// </example>
        public void SetParameterValue(string variableName, object newValue)
        {
            FieldInfo field = typeof(AutoFenceCreator).GetField(variableName, BindingFlags.Public | BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(this, newValue);
                parameterChanges.Add(new ParameterChangeRecord(variableName, newValue));
            }
            else
            {
                Debug.LogError("Field not found: " + variableName);
            }
        }

        /// <summary>
        /// Retrieves the current value of a field by its name.
        /// </summary>
        /// <param name="variableName">The name of the field to retrieve.</param>
        /// <returns>The current value of the field, or null if the field is not found.</returns>
        /// <remarks>
        /// This method uses reflection to access the field value. If the field does not exist,
        /// an error is logged. Ensure that the field name is correct and accessible.
        /// </remarks>
        /// <example>
        /// Here is how you can use the GetParameterValue method:
        /// <code>
        /// AutoFenceCreator creator = new AutoFenceCreator();
        /// int value = (int)creator.GetParameterValue("exampleField");
        /// </code>
        /// </example>
        public object GetParameterValue(string variableName)
        {
            FieldInfo field = typeof(AutoFenceCreator).GetField(variableName, BindingFlags.Public | BindingFlags.Instance);
            if (field != null)
            {
                return field.GetValue(this);
            }
            else
            {
                Debug.LogError("Field not found: " + variableName);
                return null;
            }
        }

        /// <summary>
        /// Retrieves a list of all parameter change records.
        /// </summary>
        /// <returns>A list containing all the parameter change records.</returns>
        /// <example>
        /// Here is how to retrieve the history of parameter changes:
        /// <code>
        /// AutoFenceCreator creator = new AutoFenceCreator();
        /// creator.SetParameterValue("exampleField", 42);
        /// List<ParameterChangeRecord> history = creator.GetParameterHistory();
        /// foreach (var record in history)
        /// {
        ///     Debug.Log($"Parameter: {record.Name}, New Value: {record.Value}");
        /// }
        /// </code>
        /// </example>
        public List<ParameterChangeRecord> GetParameterHistory()
        {
            return new List<ParameterChangeRecord>(parameterChanges);
        }
    }
}