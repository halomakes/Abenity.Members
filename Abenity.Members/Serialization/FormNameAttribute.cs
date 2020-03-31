using System;

namespace Abenity.Members.Serialization
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    internal class FormNameAttribute : Attribute
    {
        public FormNameAttribute(string formName)
        {
            FormName = formName;
        }

        public string FormName { get; }

        public bool Encode { get; set; }
    }
}
