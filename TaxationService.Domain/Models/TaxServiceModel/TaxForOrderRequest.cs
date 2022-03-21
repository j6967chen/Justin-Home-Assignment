namespace TaxationService.Domain.Models.TaxServiceModel
{
    public enum Calculator
    {
        Vertex,
        TaxJar
    }

    public class TaxForOrderRequest
    {
        public Calculator CalculatorType { get; set; }
        public CalculateTaxRequestSeller Seller { get; set; }
        public List<LineItemRequest> LineItems { get; set; }
        public CustomerAddress CustomerAddress { get; set; }
        public decimal Amount { get; set; }
        public decimal Shipping { get; set; }
    }

    public class Address
    {
        public string Country { get; set; }
        public string Zip { get; set; }
        public string State { get; set; }
        public string City { get; set; }
        public string Street { get; set; }
    }

    public class CalculateTaxRequestSeller : Address
    {
    }

    public class CustomerAddress: Address
    {

    }

    public class LineItemRequest
    {
        public string Id { get; set; }

        public int Quantity { get; set; }

        public string ProductTaxCode { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal Discount { get; set; }
    }
  
}
