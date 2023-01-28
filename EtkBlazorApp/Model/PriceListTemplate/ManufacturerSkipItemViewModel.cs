namespace EtkBlazorApp
{
    public class ManufacturerSkipItemViewModel
    {
        public int manufacturer_id { get; set; }
        public string Name { get; set; }
        public SkipManufacturerListType ListType { get; set; }
        public string ListTypeDescription => (ListType == SkipManufacturerListType.black_list ? "Черный список" : "Белый список");
    }

}
