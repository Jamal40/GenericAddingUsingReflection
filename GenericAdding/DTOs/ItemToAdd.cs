namespace GenericAdding.DTOs
{
    public class ItemToAdd
    {
        public string TableName { get; set; }
        public List<FieldsValues> Fields { get; set; }
    }
    public class FieldsValues
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }
}
