namespace EtkBlazorApp.Model.Marketplace;

internal class MarketplaceStepDiscountModel
{
    public string Marketplace { get; set; } = "all";
    public int MinPriceInRub { get; set; }
    public decimal Ratio { get; set; }
}
