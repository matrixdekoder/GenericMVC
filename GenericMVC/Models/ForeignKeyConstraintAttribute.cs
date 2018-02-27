using System;

namespace GenericMVC.Models
{
    internal class ForeignKeyConstraintAttribute : Attribute
    {
        private string v;

        public ForeignKeyConstraintAttribute(string v)
        {
            this.v = v;
        }
    }
}